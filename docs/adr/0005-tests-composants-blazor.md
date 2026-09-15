# ADR 0005 : Tests des composants Blazor dans un projet dédié

## Statut et date
Accepté — 15/09/2026.

## Contexte
Jusqu'ici, `back/BattleShip.Tests` couvrait le moteur de jeu et les endpoints HTTP, mais aucun test n'exerçait le front lui-même (`BoardGrid`, `FleetSetup`, `Home.razor`) : le rendu, les interactions de clic, la gestion des états de chargement/erreur n'étaient vérifiés que manuellement dans le navigateur.

## Options envisagées
- **Ajouter bUnit à `back/BattleShip.Tests`** : évite un nouveau projet, mais ce projet référence déjà `BattleShip.API` (dont le service gRPC génère ses propres types `BattleShip.API.Grpc.*` avec `GrpcServices="Server"`). Le front génère les siens avec `GrpcServices="Client"` dans le **même namespace** à partir du même `.proto` ([ADR 0003](0003-perimetre-grpc.md)). Référencer les deux projets dans un même projet de tests rend `GameStatsRequest`/`GameStatsReply`/`GameStats` ambigus (deux assemblies revendiquant le même type), ce qui a été vérifié en pratique lors d'une tentative infructueuse.
- **Un projet de tests dédié au front** (`front/BattleShip.App.Tests`), référençant uniquement `BattleShip.App` : évite la collision, et reflète la séparation back/front déjà actée ([ADR 0001](0001-structure-solution.md)).

## Décision
Nouveau projet `front/BattleShip.App.Tests` (xUnit + [bUnit](https://bunit.dev)), référençant uniquement `BattleShip.App`. Pour rendre `Home.razor` testable sans dépendre d'un vrai serveur :
- `GameApiClient` (HTTP/JSON) est testé avec un `HttpClient` réel branché sur un `HttpMessageHandler` factice — le code de sérialisation JSON reste donc réellement exercé.
- `GameStatsClient` (gRPC-Web) est désormais exposé derrière une interface `IGameStatsClient`, injectée dans `Home.razor` : le protocole gRPC-Web (framing binaire) n'est pas raisonnablement simulable au niveau d'un test de composant, contrairement au JSON. Une implémentation factice de `IGameStatsClient` est utilisée dans les tests.

## Conséquences
- `IGameStatsClient` est la première interface introduite côté front, justifiée par un besoin réel de substitution en test — pas une abstraction préventive.
- Les mises à jour asynchrones de `Home.razor` (chargement de la flotte, création de partie, tir, statistiques) sont vérifiées avec `cut.WaitForAssertion(...)`, le mécanisme standard de bUnit pour attendre un re-rendu déclenché par une tâche asynchrone plutôt que de supposer un achèvement synchrone.
- `GameApiClient` reste une classe concrète (pas d'interface) : son injectabilité passe par le `HttpClient` sous-jacent, suffisant pour les tests et évitant une abstraction superflue.

## Vérification et réexamen
64 tests au total après cet ajout (36 dans `back/BattleShip.Tests`, 28 dans `front/BattleShip.App.Tests`) : 12 sur `BoardGrid` (rendu, états, désactivation, clic), 8 sur `FleetSetup` (sélection, placement, chevauchement, débordement, validation), 8 sur `Home.razor` (bascule aléatoire/manuel, partie complète, erreurs HTTP et gRPC, retour à l'écran de configuration). Vérifié également que l'application réelle fonctionne inchangée après l'introduction de `IGameStatsClient` (partie jouée et statistiques gRPC affichées dans le navigateur). À réexaminer si `GameApiClient` doit un jour être substitué autrement qu'au niveau HTTP (par exemple pour simuler une latence réseau spécifique).

