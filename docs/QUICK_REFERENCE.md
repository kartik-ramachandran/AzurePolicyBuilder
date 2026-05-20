# Quick Reference Guide

## Common APIM Policy Patterns

### Authentication & Authorization

#### JWT Validation (Azure AD)
```xml
<validate-jwt header-name="Authorization" failed-validation-httpcode="401">
    <openid-config url="https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration" />
    <audiences>
        <audience>api://{app-id}</audience>
    </audiences>
    <required-claims>
        <claim name="roles" match="any">
            <value>admin</value>
        </claim>
    </required-claims>
</validate-jwt>
```

#### API Key in Header
```xml
<check-header name="X-API-Key" failed-check-httpcode="401" failed-check-error-message="Missing or invalid API key" ignore-case="false">
    <value>valid-key-1</value>
    <value>valid-key-2</value>
</check-header>
```

#### Managed Identity to Backend
```xml
<authentication-managed-identity resource="https://management.azure.com/" />
```

### Rate Limiting & Quotas

#### Rate Limit by IP
```xml
<rate-limit-by-key calls="100" renewal-period="60" counter-key="@(context.Request.IpAddress)" />
```

#### Rate Limit by Subscription
```xml
<rate-limit-by-key calls="1000" renewal-period="60" counter-key="@(context.Subscription.Id)" />
```

#### Quota by Key
```xml
<quota-by-key calls="10000" bandwidth="40000" renewal-period="86400" counter-key="@(context.Subscription.Id)" />
```

### Caching

#### Cache Lookup (Inbound)
```xml
<cache-lookup vary-by-developer="false" vary-by-developer-groups="false">
    <vary-by-header>Accept</vary-by-header>
    <vary-by-query-parameter>version</vary-by-query-parameter>
</cache-lookup>
```

#### Cache Store (Outbound)
```xml
<cache-store duration="3600" />
```

### Transformation

#### Set Header
```xml
<set-header name="X-Correlation-Id" exists-action="override">
    <value>@(Guid.NewGuid().ToString())</value>
</set-header>
```

#### Remove Header
```xml
<set-header name="X-Powered-By" exists-action="delete" />
```

#### Transform JSON Response
```xml
<set-body>@{
    var response = context.Response.Body.As<JObject>();
    response["timestamp"] = DateTime.UtcNow;
    response["apiVersion"] = "v1";
    return response.ToString();
}</set-body>
```

#### XML to JSON
```xml
<xml-to-json kind="direct" apply="always" consider-accept-header="false" />
```

### Routing & Backend

#### Dynamic Backend URL
```xml
<set-backend-service base-url="@{
    var env = context.Request.Headers.GetValueOrDefault("X-Environment", "prod");
    return env == "dev" 
        ? "https://dev-api.backend.com" 
        : "https://api.backend.com";
}" />
```

#### Retry Policy
```xml
<retry condition="@(context.Response.StatusCode >= 500)" count="3" interval="1" delta="1" max-interval="10">
    <forward-request timeout="10" />
</retry>
```

#### Load Balancing
```xml
<choose>
    <when condition="@(new Random().Next(1, 100) &lt; 30)">
        <set-backend-service base-url="https://backend1.com" />
    </when>
    <otherwise>
        <set-backend-service base-url="https://backend2.com" />
    </otherwise>
</choose>
```

### Error Handling

#### Custom Error Response
```xml
<on-error>
    <return-response>
        <set-status code="500" />
        <set-header name="Content-Type" exists-action="override">
            <value>application/json</value>
        </set-header>
        <set-body>@{
            return new JObject(
                new JProperty("error", new JObject(
                    new JProperty("code", "InternalError"),
                    new JProperty("message", context.LastError.Message),
                    new JProperty("source", context.LastError.Source),
                    new JProperty("timestamp", DateTime.UtcNow)
                ))
            ).ToString();
        }</set-body>
    </return-response>
</on-error>
```

#### Conditional Error Handling
```xml
<on-error>
    <choose>
        <when condition="@(context.LastError.Source == "authentication-managed-identity")">
            <return-response>
                <set-status code="401" />
                <set-body>Authentication failed</set-body>
            </return-response>
        </when>
        <when condition="@(context.Response.StatusCode == 429)">
            <return-response>
                <set-status code="503" reason="Service Temporarily Unavailable" />
            </return-response>
        </when>
    </choose>
</on-error>
```

### Security

#### IP Filtering
```xml
<ip-filter action="allow">
    <address>13.66.201.169</address>
    <address-range from="13.66.140.128" to="13.66.140.143" />
</ip-filter>
```

#### CORS
```xml
<cors allow-credentials="true">
    <allowed-origins>
        <origin>https://yourdomain.com</origin>
    </allowed-origins>
    <allowed-methods preflight-result-max-age="300">
        <method>GET</method>
        <method>POST</method>
    </allowed-methods>
    <allowed-headers>
        <header>*</header>
    </allowed-headers>
</cors>
```

### Azure OpenAI

#### Token Limit
```xml
<azure-openai-token-limit 
    tokens-per-minute="100000" 
    estimate-prompt-tokens="true" 
    tokens-consumed-variable-name="tokensConsumed" 
/>
```

#### Emit Token Metrics
```xml
<azure-openai-emit-token-metric 
    namespace="apim-openai" 
    emit-token-usage="true" 
/>
```

## Common C# Expressions

### Context Variables

```csharp
// Request
context.Request.Method              // GET, POST, etc.
context.Request.Url                 // Full URL
context.Request.Body.As<string>()   // Request body as string
context.Request.Body.As<JObject>()  // Request body as JSON

// Response
context.Response.StatusCode         // HTTP status code
context.Response.Body.As<JObject>() // Response body as JSON

// Subscription
context.Subscription.Id             // Subscription ID
context.Subscription.Key            // Subscription key

// User
context.User.Email                  // User email
context.User.Id                     // User ID

// Other
context.RequestId                   // Unique request ID
context.Deployment.Region           // APIM region
```

### Useful Functions

```csharp
// GUID
Guid.NewGuid().ToString()

// DateTime
DateTime.UtcNow
DateTime.Now.ToString("yyyy-MM-dd")

// String operations
string.IsNullOrEmpty(value)
value.ToLower()
value.Contains("substring")

// Collections
context.Request.Headers.GetValueOrDefault("key", "default")
context.Variables.GetValueOrDefault<string>("key")

// JSON manipulation
var json = context.Request.Body.As<JObject>();
json["property"] = "value";
json.ToString()
```

## Named Values

### Set at Runtime
```xml
<set-variable name="backendUrl" value="https://api.backend.com" />
```

### Use Named Value
```xml
<set-backend-service base-url="{{backend-url}}" />
```

## Policy Scopes

### Global
Applies to all APIs:
```xml
<policies>
    <inbound>
        <rate-limit calls="1000" renewal-period="60" />
    </inbound>
</policies>
```

### Product
Applies to all APIs in a product:
```xml
<policies>
    <inbound>
        <quota calls="10000" renewal-period="86400" />
    </inbound>
</policies>
```

### API
Applies to all operations in an API:
```xml
<policies>
    <inbound>
        <validate-jwt header-name="Authorization" />
    </inbound>
</policies>
```

### Operation
Applies to specific operation:
```xml
<policies>
    <inbound>
        <cache-lookup />
    </inbound>
    <outbound>
        <cache-store duration="300" />
    </outbound>
</policies>
```

## Best Practices

### ✅ Do

- Always include `<base />` to inherit parent policies
- Set timeouts on `send-request` to avoid hanging
- Use caching for expensive operations
- Implement proper error handling
- Log important events with `trace`
- Use managed identities for backend auth
- Validate JWT tokens for secured APIs
- Set rate limits to protect backends
- Use named values for configuration

### ❌ Don't

- Hardcode secrets in policies (use Key Vault)
- Forget to include error handling
- Use synchronous blocking operations
- Make external calls without timeouts
- Ignore rate limiting needs
- Skip input validation
- Log sensitive data
- Use overly complex C# expressions

## Deployment

### ARM Template
```json
{
  "type": "Microsoft.ApiManagement/service/apis/policies",
  "apiVersion": "2023-05-01-preview",
  "name": "[concat(parameters('apimServiceName'), '/', parameters('apiName'), '/policy')]",
  "properties": {
    "format": "xml",
    "value": "<policies>...</policies>"
  }
}
```

### Bicep
```bicep
resource policy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  parent: api
  name: 'policy'
  properties: {
    format: 'xml'
    value: '''<policies>...</policies>'''
  }
}
```

### Azure CLI
```bash
az apim api policy create \
  --resource-group myResourceGroup \
  --service-name myApimService \
  --api-id myApi \
  --xml-policy '<policies>...</policies>'
```

## Troubleshooting

### Enable Tracing
```xml
<trace verbosity="verbose" />
```

### View in Logs
Check Application Insights or enable logging:
```xml
<log-to-eventhub logger-id="my-logger">
@{
    return new JObject(
        new JProperty("request", context.Request.Url.ToString()),
        new JProperty("statusCode", context.Response.StatusCode)
    ).ToString();
}
</log-to-eventhub>
```

### Common Errors

**401 Unauthorized**
- Check JWT validation configuration
- Verify token is valid and not expired
- Check required claims

**429 Too Many Requests**
- Increase rate limit quotas
- Implement backoff logic in client
- Check if rate limit is too restrictive

**500 Internal Server Error**
- Check backend is accessible
- Verify C# expressions are valid
- Check for null reference exceptions

**502 Bad Gateway**
- Backend timeout
- Backend unavailable
- Check backend URL is correct

---

For more examples, check the Template Library in the application!
