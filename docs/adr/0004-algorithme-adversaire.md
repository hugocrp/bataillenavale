# ADR 0004 : Algorithme de l'ordinateur (stratégie de tir)

## Statut et date
Accepté — 15/09/2026.

## Contexte
Le socle laisse explicitement le choix de l'algorithme de l'adversaire au binôme (diapo 6 : « Stockage et algorithme de l'adversaire »), à condition qu'il respecte les mêmes règles de validité des coups que le joueur (diapo 38). La première implémentation ([ADR 0001](0001-structure-solution.md)) utilisait un tir entièrement aléatoire (`RandomComputerStrategy`), sans aucune mémoire des coups précédents.

## Options envisagées
- **Tir aléatoire pur** (solution initiale) : simple, mais l'ordinateur ne profite jamais d'un tir touché — après avoir touché un navire, il continue à tirer n'importe où au lieu de chercher à le couler. Peu crédible comme adversaire.
- **Recherche exhaustive / résolution probabiliste** (calcul d'une carte de probabilité sur toutes les positions de flotte compatibles avec les tirs déjà joués) : le plus fort possible, mais complexité largement disproportionnée pour le socle demandé.
- **Stratégie « chasse puis cible » (hunt & target)**, classique pour ce jeu : tant qu'aucun navire n'est touché, l'ordinateur « chasse » (tir quasi aléatoire, optimisé par un quadrillage en damier) ; dès qu'un tir touche un navire non coulé, il « cible » les cases adjacentes à tous les tirs touchés en attente jusqu'à couler le navire, puis revient en mode chasse.

## Décision
Stratégie « chasse puis cible » implémentée dans `HuntTargetComputerStrategy` (remplace `RandomComputerStrategy`, supprimée car devenue inutilisée). Le mode chasse tire uniquement sur les cases d'une parité `(ligne + colonne) % 2 == 0` tant qu'il en reste — cette optimisation classique garantit de toucher tout navire de taille ≥ 2 (la flotte du socle n'a pas de navire de taille 1, voir [ADR 0002](0002-regles-grille-flotte.md)) tout en réduisant de moitié le nombre de cases à tester avant le premier contact.

## Conséquences
- L'ordinateur devient un adversaire nettement plus crédible : une fois un navire touché, il ne le lâche plus tant qu'il n'est pas coulé.
- Aucune modification de l'interface `IComputerStrategy` ni de `Game` n'a été nécessaire : la stratégie reconstruit son état de « chasse/cible » à chaque appel à partir des informations déjà publiques du plateau (`Board.Ships`, `Ship.Hits`, `Board.HasBeenShotAt`), sans état mutable propre. Cela confirme le bénéfice de l'abstraction posée dès le départ (voir la revue SOLID dans `REVUE-IA.md`).
- Une seule propriété a été ajoutée à `Ship` (`Hits`, lecture seule) pour exposer les cases déjà touchées d'un navire non coulé.
- ~~La difficulté n'est pas configurable (aucun mode « facile »)~~ — complété par [ADR 0006](0006-difficulte-et-historique.md) : `RandomComputerStrategy` a été réintroduite comme niveau « Facile », sélectionnable via l'API, sans modifier `HuntTargetComputerStrategy` ni `IComputerStrategy`.

## Vérification et réexamen
Tests unitaires dans `HuntTargetComputerStrategyTests.cs` : ciblage d'une case adjacente après un tir touché, aucun tir répété sur une case déjà jouée (jusqu'à couler toute la flotte), reprise du mode chasse après qu'un navire touché soit coulé, priorité donnée à un navire encore touché quand un autre est déjà coulé, et repli sur un tir hors damier quand toutes les cases en damier sont déjà jouées. Vérification supplémentaire en conditions réelles : script Python exerçant l'API HTTP sur une partie complète, confirmant que 26/26 tirs de riposte suivant un tir touché en attente étaient bien adjacents à l'un de ces tirs.

## Références
Support de cours, diapo 6 (choix libres) et diapo 38 (spécification de la boucle de jeu). Voir `HuntTargetComputerStrategy.cs`.
