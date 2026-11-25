using System.Text;

namespace Connect4.Core;

/// <summary>Traduit un plateau en état (string) pour le ML, du point de vue du joueur qui doit jouer.</summary>
public static class StateEncoder
{
    public static string Encode(IReadOnlyBoard board, CellPlayer currentPlayer)
    {
        var stringBuild = new StringBuilder(board.Columns * board.Rows);

        for (int column = 0; column < board.Columns; column++)
        {
            for (int row = 0; row < board.Rows; row++)
            {
                var currentCell = board.GetCellContent(column, row);
                if (currentCell == CellPlayer.Empty)
                {
                    stringBuild.Append('0');
                }
                else if (currentCell == currentPlayer)
                {
                    stringBuild.Append('1');
                }
                else
                {
                    stringBuild.Append('2');
                }
            }
        }
        return stringBuild.ToString();
    }
}