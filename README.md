<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 10.0">
  <img src="https://img.shields.io/badge/Blazor-Server-512BD4?style=for-the-badge&logo=blazor" alt="Blazor Server">
  <img src="https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite" alt="SQLite">
  <img src="https://img.shields.io/badge/Tailwind-CSS-38B2AC?style=for-the-badge&logo=tailwindcss" alt="Tailwind CSS">
  <img src="https://img.shields.io/badge/Deploy-Fly.io-8B5CF6?style=for-the-badge&logo=fly.io" alt="Fly.io">
</p>

<h1 align="center">💰 FamilyFinance</h1>

<p align="center">
  <strong>Personal & Family Wealth Management Dashboard</strong><br>
  Track liquidity, investments, goals, and expenses — all in one place.
</p>

<p align="center">
  <a href="#-features">Features</a> •
  <a href="#-quick-start">Quick Start</a> •
  <a href="#-tech-stack">Tech Stack</a> •
  <a href="#-screenshots">Screenshots</a> •
  <a href="#-deployment">Deployment</a>
</p>

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 📸 **Monthly Snapshots** | Capture financial state and compare over time |
| 🎯 **Goal Tracking** | Set savings targets with progress visualization |
| 📊 **Interactive Dashboard** | Trend charts, composition breakdowns, net worth cards |
| 💼 **Investment Portfolio** | Track assets with cost basis and gain/loss |
| 📅 **Monthly Wizard** | Guided 4-step closing workflow |
| 🔄 **Import History (BETA)** | View import logs and smart revert changes |
| 💸 **Budget Tracking** | Expense categories with monthly limits |
| 🌍 **Multi-language** | Italian & English |
| 👨‍👩‍👧 **Multi-user** | Family accounts with role-based access |
| 🎭 **Demo Mode** | Try with sample data, no signup required |
| 🌙 **Dark Mode** | Beautiful UI in light and dark themes |

---

## 🚀 Quick Start

```bash
# Clone the repository
git clone https://github.com/riccardomerenda/FamilyFinance.git
cd FamilyFinance/FamilyFinance

# Run the application
dotnet run
```

Open your browser at **http://localhost:5044**

> 💡 **Demo Mode**: Use email `demo@example.com` with any password to explore with sample data!

---

## 🛠 Tech Stack

- **Framework**: ASP.NET Core 10 + Blazor Server
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Identity with role-based authorization
- **UI/UX**: Custom Tailwind CSS + Glassmorphism design
- **Charts**: Chart.js integration
- **Deployment**: Docker + Fly.io

---

## 📁 Project Structure

```
FamilyFinance/
├── Components/       # Reusable Blazor components
│   ├── Dashboard/    # Dashboard widgets
│   └── Wizard/       # Monthly closing wizard
├── Pages/            # Application pages
├── Services/         # Business logic layer
│   └── Interfaces/   # Service contracts
├── Models/           # Entity models
├── Data/             # EF Core DbContext
└── Resources/        # Localization files
```

---

## 🐳 Docker Deployment

```bash
# Build the image
docker build -t familyfinance .

# Run locally
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production familyfinance
```

### Fly.io

```bash
fly launch
fly deploy
```

---

## 📄 License

MIT License © [Riccardo Merenda](https://github.com/riccardomerenda)

---

<p align="center">
  Made with ❤️ for families who care about their financial future
</p>
