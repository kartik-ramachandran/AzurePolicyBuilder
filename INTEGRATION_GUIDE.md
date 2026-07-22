# APIM Policy Studio - Complete Integration Guide

## Overview

APIM Policy Studio is now a comprehensive platform combining:
- **Azure API Management Policy Management** - Visual editor, templates, validation, ARM/Bicep export
- **OpenAPI Tools** - API Builder, interactive documentation, API testing
- **AI-Powered Assistance** - GPT-powered policy generation and recommendations

## New Features Integrated from Webapp

### 1. API Builder (`/api-builder`)
Convert your webapp's `apibuilder.js` to a Vue component that provides:
- **Multi-Spec Loading**: Upload or fetch multiple OpenAPI specs
- **Endpoint Selection**: Choose specific endpoints from each spec
- **Schema Selection**: Pick which models/schemas to include
- **Compilation**: Merge selected items into a new OpenAPI spec
- **Export**: Download the compiled specification

**Use Case**: Build focused API specs from larger OpenAPI documents for specific integrations or client SDKs.

### 2. API Documentation Viewer (`/api-docs`)
Converted from `valdoc.js` to provide:
- **Interactive Rendering**: Redoc-style API documentation
- **Search**: Find endpoints and schemas quickly
- **Sidebar Navigation**: Jump to any endpoint or schema
- **Request/Response Examples**: See sample data for all operations
- **Schema Explorer**: Browse data models with examples

**Use Case**: Generate beautiful, searchable API documentation from OpenAPI specs created in API Builder.

### 3. API Tester (`/api-tester`)
Postman-like interface converted from `script.js`:
- **HTTP Methods**: Support for GET, POST, PUT, PATCH, DELETE, OPTIONS
- **Request Builder**: Headers, body (JSON/XML/Form), authentication
- **Authentication**: Bearer tokens, API keys, Basic auth
- **Collections**: Organize requests into collections
- **History**: Recent requests auto-saved
- **Response Viewer**: Formatted body, headers, status, timing

**Use Case**: Test APIs during development or validate APIM policies against live endpoints.

### 4. AI Assistant (Global Component)
Integrated from `ai-service.js` throughout the app:
- **Policy Generation**: Describe requirements, get valid APIM policy XML
- **Policy Explanation**: Paste policy XML, get plain English explanation
- **Policy Improvement**: Suggest optimizations and best practices
- **Configurable**: Use OpenAI or Azure OpenAI endpoints

**Use Case**: Accelerate policy development with AI-powered code generation and recommendations.

## Complete Workflow Examples

### Workflow 1: API-First Development
1. **API Builder** → Upload swagger.json containing 50 endpoints
2. **API Builder** → Select only the 5 endpoints needed for mobile app
3. **API Builder** → Download focused OpenAPI spec named `mobile-api.json`
4. **API Documentation** → Upload `mobile-api.json` to generate docs
5. **Share** → Send documentation link to mobile developers

### Workflow 2: APIM Policy Development
1. **Policy Editor** → Start with blank policy or template
2. **AI Assistant** → "Generate rate limiting policy for 1000 calls per hour per subscription"
3. **Policy Editor** → Review generated XML, make adjustments
4. **Validate** → Run validation to check syntax and best practices
5. **Export** → Download as ARM template for deployment

### Workflow 3: API Testing & Policy Validation
1. **API Tester** → POST to `https://my-api.azure-api.net/users` with bearer token
2. **Test** → Send request, observe APIM policy behavior
3. **Analyze** → Check response headers for rate limit counters
4. **Iterate** → Adjust policy in Policy Editor
5. **Re-test** → Validate changes in API Tester

### Workflow 4: Complete API Project
1. **Template Library** → Browse 16+ APIM policy templates
2. **Copy Template** → Select "JWT Validation" template
3. **AI Assistant** → "Improve this policy for Azure AD B2C tokens"
4. **Policy Editor** → Implement AI suggestions
5. **Fragment Manager** → Save as reusable fragment
6. **Export** → Generate Bicep for IaC deployment
7. **API Tester** → Test with real tokens
8. **Documentation** → Upload final OpenAPI spec with policies documented

## Architecture Overview

```
frontend/
├── src/
│   ├── views/
│   │   ├── PolicyEditor.vue         # Main APIM policy editor
│   │   ├── TemplateLibrary.vue      # 16+ policy templates
│   │   ├── FragmentManager.vue      # Reusable policy fragments
│   │   ├── ApiBuilder.vue           # OpenAPI endpoint selector ✨ NEW
│   │   ├── ApiDocumentation.vue     # Interactive API docs ✨ NEW
│   │   └── ApiTester.vue            # Postman-like tester ✨ NEW
│   ├── components/
│   │   ├── MonacoEditor.vue         # Code editor component
│   │   ├── ValidationPanel.vue      # Show validation results
│   │   └── AIAssistant.vue          # AI chat interface ✨ NEW
│   ├── services/
│   │   ├── api.ts                   # Backend API client
│   │   └── ai-assistant.ts          # OpenAI integration ✨ NEW
│   └── router/
│       └── index.ts                 # All routes configured

backend/
├── ApimPolicyStudio.Api/            # Web API project
└── ApimPolicyStudio.Core/           # Business logic
    ├── Services/
    │   ├── PolicyTemplateService.cs    # 16 templates
    │   ├── PolicyValidationService.cs  # Roslyn validation
    │   ├── ArmTemplateService.cs       # ARM/Bicep export
    │   └── PolicyFragmentService.cs    # Fragment CRUD
    └── Models/                         # Data models
```

## Navigation

The app now has 7 main sections accessible from the navigation:

1. **Project Builder** (`/`) - Import OpenAPI specs or compose manually (APIs, backends, named values, products, fragments), then generate APIM projects
2. **Templates** (`/templates`) - Browse and use policy templates
3. **Policy Editor** (`/editor`) - Main policy development interface
4. **Fragments** (`/fragments`) - Manage reusable policy components
5. **API Builder** (`/api-builder`) - OpenAPI spec compiler
6. **API Docs** (`/api-docs`) - Documentation viewer
7. **API Tester** (`/api-tester`) - HTTP client

The **AI Assistant** floats in the bottom-right corner and is available on all pages.

## Getting Started

1. **Start Backend**:
   ```powershell
   cd backend/ApimPolicyStudio.Api
   dotnet run
   ```

2. **Start Frontend**:
   ```powershell
   cd frontend
   npm run dev
   ```

3. **Configure AI Assistant** (Optional):
   - Click AI Assistant icon (bottom-right)
   - Click settings gear
   - Add OpenAI API key
   - Select model (GPT-4 recommended)

4. **Explore**:
   - Visit http://localhost:5173
   - Try each section
   - Test complete workflows

## API Endpoints

**Backend runs on http://localhost:5000**

- `GET /api/templates` - List all policy templates
- `GET /api/templates/{id}` - Get a specific template
- `GET /api/templates/category/{category}` - Get templates by category
- `POST /api/validate` - Validate policy XML (including C# expressions)
- `POST /api/validate/expression` - Validate a C# expression
- `GET /api/fragments` - List all fragments
- `GET /api/fragments/{id}` - Get a specific fragment
- `POST /api/fragments` - Create new fragment
- `PUT /api/fragments/{id}` - Update a fragment
- `DELETE /api/fragments/{id}` - Delete a fragment
- `POST /api/fragments/{id}/increment-usage` - Track fragment usage
- `POST /api/export/arm` - Generate ARM template
- `POST /api/export/bicep` - Generate Bicep code
- `POST /api/project/generate` - Generate APIM project file structure
- `POST /api/project/download` - Download APIM project as ZIP

## Local Storage

The app uses browser local storage for:
- **AI Configuration**: API key, model, base URL
- **API Tester History**: Last 50 requests
- **API Tester Collections**: Saved request collections

Data never leaves your browser except for OpenAI API calls (if configured).

## Next Steps

1. **Deploy Backend** to Azure App Service
2. **Deploy Frontend** to Azure Static Web Apps
3. **Add Authentication** with Azure AD
4. **Connect to APIM** for live policy deployment
5. **Extend Templates** with organization-specific patterns

## Support

- **Frontend Issues**: Check browser console for errors
- **Backend Issues**: Check terminal output and logs
- **AI Assistant Issues**: Verify API key in settings
- **CORS Issues**: Ensure backend CORS is configured for your domain

## Summary

You now have a complete APIM Policy Studio with integrated OpenAPI tools and AI assistance. All webapp features have been successfully converted to Vue components and seamlessly integrated into the existing application.

**Total Components**: 7 views, 3 reusable components, 2 service layers  
**Total Backend Services**: 5 services, 5 controllers, 15 endpoints  
**Total Features**: Policy management + OpenAPI tools + AI assistance + HTTP testing
