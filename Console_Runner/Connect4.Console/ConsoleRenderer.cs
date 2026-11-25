using Connect4.Core;

namespace Connect4.ConsoleApp;

/// <summary>
/// Tout l'affichage console. Le Core ne connaît ni les caractères, ni les couleurs :
/// c'est ici, et seulement ici, qu'on décide à quoi ressemble le jeu.
/// </summary>
public static class ConsoleRenderer
{
    private const ConsoleColor Player1Color = ConsoleColor.Red;
    private const ConsoleColor Player2Color = ConsoleColor.Yellow;
    private const ConsoleColor LastMoveBackground = ConsoleColor.DarkGray;

    /// <summary>Efface l'écran (sans planter si la console ne le permet pas).</summary>
    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Sortie redirigée ou console qui ne gère pas l'effacement : on continue sans effacer.
            Console.WriteLine();
        }
    }

    public static void Title(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {text} ===");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Affiche le plateau, ligne du haut en premier (la ligne 0 est en bas),
    /// puis les numéros de colonnes à partir de 1. Le dernier coup joué est surligné.
    /// </summary>
    public static void DrawBoard(IReadOnlyBoard board, (int Column, int Row)? lastMove = null)
    {
        for (int row = board.Rows - 1; row >= 0; row--)
        {
            Console.Write(" |");
            for (int column = 0; column < board.Columns; column++)
            {
                bool isLastMove = lastMove.HasValue
                                  && lastMove.Value.Column == column
                                  && lastMove.Value.Row == row;
                WriteCell(board.GetCellContent(column, row), isLastMove);
                Console.Write('|');
            }

            Console.WriteLine();
        }

        Console.Write(" +");
        for (int column = 0; column < board.Columns; column++)
        {
            Console.Write("-+");
        }

        Console.WriteLine();

        Console.Write("  ");
        for (int column = 1; column <= board.Columns; column++)
        {
            Console.Write(column.ToString().PadRight(2));
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    private static void WriteCell(CellPlayer cell, bool highlight)
    {
        if (cell != CellPlayer.Empty)
        {
            Console.ForegroundColor = PlayerColor(cell);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        if (highlight)
        {
            Console.BackgroundColor = LastMoveBackground;
        }

        Console.Write(Symbol(cell));
        Console.ResetColor();
    }

    /// <summary>Le caractère d'une case : 'X' pour Player1, 'O' pour Player2, '.' si vide.</summary>
    public static char Symbol(CellPlayer cell)
    {
        if (cell == CellPlayer.Player1)
        {
            return 'X';
        }

        if (cell == CellPlayer.Player2)
        {
            return 'O';
        }

        return '.';
    }

    /// <summary>Le nom d'un joueur dans les messages : "X", "O", ou "personne" pour Empty.</summary>
    public static string PlayerName(CellPlayer player)
    {
        if (player == CellPlayer.Empty)
        {
            return "personne";
        }

        return Symbol(player).ToString();
    }

    public static ConsoleColor PlayerColor(CellPlayer player)
    {
        if (player == CellPlayer.Player1)
        {
            return Player1Color;
        }

        return Player2Color;
    }

    /// <summary>Écrit le nom du joueur dans sa couleur, sans retour à la ligne.</summary>
    public static void WritePlayer(CellPlayer player)
    {
        Console.ForegroundColor = PlayerColor(player);
        Console.Write(PlayerName(player));
        Console.ResetColor();
    }

    /// <summary>Affiche ce que l'IA pensait de chaque colonne avant de jouer ; la colonne choisie est mise en avant.</summary>
    public static void ShowAiValues(IReadOnlyList<(int Column, double Value)> values, int chosenColumn)
    {
        if (values.Count == 0)
        {
            return;
        }

        Console.Write("  Valeurs de l'IA : ");
        foreach (var (column, value) in values)
        {
            string text = $"{column + 1}: {value.ToString("+0.00;-0.00;0.00")}";
            if (column == chosenColumn)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{text}]");
                Console.ResetColor();
            }
            else
            {
                Console.Write($" {text} ");
            }

            Console.Write(' ');
        }

        Console.WriteLine();
    }

    /// <summary>Barre de progression en caractères, colorée selon le niveau.</summary>
    public static void WriteProgressBar(double ratio, int width = 20)
    {
        int filled = (int)Math.Round(Math.Clamp(ratio, 0, 1) * width);

        if (ratio >= 0.8)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (ratio >= 0.6)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.Write(new string('█', filled));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('░', width - filled));
        Console.ResetColor();
    }

    /// <summary>Message d'erreur (colonne pleine, saisie invalide...), en rouge.</summary>
    public static void Error(string message)
    {
        WriteColored(message, ConsoleColor.Red);
    }

    /// <summary>Message de réussite, en vert.</summary>
    public static void Success(string message)
    {
        WriteColored(message, ConsoleColor.Green);
    }

    /// <summary>Message d'information, en gris.</summary>
    public static void Info(string message)
    {
        WriteColored(message, ConsoleColor.Gray);
    }

    private static void WriteColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
