# FamilyFinance 💰

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://blazor.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

A comprehensive **family wealth management application** built with Blazor Server and Entity Framework Core. Track your net worth, investments, goals, and expenses — all in one place.

[Features](#-features) • [Quick Start](#-quick-start) • [Docker](#-docker) • [Screenshots](#-screenshots) • [Contributing](#-contributing)

---

## ✨ Features

### 📊 Financial Dashboard
- **Net Worth Tracking**: Real-time calculation with/without pension
- **Trend Charts**: Visualize wealth growth over time
- **Monthly Snapshots**: Complete financial picture each month
- **Multi-currency Support**: Track accounts in different currencies

### 📈 Projections & Forecasting
- **Growth Projections**: 1-10 year wealth forecasts based on historical data
- **"When Will I Reach?"**: Calculate time to reach financial targets
- **What-If Simulator**: Model different savings scenarios
- **Goal Timeline**: Estimated completion dates for all goals

### 💼 Investment Management
- **Portfolio Grouping**: Organize by strategy (DCA, Crypto, Retirement)
- **Cost Basis Tracking**: Track gains/losses with performance %
- **Time Horizons**: Set target years for each portfolio
- **Collapsible Views**: Clean interface with expandable details

### 🎯 Goal Planning
- **Financial Goals**: Set targets with deadlines and priority levels
- **Manual Allocation**: Assign funds to specific goals
- **Progress Tracking**: Visual progress bars and completion status
- **Monthly Projections**: Required savings to reach goals on time

### 💳 Budget & Expenses
- **Custom Categories**: Create categories with emoji icons and colors
- **Monthly Budgets**: Set spending limits per category
- **Visual Progress**: See budget usage with progress bars
- **Overspend Alerts**: Get warned when exceeding budget

### 📊 Monthly Comparison
- **Side-by-Side View**: Compare any two months
- **Delta Analysis**: See what changed and by how much
- **Category Breakdown**: Detailed changes per account type
- **Growth Metrics**: Percentage changes highlighted

### 👨‍👩‍👧‍👦 Multi-Family Support
- **Isolated Data**: Each family's data is completely separate
- **User Roles**: Admin and Member permissions
- **Family Management**: Add/remove family members

### 📦 Import/Export
- **Full JSON Backup**: Complete data export
- **CSV Exports**: Snapshots, investments, goals separately
- **Smart Import**: Preview before importing with merge options
- **Data Portability**: Easy backup and restore

### 🌐 Internationalization
- 🇮🇹 Italian (default)
- 🇬🇧 English
- 🌙 Dark/Light theme with persistence

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- (Optional) [Docker](https://www.docker.com/) for containerized deployment

### Run Locally

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/FamilyBalance.git
cd FamilyBalance

# Run the application
cd FamilyFinance
dotnet run
```

Open your browser at **http://localhost:5044**

### First Setup

1. **Register** a new account
2. **Create your family** (give it a name)
3. **Add accounts** (bank, savings, pension, etc.)
4. **Create your first snapshot** with current balances
5. **Set up goals** and track your progress!

---

## 🐳 Docker

### Using Docker Compose (Recommended)

```bash
# Build and run
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

### Using Docker directly

```bash
# Build the image
docker build -t familyfinance .

# Run the container
docker run -d -p 5044:8080 \
  -v familyfinance-data:/app/data \
  --name familyfinance \
  familyfinance
```

Access at **http://localhost:5044**

---

## 📸 Screenshots

> Add your screenshots here!

<details>
<summary>📊 Dashboard</summary>

![Dashboard](docs/screenshots/dashboard.png)
</details>

<details>
<summary>📈 Projections</summary>

![Projections](docs/screenshots/projections.png)
</details>

<details>
<summary>💼 Investments</summary>

![Investments](docs/screenshots/investments.png)
</details>

---

## 🏗️ Project Structure

```
FamilyBalance/
├── FamilyBalance.sln          # Solution file
├── README.md
├── LICENSE
├── CONTRIBUTING.md
├── Dockerfile
├── docker-compose.yml
├── .github/
│   └── workflows/
│       └── dotnet.yml         # CI/CD pipeline
└── FamilyFinance/
    ├── Components/            # Reusable Blazor components
    │   └── Dashboard/         # Dashboard-specific components
    ├── Controllers/           # Auth & Culture controllers
    ├── Data/                  # DbContext configuration
    ├── Migrations/            # EF Core migrations
    ├── Models/                # Domain entities & DTOs
    ├── Pages/                 # Blazor pages (routes)
    ├── Resources/             # Localization (.resx files)
    ├── Services/              # Business logic layer
    │   └── Interfaces/        # Service contracts
    ├── Shared/                # Layout components
    └── wwwroot/               # Static assets (CSS, JS)
```

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| [.NET 9.0](https://dotnet.microsoft.com/) | Runtime & SDK |
| [Blazor Server](https://blazor.net/) | Interactive UI framework |
| [Entity Framework Core](https://docs.microsoft.com/ef/) | ORM & data access |
| [SQLite](https://sqlite.org/) | Embedded database |
| [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity) | Authentication |
| [Tailwind CSS](https://tailwindcss.com/) | Styling |
| [Chart.js](https://www.chartjs.org/) | Data visualization |

---

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting a PR.

### Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/FamilyBalance.git
cd FamilyBalance

# Create a branch
git checkout -b feature/amazing-feature

# Make changes and test
cd FamilyFinance
dotnet run

# Commit and push
git commit -m "feat: Add amazing feature"
git push origin feature/amazing-feature
```

### Running Tests

```bash
cd FamilyFinance.Tests
dotnet test
```

---

## 📋 Roadmap

- [ ] Mobile-responsive improvements
- [ ] Recurring transactions
- [ ] Bank statement import (CSV/OFX)
- [ ] Multiple currencies with conversion
- [ ] Email notifications for goals
- [ ] API for external integrations
- [ ] PWA support

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with ❤️ using [Blazor](https://blazor.net/)
- Icons by [Heroicons](https://heroicons.com/)
- Charts by [Chart.js](https://www.chartjs.org/)

---

<p align="center">
  <sub>Made with ☕ by <a href="https://github.com/YOUR_USERNAME">Your Name</a></sub>
</p>
