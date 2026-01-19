# Changelog

All notable changes to this project will be documented in this file.


## [4.8.3] - 2026-01-19

### Added
- **Investment Update Warning**: Nel wizard di chiusura mese, se gli investimenti non sono stati aggiornati nel mese corrente, appare un warning con link diretto all'import.

## [4.8.2] - 2026-01-19

### Changed
- **Investment Import Date**: L'import investimenti da CSV Directa ora utilizza la data di estrazione del file (`Data estrazione`) come timestamp, invece della data di caricamento.

## [4.8.1] - 2026-01-19

### Changed
- **Documentation UI**: Nuova interfaccia a tab per la pagina di aiuto (`/help`).
- **Docs Content**: Aggiunta documentazione completa per Concetti Chiave, Guide e FAQ.

## [4.8.0] - 2026-01-19

### Changed
- **Live-First Pension Architecture**: L'architettura delle pensioni è stata allineata a quella degli investimenti. `PensionHolding` è ora la singola fonte di verità per `ContributionBasis`.
- **SnapshotEdit Sync**: La creazione/modifica di snapshot ora legge `ContributionBasis` dai dati live e sincronizza le modifiche a `PensionHolding`.

### Added
- **UpdateContributionBasisAsync**: Nuovo metodo in `IPensionHoldingService` per aggiornare direttamente i contributi.

## [4.7.1] - 2026-01-19

### Improved
- **Rendimento Annualizzato**: La pagina Previdenza ora mostra il rendimento annualizzato (`%/anno`) invece del rendimento totale, calcolato in base alla data di creazione del conto.

## [4.7.0] - 2026-01-18

### Added
- **Pension Holdings Live Architecture**: Il tracking di previdenza e assicurazioni ora utilizza l'architettura live, analogamente agli investimenti (`AssetHolding`).
- **PensionHolding Model**: Nuova entità che traccia `ContributionBasis` e `CurrentValue` in tempo reale.
- **Auto-Sync Values**: I valori vengono sincronizzati automaticamente quando si visita la pagina Previdenza.
- **Contribution Tracking**: I contributi verso conti pensione/assicurazione aggiornano automaticamente `ContributionBasis`.
- **Seed from Snapshot**: Migrazione automatica dei dati dall'ultimo snapshot alla nuova architettura live.

## [4.6.0] - 2026-01-17

### Added
- **Investment Recurring Contributions**: Le transazioni ricorrenti possono ora essere collegate a investimenti (PAC, ETF) o fondi pensione. Durante l'import CSV, il sistema aggiorna automaticamente il valore dell'investimento collegato.
- **UI Investment Target Selector**: Nella pagina "Transazioni Ricorrenti" è ora possibile collegare una ricorrente a un investimento specifico.
- **Auto-Update PAC/Pensione**: Import wizard mostra feedback quando contributi vengono registrati (es. "📈 PAC Mensile: +200€ investiti").

## [4.5.0] - 2026-01-17

### Changed
- **Dashboard 2.0**: Layout completamente riorganizzato. Command Center e Insights ora sono compatti e posizionati in alto per un accesso rapido senza rubare spazio.
- **Dynamic Grid**: La sezione Obiettivi si adatta dinamicamente per mostrare le card Previdenza e Interessi sulla stessa riga, ottimizzando lo spazio.
- **Command Center**: Ridisegnato come barra orizzontale compatta con suggerimenti contestuali integrati.

### Removed
- Rimossa la sezione "Quick Actions" in quanto ridondante con il nuovo Command Center.

## [4.4.5] - 2026-01-17

### Added
- **Live Holdings Architecture**: Investments are now managed in real-time (`Asset Holdings`) independent of monthly snapshots.
- **Directa Integration**: Import wizard now updates live holdings directly.
- **SnapshotEdit Update**: Read-only investment view with "Live Holdings" banner.
- **Monthly Wizard UI**: Updated "Investments" step with read-only "Live Holdings" view and explanatory banner.
- **MonthClose Integration**: Creating a monthly snapshot now automatically captures current live holdings.

## [4.4.4] - 2026-01-16

### Improved
- **Automatic Balance Sync**: Manually adding or deleting transactions now automatically updates the linked Account's balance by the corresponding amount (delta update).

## [4.4.3] - 2026-01-16

### Improved
- **Import Wizard**: Added ability to **Undo** (reject) a confirmed match (recurring or transfer) directly from the badge.

## [4.4.2] - 2026-01-16

### Fixed
- **Import Wizard**: The "Import Selected" button now correctly shows the **Net Balance** (algebraic sum) instead of the total volume (absolute sum).

## [4.4.1] - 2026-01-16

### Improved
- **Wealth Insight**: "Wealth Growth" now compares your **current live total** against the last closed month, providing immediate feedback on recent changes (imports, transfers).

## [4.4.0] - 2026-01-16

### Added
- **Smart Transfers**: Intelligent handling of internal transfers during CSV import (creates withdrawal/deposit pair).
- **Smart Transaction Matching**: Automatic detection of recurring transactions and open receivables during CSV import.
- **Manual Match Learning**: Ability to manually link transactions to recurring templates with auto-learning for future imports.
- **Dynamic Category Icons**: Expanded emoji set for categories and centralized management.
- **Pension Page Live Data**: Pension & Insurance page now shows live account balances merged with historical contribution data.
- **Bulk Delete**: Ability to select and delete multiple transactions at once.
- **Credit Collection**: Added ability to mark credits as collected directly from the Dashboard
- **Asset Transfer**: Optional transfer of collected credit amount to a liquidity account
- **Dialog**: New UI component for handling credit collection with account selection
- **Improved Import Experience**: Inline match badges, quick actions, and better visual feedback.

### Fixed
- Fixed issue where Pension page showed stale snapshot data.
- Fixed `FOREIGN KEY` constraint errors on recurring match rules.

## [4.3.0] - 2025-12-26

### Added
- **Privacy Mode**: Blur financial amounts with toggle
- **Maintenance Mode**: Feature to lock access
