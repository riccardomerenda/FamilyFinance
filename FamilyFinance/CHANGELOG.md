# Changelog

Tutte le modifiche significative a FamilyFinance sono documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce al [Semantic Versioning](https://semver.org/lang/it/).

---

## [3.3.1] - 2025-12-22 🧹 SIDEBAR CLEANUP

### 🧹 Pulizia Sidebar
- **Rimossa sezione "IMPOSTAZIONI"**: Non più necessaria con il dropdown profilo
- **"Gestione Dati" spostato in "ANALISI"**: Posizionamento più logico
- **"Membri Famiglia" solo nel dropdown**: Evita duplicazione
- **"Log di Sistema" solo nel dropdown**: Accessibile solo per Admin dal menu profilo

### 📱 Menu Mobile Aggiornato
- Rimossa sezione "Impostazioni"
- "Gestione Dati" ora in "Analisi"

### ✨ Risultato
La sidebar ora è focalizzata esclusivamente sulla navigazione dei dati finanziari:
- **Panoramica**: Dashboard, Snapshots
- **Patrimonio**: Conti, Investimenti, Interessi, Previdenza, Portafogli
- **Pianificazione**: Obiettivi, Budget, Proiezioni
- **Analisi**: Confronto, Gestione Dati

Tutto il resto (Profilo, Membri Famiglia, Log Sistema, Logout) è nel dropdown utente.

---

## [3.3.0] - 2025-12-22 👤 USER PROFILE

### 👤 Nuova Pagina Profilo Utente
Una pagina dedicata per gestire il proprio profilo con:

#### Funzionalità
- **Avatar automatico**: Iniziali colorate generate dinamicamente basate sull'ID utente
- **Modifica nome**: Aggiorna il tuo nome visualizzato
- **Cambio password**: Cambia la password di accesso con validazione
- **Info account**: Visualizza ruolo, famiglia, data iscrizione, stato account

#### Header Dropdown
- **Menu profilo nell'header**: Click sull'avatar per aprire il menu
- **Info utente visibili**: Nome, email e ruolo nel dropdown
- **Accesso rapido**: Link a Profilo e Membri Famiglia
- **Logout integrato**: Esci dall'account direttamente dal menu

### 🔧 Miglioramenti Tecnici
- Nuovi metodi in `AuthService`: `GetUserByIdAsync`, `UpdateProfileAsync`, `ChangePasswordAsync`
- Validazione lato server per nome e password
- Feedback toast per tutte le operazioni

### 🌍 Localizzazione
- 25+ nuove stringhe IT/EN per la pagina profilo

---

## [3.2.2] - 2025-12-22 🔧 ROLE CLAIMS FIX

### 🔧 Bug Fix Critico
- **Ruoli non funzionanti**: Il sistema `[Authorize(Roles = "Admin")]` non riconosceva il ruolo dell'utente
- **Causa**: Il campo `AppUser.Role` era salvato nel database ma non veniva esposto come claim ASP.NET Identity
- **Soluzione**: Aggiunto `RoleClaimsTransformation` che trasforma il ruolo utente in un claim durante l'autenticazione

### 🔐 Dettagli Tecnici
- Creato `Services/RoleClaimsTransformation.cs` che implementa `IClaimsTransformation`
- Registrato il servizio in `Program.cs`
- Ora `AuthorizeView Roles="Admin"` e `[Authorize(Roles = "Admin")]` funzionano correttamente

---

## [3.2.1] - 2025-12-22 🔐 SYSTEM LOGS IMPROVEMENTS

### 🔐 Sicurezza
- **Pagina Log solo Admin**: La pagina `/logs` ora è accessibile solo agli utenti con ruolo Admin
- **Autorizzazione sidebar**: Il link ai Log di Sistema è nascosto per gli utenti non-Admin

### 🎨 UI Log di Sistema Migliorata
- **Layout professionale**: Tabella strutturata con colonne separate per timestamp, livello, sorgente e messaggio
- **Statistiche in tempo reale**: Cards con conteggi per Info, Warning ed Error
- **Filtri avanzati**: Filtra per livello (Info/Warning/Error) e nascondi log di sistema (Microsoft.*)
- **Menu dropdown migliorato**: Sfondo e contrasto corretti per dark mode
- **Parser log intelligente**: Estrae e formatta automaticamente le componenti del log

### 🌍 Localizzazione
- Nuove stringhe IT/EN per filtri, statistiche e messaggi admin

---

## [3.2.0] - 2025-12-22 🧭 NAVIGATION REDESIGN

### 🧭 Ristrutturazione Navigazione

Major refactoring dell'architettura di navigazione: i tab della dashboard sono ora pagine separate con URL dedicati.

#### Nuove Pagine
- **`/investments`** - Investments.razor: Panoramica completa del portafoglio investimenti
- **`/interests`** - Interests.razor: Interessi accumulati con grafici e dettagli per conto
- **`/pension`** - Pension.razor: Fondi pensione e polizze assicurative
- **`/data`** - DataManagement.razor: Import/Export dati consolidato

#### Dashboard Semplificata
- **Index.razor ripulito**: Rimossi tutti i tab, resta solo la dashboard principale
- **Cards cliccabili**: Liquidità, Investimenti, Previdenza, Interessi ora sono link alle rispettive pagine
- **Quick Actions aggiornate**: Link alle nuove pagine

#### Sidebar Riorganizzata con Gruppi Logici
- **Panoramica**: Dashboard, Snapshots
- **Patrimonio**: Conti, Investimenti, Interessi, Previdenza, Portafogli
- **Pianificazione**: Obiettivi, Budget, Proiezioni
- **Analisi**: Confronto
- **Impostazioni**: Membri Famiglia, Gestione Dati, Log Sistema

#### Vantaggi
- **URL diretti**: Ogni sezione ha un URL bookmarkable/condivisibile
- **Navigazione chiara**: Niente più confusione tra tab e pagine
- **Struttura logica**: Menu organizzato per funzione
- **Coerenza UX**: Stesso pattern di navigazione ovunque

### 🌍 Localizzazione

#### 50+ nuove stringhe (IT/EN)
- Navigation groups: `NavOverview`, `NavAssets`, `NavPlanning`, `NavAnalysis`, `NavSettings`
- Page titles & descriptions: `InvestmentsPageDesc`, `InterestsPageDesc`, `PensionInsurancePageDesc`
- Empty states: `NoInvestmentsYet`, `NoInterestAccountsYet`, `NoPensionInsuranceYet`
- Actions: `ManagePortfolios`, `ManageAccounts`
- Interests: `TotalInterests`, `InterestsByAccount`, `HighestInterest`, `OfTotal`
- Investments: `AssetAllocation`, `PortfolioBreakdown`, `TotalGainLoss`, `OverallReturn`
- Pension: `ContributionsVsValue`, `TotalContributions`, `PensionPlusInsurance`

### 📊 Design Bento Grid

Tutte le nuove pagine seguono il design Bento Grid introdotto in v3.0:
- Hero cards con gradienti per metriche principali
- Cards statistiche con icone e animazioni stagger
- Grafici Chart.js integrati
- Layout responsive 12 colonne

---

## [3.1.0] - 2025-12-22 🎨 PAGE REDESIGN

### 🎨 Redesign Completo Pagine Secondarie

Applicato il nuovo design Bento Grid e glassmorphism a tutte le pagine dell'applicazione.

#### Pagine Aggiornate
- **Goals.razor**: Bento cards con animazioni stagger, hero card per statistiche
- **Budget.razor**: Grid layout con cards categoria, modal glassmorphism
- **Portfolios.razor**: Cards con color accent bar e hover effects
- **Snapshots.razor**: Lista moderna con group hover e paginazione migliorata
- **Accounts.razor**: Grid cards con categoria colors e icon abbreviations
- **Compare.razor**: Summary cards con hero gradient e variazioni evidenziate
- **Projections.razor**: Cards statistiche con icone e trend indicators
- **Privacy.razor**: Content card con bento-card styling
- **Logs.razor**: Console moderna con styling aggiornato

#### Miglioramenti UI Comuni
- **Animazioni stagger**: `animate-in delay-1` a `delay-5` su tutte le pagine
- **Font .money**: Monospace per tutti i valori monetari
- **Group hover effects**: Bottoni azione visibili solo su hover
- **Modal glassmorphism**: Backdrop blur e trasparenze su tutti i modal
- **Header consistenti**: Titolo 3xl con descrizione su ogni pagina

### 📚 Documentazione
- **README aggiornato**: Nuova sezione "Modern UI Design (v3.0)"
- **Tech Stack esteso**: Aggiunti Serilog e Outfit Font
- **Roadmap aggiornato**: Mobile responsive e UI redesign completati

### 🌍 Localizzazione
- 7 nuove stringhe: `GoalsDescription`, `AccountsDescription`, `PortfoliosDescription`, `CompareDescription`, `ProjectionsDescription`, `NoGoalsHint`, pattern per descrizioni pagine

---

## [3.0.1] - 2025-12-22

### 🐛 Hotfix Critico

#### Goal.Id Type Mismatch
- **Problema**: L'app in produzione crashava con `OverflowException` durante la lettura degli obiettivi
- **Causa**: Goal.Id era stato cambiato da `long` a `int`, ma i dati esistenti contenevano ID molto grandi (timestamp in millisecondi)
- **Soluzione**: Ripristinato `Goal.Id` a tipo `long` per compatibilità con dati esistenti

#### File Modificati
- `Goal.cs`: `Id` torna a `long`
- `IGoalService.cs`: Parametri `long id`
- `GoalService.cs`: Parametri `long id`
- `FinanceService.cs`: `DeleteGoalAsync(long id)`
- `Goals.razor`: `Del(long id)`
- Test aggiornati per tipo `long`

---

## [3.0.0] - 2025-12-22 🎨 UI REDESIGN

### 🎨 Redesign Completo Dashboard

Major release con redesign completo dell'interfaccia utente per una UX moderna e distintiva.

#### Layout Bento Grid
- **Nuova dashboard** con layout Bento Grid stile Apple/Linear
- **Hero card** prominente per Patrimonio Netto con gradient viola
- **Cards ottimizzate** per Liquidità e Investimenti con metriche chiare
- **Widget Goals** integrato nella dashboard con progress bars animate
- **Quick Actions** per accesso rapido a funzionalità chiave
- **Grid responsive** 12 colonne che si adatta a mobile/tablet/desktop

#### Nuova Tipografia
- **Font Outfit**: Sostituisce Inter per un look più distintivo e moderno
- **Font JetBrains Mono**: Per valori monetari con classe `.money`
- **Gerarchia tipografica** migliorata con pesi font da 300 a 900

#### Effetti Visivi
- **Glassmorphism**: Cards con backdrop-filter blur e bordi trasparenti
- **Mesh gradient background**: Sfondo con gradienti radiali sfumati
- **Glow effects**: Ombre colorate per elementi hero
- **Animazioni stagger**: Fade-in progressivo degli elementi
- **Progress bar animate**: Barre di avanzamento con animazione CSS

#### Sidebar Rinnovata
- **Effetto glass**: Sidebar con blur e trasparenza
- **Logo più grande**: Icona 48px con shadow glow
- **Nav items migliorati**: Hover con gradient e translateX
- **Active state**: Indicator laterale con gradient + glow shadow
- **Transizioni fluide**: Cubic-bezier per animazioni smooth

#### Nuove Classi CSS
- `.bento-card`: Card base con glassmorphism e hover effect
- `.hero-gradient`: Gradient viola per card principali  
- `.mesh-bg`: Background con gradienti radiali
- `.money`: Font mono per valori monetari
- `.animate-in`, `.delay-1` a `.delay-5`: Animazioni stagger
- `.glow-primary`, `.glow-emerald`, `.glow-amber`: Effetti glow

### 🌍 Localizzazione

#### Nuove stringhe Demo Mode (IT/EN)
- `DemoMode`: "Modalità Demo" / "Demo Mode"
- `DemoModeDescription`: Descrizione modalità demo
- `DemoModeCredentials`: Istruzioni credenziali demo
- `RequestAccess`: "Richiedi Accesso" / "Request Access"
- `InterestedPersonalVersion`: Call-to-action versione personale
- `RequestAccessLinkedIn`: Link LinkedIn

### 🐛 Bug Fix
- **Localizzazione banner demo**: Testi hardcoded sostituiti con risorse localizzate

---

## [2.9.1] - 2025-12-22

### 🐛 Bug Fix & Quick Wins

#### Warning Compiler Risolti
- **4 warning CS8602** in `Index.razor`: Risolti warning di null-reference con operatore `!` nelle linee 305, 352, 450, 469
- Codice più pulito, zero warning in compilazione

#### Paginazione Snapshots
- **Nuova UI paginazione** nella pagina Storico Snapshots
- **Selezione elementi per pagina**: 10, 25, 50, 100
- **Navigazione intelligente**: Prima, Precedente, numeri pagina, Successivo, Ultima
- **Ellissi dinamiche**: Per liste lunghe (>7 pagine)
- **Info pagina**: "Visualizzati 1-12 di 48"
- **Responsive**: Layout adattivo mobile/desktop
- **Toast feedback**: Notifica successo/errore su eliminazione

### 🌍 Localizzazione
- 4 nuove stringhe: `Show`, `PerPage`, `Showing`, `Of` (IT/EN)

---

## [2.9.0] - 2025-12-22

### 🏗️ Architettura Enterprise

Rilascio importante con miglioramenti architetturali per robustezza e manutenibilità.

### ✅ Validazione Input

- **Validatori Entity**: `EntityValidators.cs` con metodi di validazione per tutte le entità
- **Goal**: Validazione nome (max 100 char), target (>0, <100M), allocated amount, deadline
- **Account**: Validazione nome, proprietario, categoria
- **Portfolio**: Validazione nome, orizzonte temporale (0-50 anni), anno target
- **BudgetCategory**: Validazione nome (max 50 char), budget mensile (>0, <1M)
- **InvestmentAsset**: Validazione nome, valore, costo di carico
- **Receivable**: Validazione descrizione, importo
- **ServiceResult**: Classe wrapper per risultati operazioni con errori strutturati

### 🔔 Gestione Errori UI

- **Toast Notifications**: Integrazione completa NotificationService in tutte le pagine
- **Messaggi Localizzati**: 18 nuove stringhe per feedback utente (IT/EN)
- **Success/Error/Warning/Info**: 4 tipi di notifiche con stili differenti
- **Auto-dismiss**: Notifiche si chiudono dopo 5 secondi

### 📝 Logging Strutturato (Serilog)

- **Serilog.AspNetCore**: Logging strutturato con sink Console e File
- **Log giornalieri**: File in `logs/familyfinance_YYYY-MM-DD.log`
- **Retention**: 30 giorni di log conservati
- **Enrichers**: MachineName, ThreadId, SourceContext
- **Log Levels**: Override per Microsoft.AspNetCore e EF Core (Warning)
- **Structured Logging**: Template con SourceContext per tracciabilità

### 🗑️ Soft Delete

- **Campo IsDeleted**: Aggiunto a tutte le entità principali
- **DeletedAt**: Timestamp della cancellazione
- **DeletedBy**: User ID che ha eseguito la cancellazione
- **Query Filter**: Tutte le query escludono automaticamente record cancellati
- **Recuperabilità**: Dati non persi, solo marcati come eliminati

### 📊 Audit Trail

- **CreatedAt/UpdatedAt**: Timestamp creazione/modifica su tutte le entità
- **CreatedBy/UpdatedBy**: User ID per tracciabilità modifiche
- **Automatico**: Popolato automaticamente dai servizi
- **Storico completo**: Chi ha modificato cosa e quando

### 🧪 Testing

- **+13 nuovi test**: Da 28 a 41 test totali
- **SnapshotServiceTests**: 12 test per SnapshotService
- **Test Soft Delete**: Verifica che le entità cancellate siano escluse
- **Test Validazione**: Verifica errori su input invalidi
- **100% Pass Rate**: Tutti i test passano

### 📦 Database

- **Migrazione AuditTrailAndSoftDelete**: Nuovi campi su tutte le tabelle
- **Indici**: Ottimizzati per query con soft delete

### 📚 Dipendenze

- `Serilog.AspNetCore` 8.0.3
- `Serilog.Enrichers.Environment` 3.0.1
- `Serilog.Enrichers.Thread` 4.0.0
- `Serilog.Sinks.Console` 6.0.0
- `Serilog.Sinks.File` 6.0.0

---

## [2.8.0] - 2025-12-22

### 🔧 Bug Fix & Performance

Rilascio importante con correzioni di bug critici, miglioramenti di performance e nuove funzionalità.

### 🐛 Bug Fix Critici

#### Goal.Id - Collisione Timestamp
- **Problema**: L'ID degli obiettivi usava `DateTimeOffset.ToUnixTimeMilliseconds()`, rischiando collisioni se si creavano 2 goal nello stesso millisecondo
- **Soluzione**: Cambiato da `long` timestamp a `int` auto-increment gestito dal database
- **File modificati**: `Goal.cs`, `IGoalService.cs`, `GoalService.cs`, `Goals.razor`

#### Goal.Deadline - Tipo Non Tipizzato
- **Problema**: Il campo Deadline era una stringa "2026-12", difficile da validare e confrontare
- **Soluzione**: Convertito a `DateOnly?` con helper properties `DeadlineDisplay` e `MonthsUntilDeadline`
- **Backward compatible**: Import/Export gestisce automaticamente la conversione

### 🔐 Sicurezza

#### Password Policy Rafforzata
- Lunghezza minima: **8 caratteri** (era 6)
- Richiede almeno **1 numero**
- Richiede almeno **4 caratteri unici**
- Lockout aumentato a **15 minuti** (era 5)

### ⚡ Performance

#### Query Dashboard Ottimizzate
- **Problema**: Query N+1 per costruire grafici - ogni snapshot richiedeva una query separata
- **Soluzione**: Nuovo metodo `GetAllWithTotalsAsync()` con singola query SQL projection
- **Beneficio**: Dashboard 5-10x più veloce con molti snapshot
- **File modificati**: `ISnapshotService.cs`, `SnapshotService.cs`, `Index.razor`, `Projections.razor`

#### Indici Database
- Aggiunti indici su `FamilyId` per: Accounts, Goals, Portfolios, BudgetCategories
- Indice composito su `Snapshots(FamilyId, SnapshotDate)`
- Miglioramento query per dataset grandi

### ✨ Nuove Funzionalità

#### Sistema Notifiche Toast
- Nuovo servizio `NotificationService` per feedback utente
- Componente `ToastContainer` con animazioni CSS
- 4 tipi: Success, Error, Warning, Info
- Auto-dismiss dopo 5 secondi
- Integrato in Goals e funzioni Export

### 🧪 Testing

- **3 nuovi test** per `Goal.MonthsUntilDeadline`, `Goal.DeadlineDisplay`
- **28 test totali** - tutti passati
- Aggiornati test esistenti per nuovi tipi

### 📦 Database Migration

- Nuova migrazione `GoalIdAndDeadlineRefactor`
- Conversione automatica dei dati esistenti
- Indici creati automaticamente

---

## [2.7.3] - 2025-12-21

### 🐛 Bug Fix - Privacy Page Navigation

Il link in fondo alla pagina Privacy ora è contestuale.

### Corretto
- **Link dinamico**: Se l'utente è loggato, mostra "Torna all'App" (→ Dashboard)
- **Link dinamico**: Se non loggato, mostra "Torna al Login" (→ Login page)

### Aggiunto
- Nuova stringa localizzata `BackToApp` (IT/EN)

---

## [2.7.2] - 2025-12-21

### 🌍 Localization - Privacy Policy

La Privacy Policy ora è completamente localizzata in italiano e inglese.

### Aggiunto
- **35+ nuove stringhe localizzate** per la Privacy Policy
- Tutte le sezioni tradotte: Dati Raccolti, Finalità, Cookie, Sicurezza, Diritti GDPR
- Privacy Policy disponibile nella lingua selezionata dall'utente

### Modificato
- `Privacy.razor` ora utilizza il sistema di localizzazione esistente
- `SharedResource.resx` e `SharedResource.en.resx` aggiornati

---

## [2.7.1] - 2025-12-21

### 📜 Privacy Policy

Aggiunta pagina Privacy Policy per conformità GDPR.

### Aggiunto
- **Pagina Privacy Policy** (`/privacy`) con informativa completa
- Dettaglio cookie tecnici utilizzati (solo essenziali)
- Informazioni sui dati raccolti e finalità
- Diritti GDPR dell'utente
- Nota sulla modalità demo
- Link al codice open-source

### UI
- Link Privacy Policy nel footer della pagina login
- Link Privacy Policy nella sidebar dell'app

---

## [2.7.0] - 2025-12-21

### 🎮 Demo Mode

Nuova modalità demo per mostrare l'applicazione senza permettere registrazioni pubbliche.

### Aggiunto

#### Account Demo
- **Utente demo** pre-configurato con dati realistici di esempio
- **Credenziali demo**: `demo@familyfinance.app` / `demo2024`
- **Seed automatico** in produzione con 6 mesi di dati storici

#### Dati Demo Inclusi
- 4 conti (corrente, deposito, pensione, assicurazione)
- 3 portafogli investimenti (PAC, Crypto, Dividendi)
- 5 obiettivi finanziari con progressi diversi
- 6 categorie budget con spese mensili
- 6 snapshot storici con trend di crescita

#### UI Demo Mode
- **Banner demo** nella pagina di login con credenziali
- **Banner persistente** nell'app per utenti demo
- **Pulsante "Richiedi Accesso"** con link a LinkedIn
- **Link al repository GitHub** nel footer

#### Sicurezza
- Registrazione pubblica disabilitata in produzione
- Pagina Setup accessibile solo in development
- Form pre-compilato con credenziali demo

---

## [2.6.1] - 2025-12-21

### 🐛 Bug Fixes

#### Mobile Navigation
- **Menu si chiude al click**: Il menu mobile ora si chiude automaticamente quando si seleziona una voce di navigazione
- **Icona dinamica**: L'icona hamburger cambia in X quando il menu è aperto
- **Refactoring**: Sostituito JavaScript inline con Blazor state management

#### Session Persistence
- **Sessioni persistenti**: Gli utenti rimangono loggati dopo i deploy
- **Data Protection Keys**: Le chiavi di crittografia ora sono salvate su volume persistente (`/app/data/keys`)
- **Niente più logout forzati** ad ogni riavvio del container

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


