# Changelog

Tutte le modifiche significative a FamilyFinance sono documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce al [Semantic Versioning](https://semver.org/lang/it/).

---

## [2.6.0] - 2025-12-21

### ☁️ Cloud Deployment - Fly.io

Deploy in produzione su Fly.io con supporto per storage persistente.

### Aggiunto

#### Cloud Deployment
- **Fly.io configuration** (`fly.toml`) per deploy one-click
- **Volume persistente** per database SQLite in cloud
- **GitHub Actions workflow** per deploy automatico su push
- **Auto-scaling** con stop automatico quando inattivo

#### Miglioramenti Dockerfile
- Fix restore per singolo progetto (no solution)
- Ottimizzazione build time

### 🌐 Live Demo
- App disponibile su: https://familyfinance-riccardo.fly.dev

---

## [2.5.0] - 2025-12-21

### 🚀 Open Source Ready

Preparazione completa per rilascio open-source su GitHub.

### Aggiunto

#### Documentazione
- **README.md** professionale con badges, features, quick start, Docker
- **CONTRIBUTING.md** con linee guida per contribuire
- **LICENSE** MIT
- **env.example** per configurazione

#### DevOps
- **Dockerfile** multi-stage ottimizzato
- **docker-compose.yml** per deploy semplificato
- **GitHub Actions** CI/CD workflow per build e test automatici
- **Health check endpoint** `/health` per Docker/Kubernetes

#### Testing
- **Test project** con xUnit
- **25 unit tests** per AccountService, GoalService, PortfolioService
- **InMemory database** per test isolati

#### GitHub Integration
- **Issue templates**: Bug Report, Feature Request
- **Pull Request template**
- **Docs folder** per screenshots

---

## [2.4.0] - 2025-12-21

### 📈 Nuova Funzionalità - Proiezioni e Forecasting

Trasformazione dell'app da "registro passivo" a "strumento di pianificazione attiva".

### Aggiunto

#### Pagina Proiezioni (`/projections`)
- **Analisi Trend**: Cards riassuntive con patrimonio attuale, crescita mensile media, tasso percentuale
- **Grafico Proiezione**: Visualizzazione storico + proiezione futura (1-10 anni)
- **Calcolatore "Quando raggiungerò?"**: Inserisci un importo obiettivo e calcola la data stimata
- **Simulatore What-If**: 
  - Simula contributi mensili aggiuntivi
  - Calcola valore futuro con compound interest
  - Mostra crescita stimata e guadagno totale
- **Timeline Obiettivi**: Stima automatica della data di completamento per ogni goal

#### Calcoli Finanziari
- Tasso di crescita composto mensile basato su storico snapshot
- Formula annuity per contributi periodici
- Proiezione logaritmica per tempo a obiettivo

#### Navigazione
- Nuovo link "Proiezioni" nel menu laterale
- Localizzazione completa IT/EN

---

## [2.3.0] - 2025-12-21

### 📊 Nuova Funzionalità - Confronto Mensile

Dashboard comparativa per analizzare le variazioni del patrimonio tra mesi diversi.

### Aggiunto

#### Pagina Confronto (`/compare`)
- Selezione di due snapshot per confronto side-by-side
- Cards riassuntive: patrimonio totale, variazione assoluta, percentuale di crescita
- Grafico a barre per confronto per categoria
- Sezione "Cosa è cambiato": aumenti e diminuzioni ordinati per importo
- Tabella dettagliata con confronto per categoria
- Tabella account con dettaglio per singolo conto/investimento

#### Navigazione
- Nuovo link "Confronto" nel menu laterale
- Localizzazione completa IT/EN

---

## [2.2.0] - 2025-12-21

### 🏗️ Refactoring - Architettura Servizi

Ristrutturazione completa dell'architettura per migliorare manutenibilità e testabilità.

### Aggiunto

#### Architettura
- **Interfacce Service**: `ISnapshotService`, `IAccountService`, `IGoalService`, `IPortfolioService`, `IBudgetService`, `IImportExportService`
- **Servizi Domain-Specific**: Ogni dominio ha il proprio servizio dedicato
- **Componenti Dashboard**: Nuovi componenti riutilizzabili (`NetWorthCards`, `TrendChart`, `CompositionChart`, `BreakdownCard`, `BudgetSummaryCard`, `GoalsSummaryCard`)

#### Multi-tenancy
- Tutti i metodi ora richiedono `familyId` per isolamento dati completo
- Filtri automatici per famiglia in tutte le query

### Rimosso
- File demo: `Counter.razor`, `FetchData.razor`, `WeatherForecast.cs`, `WeatherForecastService.cs`
- Commenti superflui in italiano nel codice
- Cartella vuota `Pages/Auth/`

### Modificato
- `FinanceService` ora è un facade che delega ai servizi specifici
- `Program.cs` con nuova registrazione DI per interfacce e implementazioni
- `_Imports.razor` include nuovi namespace

---

## [2.1.1] - 2025-12-21

### 🔧 Fix - Export/Import Budget & Spese

Correzione per includere le categorie budget e le spese mensili nel sistema di backup.

### Corretto

- **Export JSON**: Ora include `BudgetCategories` e `MonthlyExpenses` per ogni snapshot
- **Import Preview**: Mostra il conteggio delle categorie budget (nuove/esistenti)
- **Import**: Importa le categorie budget con mappatura ID corretta
- **Import**: Importa le spese mensili per ogni snapshot
- Versione formato backup aggiornata a `2.0`

### Modificato

- `BackupDto`: Aggiunti `BudgetCategoryDto` e `MonthlyExpenseDto`
- `SnapshotDto`: Aggiunta lista `MonthlyExpenses`
- `ImportPreview`: Aggiunti contatori categorie budget
- `ImportResult`: Aggiunto `BudgetCategoriesImported`

---

## [2.1.0] - 2025-12-21

### ✨ Nuova Funzionalità - Budget & Spese Mensili

Questa versione introduce il sistema di tracciamento budget e spese mensili.

### Aggiunto

#### Budget & Spese
- Nuova pagina **Gestione Budget** (`/budget`) per configurare le categorie di spesa
- Categorie predefinite inizializzabili con un click (Casa, Alimentari, Trasporti, Utenze, etc.)
- Budget mensile configurabile per ogni categoria
- Icone emoji e colori personalizzabili per le categorie
- Sezione **Spese del Mese** integrata nello SnapshotEdit
- Barra di progresso per ogni categoria rispetto al budget
- Evidenziazione visiva quando si supera il budget
- Nuovo tab **Budget** nella Dashboard con riepilogo spese vs budget
- Visualizzazione percentuale di utilizzo del budget

#### UI Improvements
- Link "Budget" nella navigazione principale
- Card riassuntive: Budget Totale, Speso, Rimanente, % Utilizzato
- Breakdown dettagliato per categoria con barre di progresso

### Modificato
- SnapshotEdit ora include sezione spese integrate
- SaveSnapshotAsync ora ritorna l'ID dello snapshot per salvare le spese correlate

---

## [2.0.0] - 2025-12-21

### 🎉 Major Release - Autenticazione e Multi-Famiglia

Questa versione introduce un sistema completo di autenticazione multi-utente con supporto per famiglie.

### Aggiunto

#### Autenticazione & Utenti
- Sistema di login/registrazione con ASP.NET Core Identity
- Supporto multi-famiglia: ogni famiglia ha i propri dati isolati
- Ruoli utente: Admin e Member
- Gestione membri della famiglia (solo Admin)
- Pagina di setup iniziale per la prima configurazione

#### Import/Export Dati
- Export completo in JSON (backup di tutti i dati)
- Export CSV per snapshots, investimenti e obiettivi
- Import intelligente con anteprima dei dati
- Opzione per sovrascrivere o aggiungere dati esistenti
- Import disponibile anche senza snapshot esistenti

#### Portafogli di Investimento
- Creazione di portafogli distinti (es. PAC 20 anni, Crypto, etc.)
- Orizzonte temporale e anno target per ogni portafoglio
- Colori personalizzabili
- Raggruppamento investimenti per portafoglio nella dashboard
- Portafogli collassabili nella visualizzazione

#### Obiettivi Finanziari Avanzati
- Priorità (Alta, Media, Bassa) con badge colorati
- Allocazione manuale dei fondi agli obiettivi
- Calcolo automatico del tempo rimanente
- Proiezione mensile per raggiungere l'obiettivo
- Toggle per mostrare/nascondere la proiezione mensile
- Visualizzazione obiettivi completati e in ritardo

#### Previdenza & Assicurazioni
- Tab dedicata "Previdenza" nella dashboard
- Tracciamento contributi versati vs valore attuale
- Calcolo gain/loss e performance percentuale
- Patrimonio netto con e senza previdenza

#### Sistema di Logging
- Log su file giornalieri (logs/familyfinance_YYYYMMDD.log)
- Pagina "Log di Sistema" per visualizzare i log
- Log colorati per tipo (errori, warning, import)
- Dettagli completi per debug degli errori

#### UI/UX
- Design completamente rinnovato con Tailwind CSS
- Tema Dark/Light con toggle e persistenza
- Layout responsive per mobile
- Animazioni e transizioni fluide
- Cards con effetti hover
- Gradients e colori moderni

#### Localizzazione
- Supporto completo Italiano (default) e Inglese
- Selettore lingua con bandiere nel header
- Persistenza della scelta lingua via cookie
- Traduzione di tutti i testi dell'interfaccia

### Modificato

#### Investimenti
- Aggiunto "Costo di Carico" per tracciare il prezzo di acquisto
- Calcolo automatico Gain/Loss e Performance %
- Visualizzazione con 2 decimali per le percentuali

#### Dashboard
- Patrimonio Netto come card principale
- Dettaglio "senza previdenza" e "incl. previdenza"
- Tabs per navigazione: Dashboard, Investimenti, Interessi, Previdenza, Export

#### Accounts
- Possibilità di eliminare accounts
- Migliorata la visualizzazione per categoria

### Corretto
- Risolto conflitto namespace con pagine Auth
- Risolto problema redirect dopo cambio lingua
- Risolto errore tracking Entity Framework negli obiettivi
- Risolto problema serializzazione JSON (riferimenti circolari)
- Risolto import con foreign key constraint (FamilyId)

---

## [1.0.0] - 2025-12-20

### 🚀 Release Iniziale

Prima versione dell'applicazione FamilyFinance.

### Aggiunto
- Gestione snapshots mensili del patrimonio
- Accounts con categorie (Liquidità, Previdenza, Assicurazione)
- Investimenti con nome, broker e valore
- Crediti in sospeso con stato e data prevista
- Obiettivi finanziari base
- Grafici con Chart.js (patrimonio nel tempo)
- Database SQLite locale
- Interfaccia Blazor Server

---

## Legenda

- 🎉 Major Release
- 🚀 Release
- ✨ Nuova funzionalità
- 🔧 Modifica
- 🐛 Bug fix
- ⚠️ Deprecato
- 🗑️ Rimosso


