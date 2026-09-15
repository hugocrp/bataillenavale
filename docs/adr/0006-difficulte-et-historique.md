# ADR 0006 : Niveau de difficulté « Facile » et historique des parties

## Statut et date
Accepté — 15/09/2026.

## Contexte
Deux demandes complémentaires : proposer un niveau de difficulté plus accessible que la stratégie « chasse puis cible » ([ADR 0004](0004-algorithme-adversaire.md)), et permettre de consulter les parties précédentes. Les deux s'appuient sur la même information : quelle stratégie a été choisie pour chaque partie, information que le moteur (`Game`) n'a pas besoin de connaître pour fonctionner (il reçoit déjà un `IComputerStrategy` concret).

## Options envisagées
- **Étiqueter la difficulté dans `Game`** (domaine) : nécessite d'ajouter une propriété qui n'a aucun rôle dans les règles du jeu — le domaine n'a besoin que d'un `IComputerStrategy`, pas de savoir comment on l'a nommé.
- **Étiqueter la difficulté dans `GameStore`** (API) : la difficulté est une métadonnée de présentation/historique, pas une règle de jeu ; `GameStore` gère déjà une métadonnée similaire (`LastActivityUtc`, ajoutée pour le nettoyage automatique des parties inactives). L'ajouter au même endroit évite de polluer le domaine.

## Décision
- `ComputerStrategyFactory.Create(ComputerDifficulty, Random)` centralise le choix entre `RandomComputerStrategy` (Easy) et `HuntTargetComputerStrategy` (Hard, toujours le défaut si `Difficulty` est omis — compatibilité avec le comportement existant).
- `GameStore` stocke la difficulté choisie à la création, à côté de `LastActivityUtc`, dans son `GameEntry` interne — une métadonnée d'API, pas une propriété du domaine.
- `GET /games` expose l'historique (identifiant, statut, difficulté, dernière activité) en réutilisant les mêmes informations déjà nécessaires au nettoyage des parties inactives.

## Conséquences
- Aucune modification du domaine (`Game`, `IComputerStrategy`) : la difficulté reste une préoccupation de l'API, cohérent avec la séparation actée dans [ADR 0001](0001-structure-solution.md) (le moteur reste indépendant de HTTP/gRPC — et maintenant, de la présentation).
- L'historique est borné par la fenêtre de nettoyage de `GameCleanupService` (30 minutes d'inactivité) : une partie ancienne disparaît de `GET /games` en même temps qu'elle est purgée de la mémoire. Ce n'est pas un historique persistant.
- Le contrat `CreateGameRequest.Difficulty` est une chaîne optionnelle (`null` = comportement précédent, difficile), validée par FluentValidation avant d'atteindre `Enum.Parse`, suivant le même patron que `ShipPlacementDto.Orientation`.

## Vérification et réexamen
Tests unitaires (`ComputerStrategyFactoryTests`) vérifiant que chaque difficulté produit le bon type de stratégie ; tests d'intégration (`GamesEndpointsTests`) pour la création avec une difficulté valide/invalide et le contenu de `GET /games` ; tests de composants (`HomeTests`) pour la bascule de difficulté et l'affichage de l'historique. Vérifié manuellement dans le navigateur : une partie créée puis abandonnée (retour à l'écran de configuration) apparaît dans « Parties précédentes » avec sa difficulté et son statut. À réexaminer si une persistance réelle (au-delà de la mémoire) est ajoutée au backlog.
