using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

public record FolderImportRequest(string Path);

public record GitHubImportRequest(string RepoUrl, string? Branch, string? Path, string? Pat);

[ApiController]
[Route("api/project/import")]
public class ImportController : ControllerBase
{
    private static readonly string[] ImportableExtensions = { ".json", ".xml", ".yaml", ".yml", ".md" };
    private const long MaxFileBytes = 5 * 1024 * 1024;
    private const int MaxFileCount = 2000;

    private readonly ApimProjectImportService _importService;
    private readonly IHttpClientFactory _httpClientFactory;

    public ImportController(ApimProjectImportService importService, IHttpClientFactory httpClientFactory)
    {
        _importService = importService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("zip")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<ActionResult<ApimProject>> ImportZip(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Upload a ZIP file in the 'file' form field." });
        }

        try
        {
            var files = new Dictionary<string, string>();
            using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries.Take(MaxFileCount))
            {
                if (entry.Length == 0 || entry.Length > MaxFileBytes || !HasImportableExtension(entry.FullName))
                {
                    continue;
                }
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                files[entry.FullName] = await reader.ReadToEndAsync();
            }

            if (files.Count == 0)
            {
                return BadRequest(new { error = "The ZIP contains no importable files (.json/.xml/.yaml/.md)." });
            }

            var fallbackName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
            return Ok(_importService.ParseProject(files, fallbackName));
        }
        catch (InvalidDataException)
        {
            return BadRequest(new { error = "The uploaded file is not a valid ZIP archive." });
        }
    }

    [HttpPost("folder")]
    public async Task<ActionResult<ApimProject>> ImportFolder([FromBody] FolderImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
        {
            return BadRequest(new { error = "Provide a folder path." });
        }

        var root = System.IO.Path.GetFullPath(request.Path.Trim());
        if (!Directory.Exists(root))
        {
            return BadRequest(new { error = $"Folder not found: {root}" });
        }

        var files = new Dictionary<string, string>();
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (files.Count >= MaxFileCount) break;

            var info = new FileInfo(path);
            if (info.Length == 0 || info.Length > MaxFileBytes || !HasImportableExtension(path))
            {
                continue;
            }

            var relative = System.IO.Path.GetRelativePath(root, path);
            files[relative] = await System.IO.File.ReadAllTextAsync(path);
        }

        if (files.Count == 0)
        {
            return BadRequest(new { error = "The folder contains no importable files (.json/.xml/.yaml/.md)." });
        }

        return Ok(_importService.ParseProject(files, new DirectoryInfo(root).Name));
    }

    [HttpPost("github")]
    public async Task<ActionResult<ApimProject>> ImportGitHub([FromBody] GitHubImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RepoUrl))
        {
            return BadRequest(new { error = "Provide a GitHub repository URL." });
        }

        if (!TryParseGitHubRepo(request.RepoUrl, out var owner, out var repo))
        {
            return BadRequest(new { error = "Could not parse the repository URL. Expected https://github.com/<owner>/<repo>." });
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("apim-policy-studio");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        if (!string.IsNullOrWhiteSpace(request.Pat))
        {
            client.DefaultRequestHeaders.Authorization = new("Bearer", request.Pat.Trim());
        }

        try
        {
            var branch = request.Branch?.Trim();
            if (string.IsNullOrEmpty(branch))
            {
                using var repoInfo = JsonDocument.Parse(
                    await GetStringOrThrowAsync(client, $"https://api.github.com/repos/{owner}/{repo}"));
                branch = repoInfo.RootElement.GetProperty("default_branch").GetString() ?? "main";
            }

            using var tree = JsonDocument.Parse(await GetStringOrThrowAsync(client,
                $"https://api.github.com/repos/{owner}/{repo}/git/trees/{Uri.EscapeDataString(branch)}?recursive=1"));

            var subPath = request.Path?.Trim().Trim('/') ?? string.Empty;
            var files = new Dictionary<string, string>();

            foreach (var node in tree.RootElement.GetProperty("tree").EnumerateArray())
            {
                if (files.Count >= MaxFileCount) break;
                if (node.GetProperty("type").GetString() != "blob") continue;

                var path = node.GetProperty("path").GetString() ?? "";
                if (!HasImportableExtension(path)) continue;
                if (subPath.Length > 0 &&
                    !path.StartsWith(subPath + "/", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals(subPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (node.TryGetProperty("size", out var size) && size.GetInt64() > MaxFileBytes) continue;

                var rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{Uri.EscapeDataString(branch)}/{path}";
                files[path] = await GetStringOrThrowAsync(client, rawUrl);
            }

            if (files.Count == 0)
            {
                return BadRequest(new { error = "No importable files found at that repository/path." });
            }

            return Ok(_importService.ParseProject(files, repo));
        }
        catch (HttpRequestException ex)
        {
            var hint = ex.StatusCode == System.Net.HttpStatusCode.NotFound
                ? " Check the repository URL/branch, and supply a PAT if the repo is private."
                : ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden
                    ? " The PAT was rejected or lacks access to this repository."
                    : string.Empty;
            return BadRequest(new { error = $"GitHub request failed ({ex.StatusCode}).{hint}" });
        }
    }

    private static async Task<string> GetStringOrThrowAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"GET {url} returned {(int)response.StatusCode}", null, response.StatusCode);
        }
        return await response.Content.ReadAsStringAsync();
    }

    private static bool HasImportableExtension(string path)
    {
        var ext = System.IO.Path.GetExtension(path);
        return ImportableExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryParseGitHubRepo(string url, out string owner, out string repo)
    {
        owner = repo = string.Empty;
        var trimmed = url.Trim();

        if (trimmed.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "https://github.com/" + trimmed.Substring("git@github.com:".Length);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            !uri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var segments = uri.AbsolutePath.Trim('/').Split('/');
        if (segments.Length < 2)
        {
            return false;
        }

        owner = segments[0];
        repo = segments[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? segments[1][..^4]
            : segments[1];
        return owner.Length > 0 && repo.Length > 0;
    }
}
