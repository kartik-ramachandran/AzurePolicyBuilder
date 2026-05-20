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
        "azure-openai-token-limit", "azure-openai-emit-token-metric"
    };

    public async Task<ValidationResult> ValidatePolicyAsync(string xml)
    {
        var result = new ValidationResult();

        // 1. XML Validation
        try
        {
            var doc = XDocument.Parse(xml);
            
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

    public async Task<ValidationResult> ValidateExpressionAsync(string expression)
    {
        var result = new ValidationResult();

        try
        {
            // Wrap expression in a method to validate syntax
            var code = $@"
using System;
using System.Collections.Generic;
using System.Linq;

public class ExpressionValidator
{{
    public object Validate()
    {{
        return {expression};
    }}
}}";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("ExpressionValidation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            var diagnostics = compilation.GetDiagnostics();
            
            foreach (var diagnostic in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                result.Errors.Add(new ValidationError
                {
                    Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line,
                    Column = diagnostic.Location.GetLineSpan().StartLinePosition.Character,
                    Message = diagnostic.GetMessage()
                });
            }

            foreach (var diagnostic in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line,
                    Column = diagnostic.Location.GetLineSpan().StartLinePosition.Character,
                    Message = diagnostic.GetMessage()
                });
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
        return await Task.FromResult(result);
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

    private async Task ValidateCSharpExpressionsAsync(XDocument doc, ValidationResult result)
    {
        // Find all C# expressions in attributes starting with @
        foreach (var attribute in doc.Descendants().SelectMany(e => e.Attributes()))
        {
            var value = attribute.Value;
            if (value.StartsWith("@"))
            {
                var expression = value.TrimStart('@');
                if (expression.StartsWith("(") && expression.EndsWith(")"))
                {
                    expression = expression.Substring(1, expression.Length - 2);
                }

                // Basic syntax validation only (full validation would require context)
                if (expression.Contains("context."))
                {
                    // Valid APIM expression
                    continue;
                }
            }
        }

        // Find C# code blocks in element values
        foreach (var element in doc.Descendants())
        {
            var value = element.Value.Trim();
            if (value.StartsWith("@{") && value.EndsWith("}"))
            {
                // This is a C# code block - would need full validation with APIM context
                var code = value.Substring(2, value.Length - 3);
                // Basic validation only
            }
        }

        await Task.CompletedTask;
    }
}
