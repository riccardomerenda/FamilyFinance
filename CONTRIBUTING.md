# Contributing to FamilyFinance

First off, thanks for taking the time to contribute! 🎉

The following is a set of guidelines for contributing to FamilyFinance. These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Pull Requests](#pull-requests)
- [Development Setup](#development-setup)
- [Style Guidelines](#style-guidelines)
- [Commit Messages](#commit-messages)

## Code of Conduct

This project and everyone participating in it is governed by our commitment to providing a welcoming and inclusive environment. Please be respectful and constructive in all interactions.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates.

**When reporting a bug, include:**

- A clear and descriptive title
- Steps to reproduce the issue
- Expected behavior vs actual behavior
- Screenshots if applicable
- Your environment (OS, .NET version, browser)

Use the [Bug Report template](.github/ISSUE_TEMPLATE/bug_report.md) when creating issues.

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues.

**When suggesting an enhancement, include:**

- A clear and descriptive title
- Detailed description of the proposed feature
- Why this enhancement would be useful
- Possible implementation approach (optional)

Use the [Feature Request template](.github/ISSUE_TEMPLATE/feature_request.md) when creating issues.

### Pull Requests

1. **Fork** the repository
2. **Clone** your fork locally
3. **Create a branch** for your feature/fix:
   ```bash
   git checkout -b feature/amazing-feature
   ```
4. **Make your changes** and test them
5. **Commit** with a descriptive message (see [Commit Messages](#commit-messages))
6. **Push** to your fork:
   ```bash
   git push origin feature/amazing-feature
   ```
7. **Open a Pull Request** against the `main` branch

#### PR Requirements

- [ ] Code follows the project style guidelines
- [ ] Self-review completed
- [ ] Tests added/updated (if applicable)
- [ ] Documentation updated (if applicable)
- [ ] No new warnings introduced
- [ ] Linked to related issue(s)

## Development Setup

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) / [JetBrains Rider](https://www.jetbrains.com/rider/)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/FamilyBalance.git
cd FamilyBalance

# Restore dependencies
dotnet restore

# Run the application
cd FamilyFinance
dotnet run

# Run tests
cd ../FamilyFinance.Tests
dotnet test
```

### Database

The application uses SQLite. The database file (`familyfinance.db`) is created automatically on first run.

To reset the database:
```bash
rm FamilyFinance/familyfinance.db*
dotnet ef database update --project FamilyFinance
```

## Style Guidelines

### C# Code Style

- Use C# 12 features where appropriate
- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Keep methods small and focused
- Add XML documentation for public APIs

### Blazor Components

- One component per file
- Use `@code` blocks at the end of `.razor` files
- Extract reusable logic into services
- Follow the existing component patterns

### CSS/Styling

- Use Tailwind CSS utility classes
- Follow existing color schemes and patterns
- Ensure dark mode compatibility
- Test on mobile viewports

## Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

```
feat(projections): Add what-if simulator

fix(dashboard): Correct net worth calculation

docs(readme): Add Docker instructions

refactor(services): Extract portfolio logic to dedicated service
```

## Questions?

Feel free to open an issue with the `question` label or reach out to the maintainers.

Thank you for contributing! 🙏

