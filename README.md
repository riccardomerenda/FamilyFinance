# FamilyFinance 💰

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Deploy](https://img.shields.io/badge/Deploy-Fly.io-8B5CF6?logo=fly.io)](https://fly.io)

> **Gestione patrimonio familiare** — Monitora liquidità, investimenti e obiettivi in un'unica dashboard.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 📸 **Snapshot Mensili** | Cattura lo stato delle finanze e confronta nel tempo |
| 🎯 **Obiettivi** | Imposta target di risparmio con progress tracking |
| 📊 **Dashboard Interattiva** | Grafici trend e composizione patrimonio |
| 💼 **Portfolio Investimenti** | Traccia asset con costo carico e gain/loss |
| 📅 **Wizard Chiusura Mese** | Procedura guidata in 4 step |
| 🌍 **Multi-lingua** | Italiano / English |
| 👨‍👩‍👧 **Multi-utente** | Famiglia con account separati |
| 🔒 **Demo Mode** | Prova l'app con dati di esempio |

---

## 🚀 Quick Start

```bash
# Clone
git clone https://github.com/riccardomerenda/FamilyFinance.git
cd FamilyFinance/FamilyFinance

# Run
dotnet run
```

Apri [http://localhost:5044](http://localhost:5044)

---

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 9, Blazor Server
- **Database**: SQLite + Entity Framework Core
- **Auth**: ASP.NET Identity
- **UI**: Tailwind CSS (custom), Chart.js
- **Deploy**: Docker, Fly.io

---

## 📁 Project Structure

```
FamilyFinance/
├── Components/       # Blazor components (Wizard, Tour, Charts)
├── Pages/           # Razor pages (Dashboard, Snapshots, Goals...)
├── Services/        # Business logic (granular services)
├── Models/          # Entity models
├── Data/            # EF Core DbContext
└── Resources/       # Localization (it-IT, en-US)
```

---

## 🐳 Docker

```bash
docker build -t familyfinance .
docker run -p 8080:8080 familyfinance
```

---

## 📄 License

MIT — [Riccardo Merenda](https://github.com/riccardomerenda)
