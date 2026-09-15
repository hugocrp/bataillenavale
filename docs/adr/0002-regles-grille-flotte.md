# ADR 0002 : Règles du jeu, taille de grille et composition de la flotte

## Statut et date
Accepté — 15/09/2026.

## Contexte
Il a été préciser explicitement que les règles, la taille de la grille et la composition de la flotte sont des choix libres du binôme, tant qu'ils sont justifiés. Il faut cependant un socle jouable contre l'ordinateur dès l'initialisation du moteur de jeu.

## Options envisagées
- **Grille réduite (8x8) avec flotte simplifiée** : parties plus rapides à jouer et à tester manuellement.
- **Grille classique 10x10 avec flotte classique** (porte-avions 5, croiseur 4, contre-torpilleur 3, sous-marin 3, torpilleur 2) : correspond au jeu de plateau traditionnel, connu du correcteur et des étudiants, ce qui facilite la démonstration.
- **Grille configurable par le joueur** : plus flexible mais ajoute de la complexité (validation des paramètres, UI de configuration) non nécessaire au socle.

## Décision
Grille 10x10 (`Board.DefaultSize`) avec la flotte classique à 5 navires (`FleetFactory.StandardFleet`), placée aléatoirement pour le joueur et pour l'ordinateur à la création de chaque partie. (Devenue la taille par défaut plutôt qu'une constante figée depuis le Mode Tempête — voir ADR 0007.)

## Conséquences
- Le moteur (`BattleShip.Models`) reste simple : une seule taille de grille, une seule composition de flotte, aucune configuration à valider côté API.
- Cela laisse la porte ouverte à une évolution future (taille ou flotte paramétrable) si le backlog le justifie ; cette ADR devra alors être révisée.
- Le placement aléatoire (`FleetFactory.CreateRandomBoard`) garantit l'absence de chevauchement et de débordement par construction (tirage avec nouvelle tentative en cas de collision).

## Vérification et réexamen
Couvert par `BoardTests.cs` (chevauchement, débordement) et `FleetFactoryTests.cs` (placement complet de la flotte sur plusieurs graines aléatoires). À réexaminer si le backlog introduit une flotte ou une grille personnalisable.
