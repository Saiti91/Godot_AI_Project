using Connect4.ML;

namespace Connect4.Core;

public enum GameState
{
    InProgress,
    Won,
    Draw
}

public enum MoveResult
{
    Ok,
    InvalidColumn,
    ColumnFull,
    GameOver,
    NotYourTurn
}

public class Game
{
    // Hyperparamètres de l'apprentissage (Q-learning)
    private const double Alpha = 0.2;
    private const double Gamma = 0.95;
    private const double Epsilon = 0.1;

    private Board _board;
    private readonly int _boardColumn;
    private readonly int _boardRow;
    private CellPlayer _currentPlayer;
    private GameState _state;
    private CellPlayer _winner;

    // Joueur virtuel : null / Empty si la partie se joue entre deux humains
    private readonly QLearningAgent? _agent;
    private readonly CellPlayer _aiPlayer;

    public IReadOnlyBoard Board => _board;

    public CellPlayer CurrentPlayer => _currentPlayer;

    public GameState State => _state;

    public CellPlayer Winner => _winner;

    /// <summary>Le joueur contrôlé par l'IA, ou Empty s'il n'y en a pas.</summary>
    public CellPlayer AiPlayer => _aiPlayer;

    /// <summary>Vrai quand c'est à l'IA de jouer : l'affichage doit alors appeler PlayAiTurn.</summary>
    public bool IsAiTurn => _agent != null && _state == GameState.InProgress && _currentPlayer == _aiPlayer;

    /// <param name="column">Nombre de colonnes du plateau.</param>
    /// <param name="row">Nombre de lignes du plateau.</param>
    /// <param name="aiPlayer">Joueur contrôlé par l'IA (Player1 ou Player2), Empty pour deux humains.</param>
    /// <param name="trainAi">Vrai : entraîne l'IA au démarrage (puis sauvegarde si modelPath est donné). Faux : charge modelPath.</param>
    /// <param name="trainingEpisodes">Nombre de parties d'entraînement (si trainAi est vrai).</param>
    /// <param name="modelPath">Fichier du modèle : destination de la sauvegarde, ou source du chargement.</param>
    public Game(int column = 7, int row = 6,
        CellPlayer aiPlayer = CellPlayer.Empty,
        bool trainAi = false,
        int trainingEpisodes = 100_000,
        string? modelPath = null)
    {
        _boardColumn = column;
        _boardRow = row;
        _board = new Board(column, row);
        _currentPlayer = CellPlayer.Player1;
        _state = GameState.InProgress;
        _winner = CellPlayer.Empty;
        _aiPlayer = aiPlayer;

        if (aiPlayer != CellPlayer.Empty)
        {
            _agent = CreateAgent(trainAi, trainingEpisodes, modelPath);
        }
    }

    private QLearningAgent CreateAgent(bool trainAi, int trainingEpisodes, string? modelPath)
    {
        var agent = new QLearningAgent();
        agent.Init(_boardColumn, Alpha, Gamma, Epsilon);

        if (trainAi)
        {
            agent.Train(new Connect4Environment(_boardColumn, _boardRow), trainingEpisodes);
            if (modelPath != null)
            {
                agent.Save(modelPath);
            }
        }
        else
        {
            if (modelPath == null)
            {
                throw new ArgumentException("Sans entraînement, il faut un modelPath à charger.", nameof(modelPath));
            }

            agent.Load(modelPath);
        }

        return agent;
    }

    /// <summary>Coup d'un joueur humain. Refusé si c'est au tour de l'IA.</summary>
    public MoveResult Play(int column)
    {
        if (IsAiTurn) return MoveResult.NotYourTurn;
        return ApplyMove(column);
    }

    /// <summary>Fait jouer l'IA : encode le plateau, demande un coup au ML, le joue.</summary>
    public MoveResult PlayAiTurn()
    {
        if (_state != GameState.InProgress) return MoveResult.GameOver;
        if (!IsAiTurn) return MoveResult.NotYourTurn;

        string state = StateEncoder.Encode(_board, _currentPlayer);
        int column = _agent!.Predict(state, AvailableColumns());
        return ApplyMove(column);
    }

    private MoveResult ApplyMove(int column)
    {
        if (_state != GameState.InProgress) return MoveResult.GameOver;
        if (!_board.IsValidColumn(column)) return MoveResult.InvalidColumn;
        if (!_board.ColumnFreeForPlay(column)) return MoveResult.ColumnFull;

        int lastPlayedRow = _board.AddChip(column, _currentPlayer);
        if (WinChecker.IsWinningMove(_board, column, lastPlayedRow))
        {
            _state = GameState.Won;
            _winner = _currentPlayer;
        }
        else if (_board.IsFull)
        {
            _state = GameState.Draw;
        }
        else
        {
            // On alterne entre les deux joueurs (1 et 2)
            if (_currentPlayer == CellPlayer.Player1)
            {
                _currentPlayer = CellPlayer.Player2;
            }
            else
            {
                _currentPlayer = CellPlayer.Player1;
            }
        }

        return MoveResult.Ok;
    }

    private List<int> AvailableColumns()
    {
        var columns = new List<int>();
        for (int c = 0; c < _boardColumn; c++)
        {
            if (_board.ColumnFreeForPlay(c))
            {
                columns.Add(c);
            }
        }

        return columns;
    }

    /// <summary>Nouvelle partie avec la même taille et la même IA (l'IA n'est pas réentraînée).</summary>
    public void Reset()
    {
        _board = new Board(_boardColumn, _boardRow);
        _currentPlayer = CellPlayer.Player1;
        _state = GameState.InProgress;
        _winner = CellPlayer.Empty;
    }
}