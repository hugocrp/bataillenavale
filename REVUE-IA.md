# Revues de propositions IA

Trois revues argumentées minimum. Aucune erreur n'est exigée ; chaque conclusion doit être étayée.

Gabarit de revue :

```
## Revue : sujet du projet

* Proposition et référence dans le dépôt :
* Hypothèse à vérifier :
* Scénario, données ou commande :
* Résultat attendu avant exécution :
* Erreur que ce contrôle pourrait détecter :
* Résultat réellement observé :
* Décision et justification :
* Preuves reproductibles et liens vers les commits :
* Après correction éventuelle : résultat avant / après :
* Limites et points non vérifiés :
```

---

## Revue : contournement proposé (Rosetta 2) face à l'échec de build `Grpc.Tools` sur Apple Silicon

* **Proposition et référence dans le dépôt** : lors de la mise en place du service gRPC [back/BattleShip.API/BattleShip.API.csproj] et [front/BattleShip.App/BattleShip.App.csproj], la commande "dotnet build" a échoué avec "Bad CPU type in executable" sur le protoc fourni par Grpc.Tools. L'IA a proposé d'installer Rosetta 2 "softwareupdate --install-rosetta --agree-to-license" pour l'exécuter sur ce Mac Apple Silicon (arm64).
* **Hypothèse à vérifier** : Rosetta 2 est-elle réellement nécessaire, ou existe-t-il un binaire protoc/grpc_csharp_plugin natif arm64 dans une version plus récente du package Grpc.Tools ?
* **Scénario, données ou commande** : comparaison de la version de Grpc.Tools réellement résolue par chaque projet livré par les versions 2.83.0 et 2.84.0.
* **Résultat attendu avant exécution** : si l'hypothèse « Rosetta indispensable » est vraie, les deux versions ne devraient proposer qu'un dossier macosx_x64.
* **Erreur que ce contrôle pourrait détecter** : une affirmation de l'IA non vérifiée qui aurait fait installer à l'utilisateur un composant système inutile.
* **Résultat réellement observé** : BattleShip.API résolvait Grpc.Tools en 2.83.0 (qui appelé indirectement par Grpc.AspNetCore), dont tools/ ne contient qu'un dossier macosx_x64 (binaire x86_64 donc pas arm64 qui est pour les apple silicon). BattleShip.App référençait explicitement 2.84.0, avce tools/ qui contient un dossier macosx_universal (binaire universel comprenant x64 et arm64). L'hypothèse était donc fausse.
* **Décision et justification** : proposition initiale (Rosetta 2) rejetée ; adaptée en épinglant explicitement Grpc.Tools en version 2.84.0 dans BattleShip.API.csproj, comme c'était déjà le cas côté BattleShip.App. Solution ciblée sur la cause réelle (version obsolète sans binaire arm64), sans modification système.
* **Preuves reproductibles et liens vers les commits** : `dotnet build` puis `dotnet test` sur l'ensemble de la solution après correction depuis la racine.
* **Après correction éventuelle : résultat avant / après** : avant : `dotnet build` échouait sur `back/BattleShip.API` avec `MSB6003: Bad CPU type in executable`. Après — `dotnet build` : 0 erreur sur les 4 projets de la solution ; `dotnet test` : 20/20 tests réussis, sans Rosetta 2 installée sur la machine
* **Limites et points non vérifiés** : cette vérification porte sur la chaîne de build à un instant donné ; elle ne garantit pas que le binaire macosx_universal restera disponible dans les futures versions de `Grpc.Tools`, ni qu'une mise à jour de `Grpc.AspNetCore` ne réintroduira pas une version plus ancienne en dépendance transitive.

---

## Revue : respect des principes SOLID dans le socle implémenté

* **Proposition et référence dans le dépôt** : l'ensemble du socle généré par l'IA.
* **Hypothèse à vérifier** : le code respecte-t-il raisonnablement les principes SOLID, ou l'IA a-t-elle produit du code plausible mais mal structuré (couplage fort, responsabilités mélangées) ?
* **Scénario, données ou commande** : relecture manuelle principe par principe.
* **Résultat attendu avant exécution** : si le principe d'inversion des dépendances (DIP) est respecté, le moteur de jeu (`Game`) devrait dépendre d'une abstraction pour la stratégie de l'ordinateur plutôt que d'une implémentation concrète, et cela devrait être visible dans la façon dont les tests substituent cette stratégie.
* **Erreur que ce contrôle pourrait détecter** : un couplage fort empêchant de faire évoluer une partie du code (ex. changer l'IA adverse) sans modifier le moteur de jeu, ou des composants aux responsabilités mélangées rendant le code difficile à faire évoluer et pour la sécurité.
* **Résultat réellement observé** : `Game` dépend de l'interface `IComputerStrategy`, pas de `RandomComputerStrategy` directement, qui substitue un `StubComputerStrategy` sans modifier `Game` (OCP/DIP/LSP respectés). `ShotRequestValidator` est consommé via `IValidator<ShotRequest>` injecté par Dependency Injection dans l'endpoint, pas instancié en dur (DIP respecté). Chaque classe du moteur (`Ship`, `Board`, `Game`) a une responsabilité distincte et cohérente (SingleResponsability respecté).
* **Décision et justification** : structure globale **acceptée** sans modification immédiate. Les deux points faibles sont des compromis assumés, `GameStore` : éviter une abstraction prématurée pour un unique consommateur ; `Home.razor` : taille encore raisonnable pour le périmètre actuel du socle plutôt que des défauts à corriger dans l'urgence. À réexaminer si `Home.razor` grossit sensiblement avec la suite.
* **Preuves reproductibles et liens vers les commits** : lecture directe de [IComputerStrategy.cs](back/BattleShip.Models/IComputerStrategy.cs), [Game.cs](back/BattleShip.Models/Game.cs), [GameStatsGrpcService.cs](back/BattleShip.API/Grpc/GameStatsGrpcService.cs), [Home.razor](front/BattleShip.App/Pages/Home.razor) ; comportement de substitution vérifié par l'exécution de `back/BattleShip.Tests/Domain/GameTests.cs` (20/20 tests réussis, y compris ceux utilisant `StubComputerStrategy`).
* **Après correction éventuelle : résultat avant / après** : aucune correction appliquée à ce stade — revue de constat, aucun défaut ne justifiant une correction immédiate.
* **Limites et points non vérifiés** : revue basée sur une lecture manuelle du code, pas sur un outil d'analyse statique dédié (ex. analyseur Roslyn) ; le couplage de `Home.razor` n'a pas été mesuré quantitativement (complexité cyclomatique, nombre de responsabilités) ; les principes ont été évalués sur le socle actuel, pas sur les extensions futures du backlog.
