# FamilyFinance 💰

A comprehensive family wealth management application built with Blazor Server and Entity Framework Core.

## Features

### 📊 Financial Tracking
- **Accounts**: Track multiple accounts (bank, savings, investments, pension)
- **Snapshots**: Monthly financial snapshots with full history
- **Net Worth**: Real-time calculation with pension projections

### 💼 Investment Management
- **Portfolios**: Group investments by strategy/goal
- **Cost Basis Tracking**: Track gains/losses on investments
- **Collapsible Views**: Clean dashboard with expandable details

### 🎯 Goal Planning
- **Financial Goals**: Set targets with deadlines
- **Priority System**: High/Medium/Low priority levels
- **Allocation Tracking**: Manual allocation with unallocated alerts
- **Monthly Projections**: See required monthly savings

### 💳 Budget & Expenses
- **Budget Categories**: Customizable expense categories with emoji icons
- **Monthly Limits**: Set and track monthly budgets
- **Visual Progress**: Progress bars showing budget usage
- **Overspend Alerts**: Visual warnings when over budget

### 👨‍👩‍👧‍👦 Multi-Family Support
- **Family Accounts**: Each family has isolated data
- **User Management**: Multiple users per family
- **Role-Based Access**: Admin and member roles

### 📦 Data Management
- **Full Backup**: JSON export with all data
- **CSV Export**: Snapshots, investments, goals
- **Smart Import**: Preview and merge capabilities
- **Data Restore**: Easy restoration from backup

### 🌐 Localization
- **Italian** 🇮🇹 (default)
- **English** 🇬🇧

## Tech Stack

- **Frontend**: Blazor Server
- **Backend**: ASP.NET Core 9.0
- **Database**: SQLite with EF Core
- **Auth**: ASP.NET Core Identity
- **Styling**: Tailwind CSS

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 / VS Code / Rider

### Run locally

```bash
cd FamilyFinance
dotnet run
```

Open http://localhost:5044

### First Setup
1. Register a new account
2. Create your family
3. Add accounts (bank, savings, etc.)
4. Create your first snapshot

## Project Structure

```
FamilyBalance/
├── FamilyBalance.sln
├── README.md
└── FamilyFinance/
    ├── Components/      # Reusable UI components
    ├── Controllers/     # Auth & API controllers
    ├── Data/           # DbContext
    ├── Migrations/     # EF Core migrations
    ├── Models/         # Domain models
    ├── Pages/          # Blazor pages
    ├── Resources/      # Localization files
    ├── Services/       # Business logic
    └── wwwroot/        # Static assets
```

## License

MIT License

## Author

Built with ❤️ using Blazor

