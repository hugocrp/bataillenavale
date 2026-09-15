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

## 15/09/2026, Initialisation de la solution et de la structure du dépôt

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

## 15/09/2026, Implémentation du socle (moteur de jeu, API, front, gRPC, CORS)

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

## 15/09/2026, CSS scopé par composant Blazor (.razor.css)

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : après une première implémentation du front avec tout le CSS du jeu dans `wwwroot/css/app.css`, le binôme souhaite que l'ensemble du jeu reste porté par Blazor et que le CSS soit scopé au composant (fichiers `.razor.css`) quand il ne concerne que ce composant, `app.css` étant réservé au CSS réellement global.
- **Prompt réellement utilisé** : « Le css doit être scopé dans blazor le plus possible quand il fait parti du composant (.razor.css) sinon quand c'est global on laisse dans app.css. »
- **Réponse et hypothèses résumées** : déplacement des règles propres à la grille de jeu (`.board-grid`, `.board-row`, `.board-cell` et ses états) vers `front/BattleShip.App/Components/BoardGrid.razor.css`, et des règles propres à la page de jeu (`.boards`, `.status`, `.error`, `.actions`) vers `front/BattleShip.App/Pages/Home.razor.css`. `app.css` ne conserve que le CSS du gabarit Blazor par défaut (police de base, focus, `#blazor-error-ui`, `.loading-progress`, etc.), réellement partagé par toute l'application. Précision apportée sur `wwwroot/index.html` : ce fichier est la page hôte minimale exigée par Blazor WebAssembly standalone (elle charge le runtime WASM et le point de montage `#app|`) — il ne contient aucune logique ni structure du jeu, qui reste entièrement écrite en composants Razor/C#.
- **Décision et justification** : acceptée. Le découpage par composant correspond à l'isolation CSS native de Blazor (chaque règle est automatiquement suffixée d'un attribut `b-xxxxxxxx` généré au build, qui ne s'applique qu'aux éléments rendus par ce composant) et réduit le risque de collision de classes CSS entre composants à mesure que le jeu grossira.
- **Scénario ou commande de vérification** : `dotnet build` sur `front/BattleShip.App`, puis inspection du bundle CSS généré (`obj/Debug/net10.0/scopedcss/projectbundle/BattleShip.App.bundle.scp.css`) pour confirmer la présence des règles scopées ; `dotnet build`/`dotnet test` sur l'ensemble de la solution.
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur et présence, dans le bundle scopé, des règles `.board-grid`/`.board-row`/`.board-cell` (issues de `BoardGrid.razor.css`) et `.boards`/`.status`/`.error`/`.actions` (issues de `Home.razor.css`), chacune suffixée d'un attribut de scope. Observé — `dotnet build` : 0 erreur ; le bundle généré contient bien `.board-grid[b-gdhcazz1ve]`, `.board-cell.hit[b-gdhcazz1ve]`, etc., confirmant l'isolation CSS par composant ; `dotnet test` : 20/20 tests toujours au vert (changement purement CSS, sans impact sur la logique).
- **Erreur que ce contrôle pourrait détecter** : une règle CSS mal placée qui resterait globale (non scopée) alors qu'elle ne concerne qu'un composant, ou une classe orpheline dans `app.css` qui ne serait plus utilisée nulle part.
- **Preuves reproductibles et limites** : `front/BattleShip.App/Components/BoardGrid.razor.css`, `front/BattleShip.App/Pages/Home.razor.css`, `front/BattleShip.App/wwwroot/css/app.css` (allégé). Limite : la vérification porte sur la génération du bundle CSS scopé, pas sur un rendu visuel comparé avant/après dans le navigateur — le rendu visuel n'a pas changé, seule l'organisation du CSS source a bougé.

---

## 15/09/2026 

- **Outil/modèle : Claude Code (Claude Sonnet 5), LI Claude Code
- **Contexte** : 
- **Prompt** : Avant de continuer, est-ce que le contrat d'API est cohérent partout (HTTP et gRPC) ? Vérifie (documentation OpenAPI générée, réponses d'erreur, ce que le front suppose côté chaînes de caractères), corrige ce qui doit l'être, et fais-moi un point clair sur ce qui reste discutable. »

---

## 15/09/2026, Tests des composants Blazor (bUnit)

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : le moteur de jeu et l'API avaient une bonne couverture de tests, mais aucun test n'exerçait le front lui-même — `BoardGrid`, `FleetSetup` et `Home.razor` n'étaient vérifiés que manuellement dans le navigateur.
- **Prompt réellement utilisé** : « Il faut faire des tests sur le composant Blazor lui même et sur le front pour couvrir le maximum de code et de cas. »
- **Réponse et hypothèses résumées** : création d'un projet de tests dédié `front/BattleShip.App.Tests` (xUnit + bUnit), séparé de `back/BattleShip.Tests` pour éviter une collision de types gRPC (les deux projets front et back génèrent chacun leur propre `BattleShip.API.Grpc.*` à partir du même `.proto`, avec des namespaces identiques mais des assemblies différentes — vérifié en pratique lors d'une tentative infructueuse de tout mettre dans le même projet). Introduction d'une interface `IGameStatsClient` pour permettre de substituer le client gRPC-Web dans les tests de `Home.razor` (le protocole gRPC-Web n'est pas simulable simplement, contrairement au JSON de `GameApiClient` qui est testé avec un vrai `HttpClient` branché sur un handler factice). Décision documentée dans [ADR 0005](docs/adr/0005-tests-composants-blazor.md).
- **Décision et justification** : acceptée. `IGameStatsClient` est la première interface introduite côté front, mais justifiée par un besoin réel de substitution en test, pas une abstraction préventive.
- **Scénario ou commande de vérification** : `dotnet build`/`dotnet test` sur l'ensemble de la solution ; vérification manuelle dans le navigateur que l'application fonctionne toujours à l'identique après l'introduction de `IGameStatsClient` (partie jouée, statistiques gRPC affichées).
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur, nouveaux tests de composants au vert, aucune régression sur les tests existants, application inchangée en conditions réelles. Observé — `dotnet build` : 0 erreur ; `dotnet test` : 64/64 tests réussis (36 dans `back/BattleShip.Tests`, 28 nouveaux dans `front/BattleShip.App.Tests` : 12 sur `BoardGrid`, 8 sur `FleetSetup`, 8 sur `Home.razor`) ; partie jouée et statistiques gRPC vérifiées dans le navigateur après le refactor.
- **Erreur que ce contrôle pourrait détecter** : un état de case mal calculé ou mal désactivé dans `BoardGrid`, une validation de placement (chevauchement, débordement) qui laisserait passer un cas invalide dans `FleetSetup`, ou une régression dans la gestion des erreurs HTTP/gRPC de `Home.razor` (message d'erreur qui ne s'affiche plus, état qui ne se met plus à jour après un tir).
- **Preuves reproductibles et liens vers les commits** : `front/BattleShip.App.Tests/Components/BoardGridTests.cs`, `front/BattleShip.App.Tests/Components/FleetSetupTests.cs`, `front/BattleShip.App.Tests/Pages/HomeTests.cs`.
- **Limites et points non vérifiés** : les tests de `Home.razor` valident la logique d'orchestration (appels, mise à jour d'état, affichage) avec des réponses HTTP/gRPC simulées ; ils ne remplacent pas la vérification de bout en bout déjà faite manuellement dans le navigateur (sérialisation réelle, CORS, gRPC-Web réel). Le rendu mobile des composants n'est pas couvert par ces tests.
-

---

## 15/09/2026, Nettoyage des parties inactives dans GameStore

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : `GameStore` (stockage en mémoire des parties) n'avait aucun mécanisme de suppression : les parties terminées ou abandonnées restaient indéfiniment, une fuite mémoire sur un serveur qui tourne longtemps.
- **Prompt réellement utilisé** : « il y'a une fuite de mémoire sur GameStore il me semble, il nettoie jamais les parties terminées / abandonnées.. corrige moi ça. »
- **Réponse et hypothèses résumées** : ajout d'une date de dernière activité par partie (mise à jour à chaque `TryGet`, donc à chaque `GET /games/{id}` ou tir), d'une méthode `RemoveExpired(TimeSpan)` purgeant les parties inactives au-delà d'un délai (30 minutes), et d'un `GameCleanupService` (`BackgroundService`) qui l'appelle toutes les 5 minutes. Utilisation de `TimeProvider` (injecté, `TimeProvider.System` en production) plutôt que `DateTimeOffset.UtcNow` directement, pour rendre le mécanisme testable sans attendre du vrai temps — cohérent avec la diapo 33 du support qui enregistre déjà `TimeProvider.System` en singleton pour la même raison.
- **Décision et justification** : acceptée. Un seul délai d'inactivité couvre à la fois les parties terminées jamais consultées à nouveau et les parties abandonnées en cours — pas besoin de distinguer les deux cas, une partie qu'on ne consulte plus (terminée ou non) doit finir par disparaître.
- **Scénario ou commande de vérification** : tests unitaires avec `Microsoft.Extensions.Time.Testing.FakeTimeProvider` (avance le temps simulé sans attendre) ; `dotnet build`/`dotnet test` sur l'ensemble de la solution ; démarrage réel de l'API pour vérifier l'absence d'exception au lancement du `BackgroundService`.
- **Résultat attendu, puis résultat observé** : attendu — une partie inactive depuis plus de 30 minutes est supprimée, une partie encore consultée dans ce délai ne l'est pas, un accès (`TryGet`) repousse l'expiration. Observé — `dotnet test` : 68/68 tests réussis (4 nouveaux dans `GameStoreTests.cs`) ; démarrage de l'API sans exception, `GameCleanupService` actif.
- **Erreur que ce contrôle pourrait détecter** : une partie supprimée alors qu'elle est encore active (délai mal calculé), une partie qui ne serait jamais supprimée (bug dans `RemoveExpired`), ou une expiration qui ignorerait les accès récents (`TryGet` qui ne rafraîchirait pas l'activité).
- **Preuves reproductibles et liens vers les commits** : `back/BattleShip.Tests/Api/GameStoreTests.cs` ; `back/BattleShip.API/GameStore.cs`, `back/BattleShip.API/GameCleanupService.cs`.
- **Limites et points non vérifiés** : le déclenchement réel du `BackgroundService` après 5 minutes n'a pas été observé en conditions réelles (seul son démarrage sans erreur a été vérifié) ; la logique de purge elle-même est testée de façon déterministe via `FakeTimeProvider`, ce qui est la vérification pertinente pour cette partie du code.

---

## 15/09/2026, Niveau de difficulté « Facile » et historique des parties

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Prompt réellement utilisé** : « réintroduit moi le niveau de difficulté facile (random) fait un historique de partie »
- **Réponse et hypothèses résumées** : réintroduction de `RandomComputerStrategy` (supprimée précédemment car inutilisée) comme niveau « Facile », centralisée avec `HuntTargetComputerStrategy` dans `ComputerStrategyFactory.Create(ComputerDifficulty, Random)` ; `CreateGameRequest` accepte un champ `Difficulty` optionnel (`null` = comportement précédent, difficile) ; `GameStore` associe la difficulté choisie à chaque partie, à côté de la date de dernière activité déjà suivie pour le nettoyage ; nouvel endpoint `GET /games` exposant l'historique (identifiant, statut, difficulté, dernière activité) ; côté front, une bascule Facile/Difficile et une section « Parties précédentes » sur l'écran d'accueil. Décision documentée dans [ADR 0006](docs/adr/0006-difficulte-et-historique.md). La troisième demande (fonctionnalité différenciante) a été traitée à part : plusieurs pistes ont été proposées au binôme pour décision commune plutôt qu'un choix unilatéral.
- **Décision et justification** : acceptée. La difficulté est traitée comme une métadonnée d'API (dans `GameStore`), pas comme une propriété du domaine (`Game`) : le moteur de jeu n'a besoin de recevoir qu'un `IComputerStrategy` concret, il n'a pas à savoir comment on l'a nommé — cohérent avec l'indépendance du moteur déjà actée ([ADR 0001](docs/adr/0001-structure-solution.md)).
- **Scénario ou commande de vérification** : `dotnet build`/`dotnet test` sur l'ensemble de la solution ; vérification manuelle dans le navigateur (bascule de difficulté, création d'une partie, retour à l'écran de configuration, apparition dans l'historique).
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur, nouveaux tests au vert, l'historique doit afficher une partie créée puis abandonnée avec sa difficulté et son statut « En cours ». Observé — `dotnet build` : 0 erreur ; `dotnet test` : 76/76 tests réussis (45 back, 31 front) ; dans le navigateur, une partie en difficulté « Difficile » créée puis abandonnée apparaît bien dans « Parties précédentes » avec la date, la difficulté et le statut corrects.
- **Erreur que ce contrôle pourrait détecter** : une difficulté « Easy » qui utiliserait quand même la stratégie « chasse puis cible » (ou l'inverse), une difficulté invalide acceptée silencieusement au lieu de renvoyer 400, ou une partie absente de l'historique alors qu'elle existe encore dans `GameStore`.
- **Preuves reproductibles et liens vers les commits** : `back/BattleShip.Tests/Domain/ComputerStrategyFactoryTests.cs`, tests ajoutés dans `back/BattleShip.Tests/Api/GamesEndpointsTests.cs` (difficulté, `GET /games`), tests ajoutés dans `front/BattleShip.App.Tests/Pages/HomeTests.cs` (bascule de difficulté envoyée à l'API, rendu de l'historique, absence de section si aucun historique).
- **Limites et points non vérifiés** : l'historique n'est pas persistant — il partage la fenêtre de 30 minutes du nettoyage automatique ([entrée précédente](#15092026-nettoyage-des-parties-inactives-dans-gamestore)) et disparaît avec la partie purgée. Le niveau « Facile » n'a pas été comparé statistiquement au niveau « Difficile » (nombre moyen de coups pour gagner).

---

## 15/09/2026, Revérification des contrats, chasse aux bugs et couverture de tests

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Contexte** : après plusieurs vagues de fonctionnalités, relecture volontaire de l'ensemble des contrats (HTTP, gRPC, OpenAPI) et recherche de bugs (fuite de données, données erronées) plutôt que d'ajouter une fonctionnalité de plus ; complétée par un passage sur les parties du code non couvertes par les tests.
- **Prompt réellement utilisé** : « Maintenant on revérifie les contrats, ensuite les bugs qui pourrait y avoir (aucune fuite de données, ou données érronnées) ... Tous les contrats doivent être correct ainsi que openAPI et enfin des tests sur des endroits de codes non couvert ou des fonctions non couvert et enfin fait en sortes que tous les adr paraissent mois IA généré, les dates en francais c'est à dire 15/09/2026, les références je suis pas sur qu'il yen ai besoins, le nom des fichiers pas tous les chemins et fait pareil pour tous les docs »
- **Réponse et hypothèses résumées** :
  - Relecture de `Board.GetState`/`GameStateMapper` : confirmé qu'aucun navire ni mine adverse non découverts n'est jamais renvoyé au client pas de fuite de données sur ce point.
  - Bug trouvé et corrigé : `Game.Shoot` ne vérifiait les bornes de la case visée que dans la branche « tir normal » ; pendant un tour sauté (mine), une coordonnée hors de la grille de la partie passait sans être rejetée. Déplacement de la vérification de bornes avant l'aiguillage sur le tour sauté.
  - Incohérence corrigée côté front : `GameApiClient` n'exposait le corps d'erreur du serveur (message FluentValidation/ProblemDetails) que pour la création de partie ; les autres appels (`ShootAsync`, `GetStateAsync`, etc.) se contentaient d'un message générique. Unifié via une méthode `EnsureSuccessAsync` commune.
  - Contrat HTTP/OpenAPI relu de bout en bout (types `Results<>` déclarés vs. réellement renvoyés par chaque endpoint) : cohérent. Le fichier `BattleShip.API.http` (utilisé comme référence manuelle du contrat) a été complété : il ne couvrait pas encore la difficulté, le Mode Tempête, `GET /games`, `GET /stats` ni le cas 409.
  - Couverture de tests mesurée avec `coverlet` plutôt que devinée : `RandomComputerStrategy` (14 %) et le service gRPC `GameStatsGrpcService` (0 %) n'avaient aucun test direct ; la victoire de l'ordinateur (`GamePhase.ComputerWon`) et le tir sur une partie déjà terminée (409) n'étaient jamais exercés ; la boucle de nettoyage périodique (`GameCleanupService.ExecuteAsync`) n'était vérifiée qu'indirectement. Ajout de tests ciblés sur chacun (dont `GameStatsGrpcServiceTests`, avec un faux `ServerCallContext` via `Grpc.Core.Testing`, pour éviter un vrai canal gRPC).
  - Nettoyage des ADR : dates au format français, sections Références allégées (noms de fichiers seulement, sans chemin, ou supprimées quand vides), un lien markdown cassé et une référence à un ADR inexistant corrigés, une mention obsolète de `Board.Size` (devenu `Board.DefaultSize`) mise à jour. Même traitement appliqué à `REVUE-IA.md` (un empilement de liens avec chemins complets simplifié en noms de fichiers).
- **Décision et justification** : acceptée. La vérification de bornes déplacée dans `Game.Shoot` plutôt que dupliquée dans chaque branche, pour garder un seul point de contrôle. Les sections Références des ADR ne sont conservées que si elles apportent réellement quelque chose (plusieurs étaient vides ou cassées) — cohérent avec le doute exprimé sur leur utilité.
- **Scénario ou commande de vérification** : `dotnet build`/`dotnet test` sur l'ensemble de la solution ; `dotnet test --collect:"XPlat Code Coverage"` sur chaque projet de tests pour mesurer précisément les lignes non couvertes avant et après les ajouts.
- **Résultat attendu, puis résultat observé** : attendu — compilation sans erreur, tous les tests (existants et nouveaux) au vert, couverture des zones identifiées en hausse sensible. Observé — `dotnet build` : 0 erreur ; `dotnet test` : 129/129 tests réussis (88 back, 41 front, contre 105 avant cette passe) ; couverture : `RandomComputerStrategy` 14 % → 100 %, `GameStatsGrpcService` 0 % → 100 %, boucle de `GameCleanupService` 33 % → 89 %.
- **Erreur que ce contrôle pourrait détecter** : une coordonnée hors grille acceptée silencieusement pendant un tour sauté, une case adverse non découverte renvoyée au client, un message d'erreur serveur perdu côté front, ou une régression dans l'algorithme probabiliste de l'IA Experte non détectée faute de test.
- **Preuves reproductibles et limites** : `GameTests.cs` (nouveau test de régression sur les bornes pendant un tour sauté), `RandomComputerStrategyTests.cs`, `ShipTests.cs`, `GameStatsGrpcServiceTests.cs`, `GameCleanupServiceTests.cs` (nouveaux fichiers), `GameApiClientTests.cs` (nouveau, front). Limite : deux branches très défensives restent non couvertes en toute rigueur — le cas où deux touches alignées ne le sont en réalité pas (géométriquement impossible avec des navires en ligne droite) dans `ExpertComputerStrategy`, et la fin « propre » (sans annulation) de la boucle de `GameCleanupService`, atteignable seulement si le minuteur est libéré sans passer par l'arrêt du service — tests jugés disproportionnés par rapport au risque réel. Par ailleurs, `REVUE-IA.md` ne compte que deux revues argumentées alors que le gabarit en demande au moins trois — signalé au binôme plutôt que complété unilatéralement, une revue authentique nécessitant un vrai jugement du binôme sur une proposition IA, pas un remplissage a posteriori.

---

## 15/09/2026, Case « Mode Tempête » qui ne répondait pas au clic, et deux revues IA supplémentaires

- **Outil / modèle si connu** : Claude Code (Claude Sonnet 5), CLI Claude Code.
- **Prompt réellement utilisé** : « un bug seulement visuel, quand on essaie de cocher mode tempete, avec le placement manuel, c'est incompatible pourtant, il n'y a rien qui change visuellement, il faut désactiver la case »
- **Décision et justification** : Correction en CSS uniquement, sans touché à la logique C#.
- **Scénario ou commande de vérification** : reproduction manuelle dans le navigateur (cocher placement manuel, constater que Mode Tempête ne réagit plus au clic, sans indice visuel), puis re-test après correctif (case et texte grisés, curseur interdit, et le comportement inverse, décocher réactive bien la case)
- **Résultat attendu, puis résultat observé** : après correctif, une case désactivée doit être visuellement reconnaissable comme telle.
- **Erreur que ce contrôle pourrait détecter** : une case ou un bouton désactivé sans aucun retour visuel ailleurs dans l'interface, laissant croire à un bug alors que le comportement est voulu.
