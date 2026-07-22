using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class PolicyValidationService
{
    private static readonly string[] AllowedPolicyElements = new[]
    {
        "policies", "inbound", "backend", "outbound", "on-error", "base",
        "rate-limit", "rate-limit-by-key", "quota", "quota-by-key",
        "validate-jwt", "openid-config", "audiences", "audience", "required-claims", "claim",
        "cors", "allowed-origins", "origin", "allowed-methods", "method", "allowed-headers", "header",
        "set-header", "remove-header", "set-query-parameter", "set-body",
        "rewrite-uri", "set-backend-service", "forward-request",
        "cache-lookup", "cache-store", "vary-by-header", "vary-by-query-parameter",
        "return-response", "set-status", "mock-response",
        "retry", "choose", "when", "otherwise",
        "send-request", "send-one-way-request",
        "authentication-certificate", "authentication-basic", "authentication-managed-identity",
        "ip-filter", "address", "address-range",
        "check-header", "validate-content", "validate-parameters", "validate-status-code",
        "jsonp", "json-to-xml", "xml-to-json",
        "find-and-replace",
        "log-to-eventhub", "trace",
        "azure-openai-token-limit", "azure-openai-emit-token-metric",
        "value", "set-variable", "include-fragment",
        "emit-metric", "dimension",
        "issuer-signing-keys", "key", "issuers", "issuer",
        "required-scopes", "scope", "message", "metadata"
    };

    public async Task<ValidationResult> ValidatePolicyAsync(string xml)
    {
        var result = new ValidationResult();

        // 1. XML Validation
        try
        {
            var doc = XDocument.Parse(xml, LoadOptions.SetLineInfo);
            
            // Check root element
            if (doc.Root?.Name.LocalName != "policies")
            {
                result.Errors.Add(new ValidationError
                {
                    Line = 1,
                    Column = 1,
                    Message = "Root element must be 'policies'"
                });
                result.IsValid = false;
                return result;
            }

            // Validate policy structure
            ValidatePolicyStructure(doc, result);

            // Validate C# expressions
            await ValidateCSharpExpressionsAsync(doc, result);

        }
        catch (XmlException ex)
        {
            result.Errors.Add(new ValidationError
            {
                Line = ex.LineNumber,
                Column = ex.LinePosition,
                Message = $"XML parsing error: {ex.Message}"
            });
            result.IsValid = false;
            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ValidationError
            {
                Line = 0,
                Column = 0,
                Message = $"Validation error: {ex.Message}"
            });
            result.IsValid = false;
            return result;
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public Task<ValidationResult> ValidateExpressionAsync(string expression)
    {
        var result = new ValidationResult();

        try
        {
            var body = expression.Trim();
            var isStatementBlock = false;
            if (body.StartsWith("@{") && body.EndsWith("}"))
            {
                body = body.Substring(2, body.Length - 3);
                isStatementBlock = true;
            }
            else if (body.StartsWith("@(") && body.EndsWith(")"))
            {
                body = body.Substring(2, body.Length - 3);
            }
            else if (body.StartsWith("@"))
            {
                body = body.Substring(1);
            }

            var (code, expressionStartLine) = BuildExpressionWrapper(body, isStatementBlock);

            foreach (var diagnostic in CompileExpression(code))
            {
                var position = diagnostic.Location.GetLineSpan().StartLinePosition;
                var line = Math.Max(1, position.Line - expressionStartLine + 1);

                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Line = line,
                        Column = position.Character + 1,
                        Message = diagnostic.GetMessage()
                    });
                }
                else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Line = line,
                        Column = position.Character + 1,
                        Message = diagnostic.GetMessage()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ValidationError
            {
                Line = 0,
                Column = 0,
                Message = $"Expression validation error: {ex.Message}"
            });
        }

        result.IsValid = result.Errors.Count == 0;
        return Task.FromResult(result);
    }

    private void ValidatePolicyStructure(XDocument doc, ValidationResult result)
    {
        var sections = new[] { "inbound", "backend", "outbound", "on-error" };
        var root = doc.Root!;

        // Check for valid sections
        foreach (var element in root.Elements())
        {
            if (!sections.Contains(element.Name.LocalName))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Line = ((IXmlLineInfo)element).LineNumber,
                    Column = ((IXmlLineInfo)element).LinePosition,
                    Message = $"Unexpected section: {element.Name.LocalName}. Expected: {string.Join(", ", sections)}"
                });
            }
        }

        // Validate policy elements
        ValidatePolicyElements(root, result);

        // Check for common anti-patterns
        CheckAntiPatterns(root, result);
    }

    private void ValidatePolicyElements(XElement root, ValidationResult result)
    {
        foreach (var element in root.Descendants())
        {
            var elementName = element.Name.LocalName;
            
            if (!AllowedPolicyElements.Contains(elementName) && !elementName.StartsWith("azure-"))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Line = ((IXmlLineInfo)element).HasLineInfo() ? ((IXmlLineInfo)element).LineNumber : 0,
                    Column = ((IXmlLineInfo)element).HasLineInfo() ? ((IXmlLineInfo)element).LinePosition : 0,
                    Message = $"Unknown policy element: {elementName}"
                });
            }

            // Validate specific policy requirements
            ValidateSpecificPolicy(element, result);
        }
    }

    private void ValidateSpecificPolicy(XElement element, ValidationResult result)
    {
        var lineInfo = (IXmlLineInfo)element;
        var line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0;
        var column = lineInfo.HasLineInfo() ? lineInfo.LinePosition : 0;

        switch (element.Name.LocalName)
        {
            case "rate-limit-by-key":
                ValidateRequiredAttribute(element, "calls", line, column, result);
                ValidateRequiredAttribute(element, "renewal-period", line, column, result);
                ValidateRequiredAttribute(element, "counter-key", line, column, result);
                break;

            case "validate-jwt":
                ValidateRequiredAttribute(element, "header-name", line, column, result);
                if (!element.Elements("openid-config").Any())
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Line = line,
                        Column = column,
                        Message = "validate-jwt should include openid-config element"
                    });
                }
                break;

            case "set-backend-service":
                if (!element.Attribute("base-url")?.Value.StartsWith("http") ?? true)
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Line = line,
                        Column = column,
                        Message = "base-url should start with http:// or https://"
                    });
                }
                break;

            case "cache-store":
                ValidateRequiredAttribute(element, "duration", line, column, result);
                break;
        }
    }

    private void ValidateRequiredAttribute(XElement element, string attributeName, int line, int column, ValidationResult result)
    {
        if (element.Attribute(attributeName) == null)
        {
            result.Errors.Add(new ValidationError
            {
                Line = line,
                Column = column,
                Message = $"Required attribute '{attributeName}' is missing from {element.Name.LocalName} element"
            });
        }
    }

    private void CheckAntiPatterns(XElement root, ValidationResult result)
    {
        // Check for multiple rate-limit policies
        var rateLimitCount = root.Descendants().Count(e => 
            e.Name.LocalName == "rate-limit" || e.Name.LocalName == "rate-limit-by-key");
        if (rateLimitCount > 3)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Line = 0,
                Column = 0,
                Message = "Multiple rate-limit policies detected. Consider consolidating."
            });
        }

        // Check for send-request without timeout
        foreach (var sendRequest in root.Descendants("send-request"))
        {
            if (sendRequest.Attribute("timeout") == null)
            {
                var lineInfo = (IXmlLineInfo)sendRequest;
                result.Warnings.Add(new ValidationWarning
                {
                    Line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                    Column = lineInfo.HasLineInfo() ? lineInfo.LinePosition : 0,
                    Message = "send-request should include timeout attribute to prevent hanging"
                });
            }
        }

        // Check for missing base policy
        var sections = new[] { "inbound", "backend", "outbound", "on-error" };
        foreach (var section in sections)
        {
            var sectionElement = root.Element(section);
            if (sectionElement != null && !sectionElement.Elements("base").Any())
            {
                var lineInfo = (IXmlLineInfo)sectionElement;
                result.Warnings.Add(new ValidationWarning
                {
                    Line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                    Column = lineInfo.HasLineInfo() ? lineInfo.LinePosition : 0,
                    Message = $"{section} section is missing <base /> element. This may override parent policies."
                });
            }
        }
    }

    private Task ValidateCSharpExpressionsAsync(XDocument doc, ValidationResult result)
    {
        foreach (var element in doc.Descendants())
        {
            foreach (var attribute in element.Attributes())
            {
                if (TryExtractExpression(attribute.Value, out var expression, out var isStatementBlock))
                {
                    ValidateEmbeddedExpression(
                        expression, isStatementBlock,
                        $"attribute '{attribute.Name.LocalName}' of <{element.Name.LocalName}>",
                        element, result);
                }
            }

            if (!element.HasElements && TryExtractExpression(element.Value, out var bodyExpression, out var bodyIsBlock))
            {
                ValidateEmbeddedExpression(
                    bodyExpression, bodyIsBlock,
                    $"<{element.Name.LocalName}>",
                    element, result);
            }
        }

        return Task.CompletedTask;
    }

    private static bool TryExtractExpression(string value, out string expression, out bool isStatementBlock)
    {
        expression = string.Empty;
        isStatementBlock = false;

        var trimmed = value.Trim();
        if (trimmed.StartsWith("@{") && trimmed.EndsWith("}"))
        {
            expression = trimmed.Substring(2, trimmed.Length - 3);
            isStatementBlock = true;
            return true;
        }
        if (trimmed.StartsWith("@(") && trimmed.EndsWith(")"))
        {
            expression = trimmed.Substring(2, trimmed.Length - 3);
            return true;
        }
        return false;
    }

    private void ValidateEmbeddedExpression(string expression, bool isStatementBlock, string location, XElement element, ValidationResult result)
    {
        var lineInfo = (IXmlLineInfo)element;
        var line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0;
        var column = lineInfo.HasLineInfo() ? lineInfo.LinePosition : 0;

        var (code, _) = BuildExpressionWrapper(expression, isStatementBlock);
        foreach (var diagnostic in CompileExpression(code).Where(d => d.Severity == DiagnosticSeverity.Error))
        {
            result.Errors.Add(new ValidationError
            {
                Line = line,
                Column = column,
                Message = $"C# expression error in {location}: {diagnostic.GetMessage()}"
            });
        }
    }

    private static (string Code, int ExpressionStartLine) BuildExpressionWrapper(string expression, bool isStatementBlock)
    {
        var prefix =
            "using System;\n" +
            "using System.Collections.Generic;\n" +
            "using System.Linq;\n" +
            "using System.Text;\n" +
            "using System.Text.RegularExpressions;\n" +
            "using Newtonsoft.Json;\n" +
            "using Newtonsoft.Json.Linq;\n" +
            "public class ExpressionHost\n" +
            "{\n" +
            "    private static readonly ProxyRequestContext context = new ProxyRequestContext();\n" +
            "    public object Evaluate()\n" +
            "    {\n" +
            (isStatementBlock ? string.Empty : "        return\n");

        var suffix = isStatementBlock ? "\n    }\n}\n" : "\n        ;\n    }\n}\n";
        var expressionStartLine = prefix.Count(c => c == '\n');

        return (prefix + expression + suffix, expressionStartLine);
    }

    private static IEnumerable<Diagnostic> CompileExpression(string wrapperCode)
    {
        var wrapperTree = CSharpSyntaxTree.ParseText(wrapperCode);
        var compilation = CSharpCompilation.Create(
            "ExpressionValidation",
            new[] { wrapperTree, ContextStubTree.Value },
            ExpressionReferences.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Only surface diagnostics from the user's expression, not the stub declarations
        return compilation.GetDiagnostics().Where(d => d.Location.SourceTree == wrapperTree).ToList();
    }

    private static readonly Lazy<SyntaxTree> ContextStubTree = new(() => CSharpSyntaxTree.ParseText(ApimContextStubSource));

    private static readonly Lazy<List<MetadataReference>> ExpressionReferences = new(() =>
    {
        var references = new List<MetadataReference>();

        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrEmpty(trustedAssemblies))
        {
            var wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "System.Private.CoreLib.dll",
                "System.Runtime.dll",
                "netstandard.dll",
                "System.Linq.dll",
                "System.Collections.dll",
                "System.Text.RegularExpressions.dll",
                "System.Text.Encoding.Extensions.dll",
                "System.Private.Uri.dll",
                "System.Runtime.Extensions.dll",
                "System.Runtime.Numerics.dll",
                "System.Net.Primitives.dll",
                "System.Security.Cryptography.X509Certificates.dll"
            };

            references.AddRange(trustedAssemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Where(path => wanted.Contains(Path.GetFileName(path)))
                .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)));
        }
        else
        {
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        }

        references.Add(MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location));
        return references;
    });

    // Minimal stand-in for the APIM policy expression context so real-world
    // expressions (context.Request.Headers..., context.Variables..., etc.) compile.
    private const string ApimContextStubSource = @"
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class ProxyRequestContext
{
    public ProxyApi Api { get; set; }
    public ProxyDeployment Deployment { get; set; }
    public TimeSpan Elapsed { get; set; }
    public ProxyError LastError { get; set; }
    public ProxyOperation Operation { get; set; }
    public ProxyProduct Product { get; set; }
    public ProxyRequest Request { get; set; }
    public Guid RequestId { get; set; }
    public ProxyResponse Response { get; set; }
    public ProxySubscription Subscription { get; set; }
    public DateTime Timestamp { get; set; }
    public ProxyUser User { get; set; }
    public IReadOnlyDictionary<string, object> Variables { get; set; }
    public void Trace(string message) { }
}

public class ProxyApi
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public IEnumerable<string> Protocols { get; set; }
    public Uri ServiceUrl { get; set; }
    public ProxyApiVersion Version { get; set; }
    public bool IsCurrentRevision { get; set; }
    public string Revision { get; set; }
}

public class ProxyApiVersion
{
    public string VersionHeaderName { get; set; }
    public string VersionQueryName { get; set; }
}

public class ProxyDeployment
{
    public string GatewayId { get; set; }
    public string Region { get; set; }
    public string ServiceId { get; set; }
    public string ServiceName { get; set; }
    public IReadOnlyDictionary<string, X509Certificate2> Certificates { get; set; }
}

public class ProxyError
{
    public string Source { get; set; }
    public string Reason { get; set; }
    public string Message { get; set; }
    public string Scope { get; set; }
    public string Section { get; set; }
    public string Path { get; set; }
    public string PolicyId { get; set; }
}

public class ProxyOperation
{
    public string Id { get; set; }
    public string Method { get; set; }
    public string Name { get; set; }
    public string UrlTemplate { get; set; }
}

public class ProxyProduct
{
    public IEnumerable<ProxyApi> Apis { get; set; }
    public bool ApprovalRequired { get; set; }
    public IEnumerable<ProxyGroup> Groups { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
    public int? SubscriptionLimit { get; set; }
    public bool SubscriptionRequired { get; set; }
}

public class ProxyGroup
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class ProxyMessageBody
{
    public T As<T>(bool preserveContent = false) { return default(T); }
}

public class ProxyUrl
{
    public string Host { get; set; }
    public string Path { get; set; }
    public int Port { get; set; }
    public IReadOnlyDictionary<string, string[]> Query { get; set; }
    public string QueryString { get; set; }
    public string Scheme { get; set; }
}

public class ProxyRequest
{
    public ProxyMessageBody Body { get; set; }
    public X509Certificate2 Certificate { get; set; }
    public IReadOnlyDictionary<string, string[]> Headers { get; set; }
    public string IpAddress { get; set; }
    public IReadOnlyDictionary<string, string> MatchedParameters { get; set; }
    public string Method { get; set; }
    public ProxyUrl OriginalUrl { get; set; }
    public ProxyUrl Url { get; set; }
}

public class ProxyResponse
{
    public ProxyMessageBody Body { get; set; }
    public IReadOnlyDictionary<string, string[]> Headers { get; set; }
    public int StatusCode { get; set; }
    public string StatusReason { get; set; }
}

public class ProxySubscription
{
    public DateTime CreatedDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Id { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string PrimaryKey { get; set; }
    public string SecondaryKey { get; set; }
    public DateTime? StartDate { get; set; }
}

public class ProxyUserIdentity
{
    public string Id { get; set; }
    public string Provider { get; set; }
}

public class ProxyUser
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public IEnumerable<ProxyGroup> Groups { get; set; }
    public string Id { get; set; }
    public IEnumerable<ProxyUserIdentity> Identities { get; set; }
    public string LastName { get; set; }
    public string Note { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class ProxyJwt
{
    public string Algorithm { get; set; }
    public IEnumerable<string> Audiences { get; set; }
    public IReadOnlyDictionary<string, string[]> Claims { get; set; }
    public DateTime? ExpirationTime { get; set; }
    public string Id { get; set; }
    public string Issuer { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? NotBefore { get; set; }
    public string Subject { get; set; }
    public string Type { get; set; }
}

public class ProxyBasicAuthCredentials
{
    public string Password { get; set; }
    public string Username { get; set; }
}

public static class ApimDictionaryExtensions
{
    public static string GetValueOrDefault(this IReadOnlyDictionary<string, string[]> dictionary, string key)
    {
        return string.Empty;
    }

    public static string GetValueOrDefault(this IReadOnlyDictionary<string, string[]> dictionary, string key, string defaultValue)
    {
        return defaultValue;
    }
}

public static class ApimStringExtensions
{
    public static ProxyBasicAuthCredentials AsBasic(this string value) { return null; }
    public static bool TryParseBasic(this string value, out ProxyBasicAuthCredentials credentials) { credentials = null; return false; }
    public static ProxyJwt AsJwt(this string value) { return null; }
    public static bool TryParseJwt(this string value, out ProxyJwt token) { token = null; return false; }
}
";
}
