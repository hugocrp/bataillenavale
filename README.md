# Bataille Navale — C# / ASP.NET Core / Blazor WebAssembly

Projet réalisé dans le cadre du cours C# / ASP.NET (HTS Learning, Christophe MOMMER).

## Binôme

- Hugo CREPIN
- _(à compléter)_

## Stack technique

- **.NET 10** (LTS)
- **BattleShip.API** — ASP.NET Core Minimal API (back)
- **BattleShip.App** — Blazor WebAssembly (front)
- **BattleShip.Models** — bibliothèque de modèles partagée entre l'API et le front
- **BattleShip.Tests** — tests xUnit

## Structure du dépôt

```
BattleShip.slnx
global.json
back/
  BattleShip.API/       API ASP.NET Core (Minimal API)
  BattleShip.Models/    Modèles partagés (aucune dépendance)
  BattleShip.Tests/     Tests xUnit
front/
  BattleShip.App/       Application Blazor WebAssembly
docs/adr/                Décisions d'architecture (ADR)
PROMPTS.md                Échanges IA décisifs
REVUE-IA.md               Revues argumentées des propositions IA
```

## Prérequis

- [SDK .NET 10](https://dotnet.microsoft.com/download) (version pinnée dans `global.json`)
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

Lancer l'API (back) :

```bash
dotnet run --project back/BattleShip.API
```

Lancer le front (Blazor WebAssembly), dans un autre terminal :

```bash
dotnet run --project front/BattleShip.App
```

Les ports HTTPS locaux sont définis dans les `launchSettings.json` de chaque projet ; adaptez `BaseAddress` (front) et la configuration CORS (back) si vous les changez.

## Fonctionnalités

_À compléter au fur et à mesure de l'avancement du projet._

- [ ] Moteur de jeu (grilles, placement de flotte, résolution des tirs, fin de partie)
- [ ] Contrat d'API (créer une partie, connaître son état, jouer)
- [ ] Interface Blazor (créer une partie, jouer, afficher le résultat)
- [ ] Échange gRPC / gRPC-Web fonctionnel
- [ ] Validation des entrées (FluentValidation) côté HTTP et gRPC
- [ ] Tests métier et tests d'intégration

## Arbitrages du backlog et limites

_À compléter : fonctionnalités réalisées, priorités choisies, renoncements et raisons._
