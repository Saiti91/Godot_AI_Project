namespace Connect4.Core;

public interface IReadOnlyBoard
{
    int Columns { get; }
    int Rows { get; }
    bool IsFull { get; }
    CellPlayer GetCellContent(int column, int row);
    bool IsInside(int column, int row);
    bool IsValidColumn(int column);
    bool ColumnFreeForPlay(int column);
}