# 🌐 TelcoNet AI — Network Intelligence Backend

![.NET Core](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Azure OpenAI](https://img.shields.io/badge/Azure_OpenAI-0078D4?style=for-the-badge&logo=microsoft-azure&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-07405E?style=for-the-badge&logo=sqlite&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

**TelcoNet AI** is a next-generation Network Operations Center (NOC) backend built for the **MTN Telco AI-Native Network Intelligence Hackathon**. It leverages modern .NET 8 Architecture and Microsoft's Semantic Kernel to power a proactive, AI-driven telco management system.

---

## 🚀 Features

- **🧠 AI Copilot Integration:** Powered by Azure OpenAI and Semantic Kernel, allowing operators to query network status, detect outages, and resolve issues via natural language.
- **🔌 5 Custom AI Plugins:** Real-time semantic plugins for Network Queries, Outage Detection, Coverage Finding, KPIs, and Alert Monitoring.
- **🗺️ Geo-Spatial Node Tracking:** Live API endpoints for mapping cell towers and hubs across regions (Healthy, Degraded, Down).
- **📈 Live Analytics & KPIs:** Real-time tracking of latency, packet loss, throughput, and active user density.
- **🔐 Role-Based Access Control (RBAC):** Secure JWT authentication with strict roles (`Admin`, `Operator`, `Viewer`).
- **🛡️ Audit Logging:** Comprehensive security middleware that tracks and records every interaction for compliance.

---

## 🏗️ Architecture

The solution follows a clean, highly-scalable layered architecture:
- `TelcoNet.API`: Presentation layer (Controllers, Middleware, OpenAPI configuration).
- `TelcoNet.Core`: Business logic, DTOs, and Interfaces.
- `TelcoNet.Plugins`: Microsoft Semantic Kernel custom function definitions.
- `TelcoNet.Data`: Entity Framework Core configurations, SQLite context, and initial Seed Data generation.

---

## 🛠️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- IDE (Visual Studio, VS Code, or JetBrains Rider)

### 1. Environment Setup
Create a `.env` file in the root directory (where the `.sln` file is located) and add your Azure OpenAI credentials and JWT secrets:

```env
# Azure OpenAI Configuration
AZURE_OPENAI_ENDPOINT=https://your-endpoint.cognitiveservices.azure.com
AZURE_OPENAI_API_KEY=your_api_key_here
AZURE_OPENAI_MODEL_ID=your_deployment_name

# JWT Configuration
JWT_SECRET=Your-Super-Secret-Key-Must-Be-32-Chars!
JWT_ISSUER=TelcoNet.API
JWT_AUDIENCE=TelcoNet.Client
```

### 2. Run the Application
The application uses Entity Framework Core with SQLite. On the first run, it will automatically create the database (`telconet.db`) and inject realistic network seed data.

```bash
# Navigate to the API project
cd TelcoNet.API

# Run the project
dotnet run
```

### 3. API Documentation
Once running, you can access the beautiful API documentation (powered by Scalar) at:
👉 `http://localhost:5096/scalar/v1`

*(Or access the raw OpenAPI spec at `http://localhost:5096/openapi/v1.json`)*

---

## 📡 Core API Endpoints

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `/api/auth/login` | POST | Public | Authenticate user & get JWT token |
| `/api/network/health` | GET | Authenticated | Get global network status and node counts |
| `/api/network/nodes` | GET | Authenticated | Get geographical node data for mapping |
| `/api/network/timeline` | GET | Authenticated | Get real-time incident timeline |
| `/api/dashboard/kpis` | GET | Authenticated | Fetch primary dashboard metrics |
| `/api/copilot/chat` | POST | Authenticated | Interact with the Semantic Kernel AI |
| `/api/users` | GET | Admin | Manage platform users and roles |

*(See the `API_DOCUMENTATION.md` file in the repository for detailed request/response payloads).*

---
*Built with ❤️ for the TeKnowledge AI Hackathon*
