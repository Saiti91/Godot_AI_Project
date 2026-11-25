# Godot_AI_Project – Puissance 4 avec IA

Un Puissance 4 dont **toute la logique est en C# pur** (aucun moteur), avec une **IA qui apprend par renforcement** (Q-learning), affiché par deux « visualisations » : une **console** et **Godot**.

> 👉 **Tu travailles sur Godot ?** Lis directement le guide pas à pas : [Affichage_Godot/GUIDE_GODOT.md](Affichage_Godot/GUIDE_GODOT.md)

## Architecture

```
Godot_AI_Project/
├── Solution_Core/
│   ├── Connect4.Core/        Les règles du jeu + le point d'entrée Game   (Class Library .NET 8)
│   └── Connect4.ML/          L'IA (Q-learning), ne connaît rien du jeu     (Class Library .NET 8)
├── Console_Runner/
│   └── Connect4.Console/     Version console : jouer, entraîner l'IA       (Console App)
└── Affichage_Godot/
    └── connect-4/            Version Godot 4.7 .NET                        (projet Godot)
```

**Qui utilise qui :**

```
 Console ──┐
           ├──►  Connect4.Core (Game)  ──►  Connect4.ML
 Godot  ───┘
```

- La console et Godot ne connaissent **que `Game`**. Ils affichent et transmettent les clics / saisies, rien d'autre.
- **Seul `Game` utilise le ML** (consigne du cours). Le ML ne connaît pas le Core : il manipule des états (`string`), des actions (`int`) et des récompenses (`double`).
- Le Core ne contient **aucun affichage** (pas de couleur, de position à l'écran, de `Console.WriteLine`).

## Utiliser `Game` (console comme Godot)

C'est la seule classe à connaître.

```csharp
// Deux humains, plateau classique 7 x 6
var game = new Game();

// Contre l'IA (elle joue O / Player2), avec un modèle déjà entraîné
var game = new Game(7, 6, aiPlayer: CellPlayer.Player2, trainAi: false, modelPath: "chemin/vers/connect4_7x6.txt");
```

| Paramètre du constructeur | Rôle |
|---|---|
| `column`, `row` | taille du plateau (7 x 6 par défaut) |
| `aiPlayer` | `Player1` ou `Player2` pour jouer contre l'IA, `Empty` (défaut) pour deux humains |
| `trainAi` | `true` : entraîne l'IA au démarrage (long !) ; `false` : charge `modelPath` |
| `trainingEpisodes` | nombre de parties d'entraînement si `trainAi` est vrai |
| `modelPath` | fichier du modèle : chargé si `trainAi = false`, sauvegardé si `trainAi = true` |

| Membre | Rôle |
|---|---|
| `Play(colonne)` | coup d'un humain (colonnes numérotées **à partir de 0**). Renvoie un `MoveResult` |
| `IsAiTurn` | vrai quand c'est à l'IA de jouer |
| `PlayAiTurn()` | fait jouer l'IA |
| `Board` | le plateau en lecture seule : `Columns`, `Rows`, `GetCellContent(colonne, ligne)` |
| `LastMove` | la dernière case jouée `(Column, Row)`, ou `null` |
| `CurrentPlayer`, `State`, `Winner`, `AiPlayer` | à qui le tour, partie en cours / gagnée / nulle, le gagnant, le joueur IA |
| `Reset()` | nouvelle partie (l'IA garde ce qu'elle a appris) |

`MoveResult` : `Ok`, `InvalidColumn` (hors plateau), `ColumnFull`, `GameOver`, `NotYourTurn` (c'est au tour de l'IA).

> ⚠️ **La ligne 0 est en BAS du plateau** (là où tombent les premiers jetons).

## Connect4.Core – les règles

| Fichier | Rôle |
|---|---|
| `Game.cs` | **point d'entrée** : tours, victoire, nul, joueur IA (appels au ML) |
| `Board.cs` / `IReadOnlyBoard.cs` | la grille ; l'extérieur ne la voit qu'en lecture seule |
| `WinChecker.cs` | détecte un alignement de 4 autour du dernier jeton posé |
| `CellPlayer.cs` | contenu d'une case : `Empty`, `Player1`, `Player2` |
| `StateEncoder.cs` | traduit le plateau en état (`string`) pour le ML, du point de vue du joueur qui doit jouer |
| `Connect4Environment.cs` | terrain d'entraînement de l'IA : l'adversaire (aléatoire) fait partie de l'environnement |
| `AiEvaluation.cs` | résultat d'une évaluation de l'IA (victoires, nuls, défaites) |

## Connect4.ML – l'IA

| Fichier | Rôle |
|---|---|
| `QLearningAgent.cs` | **Q-learning tabulaire** : `Init`, `Train`, `Predict`, `Load` (+ `Save`) |
| `IEnvironment.cs` | ce qu'un environnement doit fournir pour l'entraînement : `Reset`, `AvailableActions`, `Step` |

**Principe :** l'IA joue des milliers de parties contre un adversaire aléatoire. Après chaque coup, elle met à jour la valeur du coup joué : `Q(s,a) ← Q(s,a) + α [r + γ max Q(s',·) − Q(s,a)]`. Récompenses : +1 victoire, −1 défaite, 0 sinon. Réglages (dans `Game`) : α = 0,2, γ = 0,95, ε = 0,1.

**Le modèle** est un fichier texte : une ligne par position rencontrée, `état;valeur_colonne_0;valeur_colonne_1;…`.

## Connect4.Console – la version console

```bash
dotnet run --project Console_Runner/Connect4.Console
```

Menu : **1.** joueur contre joueur · **2.** joueur contre IA (affiche ce que l'IA pense de chaque colonne) · **3.** entraîner l'IA (progression et taux de victoire, puis sauvegarde).

Les modèles sont rangés dans `Console_Runner/Connect4.Console/bin/Debug/net8.0/models/` :
`connect4_7x6.txt` (plateau classique) et `connect4_5x4.txt` (petit plateau). **C'est la console qui fabrique le modèle que Godot charge.**

## Notes techniques

- Tout cible `net8.0` (le runtime de Godot 4) : ne pas monter de version. La console a `RollForward=Major` pour tourner sur un runtime plus récent.
- La CLI `dotnet` ne lit les `.slnx` qu'à partir du SDK 9.0.200 : en cas de doute, lancer les `.csproj`.
- Le `.gitignore` exclut `bin/`, `obj/`, `.idea/`, `.vs/`, `.godot/`.

## Planning

| Date | Étape |
|---|---|
| 23 nov. | Logique C# + console |
| 24 nov. | Hackathon packages |
| 25 nov. | IA (Q-learning) + entraînement, version Godot |
| 16 déc. | Soutenance |
