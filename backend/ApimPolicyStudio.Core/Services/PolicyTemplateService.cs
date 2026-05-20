using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class PolicyTemplateService
{
    private readonly List<PolicyTemplate> _templates;

    public PolicyTemplateService()
    {
        _templates = InitializeTemplates();
    }

    public Task<List<PolicyTemplate>> GetAllTemplatesAsync()
    {
        return Task.FromResult(_templates);
    }

    public Task<PolicyTemplate?> GetTemplateByIdAsync(string id)
    {
        return Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));
    }

    public Task<List<PolicyTemplate>> GetTemplatesByCategoryAsync(PolicyCategory category)
    {
        return Task.FromResult(_templates.Where(t => t.Category == category).ToList());
    }

    private List<PolicyTemplate> InitializeTemplates()
    {
        return new List<PolicyTemplate>
        {
            // Rate Limiting
            new PolicyTemplate
            {
                Id = "rate-limit-by-key",
                Name = "Rate Limit by Key",
                Description = "Prevents API usage spikes by limiting call rate per key",
                Category = PolicyCategory.Inbound,
                Xml = @"<rate-limit-by-key calls=""10"" renewal-period=""60"" counter-key=""@(context.Request.IpAddress)"" />",
                Parameters = new List<PolicyParameter>
                {
                    new PolicyParameter { Name = "calls", Type = ParameterType.Number, Description = "Maximum number of calls allowed", Required = true },
                    new PolicyParameter { Name = "renewal-period", Type = ParameterType.Number, Description = "Time period in seconds", Required = true },
                    new PolicyParameter { Name = "counter-key", Type = ParameterType.Expression, Description = "Key to track rate limit", Required = true }
                }
            },

            // JWT Validation
            new PolicyTemplate
            {
                Id = "validate-jwt",
                Name = "Validate JWT Token",
                Description = "Validates JWT tokens and enforces authorization",
                Category = PolicyCategory.Inbound,
                Xml = @"<validate-jwt header-name=""Authorization"" failed-validation-httpcode=""401"" failed-validation-error-message=""Unauthorized"">
    <openid-config url=""https://login.microsoftonline.com/{tenant-id}/v2.0/.well-known/openid-configuration"" />
    <audiences>
        <audience>api://your-app-id</audience>
    </audiences>
    <required-claims>
        <claim name=""roles"" match=""any"">
            <value>admin</value>
            <value>user</value>
        </claim>
    </required-claims>
</validate-jwt>",
                Parameters = new List<PolicyParameter>
                {
                    new PolicyParameter { Name = "header-name", Type = ParameterType.String, Description = "Header containing the JWT", DefaultValue = "Authorization", Required = true },
                    new PolicyParameter { Name = "tenant-id", Type = ParameterType.String, Description = "Azure AD tenant ID", Required = true }
                }
            },

            // CORS
            new PolicyTemplate
            {
                Id = "cors",
                Name = "CORS Policy",
                Description = "Enable Cross-Origin Resource Sharing",
                Category = PolicyCategory.Inbound,
                Xml = @"<cors allow-credentials=""true"">
    <allowed-origins>
        <origin>https://yourdomain.com</origin>
        <origin>https://localhost:3000</origin>
    </allowed-origins>
    <allowed-methods>
        <method>GET</method>
        <method>POST</method>
        <method>PUT</method>
        <method>DELETE</method>
    </allowed-methods>
    <allowed-headers>
        <header>*</header>
    </allowed-headers>
</cors>"
            },

            // Set Header
            new PolicyTemplate
            {
                Id = "set-header",
                Name = "Set Header",
                Description = "Add or modify HTTP headers",
                Category = PolicyCategory.Inbound,
                Xml = @"<set-header name=""X-Custom-Header"" exists-action=""override"">
    <value>@(context.Request.Headers.GetValueOrDefault(""User-Agent""))</value>
</set-header>",
                Parameters = new List<PolicyParameter>
                {
                    new PolicyParameter { Name = "name", Type = ParameterType.String, Description = "Header name", Required = true },
                    new PolicyParameter { Name = "value", Type = ParameterType.Expression, Description = "Header value", Required = true }
                }
            },

            // Cache Lookup
            new PolicyTemplate
            {
                Id = "cache-lookup",
                Name = "Cache Lookup",
                Description = "Check cache for response",
                Category = PolicyCategory.Inbound,
                Xml = @"<cache-lookup vary-by-developer=""false"" vary-by-developer-groups=""false"" downstream-caching-type=""none"" must-revalidate=""true"" cache-preference=""internal"">
    <vary-by-header>Accept</vary-by-header>
    <vary-by-header>Accept-Charset</vary-by-header>
</cache-lookup>"
            },

            // Cache Store
            new PolicyTemplate
            {
                Id = "cache-store",
                Name = "Cache Store",
                Description = "Store response in cache",
                Category = PolicyCategory.Outbound,
                Xml = @"<cache-store duration=""3600"" />"
            },

            // Set Backend Service
            new PolicyTemplate
            {
                Id = "set-backend-service",
                Name = "Set Backend Service",
                Description = "Override backend service URL",
                Category = PolicyCategory.Backend,
                Xml = @"<set-backend-service base-url=""https://api.backend.com"" />",
                Parameters = new List<PolicyParameter>
                {
                    new PolicyParameter { Name = "base-url", Type = ParameterType.String, Description = "Backend service URL", Required = true }
                }
            },

            // Retry
            new PolicyTemplate
            {
                Id = "retry",
                Name = "Retry Policy",
                Description = "Retry failed requests",
                Category = PolicyCategory.Backend,
                Xml = @"<retry condition=""@(context.Response.StatusCode >= 500)"" count=""3"" interval=""1"" first-fast-retry=""true"">
    <forward-request timeout=""10"" />
</retry>"
            },

            // Transform Response
            new PolicyTemplate
            {
                Id = "set-body",
                Name = "Transform Response Body",
                Description = "Modify response body content",
                Category = PolicyCategory.Outbound,
                Xml = @"<set-body>@{
    var response = context.Response.Body.As<JObject>();
    response[""timestamp""] = DateTime.UtcNow.ToString(""o"");
    response[""version""] = ""1.0"";
    return response.ToString();
}</set-body>"
            },

            // Set Status Code
            new PolicyTemplate
            {
                Id = "set-status",
                Name = "Set Status Code",
                Description = "Override response status code",
                Category = PolicyCategory.Outbound,
                Xml = @"<set-status code=""200"" reason=""OK"" />"
            },

            // Mock Response
            new PolicyTemplate
            {
                Id = "mock-response",
                Name = "Mock Response",
                Description = "Return mock response",
                Category = PolicyCategory.Inbound,
                Xml = @"<mock-response status-code=""200"" content-type=""application/json"" />",
                Parameters = new List<PolicyParameter>
                {
                    new PolicyParameter { Name = "status-code", Type = ParameterType.Number, Description = "HTTP status code", DefaultValue = "200", Required = true }
                }
            },

            // Error Handling
            new PolicyTemplate
            {
                Id = "on-error-return",
                Name = "Return Error Response",
                Description = "Return custom error response",
                Category = PolicyCategory.OnError,
                Xml = @"<return-response>
    <set-status code=""500"" reason=""Internal Server Error"" />
    <set-header name=""Content-Type"" exists-action=""override"">
        <value>application/json</value>
    </set-header>
    <set-body>@{
        return new JObject(
            new JProperty(""error"", new JObject(
                new JProperty(""code"", ""InternalError""),
                new JProperty(""message"", context.LastError.Message),
                new JProperty(""timestamp"", DateTime.UtcNow)
            ))
        ).ToString();
    }</set-body>
</return-response>"
            },

            // IP Filter
            new PolicyTemplate
            {
                Id = "ip-filter",
                Name = "IP Filter",
                Description = "Allow or deny requests based on IP address",
                Category = PolicyCategory.Inbound,
                Xml = @"<ip-filter action=""allow"">
    <address>13.66.201.169</address>
    <address-range from=""13.66.140.128"" to=""13.66.140.143"" />
</ip-filter>"
            },

            // Quota by Key
            new PolicyTemplate
            {
                Id = "quota-by-key",
                Name = "Quota by Key",
                Description = "Enforce usage quota per key",
                Category = PolicyCategory.Inbound,
                Xml = @"<quota-by-key calls=""10000"" bandwidth=""40000"" renewal-period=""3600"" counter-key=""@(context.Request.IpAddress)"" />"
            },

            // Azure OpenAI Integration
            new PolicyTemplate
            {
                Id = "azure-openai-token-limit",
                Name = "Azure OpenAI Token Limit",
                Description = "Enforce token limits for Azure OpenAI requests",
                Category = PolicyCategory.Inbound,
                Xml = @"<azure-openai-token-limit tokens-per-minute=""100000"" estimate-prompt-tokens=""true"" tokens-consumed-variable-name=""tokensConsumed"" remaining-tokens-variable-name=""tokensRemaining"" />"
            },

            // Request Tracing
            new PolicyTemplate
            {
                Id = "trace",
                Name = "Enable Request Tracing",
                Description = "Enable detailed request/response tracing",
                Category = PolicyCategory.Inbound,
                Xml = @"<trace verbosity=""verbose"" />"
            }
        };
    }
}
