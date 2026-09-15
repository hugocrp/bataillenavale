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
