---
description: Pipeline completa per il rilascio di una nuova versione (Version -> Changelog -> UI -> Git)
---

Segui questa checklist rigorosa per ogni nuova release.

1. **Aggiorna Version Number**
   - [ ] Modifica `FamilyFinance/FamilyFinance.csproj`.
   - [ ] Aggiorna i tag `<Version>`, `<AssemblyVersion>`, `<FileVersion>`, `<InformationalVersion>`.
   - [ ] Esempio: `4.4.5` -> `4.5.0`.

2. **Aggiorna Changelog**
   - [ ] Apri `CHANGELOG.md`.
   - [ ] Aggiungi una nuova sezione in cima: `## [X.Y.Z] - YYYY-MM-DD`.
   - [ ] Raggruppa le modifiche in: `### Added`, `### Changed`, `### Fixed`, `### Removed`.

3. **Aggiorna Release Notes UI (Razor)**
   - [ ] Apri `FamilyFinance/Pages/ReleaseNotes.razor`.
   - [ ] Aggiungi un nuovo oggetto `ReleaseInfo` in `GetItalianReleases()`.
   - [ ] Aggiungi un nuovo oggetto `ReleaseInfo` in `GetEnglishReleases()`.
   - [ ] Assicurati che `Title`, `Emoji` e `Sections` corrispondano al Changelog.

4. **Aggiorna Documentazione (Opzionale)**
   - [ ] Se ci sono nuove feature maggiori, aggiorna la tabella Features in `README.md`.
   - [ ] Se cambia l'UI o il flusso utente, aggiorna `FamilyFinance/Pages/Help.razor`.
   - [ ] Aggiorna il numero di versione nel footer di `Help.razor`.

5. **Verifica Build**
   - [ ] Esegui `dotnet build` per assicurarti che non ci siano errori di sintassi nei file modificati.

6. **Commit & Push**
   - [ ] `git add .`
   - [ ] `git commit -m "chore: release vX.Y.Z"`
   - [ ] `git push origin master`
