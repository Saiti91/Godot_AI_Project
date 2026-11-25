# Guide Godot – afficher le Puissance 4 (pour débutants)

Ce guide part du projet Godot vide `Affichage_Godot/connect-4` et arrive à un **Puissance 4 jouable contre l'IA**, en **un seul script**. Pas besoin de connaître Godot à l'avance : suis les étapes dans l'ordre.

> Les noms de menus sont donnés en anglais, avec le français entre parenthèses si ton Godot est en français.

---

## Ce qu'il faut comprendre avant de commencer (2 minutes)

- **Toute la logique du jeu existe déjà** dans `Solution_Core/Connect4.Core`, dans la classe `Game`. Godot n'a **rien à calculer** : il **dessine** le plateau et **transmet les clics** à `Game`.
- Dans Godot, tout est fait de **nœuds** (*nodes*) rangés en arbre dans une **scène**. On va en utiliser deux :
  - `Main` (un `Node2D`) : il dessine le plateau et reçoit les clics. C'est lui qui porte notre script C#.
  - `Status` (un `Label`) : un texte qui affiche « Tour de Rouge », « Jaune gagne ! »…
- **L'IA n'apprend pas dans Godot** (ce serait trop long, la fenêtre se figerait). Elle est **entraînée dans la console**, sauvegardée dans un fichier, et Godot **charge** ce fichier.

```
Godot (clics + dessin)  ──►  Game (règles + IA)  ──►  modèle de l'IA (fichier .txt)
```

---

## Étape 1 – Préparer le modèle de l'IA (dans la console)

1. Lance la console :
   ```bash
   dotnet run --project Console_Runner/Connect4.Console
   ```
2. Menu **3. Entraîner l'IA** → plateau **1. Classique 7 x 6** → Entrée (100 000 parties).
3. À la fin, le fichier est créé ici :
   `Console_Runner/Connect4.Console/bin/Debug/net8.0/models/connect4_7x6.txt`
4. Dans `Affichage_Godot/connect-4/`, crée un dossier **`models`** et **copie-y** ce fichier :
   `Affichage_Godot/connect-4/models/connect4_7x6.txt`

> Pour tester plus vite, tu peux faire pareil avec le petit plateau 5 x 4 (`connect4_5x4.txt`) : voir « Changer de plateau » à la fin.

---

## Étape 2 – Ouvrir le projet

1. Lance **Godot .NET** (l'exécutable qui contient `mono` dans son nom).
2. **Import** (Importer) → choisis `Affichage_Godot/connect-4/project.godot` → **Import & Edit**.

---

## Étape 3 – Créer la scène

1. Menu **Scene** (Scène) → **New Scene** (Nouvelle scène).
2. Dans le panneau **Scene** à gauche, clique sur **2D Scene** (Scène 2D). Un nœud `Node2D` apparaît.
3. Double-clique dessus et renomme-le **`Main`**.
4. Avec `Main` sélectionné, clique sur le **+** (Add Child Node / Ajouter un nœud enfant), cherche **`Label`**, valide.
5. Renomme ce `Label` en **`Status`** (exactement ce nom : le script le cherche).
6. **Ctrl + S** → enregistre sous `main.tscn`.

Ton arbre doit ressembler à ça :

```
Main      (Node2D)
└─ Status (Label)
```

---

## Étape 4 – Attacher le script C#

1. Clic droit sur **`Main`** → **Attach Script** (Attacher un script).
2. **Language** : **C#**. **Path** : `res://Main.cs`. Clique sur **Create** (Créer).

Godot crée `Main.cs` **et**, la première fois, les fichiers du projet C# : `Connect_4.csproj` et `Connect_4.sln` dans `Affichage_Godot/connect-4/`.

> Si tu ne vois pas `Connect_4.csproj` : menu **Project** → **Tools** → **C#** → **Create C# solution**.

---

## Étape 5 – Relier Godot au Core

Godot doit savoir où se trouve `Game`. Ouvre **`Affichage_Godot/connect-4/Connect_4.csproj`** dans Rider (ou un éditeur de texte) et ajoute ce bloc **juste avant** la dernière ligne `</Project>` :

```xml
  <ItemGroup>
    <ProjectReference Include="..\..\Solution_Core\Connect4.Core\Connect4.Core.csproj" />
  </ItemGroup>
```

Le chemin part du dossier `connect-4`, remonte deux fois (`..\..\` = `Godot_AI_Project`), puis descend vers le Core. **Pas besoin de référencer `Connect4.ML`** : il vient automatiquement avec le Core.

Ne touche pas au reste du fichier (en particulier la première ligne `<Project Sdk="Godot.NET.Sdk/…">`).

---

## Étape 6 – Le script

Ouvre `Affichage_Godot/connect-4/Main.cs` et **remplace tout son contenu** par :

```csharp
using Godot;
using Connect4.Core;

/// <summary>
/// Affiche le Puissance 4 et transmet les clics à Game.
/// Toute la logique (règles, IA) est dans Connect4.Core : ici on ne fait que dessiner.
/// </summary>
public partial class Main : Node2D
{
    // ---- Réglages modifiables dans l'Inspecteur de Godot (panneau de droite) ----
    [Export] public float CellSize = 80f;          // taille d'une case, en pixels
    [Export] public bool PlayAgainstAi = true;     // false = deux humains
    [Export] public string ModelPath = "res://models/connect4_7x6.txt";
    [Export] public float AiDelay = 0.5f;          // pause avant le coup de l'IA, en secondes

    private const int Columns = 7;
    private const int Rows = 6;

    private Game _game;
    private Label _status;
    private bool _aiIsThinking;

    // Appelé une fois, au lancement de la scène
    public override void _Ready()
    {
        Position = new Vector2(40, 40);                     // marge autour du plateau
        _status = GetNode<Label>("Status");                 // le Label enfant nommé "Status"
        _status.Position = new Vector2(0, Rows * CellSize + 15);

        if (PlayAgainstAi)
        {
            // res://... est un chemin Godot : on le convertit en vrai chemin pour .NET
            string path = ProjectSettings.GlobalizePath(ModelPath);
            _game = new Game(Columns, Rows, aiPlayer: CellPlayer.Player2, trainAi: false, modelPath: path);
        }
        else
        {
            _game = new Game(Columns, Rows);
        }

        Refresh();
    }

    // Appelé à chaque clic / touche que rien d'autre n'a utilisé
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse || !mouse.Pressed || mouse.ButtonIndex != MouseButton.Left)
        {
            return;     // on ne s'intéresse qu'au clic gauche
        }

        // Partie finie : un clic relance une partie
        if (_game.State != GameState.InProgress)
        {
            _game.Reset();
            Refresh();
            return;
        }

        if (_aiIsThinking)
        {
            return;     // on attend que l'IA ait joué
        }

        // Position de la souris → numéro de colonne (0 à 6)
        int column = (int)(GetLocalMousePosition().X / CellSize);
        MoveResult result = _game.Play(column);
        Refresh();

        if (result == MoveResult.Ok && _game.IsAiTurn)
        {
            PlayAiAfterDelay();
        }
    }

    // Attend un peu (pour qu'on voie notre coup), puis fait jouer l'IA
    private async void PlayAiAfterDelay()
    {
        _aiIsThinking = true;
        await ToSignal(GetTree().CreateTimer(AiDelay), SceneTreeTimer.SignalName.Timeout);
        _game.PlayAiTurn();
        _aiIsThinking = false;
        Refresh();
    }

    // Met à jour le texte et demande à Godot de redessiner (il rappellera _Draw)
    private void Refresh()
    {
        if (_game.State == GameState.Won)
        {
            _status.Text = PlayerName(_game.Winner) + " gagne !   (clic pour rejouer)";
        }
        else if (_game.State == GameState.Draw)
        {
            _status.Text = "Match nul !   (clic pour rejouer)";
        }
        else
        {
            _status.Text = "Tour de " + PlayerName(_game.CurrentPlayer);
        }

        QueueRedraw();
    }

    // Dessine le plateau. Godot l'appelle après chaque QueueRedraw().
    public override void _Draw()
    {
        if (_game == null)
        {
            return;
        }

        IReadOnlyBoard board = _game.Board;

        // Le fond bleu du plateau
        DrawRect(new Rect2(0, 0, board.Columns * CellSize, board.Rows * CellSize), new Color(0.12f, 0.31f, 0.85f));

        // Un rond par case
        for (int column = 0; column < board.Columns; column++)
        {
            for (int row = 0; row < board.Rows; row++)
            {
                Vector2 center = CellCenter(column, row);
                DrawCircle(center, CellSize * 0.4f, CellColor(board.GetCellContent(column, row)));
            }
        }

        // Un contour blanc autour du dernier jeton joué
        if (_game.LastMove.HasValue)
        {
            Vector2 center = CellCenter(_game.LastMove.Value.Column, _game.LastMove.Value.Row);
            DrawArc(center, CellSize * 0.42f, 0, Mathf.Tau, 32, Colors.White, 3);
        }
    }

    // ⚠️ Dans le Core, la ligne 0 est EN BAS. Dans Godot, l'axe Y descend.
    // Il faut donc inverser : la ligne 0 est dessinée tout en bas de l'écran.
    private Vector2 CellCenter(int column, int row)
    {
        float x = (column + 0.5f) * CellSize;
        float y = (Rows - 1 - row + 0.5f) * CellSize;
        return new Vector2(x, y);
    }

    private static Color CellColor(CellPlayer cell)
    {
        if (cell == CellPlayer.Player1)
        {
            return Colors.Red;
        }

        if (cell == CellPlayer.Player2)
        {
            return Colors.Yellow;
        }

        return Colors.White;    // case vide
    }

    private string PlayerName(CellPlayer player)
    {
        if (PlayAgainstAi && player == _game.AiPlayer)
        {
            return "L'IA (jaune)";
        }

        if (player == CellPlayer.Player1)
        {
            return "Rouge";
        }

        return "Jaune";
    }
}
```

> Pourquoi `PlayerName` et pas `Name` ? Parce que `Name` existe déjà dans tous les nœuds Godot (c'est le nom du nœud dans l'arbre).

---

## Étape 7 – Lancer

1. En haut à droite de Godot, clique sur **Build** (l'icône marteau) : ça compile le C# et le Core. En cas d'erreur, regarde l'onglet **MSBuild** en bas.
2. Appuie sur **F5** (Run Project). La première fois, Godot demande la scène principale : clique sur **Select Current** (Choisir l'actuelle) pour prendre `main.tscn`.
3. Clique dans une colonne : ton jeton rouge tombe, l'IA répond en jaune une demi-seconde après.

---

## Si ça ne marche pas

| Problème | Cause probable | Solution |
|---|---|---|
| Erreur `The type or namespace name 'Connect4' could not be found` | la référence de l'étape 5 manque ou le chemin est faux | vérifier le bloc `<ProjectReference>` puis **Build** |
| Erreur `FileNotFoundException … connect4_7x6.txt` | le modèle n'est pas dans `connect-4/models/` | refaire l'étape 1 (copie du fichier) |
| `InvalidDataException … valeurs trouvées` | le modèle ne correspond pas à la taille du plateau (ex. fichier 5 x 4 avec un plateau 7 x 6) | utiliser le bon fichier (voir ci-dessous) |
| `Node not found: "Status"` | le Label ne s'appelle pas exactement `Status`, ou n'est pas **enfant** de `Main` | le renommer / le déplacer sous `Main` |
| L'écran reste gris, rien ne s'affiche | le script n'est pas attaché à `Main`, ou la scène lancée n'est pas `main.tscn` | vérifier l'icône de script à côté de `Main` ; **Project Settings → Application → Run → Main Scene** |
| Les jetons apparaissent en haut au lieu d'en bas | la conversion ligne → position n'inverse pas Y | garder `Rows - 1 - row` dans `CellCenter` |

---

## Changer de plateau (5 x 4, l'IA y est plus forte)

Dans `Main.cs`, mets `Columns = 5`, `Rows = 4`, et dans l'**Inspecteur** (panneau de droite, nœud `Main` sélectionné) change **Model Path** en `res://models/connect4_5x4.txt` (après avoir copié ce fichier, comme à l'étape 1).

## Jouer à deux humains

Dans l'Inspecteur, décoche **Play Against Ai**. Plus besoin de modèle.

---

## Pour aller plus loin (une fois que tout marche)

- **De vrais jetons** : une scène `Disc.tscn` (un `Sprite2D` avec une image de rond) instanciée à chaque coup avec `Instantiate()` (exemple 03 du cours), au lieu de `DrawCircle`.
- **Une animation de chute** avec un `Tween` sur la position du jeton.
- **Choisir qui commence** : avec `aiPlayer: CellPlayer.Player1`, l'IA joue en premier. Il faut alors appeler `PlayAiAfterDelay()` dès le `_Ready` (et après chaque `Reset`) quand `_game.IsAiTurn` est vrai.
- **Un bouton « Rejouer »** (nœud `Button` + son signal `pressed`) au lieu du clic sur le plateau.
- **Exporter le jeu** (.exe) : `res://` est alors empaqueté et `GlobalizePath` ne suffit plus. Il faudra copier le modèle vers `user://` au premier lancement (avec `FileAccess` de Godot), et ajouter `models/*` dans **Export → Resources → Filters to export non-resource files**. On s'en occupera à ce moment-là.
