namespace Connect4.Core;

public static class WinChecker
{
    private static bool CheckColumn(Board board, int column, CellPlayer player)
    {
        int chipCount = 0;
        for (int i = 0; i < board.Rows; i++)
        {
            if (board.GetCellContent(column, i) == player)
            {
                chipCount++;
                if (chipCount >= 4)
                {
                    return true;
                }
            }
            else
            {
                chipCount = 0;
            }
        }

        return false;
    }
    private static bool CheckRow(Board board, int row, CellPlayer player)
    {
        int chipCount = 0;
        for (int i = 0; i < board.Columns; i++)
        {
            if (board.GetCellContent(i, row) == player)
            {
                chipCount++;
                if (chipCount >= 4)
                {
                    return true;
                }
            }
            else
            {
                chipCount = 0;
            }
        }

        return false;
    }
    private static bool CheckRisingDiagonal(Board board, int column, int row, CellPlayer player)
    {
        int c = column;
        int r = row;
        while (board.IsInside(c-1, r-1))
        {
            c--;
            r--;
        }
        int chipCount = 0;

        while (board.IsInside(c, r))
        {
            if (board.GetCellContent(c, r) == player)
            {
                chipCount++;
                if (chipCount >= 4)
                {
                    return true;
                }
            }
            else
            {
                chipCount = 0;
            }

            r++;
            c++;
        }
        return false;
    }
    private static bool CheckDescendingDiagonal(Board board, int column, int row, CellPlayer player)
    {
        int c = column;
        int r = row;
        while (board.IsInside(c+1, r-1))
        {
            c++;
            r--;
        }
        int chipCount = 0;

        while (board.IsInside(c, r))
        {
            if (board.GetCellContent(c, r) == player)
            {
                chipCount++;
                if (chipCount >= 4)
                {
                    return true;
                }
            }
            else
            {
                chipCount = 0;
            }
            
            c--;
            r++;
        }
        return false;
    }

    /// <summary>
    /// Le jeton posé en (column, row) crée-t-il un alignement de 4 ?
    /// La couleur est lue sur le plateau : pas besoin de la passer.
    /// </summary>
    public static bool IsWinningMove(Board board, int column, int row)
    {
        CellPlayer player = board.GetCellContent(column, row);
        if (player == CellPlayer.Empty)
        {
            throw new ArgumentException($"La case ({column}, {row}) est vide : aucun jeton à vérifier.");
        }

        if (CheckColumn(board, column, player)) return true;
        if (CheckRow(board, row, player)) return true;
        if (CheckRisingDiagonal(board, column, row, player)) return true;
        if (CheckDescendingDiagonal(board, column, row, player)) return true;
        return false;
    }
}