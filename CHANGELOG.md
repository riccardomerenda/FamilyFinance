# Changelog

All notable changes to this project will be documented in this file.

## [0.4.6] - 2026-01-16

### Added
- **Smart Transfers**: Intelligent handling of internal transfers during CSV import (creates withdrawal/deposit pair).
- **Pension Page Live Data**: Pension & Insurance page now shows live account balances merged with historical contribution data.
- **Bulk Delete**: Ability to select and delete multiple transactions at once.

### Fixed
- Fixed issue where Pension page showed stale snapshot data.
- Fixed `FOREIGN KEY` constraint errors on recurring match rules.


## [4.4.0] - 2026-01-16

### Added
- **Smart Transaction Matching**: Automatic detection of recurring transactions and open receivables during CSV import.
- **Manual Match Learning**: Ability to manually link transactions to recurring templates with auto-learning for future imports.
- **Dynamic Category Icons**: Expanded emoji set for categories and centralized management.
- **Improved Import Experience**: Inline match badges, quick actions, and better visual feedback.


### Added
- **Credit Collection**: Added ability to mark credits as collected directly from the Dashboard
- **Asset Transfer**: Optional transfer of collected credit amount to a liquidity account
- **Dialog**: New UI component for handling credit collection with account selection

## [4.3.0] - 2025-12-26

### Added
- Maintenance mode feature
