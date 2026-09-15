# Échanges décisifs avec l'IA

Une entrée par prompt qui a compté — pas tous les échanges, seulement les décisifs.

Gabarit d'entrée :

```
## Date et sujet

- Outil / modèle si connu :
- Contexte :
- Prompt réellement utilisé :
- Réponse et hypothèses résumées :
- Décision et justification :
- Scénario ou commande de vérification :
- Résultat attendu, puis résultat observé :
- Erreur que ce contrôle pourrait détecter :
- Preuves reproductibles et limites :
```

---

## 2026-09-15 — Initialisation de la solution et de la structure du dépôt

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : démarrage du projet Bataille Navale. Contraintes du socle : API ASP.NET Core (Minimal API), front Blazor WebAssembly, bibliothèque de modèles partagée, tests, le tout en .NET 10. Choix demandé : séparer le dépôt en deux sous-dossiers, un pour le back et un pour le front.
- **Prompt réellement utilisé** : « Il faut utiliser la stack technique blazor webassembly pour le front et pour le back une api, tout en dotNET, j'ai installé le sdk d'installer donc tu peux me faire les commandes pour init un projet tu peux me faire deux sous dossier un pour le back et un pour le front. »
- **Réponse et hypothèses résumées** : proposition de créer une solution `BattleShip.slnx` avec quatre projets — `BattleShip.API`, `BattleShip.Models`, `BattleShip.Tests` dans `back/`, et `BattleShip.App` (Blazor WebAssembly) dans `front/` — avec les références de projet (`API` → `Models`, `App` → `Models`, `Tests` → `API`), un `global.json` pinnant le SDK .NET 10.0.401, et un `.gitignore` généré par `dotnet new gitignore`.
- **Décision et justification** : acceptée telle quelle. La séparation back/front demandée est respectée, et `BattleShip.Models` reste indépendant de tout autre projet, conformément à la diapo 28 du support (« le moteur reste indépendant de HTTP, JSON et gRPC »).
- **Scénario ou commande de vérification** : `dotnet build` puis `dotnet test` depuis la racine du dépôt.
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur des quatre projets et exécution du test par défaut du gabarit xUnit. Observé — `dotnet build` : 0 erreur, 0 avertissement ; `dotnet test` : 1/1 test réussi.
- **Erreur que ce contrôle pourrait détecter** : une référence de projet manquante ou incorrecte entre dossiers (`back/`/`front/`), un projet absent de la solution, ou une incompatibilité de version de SDK avec `global.json`.
- **Preuves reproductibles et limites** : commande reproductible (`dotnet build && dotnet test` depuis la racine). Lien vers le commit à ajouter après le commit initial. Limite : ce contrôle vérifie uniquement que la solution compile et que le harnais de test est fonctionnel — aucune règle métier n'existe encore à ce stade.

---

## 2026-09-15 — Implémentation du socle (moteur de jeu, API, front, gRPC, CORS)

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : après l'initialisation de la solution, mise en œuvre des contraintes du socle : moteur de jeu jouable contre l'ordinateur, contrat d'API, interface Blazor, validation FluentValidation, un échange gRPC fonctionnel, CORS entre le front et l'API, tests métier et d'intégration.
- **Prompt réellement utilisé** : « après l'initialisation de la solution, mise en œuvre des contraintes du socle : moteur de jeu jouable contre l'ordinateur, contrat d'API, interface Blazor, validation FluentValidation, un échange gRPC fonctionnel, CORS entre le front et l'API, tests métier et d'intégration. Construit le moi avec les règles classique pour le moment du jeu navale
- **Réponse et hypothèses résumées** : proposition d'un moteur de jeu indépendant des transports dans `BattleShip.Models` (grille 10x10, flotte classique à 5 navires placée aléatoirement, résolution des tirs, alternance stricte joueur/ordinateur, détection de fin de partie), un contrat DTO explicite (`Contracts/`), une API Minimal API avec FluentValidation sur les coordonnées de tir, un service gRPC `GameStats` (un échange dédié, avec un cas de succès et un cas d'erreur `NotFound`), une politique CORS nommée dérivée de la configuration, et une interface Blazor (grilles cliquables, gestion des erreurs réseau/gRPC). Hypothèse : la flotte standard et la grille 10x10 sont un choix par défaut raisonnable, documenté en ADR plutôt qu'imposé silencieusement.
- **Décision et justification** : acceptée. Le découpage respecte l'indépendance du moteur vis-à-vis de HTTP/gRPC (diapo 28, [ADR 0001](docs/adr/0001-structure-solution.md)) et les choix de règles/flotte et de périmètre gRPC sont documentés séparément ([ADR 0002](docs/adr/0002-regles-grille-flotte.md), [ADR 0003](docs/adr/0003-perimetre-grpc.md)) plutôt que justifiés uniquement ici.
- **Scénario ou commande de vérification** : `dotnet build` puis `dotnet test` depuis la racine ; lancement manuel de l'API et du front pour jouer une partie complète dans le navigateur, et appel du service gRPC `GameStats` avec un identifiant de partie valide puis invalide.
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur, tests de domaine et d'intégration HTTP au vert, partie jouable jusqu'à un tour complet dans le navigateur, statistiques gRPC visibles pour une partie existante et erreur `NotFound` pour un identifiant inconnu. Observé — `dotnet build` sur l'ensemble de la solution (Models, API, Tests, App) : 0 erreur, 0 avertissement ; `dotnet test` : 20/20 tests réussis ; partie créée et un tour joué dans le navigateur (Blazor Browser pane), avec alternance joueur/ordinateur correcte et grilles mises à jour ; bouton « Statistiques (gRPC) » retournant les bons compteurs ; appel gRPC-Web direct avec un identifiant de partie aléatoire renvoyant `StatusCode.NotFound` / « Partie inconnue. ».
- **Erreur que ce contrôle pourrait détecter** : une règle de jeu violée (chevauchement, débordement, rejeu d'une case comptant comme un nouveau coup, coup accepté après la fin de partie), un contrat d'API incohérent avec le front (désérialisation en échec), une politique CORS bloquant les appels du navigateur, ou un service gRPC ne renvoyant pas l'erreur attendue.
- **Preuves reproductibles et limites** : tests dans `back/BattleShip.Tests/Domain/` (moteur, 15 tests) et `back/BattleShip.Tests/Api/` (endpoints HTTP via `WebApplicationFactory`, 5 tests) ; vérification manuelle dans le navigateur (création de partie, tir, riposte, statistiques gRPC) ; vérification du cas d'erreur gRPC-Web via un client `Grpc.Net.Client.Web` jetable (hors dépôt, dans le répertoire de travail temporaire). Un premier blocage de build lié à `Grpc.Tools` a été rencontré puis corrigé — voir la revue dédiée dans [REVUE-IA.md](REVUE-IA.md). Limite : la vérification manuelle du navigateur a été faite en HTTP simple (le certificat de développement HTTPS n'étant pas approuvé sur cette machine) ; la configuration HTTPS par défaut du dépôt n'a pas encore été testée de bout en bout dans un navigateur, seulement par compilation.

---

## 2026-09-15 — CSS scopé par composant Blazor (.razor.css)

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : après une première implémentation du front avec tout le CSS du jeu dans `wwwroot/css/app.css`, le binôme souhaite que l'ensemble du jeu reste porté par Blazor et que le CSS soit scopé au composant (fichiers `.razor.css`) quand il ne concerne que ce composant, `app.css` étant réservé au CSS réellement global.
- **Prompt réellement utilisé** : « Le css doit être scopé dans blazor le plus possible quand il fait parti du composant (.razor.css) sinon quand c'est global on laisse dans app.css. »
- **Réponse et hypothèses résumées** : déplacement des règles propres à la grille de jeu (`.board-grid`, `.board-row`, `.board-cell` et ses états) vers `front/BattleShip.App/Components/BoardGrid.razor.css`, et des règles propres à la page de jeu (`.boards`, `.status`, `.error`, `.actions`) vers `front/BattleShip.App/Pages/Home.razor.css`. `app.css` ne conserve que le CSS du gabarit Blazor par défaut (police de base, focus, `#blazor-error-ui`, `.loading-progress`, etc.), réellement partagé par toute l'application. Précision apportée sur `wwwroot/index.html` : ce fichier est la page hôte minimale exigée par Blazor WebAssembly standalone (elle charge le runtime WASM et le point de montage `#app|`) — il ne contient aucune logique ni structure du jeu, qui reste entièrement écrite en composants Razor/C#.
- **Décision et justification** : acceptée. Le découpage par composant correspond à l'isolation CSS native de Blazor (chaque règle est automatiquement suffixée d'un attribut `b-xxxxxxxx` généré au build, qui ne s'applique qu'aux éléments rendus par ce composant) et réduit le risque de collision de classes CSS entre composants à mesure que le jeu grossira.
- **Scénario ou commande de vérification** : `dotnet build` sur `front/BattleShip.App`, puis inspection du bundle CSS généré (`obj/Debug/net10.0/scopedcss/projectbundle/BattleShip.App.bundle.scp.css`) pour confirmer la présence des règles scopées ; `dotnet build`/`dotnet test` sur l'ensemble de la solution.
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur et présence, dans le bundle scopé, des règles `.board-grid`/`.board-row`/`.board-cell` (issues de `BoardGrid.razor.css`) et `.boards`/`.status`/`.error`/`.actions` (issues de `Home.razor.css`), chacune suffixée d'un attribut de scope. Observé — `dotnet build` : 0 erreur ; le bundle généré contient bien `.board-grid[b-gdhcazz1ve]`, `.board-cell.hit[b-gdhcazz1ve]`, etc., confirmant l'isolation CSS par composant ; `dotnet test` : 20/20 tests toujours au vert (changement purement CSS, sans impact sur la logique).
- **Erreur que ce contrôle pourrait détecter** : une règle CSS mal placée qui resterait globale (non scopée) alors qu'elle ne concerne qu'un composant, ou une classe orpheline dans `app.css` qui ne serait plus utilisée nulle part.
- **Preuves reproductibles et limites** : `front/BattleShip.App/Components/BoardGrid.razor.css`, `front/BattleShip.App/Pages/Home.razor.css`, `front/BattleShip.App/wwwroot/css/app.css` (allégé). Limite : la vérification porte sur la génération du bundle CSS scopé, pas sur un rendu visuel comparé avant/après dans le navigateur — le rendu visuel n'a pas changé, seule l'organisation du CSS source a bougé.
