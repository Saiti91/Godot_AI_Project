namespace Connect4.Core;

/// <summary>
/// Plateau du puissance 4. La ligne 0 est en bas.
/// </summary>
public class Board
{
    private readonly int _maxColumn;
    private readonly int _maxRow;
    private readonly Cell[,] _board;
    private int _chipCount;

    public Board(int columns, int rows)
    {
        _maxColumn = columns;
        _maxRow = rows;
        _board = new Cell[columns, rows];
    }

    /// <summary>Nombre de colonnes.</summary>
    public int Columns => _maxColumn;

    /// <summary>Nombre de lignes.</summary>
    public int Rows => _maxRow;

    /// <summary>Toutes les cases sont occupées.</summary>
    public bool IsFull => _chipCount == _maxColumn * _maxRow;

    // Répond à la question : la valeur est-elle dans [0, count[ ?
    private static bool IsInRange(int value, int count)
    {
        return value >= 0 && value < count;
    }

    // Bloque si la valeur est hors bornes, ne fait rien sinon.
    private static void EnsureInRange(int value, int count, string paramName)
    {
        if (!IsInRange(value, count))
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"Doit être entre 0 et {count - 1}.");
        }
    }

    public bool IsValidColumn(int column)
    {
        return IsInRange(column, _maxColumn);
    }

    public Cell GetCellContent(int column, int row)
    {
        EnsureInRange(column, _maxColumn, nameof(column));
        EnsureInRange(row, _maxRow, nameof(row));

        return _board[column, row];
    }

    public bool ColumnFreeForPlay(int column)
    {
        EnsureInRange(column, _maxColumn, nameof(column));
        return _board[column, _maxRow - 1] == Cell.Empty;
    }

    /// <summary>Fait tomber un jeton dans la colonne et renvoie la ligne où il atterrit.</summary>
    public int AddChip(int column, Cell player)
    {
        EnsureInRange(column, _maxColumn, nameof(column));
        if (player == Cell.Empty) throw new ArgumentException("Impossible de jouer une case vide.", nameof(player));
        if (!ColumnFreeForPlay(column)) throw new InvalidOperationException($"La colonne {column} est pleine.");

        for (int i = 0; i < _maxRow; i++)
        {
            if (GetCellContent(column, i) == Cell.Empty)
            {
                _board[column, i] = player;
                _chipCount += 1;
                return i;
            }
        }

        throw new InvalidOperationException("Impossible d'ajouter le jeton : aucune case libre trouvée.");
    }

    public bool IsInside(int column, int row) => IsInRange(column, _maxColumn) && IsInRange(row, _maxRow);
    
}