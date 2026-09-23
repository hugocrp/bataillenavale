# Bataille Navale — C# / ASP.NET Core / Blazor WebAssembly

Projet réalisé dans le cadre du cours C# / ASP.NET (HTS Learning, Christophe MOMMER).

## Binôme

- Hugo CREPIN
- Tanguy MACE

## Stack technique

- **.NET 10** (LTS)
- **BattleShip.API** — ASP.NET Core Minimal API (back), FluentValidation, gRPC
- **BattleShip.App** — Blazor WebAssembly (front)
- **BattleShip.Models** — moteur de jeu et contrat DTO partagés entre l'API et le front
- **BattleShip.Tests** — tests xUnit (domaine + intégration API)
- **BattleShip.App.Tests** — tests de composants Blazor (bUnit)

## Prérequis

- SDK .NET 10
- Faire confiance au certificat de développement HTTPS local :

```bash
dotnet dev-certs https --trust
```

## Lancement

Depuis la racine du dépôt :

```bash
dotnet build
dotnet test
```

Lancer l'API (back), dans un terminal :

```bash
dotnet run --project back/BattleShip.API
```

Lancer le front (Blazor WebAssembly), dans un **autre** terminal :

```bash
dotnet run --project front/BattleShip.App
```

Ouvrir ensuite l'URL HTTPS affichée pour `BattleShip.App` (par défaut `https://localhost:7068`, voir `front/BattleShip.App/Properties/launchSettings.json`).

### ATTENTION AU CORS SI JAMAIS CA CHANGE SUR VOTRE MACHINE

### Démontrer l'erreur gRPC attendue

Le service `GameStats.GetStats` renvoie une erreur `NotFound` pour un identifiant de partie inconnu. Pour l'observer manuellement (API lancée) :

```bash
grpcurl -insecure -d '{"game_id": "00000000-0000-0000-0000-000000000000"}' localhost:7184 battleship.GameStats/GetStats
```