# Godot_AI_Project – Puissance 4

Puissance 4 dont la logique est écrite en C# pur (sans dépendance à un moteur), puis affichée par plusieurs « visualisations » : une console et Godot.

## Architecture

```
Godot_AI_Project/
├── Solution_Core/              Logique du jeu (+ IA à venir)
│   ├── Connect4.Core.slnx
│   └── Connect4.Core/          Class Library .NET 8
├── Console_Runner/             Visualisation console : test, débogage, entraînement IA
│   ├── Connect4.Console.slnx   (contient aussi le Core, pour tout ouvrir dans l'IDE)
│   └── Connect4.Console/       Console App, référence Connect4.Core
└── Affichage_Godot/
    └── connect-4/              Projet Godot 4.7 .NET, visualisation principale
```

Règle de base : **toute la logique vit dans `Connect4.Core`**. La console et Godot ne font qu'afficher l'état du jeu et transmettre les entrées du joueur. Le Core ne contient aucune notion graphique (pas de couleur, de position à l'écran ni de caractère) : chaque visualisation choisit son propre rendu.

```
 Entrée joueur ──► Game.Play(colonne) ──► MoveResult (coup accepté / refusé)
                         │
                         └──► événements ──► DiscPlaced, TurnChanged, GameEnded, GameReset
                                              (la console / Godot s'abonnent et affichent)
```

## Connect4.Core – la logique

| Fichier | Visibilité | Rôle |
|---|---|---|
| `Game.cs` | public | **Point d'entrée unique.** `Play(colonne)` joue un coup, `Reset()` relance une partie. Expose l'état (`CurrentPlayer`, `Status`, `Winner`, `WinningLine`, `History`) et les événements `DiscPlaced`, `TurnChanged`, `GameEnded`, `GameReset`. |
| `IReadOnlyBoard.cs` | public | Vue en lecture seule du plateau, la seule que voient les affichages et les joueurs IA : `board[col, row]`, `CanDrop`, `NextFreeRow`, `LegalColumns`, `IsFull`. |
| `Board.cs` | internal | Implémentation de la grille 7×6 : `Drop`, `Undo`, `Clone`, `Clear`. `Undo` et `Clone` servent à l'IA pour simuler des coups. |
| `WinChecker.cs` | internal | Cherche un alignement de 4 (horizontal, vertical, deux diagonales) autour du dernier jeton posé et renvoie les cases gagnantes. |
| `Disc.cs` | public | Contenu d'une case : `None`, `Red`, `Yellow`. `Opponent()` donne l'adversaire. |
| `Cell.cs` | public | Coordonnée `(Column, Row)`. **La ligne 0 est en bas** du plateau. |
| `GameStatus.cs` | public | `GameStatus` (`InProgress`, `Won`, `Draw`) et `MoveResult` (`Ok`, `OutOfRange`, `ColumnFull`, `GameOver`). |
| `IPlayer.cs` | public | Interface d'un joueur automatique (`ChooseColumn`) et `RandomPlayer`, une IA qui joue au hasard. Les humains n'implémentent pas `IPlayer` : l'affichage appelle directement `Game.Play`. |

## Connect4.Console – visualisation console

| Fichier | Rôle |
|---|---|
| `Program.cs` | Menu (JvJ, J vs IA aléatoire, IA vs IA), boucle de jeu, lecture des entrées et abonnement aux événements du `Game`. |
| `ConsoleRenderer.cs` | Tout le rendu texte : grille, symboles `X` / `O`, couleurs, surlignage de la ligne gagnante. |

Lancer :

```bash
dotnet run --project Console_Runner/Connect4.Console
```

## Affichage_Godot/connect-4 – visualisation Godot

À venir : référence vers `Connect4.Core`, scène du plateau, scène du jeton, conversion `Cell` → position Godot (attention : l'axe Y de Godot pointe vers le bas).

## Notes techniques

- **Cibles .NET.** Le Core cible `net8.0`, le runtime qu'utilise Godot 4 : ne pas le monter de version. La console cible aussi `net8.0`, avec `RollForward=Major` pour tourner sur un runtime plus récent (10 ou 11) si le 8 n'est pas installé.
- **`global.json`.** Les deux fichiers demandent un SDK 8 minimum (`rollForward: latestMajor`), donc tout SDK récent convient.
- **`.slnx` et ligne de commande.** La CLI `dotnet` ne lit les `.slnx` qu'à partir du SDK 9.0.200. En cas de doute, lancer les `.csproj` directement.
- **`.gitignore`.** Le fichier à la racine exclut `bin/`, `obj/`, `.idea/`, `.vs/` et `.godot/`.

## Planning

| Date | Étape |
|---|---|
| 23 nov. | Logique C# + console, début de la version Godot (joueurs humains) |
| 25 nov. | Intégration du joueur IA + entraînement, finalisation Godot |
| 16 déc. | Soutenance |
