# 🎯 APIM Policy Studio

> A powerful visual policy builder and management tool for Azure API Management

[![License: AGPL v3+](https://img.shields.io/badge/License-AGPL%20v3%2B-blue.svg)](https://www.gnu.org/licenses/agpl-3.0.html)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Vue](https://img.shields.io/badge/Vue-3.4-4FC08D)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.4-3178C6)](https://www.typescriptlang.org/)

---

## 📸 Screenshots

### Policy Editor
Create and edit APIM policies with Monaco Editor (VS Code engine) with real-time validation.

### Template Library  
Browse 16+ pre-built policy templates for rate limiting, JWT validation, caching, CORS, and more.

### Fragment Manager
Create reusable policy fragments with version control and usage tracking.

---

## ✨ Features

### 🎨 Visual Policy Management
- **Monaco Editor Integration**: Full VS Code editing experience
- **Real-time Validation**: Instant XML and C# expression validation  
- **Syntax Highlighting**: XML and C# syntax coloring
- **IntelliSense Ready**: Prepared for C# expression autocomplete

### 📚 Template Library
- **16+ Templates**: Rate limiting, JWT validation, CORS, caching, error handling, and more
- **Category Filtering**: Browse by inbound, backend, outbound, or error policies
- **One-Click Use**: Apply templates instantly to your policy
- **Copy to Clipboard**: Easy sharing and reuse

### 🧩 Fragment Manager
- **Reusable Fragments**: Create once, use everywhere
- **Version Control**: Track fragment versions over time
- **Usage Analytics**: See where fragments are used
- **CRUD Operations**: Full create, read, update, delete support

### ✅ Validation Engine
- **XML Schema Validation**: Ensure well-formed policy documents
- **C# Expression Checking**: Syntax validation using Roslyn compiler
- **Best Practices**: Warnings for anti-patterns and missing elements
- **Detailed Feedback**: Line-by-line error and warning reporting

### 📦 Export Capabilities  
- **ARM Templates**: Generate Azure Resource Manager JSON
- **Bicep**: Export to modern Azure IaC format
- **Direct Copy**: Clipboard integration for quick sharing

---

## 🚀 Quick Start

### Prerequisites
- **Node.js** 20+ ([Download](https://nodejs.org/))
- **.NET 8 SDK** ([Download](https://dotnet.microsoft.com/download))

### Installation

```powershell
# Clone or navigate to project
cd c:\Others\APIMPolicyBuilder

# Run the quick start script
.\start.ps1
```

Choose option 1 for full setup and automatic launch!

### Manual Setup

```powershell
# Install frontend dependencies
cd frontend
npm install

# Restore backend dependencies  
cd ..\backend
dotnet restore

# Run backend (Terminal 1)
dotnet run --project ApimPolicyStudio.Api

# Run frontend (Terminal 2)
cd ..\frontend
npm run dev
```

### Access Application

| Service | URL |
|---------|-----|
| **Frontend** | http://localhost:5173 |
| **Backend API** | http://localhost:5000 |
| **Swagger UI** | http://localhost:5000/swagger |

---

## 📖 Documentation

- **[Getting Started Guide](GETTING_STARTED.md)** - Detailed setup and usage instructions
- **[Development Guide](docs/DEVELOPMENT.md)** - Architecture and contribution guidelines
- **[Quick Reference](docs/QUICK_REFERENCE.md)** - APIM policy patterns and examples

---

## 🏗️ Architecture

```
┌─────────────────────┐
│   Vue 3 Frontend    │
│  ┌───────────────┐  │
│  │ Policy Editor │  │
│  │  Template Lib │  │
│  │  Fragments    │  │
│  └───────────────┘  │
│  Monaco | Tailwind  │
└──────────┬──────────┘
           │ HTTP/REST
           │
┌──────────▼──────────┐
│   .NET 8 Backend    │
│  ┌───────────────┐  │
│  │  Templates    │  │
│  │  Validation   │  │
│  │  Fragments    │  │
│  │  ARM Export   │  │
│  └───────────────┘  │
│  Roslyn | Azure SDK │
└─────────────────────┘
```

### Tech Stack

**Frontend**
- Vue 3 + TypeScript
- Vite (build tool)
- Pinia (state management)
- Monaco Editor (code editing)
- Tailwind CSS (styling)
- Axios (HTTP client)

**Backend**
- .NET 8 Web API
- Roslyn (C# compiler for validation)
- Azure.ResourceManager.ApiManagement SDK
- FluentValidation
- Swagger/OpenAPI

---

## 🎯 Use Cases

### For DevOps Engineers
- Quickly prototype APIM policies
- Validate policies before deployment
- Generate ARM/Bicep for CI/CD pipelines
- Share policy fragments across teams

### For API Developers
- Understand APIM policy capabilities
- Learn policy syntax with examples
- Test C# expressions safely
- Document API patterns

### For Azure Architects
- Design API management strategies
- Standardize policies across APIs
- Create reusable policy components
- Export infrastructure as code

---

## 🗺️ Roadmap

### ✅ MVP (Current - v0.1)
- [x] Policy XML editor with Monaco
- [x] 16+ policy templates
- [x] XML and C# validation
- [x] Fragment management
- [x] ARM/Bicep export

### 🚧 Phase 2 (v0.2)
- [ ] Visual drag-and-drop policy builder
- [ ] Policy testing with mock contexts
- [ ] Dark mode
- [ ] Additional templates (20+)
- [ ] Policy snippets library

### 🔮 Phase 3 (v0.3)
- [ ] Direct APIM integration (Azure auth)
- [ ] Policy deployment to APIM
- [ ] Multi-environment support (Dev/Test/Prod)
- [ ] Policy diffing tool
- [ ] Version history

### 🌟 Future
- [ ] Git integration
- [ ] Real-time collaboration
- [ ] Policy analytics
- [ ] AI-powered policy suggestions
- [ ] Terraform export
- [ ] Docker deployment

---

## 🤝 Contributing

Contributions are welcome! Here's how:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed guidelines.

---

## 📄 License

This project is licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later). You can use, study, modify, and share it, but modified versions that are distributed or offered over a network must make their corresponding source code available under the same license. See the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Azure API Management** - Microsoft's API gateway platform
- **Monaco Editor** - The code editor powering VS Code
- **Vue.js** - The progressive JavaScript framework
- **Roslyn** - The .NET compiler platform

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/APIMPolicyBuilder/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/APIMPolicyBuilder/discussions)
- **Documentation**: [Getting Started](GETTING_STARTED.md)

---

## 🌟 Show Your Support

If you find this project helpful, please consider:
- ⭐ Starring the repository
- 🐛 Reporting bugs
- 💡 Suggesting new features
- 📝 Improving documentation
- 🔀 Contributing code

---

Made with ❤️ for the Azure APIM community

**Happy Policy Building! 🚀**
# AzurePolicyBuilder
