# Changelog

Tutte le modifiche significative a FamilyFinance sono documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce al [Semantic Versioning](https://semver.org/lang/it/).

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

