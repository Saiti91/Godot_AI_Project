namespace Connect4.Core;

enum WinState
{
    None,
    Win,
    Lose,
    Null
}

public class Game
{
    // Ce qu'elle doit retenir
    //     Le plateau, créé dans son constructeur.
    //     Le joueur courant, Player1 au départ.
    //     L'état de la partie : en cours, gagnée ou nulle. C'est un nouvel enum à créer.
    //     Le gagnant : Empty tant que personne n'a gagné.
    private Board _board;
    private Cell _currentPlayer;
    private WinState _state =  WinState.None;
    private Cell _winner = Cell.Empty;
    
    
    // Play(int column) est le cœur de la classe. Les étapes, dans l'ordre :
    //
    //     Refuser le coup si la partie est finie.
    //     Refuser le coup si la colonne est invalide. Tu as IsValidColumn.
    //     Refuser le coup si la colonne est pleine. Tu as ColumnFreeForPlay.
    //     Poser le jeton du joueur courant, en récupérant la ligne d'arrivée.
    //     Vérifier la victoire avec WinChecker. Si elle est gagnée, mets à jour l'état et le gagnant.
    //     Sinon, vérifier si le plateau est plein. Si oui, l'état passe à match nul.
    //     Sinon, passer au joueur suivant.

    void Play(int column)
    {
        int lastPlayedRow;
        if(_state != WinState.None) return;
        if(!_board.IsValidColumn(column)) return;
        if (!_board.ColumnFreeForPlay(column)) return;
        lastPlayedRow = _board.AddChip(column,_currentPlayer);
        if (WinChecker.IsWinningMove(_board, column, lastPlayedRow))
        {
            _state = WinState.Win;
            _winner = _currentPlayer;
        }
        else if (_board.IsFull)
        {
            _state = WinState.Null;
        }
        else
        {
            //On alterne entre les deux joueurs (1 et 2)
            _currentPlayer = _currentPlayer == Cell.Player1 ? Cell.Player2 : Cell.Player1;
            
        }
    }

}
