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

		try
		{
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
		}
		catch (System.Exception e)
		{
			// Modèle absent ou incompatible : on l'affiche à l'écran au lieu de planter au premier clic
			_status.Text = "Impossible de démarrer : " + e.Message;
			GD.PrintErr(e.Message);
			return;
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

		if (_game == null)
		{
			return;     // la partie n'a pas pu démarrer (voir le message affiché)
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
