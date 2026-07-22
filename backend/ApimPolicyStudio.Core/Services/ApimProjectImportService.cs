using System.Text.Json;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

/// <summary>
/// Reverses ApimProjectGeneratorService: parses an APIOps-style file structure
/// (from a ZIP, folder, or Git repository) back into an ApimProject.
/// </summary>
public class ApimProjectImportService
{
    public ApimProject ParseProject(IDictionary<string, string> inputFiles, string? fallbackName = null)
    {
        var files = StripCommonRoot(NormalizePaths(inputFiles));
        var project = new ApimProject
        {
            Name = fallbackName ?? "imported-apim-project"
        };

        if (files.TryGetValue("README.md", out var readme))
        {
            ParseReadme(readme, project);
        }

        if (files.TryGetValue("policy.xml", out var globalPolicy))
        {
            project.GlobalPolicy = globalPolicy;
        }

        ParseApis(files, project);
        ParseBackends(files, project);
        ParseNamedValues(files, project);
        ParsePolicyFragments(files, project);
        ParseProducts(files, project);
        ParseTags(files, project);
        ParseSchemas(files, project);

        return project;
    }

    private static Dictionary<string, string> NormalizePaths(IDictionary<string, string> files)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, content) in files)
        {
            var clean = path.Replace('\\', '/').TrimStart('/');
            if (clean.Length > 0)
            {
                normalized[clean] = content;
            }
        }
        return normalized;
    }

    /// <summary>
    /// Section folders can be spelled "named values", "named-values", or "namedValues"
    /// depending on the producing tool — compare with spaces/dashes removed.
    /// </summary>
    private static string NormalizeSegment(string segment)
    {
        return segment.Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
    }

    private static readonly HashSet<string> SectionMarkers = new()
    {
        "apis", "backends", "namedvalues", "policyfragments", "products", "tags", "schemas"
    };

    /// <summary>
    /// When the package lives in a repo subfolder, every path shares a prefix
    /// (e.g. "infra/apim/apis/..."). Detect the most common prefix before a
    /// known section folder and re-key everything relative to it.
    /// </summary>
    private static Dictionary<string, string> StripCommonRoot(Dictionary<string, string> files)
    {
        var rootVotes = new Dictionary<string, int>();
        foreach (var path in files.Keys)
        {
            var segments = path.Split('/');
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (SectionMarkers.Contains(NormalizeSegment(segments[i])))
                {
                    var root = string.Join('/', segments.Take(i));
                    rootVotes[root] = rootVotes.GetValueOrDefault(root) + 1;
                    break;
                }
            }
        }

        if (rootVotes.Count == 0)
        {
            return files;
        }

        var bestRoot = rootVotes.OrderByDescending(v => v.Value).First().Key;
        if (bestRoot.Length == 0)
        {
            return files;
        }

        var prefix = bestRoot + "/";
        var rebased = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, content) in files)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                rebased[path.Substring(prefix.Length)] = content;
            }
        }
        return rebased;
    }

    private static void ParseReadme(string readme, ApimProject project)
    {
        var lines = readme.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        var title = lines.FirstOrDefault(l => l.StartsWith("# "));
        if (title != null)
        {
            project.Name = title.Substring(2).Trim();
            var titleIndex = lines.IndexOf(title);
            var description = lines.Skip(titleIndex + 1)
                .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"));
            if (description != null)
            {
                project.Description = description.Trim();
            }
        }
    }

    /// <summary>Groups files under a section folder by their immediate child folder name.</summary>
    private static Dictionary<string, Dictionary<string, string>> GroupSection(Dictionary<string, string> files, string sectionMarker)
    {
        var groups = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, content) in files)
        {
            var segments = path.Split('/');
            if (segments.Length < 3 || NormalizeSegment(segments[0]) != sectionMarker)
            {
                continue;
            }

            var itemName = segments[1];
            var rest = string.Join('/', segments.Skip(2));
            if (!groups.TryGetValue(itemName, out var group))
            {
                group = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                groups[itemName] = group;
            }
            group[rest] = content;
        }
        return groups;
    }

    private static void ParseApis(Dictionary<string, string> files, ApimProject project)
    {
        foreach (var (folderName, apiFiles) in GroupSection(files, "apis"))
        {
            var api = new ApiDefinition { Name = folderName, DisplayName = folderName };

            if (apiFiles.TryGetValue("apiInformation.json", out var infoJson) && TryParseJson(infoJson, out var info))
            {
                api.Name = GetString(info, "name") ?? folderName;
                if (TryGetProperty(info, "properties", out var props))
                {
                    api.DisplayName = GetString(props, "displayName") ?? api.Name;
                    api.Path = GetString(props, "path") ?? "";
                    api.Description = GetString(props, "description") ?? "";
                    api.ServiceUrl = GetString(props, "serviceUrl");
                    api.ApiVersion = GetString(props, "apiVersion");
                    api.ApiVersionSetId = GetString(props, "apiVersionSetId");
                    api.ApiRevision = GetString(props, "apiRevision") ?? "1";
                    api.IsCurrent = GetBool(props, "isCurrent", true);
                    api.SubscriptionRequired = GetBool(props, "subscriptionRequired", true);
                    api.Protocols = GetStringArray(props, "protocols") is { Count: > 0 } protocols
                        ? protocols
                        : new List<string> { "https" };
                    if (TryGetProperty(props, "subscriptionKeyParameterNames", out var keyNames))
                    {
                        api.SubscriptionKeyParameterName = GetString(keyNames, "header");
                    }
                }
            }

            if (apiFiles.TryGetValue("policy.xml", out var policy))
            {
                api.Policy = policy;
            }

            var specEntry = apiFiles.FirstOrDefault(f =>
                f.Key.StartsWith("specification.", StringComparison.OrdinalIgnoreCase));
            if (specEntry.Key != null)
            {
                api.SpecificationContent = specEntry.Value;
                api.SpecificationFormat = specEntry.Key.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
                                          specEntry.Key.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)
                    ? "openapi-yaml"
                    : specEntry.Value.Contains("\"swagger\"") ? "swagger" : "openapi";
            }

            // Operations live in operations/<name>/...
            var operationGroups = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (path, content) in apiFiles)
            {
                var segments = path.Split('/');
                if (segments.Length >= 3 && NormalizeSegment(segments[0]) == "operations")
                {
                    var opName = segments[1];
                    if (!operationGroups.TryGetValue(opName, out var group))
                    {
                        group = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        operationGroups[opName] = group;
                    }
                    group[string.Join('/', segments.Skip(2))] = content;
                }
            }

            foreach (var (opFolder, opFiles) in operationGroups)
            {
                var operation = new Operation { Name = opFolder, DisplayName = opFolder, Method = "GET", UrlTemplate = "/" };

                if (opFiles.TryGetValue("operationInformation.json", out var opJson) && TryParseJson(opJson, out var opInfo))
                {
                    operation.Name = GetString(opInfo, "name") ?? opFolder;
                    if (TryGetProperty(opInfo, "properties", out var opProps))
                    {
                        operation.DisplayName = GetString(opProps, "displayName") ?? operation.Name;
                        operation.Method = GetString(opProps, "method") ?? "GET";
                        operation.UrlTemplate = GetString(opProps, "urlTemplate") ?? "/";
                        operation.Description = GetString(opProps, "description");
                    }
                }

                if (opFiles.TryGetValue("policy.xml", out var opPolicy))
                {
                    operation.Policy = opPolicy;
                }

                api.Operations.Add(operation);
            }

            project.Apis.Add(api);
        }
    }

    private static void ParseBackends(Dictionary<string, string> files, ApimProject project)
    {
        foreach (var (folderName, backendFiles) in GroupSection(files, "backends"))
        {
            var backend = new Backend { Name = folderName, Title = folderName };

            if (backendFiles.TryGetValue("backendInformation.json", out var json) && TryParseJson(json, out var info))
            {
                backend.Name = GetString(info, "name") ?? folderName;
                if (TryGetProperty(info, "properties", out var props))
                {
                    backend.Title = GetString(props, "title") ?? backend.Name;
                    backend.Url = GetString(props, "url") ?? "";
                    backend.Protocol = GetString(props, "protocol") ?? "http";
                }
            }

            project.Backends.Add(backend);
        }
    }

    private static void ParseNamedValues(Dictionary<string, string> files, ApimProject project)
    {
        foreach (var (folderName, nvFiles) in GroupSection(files, "namedvalues"))
        {
            var namedValue = new NamedValue { Name = folderName, DisplayName = folderName };

            if (nvFiles.TryGetValue("namedValueInformation.json", out var json) && TryParseJson(json, out var info))
            {
                namedValue.Name = GetString(info, "name") ?? folderName;
                if (TryGetProperty(info, "properties", out var props))
                {
                    namedValue.DisplayName = GetString(props, "displayName") ?? namedValue.Name;
                    namedValue.Value = GetString(props, "value") ?? "";
                    namedValue.Secret = GetBool(props, "secret", false);
                    namedValue.Tags = GetStringArray(props, "tags") ?? new List<string>();
                }
            }

            project.NamedValues.Add(namedValue);
        }
    }

    private static void ParsePolicyFragments(Dictionary<string, string> files, ApimProject project)
    {
        foreach (var (folderName, fragmentFiles) in GroupSection(files, "policyfragments"))
        {
            var fragment = new PolicyFragmentDefinition { Name = folderName };

            if (fragmentFiles.TryGetValue("policyFragmentInformation.json", out var json) && TryParseJson(json, out var info))
            {
                fragment.Name = GetString(info, "name") ?? folderName;
                if (TryGetProperty(info, "properties", out var props))
                {
                    fragment.Description = GetString(props, "description") ?? "";
                }
            }

            if (fragmentFiles.TryGetValue("policy.xml", out var policy))
            {
                fragment.PolicyContent = policy;
            }

            project.PolicyFragments.Add(fragment);
        }
    }

    private static void ParseProducts(Dictionary<string, string> files, ApimProject project)
    {
        foreach (var (folderName, productFiles) in GroupSection(files, "products"))
        {
            var product = new Product { Name = folderName, DisplayName = folderName };

            if (productFiles.TryGetValue("productInformation.json", out var json) && TryParseJson(json, out var info))
            {
                product.Name = GetString(info, "name") ?? folderName;
                if (TryGetProperty(info, "properties", out var props))
                {
                    product.DisplayName = GetString(props, "displayName") ?? product.Name;
                    product.Description = GetString(props, "description") ?? "";
                    product.SubscriptionRequired = GetBool(props, "subscriptionRequired", true);
                    product.ApprovalRequired = GetBool(props, "approvalRequired", false);
                    product.Published = string.Equals(GetString(props, "state"), "published", StringComparison.OrdinalIgnoreCase);
                }
            }

            if (productFiles.TryGetValue("apis.json", out var apisJson) && TryParseJson(apisJson, out var apisInfo))
            {
                product.Apis = GetStringArray(apisInfo, "apis") ?? new List<string>();
            }

            project.Products.Add(product);
        }
    }

    private static void ParseTags(Dictionary<string, string> files, ApimProject project)
    {
        var tagsEntry = files.FirstOrDefault(f =>
        {
            var segments = f.Key.Split('/');
            return segments.Length == 2 && NormalizeSegment(segments[0]) == "tags" &&
                   segments[1].Equals("tags.json", StringComparison.OrdinalIgnoreCase);
        });

        if (tagsEntry.Key == null || !TryParseJson(tagsEntry.Value, out var info) ||
            !TryGetProperty(info, "tags", out var tags) || tags.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var tag in tags.EnumerateArray())
        {
            if (tag.ValueKind == JsonValueKind.String)
            {
                project.Tags.Add(tag.GetString()!);
            }
            else if (tag.ValueKind == JsonValueKind.Object)
            {
                var name = GetString(tag, "displayName") ?? GetString(tag, "name");
                if (!string.IsNullOrEmpty(name))
                {
                    project.Tags.Add(name);
                }
            }
        }
    }

    private static void ParseSchemas(Dictionary<string, string> files, ApimProject project)
    {
        foreach (var (folderName, schemaFiles) in GroupSection(files, "schemas"))
        {
            var schema = new Schema { Name = folderName };

            if (schemaFiles.TryGetValue("schemaInformation.json", out var json) && TryParseJson(json, out var info))
            {
                schema.Name = GetString(info, "name") ?? folderName;
                if (TryGetProperty(info, "properties", out var props))
                {
                    schema.ContentType = GetString(props, "contentType") ?? schema.ContentType;
                }
            }

            var contentEntry = schemaFiles.FirstOrDefault(f =>
                f.Key.StartsWith("schema.", StringComparison.OrdinalIgnoreCase));
            if (contentEntry.Key != null)
            {
                schema.Content = contentEntry.Value;
            }

            project.Schemas.Add(schema);
        }
    }

    private static bool TryParseJson(string content, out JsonElement element)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            element = doc.RootElement.Clone();
            return true;
        }
        catch (JsonException)
        {
            element = default;
            return false;
        }
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    private static string? GetString(JsonElement element, string name)
    {
        return TryGetProperty(element, name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static bool GetBool(JsonElement element, string name, bool defaultValue)
    {
        if (TryGetProperty(element, name, out var value))
        {
            if (value.ValueKind == JsonValueKind.True) return true;
            if (value.ValueKind == JsonValueKind.False) return false;
        }
        return defaultValue;
    }

    private static List<string>? GetStringArray(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }
        return value.EnumerateArray()
            .Where(v => v.ValueKind == JsonValueKind.String)
            .Select(v => v.GetString()!)
            .ToList();
    }
}
