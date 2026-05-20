# APIM Policy Studio - Getting Started Guide

## 🚀 Quick Start

### Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js** (v20 or higher) - [Download](https://nodejs.org/)
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio Code** (recommended) - [Download](https://code.visualstudio.com/)

### Installation

#### 1. Clone and Navigate to Project

```powershell
cd c:\Others\APIMPolicyBuilder
```

#### 2. Install Frontend Dependencies

```powershell
cd frontend
npm install
```

#### 3. Restore Backend Dependencies

```powershell
cd ..\backend
dotnet restore
```

### Running the Application

#### Option 1: Run Both (Recommended)

Open two terminal windows:

**Terminal 1 - Backend:**
```powershell
cd backend
dotnet run --project ApimPolicyStudio.Api
```

**Terminal 2 - Frontend:**
```powershell
cd frontend
npm run dev
```

#### Option 2: Use VS Code Tasks

1. Press `Ctrl+Shift+B` to build the backend
2. Press `Ctrl+Shift+P` and run "Tasks: Run Task" → "npm: dev"

#### Option 3: Use VS Code Debug

1. Press `F5` and select "Full Stack" to launch both frontend and backend

### Access the Application

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

## 📖 Features Overview

### 1. Policy Editor

The main editor interface for creating and editing APIM policies:

- **Monaco Editor**: Full VS Code editing experience with syntax highlighting
- **Real-time Validation**: Instant feedback on XML structure and C# expressions
- **XML Formatting**: Auto-format your policy XML
- **Template Integration**: Select from pre-built templates

### 2. Template Library

Browse 16+ pre-built APIM policy templates:

- **Rate Limiting**: Prevent API abuse
- **JWT Validation**: Secure APIs with token validation
- **CORS**: Enable cross-origin requests
- **Caching**: Improve performance
- **Error Handling**: Custom error responses
- **Azure OpenAI**: Token limit enforcement
- And more...

### 3. Fragment Manager

Create reusable policy fragments:

- **Version Control**: Track fragment versions
- **Usage Tracking**: See where fragments are used
- **Quick Copy**: Copy fragment XML to clipboard
- **Direct Integration**: Use fragments in the editor

### 4. Validation Engine

Comprehensive policy validation:

- **XML Schema Validation**: Ensure well-formed XML
- **C# Expression Validation**: Check syntax using Roslyn
- **Best Practices**: Warnings for anti-patterns
- **Required Attributes**: Validate policy element requirements

### 5. Export Capabilities

Export policies for deployment:

- **ARM Templates**: Generate Azure Resource Manager templates
- **Bicep**: Export to Bicep (preferred Azure IaC)
- **Copy/Paste**: Direct XML copying

## 🎯 Usage Examples

### Example 1: Create a Rate-Limited API

1. Open the Policy Editor
2. Select "Rate Limit by Key" from templates
3. Configure parameters:
   ```xml
   <rate-limit-by-key 
       calls="100" 
       renewal-period="60" 
       counter-key="@(context.Request.IpAddress)" />
   ```
4. Click "Validate Policy"
5. Click "Export ARM" to deploy

### Example 2: Add JWT Authentication

1. Select "Validate JWT Token" template
2. Update with your Azure AD details:
   ```xml
   <validate-jwt header-name="Authorization">
       <openid-config url="https://login.microsoftonline.com/{your-tenant-id}/v2.0/.well-known/openid-configuration" />
       <audiences>
           <audience>api://{your-app-id}</audience>
       </audiences>
   </validate-jwt>
   ```
3. Save as a fragment for reuse

### Example 3: Create Custom Error Handling

1. Switch to "On Error" section template
2. Customize error response:
   ```xml
   <return-response>
       <set-status code="500" />
       <set-body>@{
           return new JObject(
               new JProperty("error", context.LastError.Message),
               new JProperty("timestamp", DateTime.UtcNow)
           ).ToString();
       }</set-body>
   </return-response>
   ```

## 🔧 API Endpoints

### Templates

- `GET /api/templates` - Get all templates
- `GET /api/templates/{id}` - Get specific template
- `GET /api/templates/category/{category}` - Get templates by category

### Validation

- `POST /api/validate` - Validate policy XML
  ```json
  { "xml": "<policies>...</policies>" }
  ```
- `POST /api/validate/expression` - Validate C# expression
  ```json
  { "expression": "context.Request.Headers['Authorization']" }
  ```

### Fragments

- `GET /api/fragments` - List all fragments
- `GET /api/fragments/{id}` - Get specific fragment
- `POST /api/fragments` - Create new fragment
- `PUT /api/fragments/{id}` - Update fragment
- `DELETE /api/fragments/{id}` - Delete fragment

### Export

- `POST /api/export/arm` - Export to ARM template
- `POST /api/export/bicep` - Export to Bicep

## 🏗️ Project Structure

```
APIMPolicyBuilder/
├── frontend/                    # Vue 3 Frontend
│   ├── src/
│   │   ├── components/         # Reusable components
│   │   │   ├── MonacoEditor.vue
│   │   │   └── ValidationPanel.vue
│   │   ├── views/              # Page components
│   │   │   ├── PolicyEditor.vue
│   │   │   ├── TemplateLibrary.vue
│   │   │   └── FragmentManager.vue
│   │   ├── services/           # API services
│   │   ├── stores/             # Pinia stores
│   │   └── types/              # TypeScript types
│   └── package.json
│
├── backend/                     # .NET 8 Backend
│   ├── ApimPolicyStudio.Api/   # Web API
│   │   ├── Controllers/
│   │   │   ├── TemplatesController.cs
│   │   │   ├── ValidateController.cs
│   │   │   ├── FragmentsController.cs
│   │   │   └── ExportController.cs
│   │   └── Program.cs
│   │
│   └── ApimPolicyStudio.Core/  # Business Logic
│       ├── Models/
│       │   ├── PolicyTemplate.cs
│       │   ├── PolicyFragment.cs
│       │   ├── ValidationResult.cs
│       │   └── ArmTemplate.cs
│       └── Services/
│           ├── PolicyTemplateService.cs
│           ├── PolicyValidationService.cs
│           ├── PolicyFragmentService.cs
│           └── ArmTemplateService.cs
│
└── docs/                        # Documentation
```

## 🎨 Technologies Used

### Frontend
- **Vue 3**: Progressive JavaScript framework
- **TypeScript**: Type-safe JavaScript
- **Vite**: Fast build tool
- **Pinia**: State management
- **Monaco Editor**: VS Code editor component
- **Tailwind CSS**: Utility-first CSS framework
- **Axios**: HTTP client

### Backend
- **.NET 8**: Modern, cross-platform framework
- **ASP.NET Core**: Web API framework
- **Roslyn**: C# compiler (for expression validation)
- **Azure SDK**: APIM management capabilities
- **FluentValidation**: Object validation
- **Swagger**: API documentation

## 🐛 Troubleshooting

### Frontend won't start

```powershell
# Clear node_modules and reinstall
rm -r .\frontend\node_modules
cd frontend
npm install
npm run dev
```

### Backend build errors

```powershell
# Clean and rebuild
cd backend
dotnet clean
dotnet restore
dotnet build
```

### API not connecting

1. Check backend is running on port 5000
2. Check CORS settings in [Program.cs](backend/ApimPolicyStudio.Api/Program.cs)
3. Verify proxy settings in [vite.config.ts](frontend/vite.config.ts)

### Monaco Editor not loading

The Monaco Editor is loaded from CDN. Ensure you have internet connectivity during development.

## 🚀 Next Steps

### Planned Features (Beyond MVP)

1. **Visual Drag-and-Drop Builder**
   - Drag policy elements onto canvas
   - Visual pipeline representation
   - Auto-generate XML from visual design

2. **Policy Testing**
   - Mock context objects
   - Test C# expressions with sample data
   - Preview policy behavior

3. **Direct APIM Integration**
   - Azure authentication
   - Browse APIM instances
   - Deploy policies directly
   - Compare live vs. local policies

4. **Multi-Environment Management**
   - Environment configurations (Dev/Test/Prod)
   - Parameter substitution
   - Deployment pipelines

5. **Policy Diffing**
   - Visual diff tool
   - Version history
   - Rollback capabilities

6. **Git Integration**
   - Commit policies to Git
   - Branch management
   - Pull request workflows

### Contributing

Feel free to extend this project! Some ideas:

- Add more policy templates
- Improve validation rules
- Create policy snippets library
- Add dark mode
- Create policy documentation generator

## 📚 Resources

- [Azure APIM Documentation](https://learn.microsoft.com/azure/api-management/)
- [APIM Policies Reference](https://learn.microsoft.com/azure/api-management/api-management-policies)
- [C# Expression Reference](https://learn.microsoft.com/azure/api-management/api-management-policy-expressions)
- [ARM Template Reference](https://learn.microsoft.com/azure/templates/)

## 💡 Tips & Best Practices

1. **Always include `<base />`** - Ensures parent policies are executed
2. **Set timeouts on send-request** - Prevents hanging requests
3. **Use rate limiting** - Protect backend services
4. **Validate JWT tokens** - Secure your APIs
5. **Cache aggressively** - Reduce backend load
6. **Handle errors gracefully** - Provide meaningful error messages
7. **Test policies thoroughly** - Use the validation engine
8. **Version your fragments** - Track changes over time

## 📝 License

GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later). You can use, study, modify, and share this project, but modified versions that are distributed or offered over a network must make their corresponding source code available under the same license.

---

**Happy Policy Building! 🎉**

If you have questions or need help, feel free to open an issue or contribute to the project.
