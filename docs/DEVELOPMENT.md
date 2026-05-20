# Development Guide

## Architecture Overview

### System Design

```
┌─────────────────┐         ┌─────────────────┐
│                 │         │                 │
│  Vue 3 Frontend │◄────────┤  .NET 8 Backend │
│   (Port 5173)   │  HTTP   │   (Port 5000)   │
│                 │         │                 │
└────────┬────────┘         └────────┬────────┘
         │                           │
         │                           │
    ┌────▼────┐                 ┌────▼────┐
    │ Monaco  │                 │ Roslyn  │
    │ Editor  │                 │ Compiler│
    └─────────┘                 └─────────┘
```

### Component Responsibilities

#### Frontend Components

**MonacoEditor.vue**
- Wraps Monaco Editor
- Provides XML syntax highlighting
- Emits change events
- Supports read-only mode

**ValidationPanel.vue**
- Displays validation results
- Shows errors and warnings
- Color-coded severity levels

**PolicyEditor.vue**
- Main editing interface
- Template selection
- Validation triggering
- Export functionality

**TemplateLibrary.vue**
- Template browsing
- Category filtering
- Template preview

**FragmentManager.vue**
- Fragment CRUD operations
- Usage tracking
- Version management

#### Backend Services

**PolicyTemplateService**
- Provides 16+ policy templates
- Category-based filtering
- Template metadata management

**PolicyValidationService**
- XML structure validation
- C# expression syntax checking
- Best practices enforcement
- Anti-pattern detection

**PolicyFragmentService**
- Fragment lifecycle management
- Version control
- Usage tracking

**ArmTemplateService**
- ARM template generation
- Bicep code generation
- Parameter management

## API Design

### RESTful Endpoints

All endpoints follow REST conventions:

```
Templates:
GET    /api/templates              # List all
GET    /api/templates/{id}         # Get one
GET    /api/templates/category/{c} # Filter by category

Validation:
POST   /api/validate               # Validate policy
POST   /api/validate/expression    # Validate C# expression

Fragments:
GET    /api/fragments              # List all
GET    /api/fragments/{id}         # Get one
POST   /api/fragments              # Create
PUT    /api/fragments/{id}         # Update
DELETE /api/fragments/{id}         # Delete

Export:
POST   /api/export/arm             # Export ARM template
POST   /api/export/bicep           # Export Bicep
```

### Request/Response Examples

**Validate Policy**
```json
POST /api/validate
{
  "xml": "<policies><inbound><base /></inbound></policies>"
}

Response:
{
  "isValid": true,
  "errors": [],
  "warnings": [
    {
      "line": 1,
      "column": 10,
      "message": "Missing backend section",
      "severity": "warning"
    }
  ]
}
```

**Create Fragment**
```json
POST /api/fragments
{
  "name": "JWT Validation",
  "description": "Standard JWT validation for APIs",
  "xml": "<validate-jwt>...</validate-jwt>",
  "version": 1
}

Response:
{
  "id": "1",
  "name": "JWT Validation",
  "description": "Standard JWT validation for APIs",
  "xml": "<validate-jwt>...</validate-jwt>",
  "version": 1,
  "createdAt": "2026-02-05T10:30:00Z",
  "updatedAt": "2026-02-05T10:30:00Z",
  "usageCount": 0
}
```

## State Management

### Pinia Store Structure

```typescript
// Policy Store
{
  templates: PolicyTemplate[]       // Cached templates
  fragments: PolicyFragment[]       // User fragments
  currentPolicy: string             // Active policy XML
  validationResult: ValidationResult | null
  isLoading: boolean
  error: string | null
}
```

### State Flow

```
User Action → Component → Store Action → API Call → Update State → Re-render
```

## Validation Rules

### XML Validation

1. **Well-formed XML**: Must parse without errors
2. **Root element**: Must be `<policies>`
3. **Valid sections**: inbound, backend, outbound, on-error
4. **Known elements**: Only APIM policy elements allowed
5. **Required attributes**: Element-specific requirements

### C# Expression Validation

Using Roslyn compiler:

```csharp
var code = $@"
using System;
public class Validator {{
    public object Validate() {{ return {expression}; }}
}}";

var syntaxTree = CSharpSyntaxTree.ParseText(code);
var compilation = CSharpCompilation.Create("Validation")
    .AddReferences(...)
    .AddSyntaxTrees(syntaxTree);

var diagnostics = compilation.GetDiagnostics();
```

### Best Practice Checks

- ✅ Include `<base />` in each section
- ✅ Set timeouts on `send-request`
- ✅ Add rate limiting for public APIs
- ❌ Avoid multiple rate-limit policies
- ❌ Don't use hardcoded secrets

## Policy Template Structure

### Template Definition

```csharp
new PolicyTemplate
{
    Id = "unique-id",
    Name = "Display Name",
    Description = "What it does",
    Category = PolicyCategory.Inbound,
    Xml = @"<policy-element>...</policy-element>",
    Parameters = new List<PolicyParameter>
    {
        new PolicyParameter
        {
            Name = "paramName",
            Type = ParameterType.String,
            Description = "What this param does",
            DefaultValue = "default",
            Required = true
        }
    }
}
```

### Adding New Templates

1. Open `PolicyTemplateService.cs`
2. Add to `InitializeTemplates()` method
3. Follow existing template structure
4. Include parameters if configurable
5. Test validation

## ARM Template Generation

### Process Flow

```
Policy XML → Parse → Extract Resources → Generate ARM JSON → Return
```

### ARM Template Structure

```json
{
  "$schema": "...",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "apimServiceName": { "type": "string" }
  },
  "resources": [
    {
      "type": "Microsoft.ApiManagement/service/policies",
      "apiVersion": "2023-05-01-preview",
      "properties": {
        "format": "xml",
        "value": "<policies>...</policies>"
      }
    }
  ]
}
```

### Bicep Generation

```bicep
param apimServiceName string

resource apimService 'Microsoft.ApiManagement/service@2023-05-01-preview' existing = {
  name: apimServiceName
}

resource policy 'Microsoft.ApiManagement/service/policies@2023-05-01-preview' = {
  parent: apimService
  name: 'policy'
  properties: {
    format: 'xml'
    value: '''<policies>...</policies>'''
  }
}
```

## Testing Strategy

### Unit Tests (TODO)

```csharp
[Fact]
public async Task ValidatePolicy_ValidXml_ReturnsNoErrors()
{
    var service = new PolicyValidationService();
    var xml = "<policies><inbound><base /></inbound></policies>";
    
    var result = await service.ValidatePolicyAsync(xml);
    
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}
```

### Integration Tests (TODO)

```csharp
[Fact]
public async Task CreateFragment_ValidData_ReturnsCreatedFragment()
{
    var client = _factory.CreateClient();
    var request = new CreateFragmentRequest { ... };
    
    var response = await client.PostAsJsonAsync("/api/fragments", request);
    
    response.EnsureSuccessStatusCode();
    var fragment = await response.Content.ReadFromJsonAsync<PolicyFragment>();
    Assert.NotNull(fragment.Id);
}
```

### Frontend Tests (TODO)

```typescript
describe('PolicyEditor', () => {
  it('validates policy on button click', async () => {
    const wrapper = mount(PolicyEditor)
    await wrapper.find('button.validate').trigger('click')
    
    expect(wrapper.find('.validation-panel').exists()).toBe(true)
  })
})
```

## Performance Considerations

### Frontend

- **Debounce validation**: Don't validate on every keystroke
- **Virtual scrolling**: For large template/fragment lists
- **Code splitting**: Lazy load Monaco Editor
- **Memoization**: Cache computed values in store

### Backend

- **In-memory caching**: Templates don't change often
- **Async operations**: All I/O operations are async
- **Connection pooling**: Reuse HTTP connections
- **Response compression**: Enable for large payloads

## Security Considerations

### Frontend

- **Input sanitization**: Sanitize XML before display
- **CSP headers**: Content Security Policy
- **XSS prevention**: Escape user input
- **HTTPS only**: In production

### Backend

- **CORS**: Restrict origins in production
- **Rate limiting**: Prevent API abuse
- **Input validation**: Validate all inputs
- **Error handling**: Don't expose stack traces

## Deployment

### Frontend (Static Site)

```bash
cd frontend
npm run build
# Deploy dist/ folder to:
# - Azure Static Web Apps
# - Azure Storage + CDN
# - Netlify, Vercel, etc.
```

### Backend (Web API)

```bash
cd backend
dotnet publish -c Release
# Deploy to:
# - Azure App Service
# - Azure Container Apps
# - Docker container
```

### Environment Variables

```bash
# Frontend (.env)
VITE_API_URL=https://api.apimpolicystudio.com

# Backend (appsettings.json)
{
  "AllowedOrigins": ["https://apimpolicystudio.com"],
  "Logging": { ... }
}
```

## Extending the Application

### Adding New Policy Elements

1. Update `AllowedPolicyElements` in `PolicyValidationService`
2. Add validation rules in `ValidateSpecificPolicy`
3. Create template in `PolicyTemplateService`

### Adding New Validation Rules

1. Create method in `PolicyValidationService`
2. Call from `ValidatePolicyStructure`
3. Add to `CheckAntiPatterns` if best practice

### Custom Export Formats

1. Create new service (e.g., `TerraformExportService`)
2. Add controller endpoint
3. Add frontend button and API call

### Integration with APIM

```csharp
// Future: Direct deployment
using Azure.ResourceManager.ApiManagement;

var client = new ApiManagementClient(...);
await client.Policies.CreateOrUpdateAsync(
    resourceGroup,
    serviceName,
    PolicyIdName.Policy,
    new PolicyContract { Value = policyXml }
);
```

## Troubleshooting

### Common Issues

**Monaco Editor not loading**
- Check internet connection (loaded from CDN)
- Verify Content-Security-Policy allows CDN

**Validation fails on valid XML**
- Check for unsupported policy elements
- Verify XML is well-formed
- Check for required attributes

**CORS errors**
- Verify backend CORS policy includes frontend URL
- Check preflight requests in Network tab

**Build errors**
- Clear node_modules: `rm -r node_modules && npm install`
- Clear .NET cache: `dotnet clean && dotnet restore`

## Contributing Guidelines

### Code Style

**TypeScript/Vue:**
- Use TypeScript strict mode
- Follow Vue 3 Composition API patterns
- Use Prettier for formatting
- ESLint for linting

**C#:**
- Follow Microsoft naming conventions
- Use async/await consistently
- Add XML documentation comments
- Enable nullable reference types

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/new-policy-template

# Make changes
# Commit with descriptive messages
git commit -m "Add Azure OpenAI rate limiting template"

# Push and create PR
git push origin feature/new-policy-template
```

### Pull Request Checklist

- [ ] Code follows style guidelines
- [ ] New features have tests
- [ ] Documentation updated
- [ ] No console errors
- [ ] Build succeeds
- [ ] Tested in browser

---

For questions, open an issue on GitHub!
