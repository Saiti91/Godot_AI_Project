using Connect4.ML;

namespace Connect4.Core;

/// <summary>
/// Environnement d'entraînement : transforme le Puissance 4 à deux joueurs en problème à un seul agent.
/// L'adversaire fait partie de l'environnement : un Step = le coup de l'agent, puis la réponse de l'adversaire.
/// </summary>
public class Connect4Environment : IEnvironment
{
    private Board _board;
    private int _column;
    private int _row;
    private CellPlayer _agentPlayer;
    private Random _random = new Random();
    private bool _done = false;

    public Connect4Environment(int columns = 7, int rows = 6, int? seed = null)
    {
        _column = columns;
        _row = rows;
        if (seed != null) _random = new Random(seed.Value);
        _board = new Board(columns, rows);
    }

    public string Reset()
    {
        // TODO :
        // 1. plateau vide, partie non finie
        // 2. tirer au hasard si l'agent est Player1 ou Player2
        // 3. si l'agent est Player2, l'adversaire joue un premier coup
        // 4. renvoyer l'état du point de vue de l'agent

        _board = new Board(_column, _row);
        _done = false;
        if (_random.Next(2) == 0)
        {
            _agentPlayer = CellPlayer.Player1;
        }
        else
        {
            _agentPlayer = CellPlayer.Player2;
        }

        if (_agentPlayer == CellPlayer.Player2)
        {
            PlayOpponent();
        }

        return StateEncoder.Encode(_board, _agentPlayer);
    }

    private CellPlayer Opponent()
    {
        if (_agentPlayer == CellPlayer.Player1)
        {
            return CellPlayer.Player2;
        }

        return CellPlayer.Player1;
    }

    private (int Column, int Row) PlayOpponent()
    {
        // 1. récupérer les colonnes jouables
        // 2. en tirer une au hasard
        // 3. poser le jeton de l'adversaire et récupérer la ligne
        // 4. renvoyer la colonne et la ligne
        var availableColumns = AvailableActions();
        var index = _random.Next(availableColumns.Count);

        int column = availableColumns[index];
        int droppedRow = _board.AddChip(column, Opponent());
        return (column, droppedRow);
    }

    public IReadOnlyList<int> AvailableActions()
    {
        if (_done) return new List<int>();

        var availableAction = new List<int>();
        for (int i = 0; i < _column; i++)
        {
            if (_board.ColumnFreeForPlay(i))
            {
                availableAction.Add(i);
            }
        }

        return availableAction;
    }

    public (string NextState, double Reward, bool Done) Step(int action)
    {
        // 1. Le coup de l'agent
        int agentRow = _board.AddChip(action, _agentPlayer);
        if (WinChecker.IsWinningMove(_board, action, agentRow))
        {
            return End(1.0);
        }

        if (_board.IsFull)
        {
            return End(0.0);
        }

        // 2. La réponse de l'adversaire
        var (opponentColumn, opponentRow) = PlayOpponent();
        if (WinChecker.IsWinningMove(_board, opponentColumn, opponentRow))
        {
            return End(-1.0);
        }

        if (_board.IsFull)
        {
            return End(0.0);
        }

        // 3. La partie continue
        return (StateEncoder.Encode(_board, _agentPlayer), 0.0, false);
    }

    /// <summary>Termine l'épisode : marque la partie finie et renvoie l'état final avec la récompense.</summary>
    private (string NextState, double Reward, bool Done) End(double reward)
    {
        _done = true;
        return (StateEncoder.Encode(_board, _agentPlayer), reward, true);
    }
}