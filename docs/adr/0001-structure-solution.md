# ADR 0001 : Structure de la solution et disposition back/front

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le socle imposé (diapo 6 du support) requiert une API ASP.NET Core en Minimal API, un front Blazor WebAssembly, une bibliothèque de modèles partagée et un projet de tests, le tout en .NET 10. Le déroulé du support (diapo 15/28) propose de placer les quatre projets à plat à la racine du dépôt. Nous souhaitons cependant une séparation physique claire entre le code serveur et le code client dès le début du projet.

## Options envisagées
- **Tout à plat à la racine** (tel que suggéré littéralement par le support) : simple, mais mélange dans un même niveau de dossier des projets aux responsabilités très différentes (serveur, client, modèles, tests).
- **Deux sous-dossiers `back/` et `front/`** : sépare visuellement le code serveur du code client, tout en gardant `BattleShip.Models` dans `back/` puisqu'il s'agit historiquement du projet le moins couplé et que l'API en est le principal consommateur métier.
- **Un troisième dossier `shared/` pour `BattleShip.Models`** : plus symétrique, mais ajoute un niveau de découpage supplémentaire non demandé à ce stade.

## Décision
Structure en deux sous-dossiers : `back/` (BattleShip.API, BattleShip.Models, BattleShip.Tests) et `front/` (BattleShip.App). `BattleShip.Models` reste sans dépendance vers les autres projets et est référencé à la fois par `back/BattleShip.API` et par `front/BattleShip.App`, ce qui est possible via une référence de projet relative même entre les deux dossiers.

## Conséquences
- Le code serveur et le code client sont immédiatement identifiables dans l'arborescence.
- `BattleShip.Models` reste indépendant de HTTP, JSON et gRPC (conformément à la diapo 28), ce qui permettra de tester le moteur de jeu indépendamment des transports.
- Les chemins de référence de projet sont légèrement plus longs (`../../back/BattleShip.Models/...` depuis le front) qu'avec une structure à plat, sans impact fonctionnel.
- Si un dossier `shared/` devient pertinent plus tard (par exemple si d'autres bibliothèques partagées apparaissent), cette décision devra être réexaminée.

## Vérification et réexamen
`dotnet build` et `dotnet test` exécutés depuis la racine compilent l'ensemble de la solution et exécutent les tests sans erreur, ce qui confirme que les références inter-dossiers fonctionnent correctement. À réexaminer si un projet supplémentaire partagé entre back et front est introduit.

## Références
- 2026-09-15 sur l'initialisation de la solution dans prompt.md.
