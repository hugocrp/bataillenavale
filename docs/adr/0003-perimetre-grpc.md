# ADR 0003 : Périmètre de l'échange gRPC

## Statut et date
Accepté — 15/09/2026.

## Contexte
Le socle impose « gRPC fonctionnel sur au moins un échange » entre le front et l'API, avec une réponse et une erreur attendue démontrables, accessible depuis le navigateur via gRPC-Web. La boucle de jeu principale (créer une partie, tirer) est déjà exposée en HTTP/JSON.

## Options envisagées
- **Basculer toute la boucle de jeu sur gRPC** : redondant avec le contrat HTTP déjà exigé par le socle, et complexifie inutilement le front pour un seul jeu simple.
- **Dupliquer un endpoint existant en gRPC** (ex. créer une partie) : ne démontre pas d'usage réellement différent, juste une deuxième façon d'appeler la même chose.
- **Un service gRPC dédié à une fonctionnalité complémentaire — les statistiques d'une partie** (`GameStats.GetStats` : phase, navires restants de chaque côté, tirs joués) : distinct du flux HTTP principal, avec un cas d'erreur naturel (partie inconnue → `NotFound`).

## Décision
Un service gRPC `GameStats` exposant une unique RPC `GetStats`, consommé depuis Blazor via gRPC-Web (bouton « Statistiques » sur la page de jeu). Le contrat est défini dans `battleship.proto`, partagé par l'API et le front.

## Conséquences
- La boucle de jeu principale reste simple et entièrement HTTP/JSON.
- Le cas d'erreur (identifiant de partie inconnu ou invalide, `StatusCode.NotFound`) est démontrable indépendamment du succès
- CORS doit exposer les en-têtes `Grpc-Status`, `Grpc-Message`, `Grpc-Encoding` et `Grpc-Accept-Encoding` pour que le navigateur puisse lire le statut gRPC-Web.

## Vérification et réexamen
Démonstration manuelle : créer une partie dans l'interface, cliquer sur « Statistiques (gRPC) » (succès), puis appeler le même service avec un identifiant de partie aléatoire (via `grpcurl`, directement sur l'API) pour observer l'erreur `NotFound`. Complété depuis par des tests unitaires (`GameStatsGrpcServiceTests`) qui appellent le service directement avec un faux `ServerCallContext` : succès, identifiant mal formé (`InvalidArgument`) et partie inconnue (`NotFound`).

