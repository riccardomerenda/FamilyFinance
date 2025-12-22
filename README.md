# FamilyFinance 💰

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://blazor.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A modern, comprehensive **wealth management application** for families. Track net worth, investments, goals, and expenses in one beautiful, private dashboard.

🌐 **Live Demo**: [familyfinance-riccardo.fly.dev](https://familyfinance-riccardo.fly.dev)

---

## ✨ Key Features

*   **📊 Interactive Dashboard**: Visual overview of your financial health with Bento-grid layout.
*   **💰 Asset Tracking**: Detailed views for Investments (Portfolios), Liquidity (Accounts), and Pensions.
*   **🔮 Projections**: Forecast your wealth 10+ years into the future with "What-If" scenarios.
*   **🎯 Smart Goals**: Track progress towards financial targets with deadlines and priority.
*   **💳 Budgeting**: Categorize monthly expenses and set limits with visual alerts.
*   **📈 Comparison**: Analyze changes month-over-month with detailed deltas.
*   **👨‍👩‍👧‍👦 Multi-User**: Family-based isolation with granular permissions (Admin/Member).
*   **🎨 Modern UI**: Fully responsive design with Dark/Light modes and glassmorphism.

---

## 🚀 Quick Start

### Prerequisites
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run Locally
```bash
git clone https://github.com/riccardomerenda/FamilyFinance.git
cd FamilyFinance/FamilyFinance
dotnet run
```
Access at **http://localhost:5044**

### 🐳 Docker
```bash
docker-compose up -d
```

---

## 📸 Gallery

| Dashboard | Projections |
|:---:|:---:|
| ![Dashboard](docs/screenshots/dashboard.png) | ![Projections](docs/screenshots/projections.png) |

---

## 🛠️ Tech Stack

*   **Core**: .NET 9, Blazor Server, Entity Framework Core
*   **Data**: SQLite (Embedded), Serilog
*   **UI**: Tailwind CSS, Chart.js, Outfit Font
*   **Auth**: ASP.NET Core Identity (Multi-family support)

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## 📄 License
MIT License - see [LICENSE](LICENSE).

<p align="center"><sub>Built with ❤️ by <a href="https://github.com/riccardomerenda">Riccardo Merenda</a></sub></p>
