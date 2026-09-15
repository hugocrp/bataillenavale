# ADR 0007 : Mode Tempête, IA « Experte » probabiliste et statistiques globales

## Statut et date
Accepté — 15/09/2026.

## Contexte
Trois demandes combinées par le binôme : (1) un mode de jeu « maison », activable par une case à cocher, avec ses propres règles (grille agrandie, flotte élargie, mines qui font sauter le tour en cas de déclenchement) et un encart explicatif quand il est actif ; (2) une IA plus forte que la stratégie « chasse puis cible » existante ([ADR 0004](0004-algorithme-adversaire.md)) ; (3) des statistiques globales, au-delà de l'historique par partie déjà en place ([ADR 0006](0006-difficulte-et-historique.md)).

Ces trois besoins touchent des couches différentes (règles du domaine, stratégie de l'ordinateur, agrégation côté API) mais partagent une contrainte commune : ne pas casser le mode standard existant, qui doit rester le comportement par défaut.

## Options envisagées

**Grille de taille variable**
- Garder `Board.Size` en constante statique (`10`) et créer un type de plateau parallèle pour le mode Tempête : duplique la logique de placement/tir plutôt que de la partager, et un domaine qui distingue deux "sortes" de plateau alors que les règles de résolution des tirs sont identiques.
- Transformer `Board.Size` en propriété d'instance, fixée au constructeur : une seule classe `Board` sert les deux modes, la taille n'est qu'un paramètre. Retenue — impact de compilation large (tout code utilisant l'ancienne constante statique), mais localisé et mécanique.

**Sauter un tour après une mine**
- Rendre `TurnResult.PlayerShot` nullable pour représenter « ce tour n'a pas eu lieu » : casse le contrat existant (tous les tests et le front supposent `PlayerShot` toujours présent) pour un cas qui ne concerne qu'un mode optionnel.
- Ajouter `ShotOutcome.TurnSkipped` comme résultat possible sur le `ShotResult` **existant**, non-nullable : le tir « sauté » est représenté comme un résultat parmi d'autres (au même titre que `Miss`/`Hit`), sans toucher la forme du contrat. Retenue. Le cas symétrique côté ordinateur réutilise `TurnResult.ComputerShot`, déjà nullable (cas déjà existant : partie gagnée par le joueur avant la riposte adverse).

**IA « Experte »**
- Améliorer `HuntTargetComputerStrategy` en place : mélange deux stratégies dans une seule classe, rend le code existant plus difficile à suivre pour un gain incertain.
- Nouvelle stratégie `ExpertComputerStrategy`, sélectionnable via `ComputerStrategyFactory` au même titre que les niveaux existants : cohérent avec le pattern Stratégie déjà en place (`IComputerStrategy`), n'affecte aucun code existant. Retenue.

** Statistiques globales **
- Calculer les statistiques à la volée depuis `GameStore` (parcours des parties en mémoire) plutôt que de maintenir des compteurs incrémentaux séparés : plus simple, cohérent avec le fait que `GameStore` est déjà la source de vérité pour l'historique par partie ([ADR 0006](0006-difficulte-et-historique.md)), et évite un risque de désynchronisation entre compteurs et parties réelles. Retenue — au prix d'un coût `O(n)` par appel à `GET /stats`, jugé négligeable vu le volume de parties en mémoire (purgées après 30 minutes d'inactivité par `GameCleanupService`).

## Décision
- `Board.Size` devient une propriété d'instance (`public int Size { get; }`), fixée au constructeur (`Board(int size = Board.DefaultSize)`). `FleetFactory` expose deux compositions : `StandardFleet` (5 navires, grille 10×10, comportement inchangé) et `StormFleet` (8 navires, grille 12×12, `FleetFactory.CreateStormBoard` place en plus 3 mines aléatoires via `Board.PlaceMine`).
- `ShotOutcome` gagne deux valeurs : `MineHit` (la case touchée était une mine — ne coule aucun navire, mais programme le saut du prochain tour de son camp) et `TurnSkipped` (résultat du tir sauté). `Game.Shoot` porte deux drapeaux internes (`_playerSkipsNextTurn`, `_computerSkipsNextTurn`) consommés au tour suivant.
- `ComputerDifficulty` gagne `Expert`, servi par `ExpertComputerStrategy` : ciblage par ligne (deux touches alignées → vise les extrémités de la ligne) quand une chasse est en cours, sinon choix par densité de probabilité (pour chaque taille de navire restant à couler, score de chaque case selon le nombre de placements valides qui l'occuperaient).
- `GameStore.CreateStorm` construit une partie avec deux plateaux Tempête (le joueur ne place jamais sa flotte manuellement en mode Tempête — incompatibilité assumée et validée côté API) ; `GameStore.GetGlobalStats` agrège `TotalGames`, `PlayerWins`, `ComputerWins`, `InProgressCount` et `AverageShotsToFinish` (moyenne des tirs cumulés des deux plateaux, uniquement sur les parties terminées) sur l'ensemble des parties en mémoire. Nouvel endpoint `GET /stats`.
- Côté front, une case à cocher « Mode Tempête (règles maison) », mutuellement exclusive avec le placement manuel (les deux activent/désactivent l'autre), affiche un encart de règles (`.storm-rules`) uniquement quand le mode est actif, et `BoardGrid.GridSize` devient un paramètre (au lieu d'une constante) pour rendre la grille 12×12.

## Conséquences
- La bascule statique → instance sur `Board.Size` s'est répercutée dans tout le code qui la référençait (stratégies IA, `FleetFactory`, mapping API, validation, composants Blazor) — changement mécanique mais large, effectué en une passe pour éviter un état intermédiaire incohérent.
- La borne de validation FluentValidation sur les coordonnées de tir (`ShotRequestValidator`) est désormais large (jusqu'à la taille maximale possible, celle du mode Tempête) : la borne exacte par partie reste appliquée au niveau du domaine (`Board.ReceiveShot` lève `ArgumentOutOfRangeException`, capturée par l'endpoint) — même patron à deux niveaux que la validation du placement manuel.
- `GET /stats` recalcule à chaque appel plutôt que de maintenir un compteur : coût acceptable tant que le nombre de parties en mémoire reste borné par `GameCleanupService`, à revoir si ce n'est plus le cas.
- Le mode Tempête est incompatible avec le placement manuel de la flotte (la position des mines et la flotte élargie ne sont pas exposées à un éditeur manuel) — rejeté avec une erreur de validation explicite (400) plutôt qu'silencieusement ignoré.

## Vérification et réexamen
Tests unitaires : mines sur `Board` (placement invalide, déclenchement, non-révélation avant déclenchement — `BoardTests`), saut de tour côté joueur et ordinateur sans annuler la riposte du tour en cours (`GameTests`), ciblage par ligne et par probabilité de `ExpertComputerStrategy` (`ExpertComputerStrategyTests`, simulation de partie complète pour vérifier l'absence de re-ciblage), composition et absence de chevauchement de la flotte/mines Tempête sur plusieurs graines aléatoires (`FleetFactoryTests`). Tests d'intégration : création d'une partie Tempête (grille 12×12), rejet d'une flotte manuelle en mode Tempête, `GET /stats` reflétant les parties créées (`GamesEndpointsTests`), agrégation de `GameStore.GetGlobalStats` (victoires/défaites/parties en cours comptées séparément, moyenne absente sans partie terminée — `GameStoreTests`). Tests de composants : affichage de l'encart de règles à l'activation, exclusivité mutuelle avec le placement manuel, transmission de `StormMode` à la création, sélection de la difficulté Experte, affichage des statistiques globales (`HomeTests`). Vérifié manuellement dans le navigateur : activation du mode Tempête (encart de règles affiché), démarrage d'une partie 12×12 avec mine visible (icône ☢) sur « Votre flotte », tir déclenché sur la grille adverse avec IA Experte sélectionnée, statistiques globales affichant un nombre de parties non nul après création. À réexaminer si le nombre de parties en mémoire simultanées devient assez grand pour que le recalcul de `GET /stats` à chaque appel devienne coûteux.

Bug trouvé lors d'une relecture ultérieure des contrats : `Game.Shoot` ne validait les bornes de la case visée que dans la branche « tir normal » (via `ComputerBoard.ReceiveShot`), pas dans la branche « tour sauté » (mine) — un client pouvait donc y glisser des coordonnées hors de la grille de la partie sans être rejeté. Corrigé en déplaçant la vérification de bornes avant l'aiguillage sur `_playerSkipsNextTurn`, avec un test de non-régression dédié.

## Références
Voir `Board.cs`, `ExpertComputerStrategy.cs`, `FleetFactory.cs` et `GameStore.cs`.
