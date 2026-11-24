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
    GameOver
}

public class Game
{
    private Board _board;
    private int _boardColumn;
    private int _boardRow;
    private CellPlayer _currentPlayer;
    private GameState _state =  GameState.InProgress;
    private CellPlayer _winner = CellPlayer.Empty;
    
    public IReadOnlyBoard Board => _board;

    public CellPlayer CurrentPlayer => _currentPlayer;

    public GameState State => _state;

    public CellPlayer Winner => _winner;
    
    public Game(int column, int row)
    {
        _boardColumn = column;
        _boardRow = row;
        _board = new Board(column, row);
        _currentPlayer = CellPlayer.Player1;
        _state = GameState.InProgress;
        _winner = CellPlayer.Empty;
    }
    
    public MoveResult Play(int column)
    {
        if(_state != GameState.InProgress) return MoveResult.GameOver;
        if(!_board.IsValidColumn(column)) return MoveResult.InvalidColumn;
        if (!_board.ColumnFreeForPlay(column)) return MoveResult.ColumnFull;
        int lastPlayedRow = _board.AddChip(column,_currentPlayer);
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
            //On alterne entre les deux joueurs (1 et 2)
            _currentPlayer = _currentPlayer == CellPlayer.Player1 ? CellPlayer.Player2 : CellPlayer.Player1;
        }
        return MoveResult.Ok;
    }

    public void Reset()
    {
        _board = new Board(_boardColumn, _boardRow);
        _currentPlayer = CellPlayer.Player1;
        _state = GameState.InProgress;
        _winner = CellPlayer.Empty;
    }
}
