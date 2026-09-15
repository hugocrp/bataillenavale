# Bataille Navale — C# / ASP.NET Core / Blazor WebAssembly

Projet réalisé dans le cadre du cours C# / ASP.NET (HTS Learning, Christophe MOMMER).

## Binôme

- Hugo CREPIN
- _(à compléter)_

## Stack technique

- **.NET 10** (LTS)
- **BattleShip.API** — ASP.NET Core Minimal API (back), FluentValidation, gRPC
- **BattleShip.App** — Blazor WebAssembly (front)
- **BattleShip.Models** — moteur de jeu et contrat DTO partagés entre l'API et le front
- **BattleShip.Tests** — tests xUnit (domaine + intégration API)
- **BattleShip.App.Tests** — tests de composants Blazor (bUnit)

## Structure du dépôt

```
BattleShip.slnx
global.json
Protos/battleship.proto     Contrat gRPC partagé (API + front)
back/
  BattleShip.API/            API ASP.NET Core (Minimal API, CORS, gRPC)
  BattleShip.Models/         Moteur de jeu (Board, Ship, Game...) et DTOs (Contracts/)
  BattleShip.Tests/          Tests xUnit (Domain/, Api/)
front/
  BattleShip.App/             Application Blazor WebAssembly
  BattleShip.App.Tests/       Tests de composants Blazor (bUnit)
docs/adr/                     Décisions d'architecture (ADR)
PROMPTS.md                     Échanges IA décisifs
REVUE-IA.md                    Revues argumentées des propositions IA
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

Lancer l'API (back), dans un terminal :

```bash
dotnet run --project back/BattleShip.API
```

Lancer le front (Blazor WebAssembly), dans un **autre** terminal :

```bash
dotnet run --project front/BattleShip.App
```

Ouvrir ensuite l'URL HTTPS affichée pour `BattleShip.App` (par défaut `https://localhost:7068`, voir `front/BattleShip.App/Properties/launchSettings.json`).

### Ports et CORS

- L'API écoute par défaut sur `https://localhost:7184` (voir `back/BattleShip.API/Properties/launchSettings.json`).
- Le front lit l'URL de l'API dans `front/BattleShip.App/wwwroot/appsettings.json` (clé `ApiBaseUrl`).
- L'API autorise en CORS les origines listées dans `back/BattleShip.API/appsettings.json` (clé `Cors:AllowedOrigins`), pré-remplies avec les ports par défaut du front.

Si les ports générés localement diffèrent (Visual Studio/Rider en attribuent parfois d'autres), mettre à jour ces deux fichiers de configuration en conséquence.

### Jouer une partie

1. Cliquer sur **Nouvelle partie**.
2. Cliquer sur une case de la grille « Repérage de l'adversaire » pour tirer ; l'ordinateur riposte automatiquement si la partie continue.
3. La partie se termine quand une flotte est entièrement coulée (message de victoire ou défaite affiché).
4. Le bouton **Statistiques (gRPC)** interroge le service gRPC-Web `GameStats` (navires restants, tirs joués) — voir [ADR 0003](docs/adr/0003-perimetre-grpc.md).

### Démontrer l'erreur gRPC attendue

Le service `GameStats.GetStats` renvoie une erreur `NotFound` pour un identifiant de partie inconnu. Pour l'observer manuellement (API lancée) :

```bash
grpcurl -insecure -d '{"game_id": "00000000-0000-0000-0000-000000000000"}' localhost:7184 battleship.GameStats/GetStats
```

## Fonctionnalités

- [x] Moteur de jeu (grille 10x10, placement aléatoire ou manuel de la flotte, résolution des tirs, fin de partie) — [back/BattleShip.Models](back/BattleShip.Models)
- [x] Contrat d'API (créer une partie — flotte aléatoire ou manuelle, difficulté au choix, mode Tempête —, lister l'historique, consulter les statistiques globales, connaître l'état d'une partie, jouer un tir) — `GET /fleet`, `POST /games`, `GET /games`, `GET /stats`, `GET /games/{id}`, `POST /games/{id}/shots`
- [x] Interface Blazor (créer une partie, choisir la difficulté, placer sa flotte manuellement en option, activer le mode Tempête, jouer, consulter l'historique et les statistiques globales, afficher le résultat) — [front/BattleShip.App/Pages/Home.razor](front/BattleShip.App/Pages/Home.razor), [front/BattleShip.App/Components/FleetSetup.razor](front/BattleShip.App/Components/FleetSetup.razor)
- [x] Échange gRPC / gRPC-Web fonctionnel (statistiques de partie, succès + erreur `NotFound`)
- [x] Validation des entrées (FluentValidation sur les coordonnées de tir, la flotte manuelle, la difficulté et l'incompatibilité mode Tempête / flotte manuelle)
- [x] Trois niveaux de difficulté pour l'ordinateur (aléatoire / chasse puis cible / probabiliste) — voir [ADR 0004](docs/adr/0004-algorithme-adversaire.md) et [ADR 0007](docs/adr/0007-mode-tempete-ia-experte-stats.md)
- [x] Historique des parties (statut, difficulté, mode Tempête, date) — `GET /games`
- [x] Mode Tempête (règles maison, activable par case à cocher) : grille 12×12, flotte élargie à 8 navires, 3 mines qui font sauter le tour de qui les déclenche — voir [ADR 0007](docs/adr/0007-mode-tempete-ia-experte-stats.md)
- [x] Statistiques globales (nombre de parties, victoires/défaites, moyenne de tirs pour terminer une partie) — `GET /stats`
- [x] Tests métier (moteur de jeu), tests d'intégration (endpoints HTTP et gRPC), tests de composants Blazor (bUnit) — 134 tests au total
- [ ] Extensions au-delà du socle (à définir dans le backlog)

## Arbitrages du backlog et limites

- Le stockage des parties est **en mémoire** (`GameStore`, `ConcurrentDictionary`) : les parties sont perdues au redémarrage de l'API. Aucune persistance n'a été demandée par le socle. Un service en arrière-plan (`GameCleanupService`) supprime automatiquement les parties inactives depuis plus de 30 minutes, pour éviter une accumulation indéfinie en mémoire — l'historique (`GET /games`) et les statistiques globales (`GET /stats`) ne remontent donc que sur cette fenêtre de 30 minutes.
- La flotte standard reste 5 navires classiques sur une grille 10x10 fixe — voir [ADR 0002](docs/adr/0002-regles-grille-flotte.md). Le mode Tempête (optionnel) porte la grille à 12×12 et la flotte à 8 navires, avec 3 mines — voir [ADR 0007](docs/adr/0007-mode-tempete-ia-experte-stats.md). Le mode Tempête est incompatible avec le placement manuel de la flotte (rejeté par la validation, 400).
- L'ordinateur propose trois niveaux : **Facile** (tir aléatoire, `RandomComputerStrategy`), **Difficile** (« chasse puis cible », `HuntTargetComputerStrategy`, tir en damier puis ciblage des cases adjacentes après un tir touché) et **Experte** (`ExpertComputerStrategy`, ciblage par alignement de touches puis choix par densité de probabilité sur les placements de navires restants) — voir [ADR 0004](docs/adr/0004-algorithme-adversaire.md) et [ADR 0007](docs/adr/0007-mode-tempete-ia-experte-stats.md).
- Les statistiques globales (`GET /stats`) sont recalculées à chaque appel à partir des parties en mémoire, sans compteur persistant séparé — cohérent avec le choix de ne pas dupliquer `GameStore` comme source de vérité.
- gRPC est limité à un échange dédié (statistiques) plutôt qu'à toute la boucle de jeu — voir [ADR 0003](docs/adr/0003-perimetre-grpc.md).
