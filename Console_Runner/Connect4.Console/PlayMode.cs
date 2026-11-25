using Connect4.Core;

namespace Connect4.ConsoleApp;

/// <summary>Les parties jouables : joueur contre joueur, joueur contre IA, avec « Rejouer ».</summary>
public static class PlayMode
{
    // Parties d'entraînement si aucun modèle n'existe encore pour le plateau choisi
    private const int QuickTrainingEpisodes = 50_000;

    // Pause avant le coup de l'IA, pour qu'on ait le temps de voir ce qui se passe
    private const int AiDelayMs = 600;

    public static void PlayerVsPlayer()
    {
        var game = new Game();
        RunGames(game, "Joueur contre joueur");
    }

    public static void PlayerVsAi()
    {
        ConsoleRenderer.Clear();
        ConsoleRenderer.Title("Joueur contre IA");

        var size = ConsoleInput.ChooseBoardSize();
        if (size == null)
        {
            return;
        }

        var (columns, rows) = size.Value;

        CellPlayer aiPlayer;
        if (ConsoleInput.ReadYesNo("Veux-tu commencer ? Tu joueras X (o/n) : "))
        {
            aiPlayer = CellPlayer.Player2;
        }
        else
        {
            aiPlayer = CellPlayer.Player1;
        }

        Game? game = CreateAiGame(columns, rows, aiPlayer);
        if (game == null)
        {
            ConsoleInput.Pause();
            return;
        }

        RunGames(game, "Joueur contre IA");
    }

    /// <summary>Charge le modèle du plateau s'il existe, sinon entraîne rapidement une IA et la sauvegarde.</summary>
    private static Game? CreateAiGame(int columns, int rows, CellPlayer aiPlayer)
    {
        string path = ModelStore.PathFor(columns, rows);

        if (File.Exists(path))
        {
            try
            {
                ConsoleRenderer.Info($"Chargement du modèle : {path}");
                return new Game(columns, rows, aiPlayer, trainAi: false, modelPath: path);
            }
            catch (InvalidDataException e)
            {
                ConsoleRenderer.Error($"Modèle illisible : {e.Message}");
                ConsoleRenderer.Info("Supprime ce fichier ou réentraîne l'IA depuis le menu « Entraîner l'IA ».");
                return null;
            }
        }

        ConsoleRenderer.Info($"Aucun modèle pour un plateau {columns} x {rows} : entraînement rapide ({QuickTrainingEpisodes:N0} parties)...");
        ConsoleRenderer.Info("Pour une IA plus forte, utilise le menu « Entraîner l'IA ».");
        return new Game(columns, rows, aiPlayer, trainAi: true, trainingEpisodes: QuickTrainingEpisodes, modelPath: path);
    }

    /// <summary>Enchaîne les parties tant que le joueur veut rejouer.</summary>
    private static void RunGames(Game game, string title)
    {
        while (true)
        {
            bool quit = PlayOneGame(game, title);
            if (quit)
            {
                return;
            }

            if (!ConsoleInput.ReadYesNo("Rejouer ? (o/n) : "))
            {
                return;
            }

            game.Reset();
        }
    }

    /// <summary>Joue une partie complète. Renvoie vrai si le joueur a quitté en cours de route.</summary>
    private static bool PlayOneGame(Game game, string title)
    {
        string? message = null;
        bool messageIsError = false;
        IReadOnlyList<(int Column, double Value)>? aiValues = null;
        int aiColumn = -1;

        while (game.State == GameState.InProgress)
        {
            Draw(game, title, message, messageIsError, aiValues, aiColumn);
            message = null;
            messageIsError = false;
            aiValues = null;

            if (game.IsAiTurn)
            {
                ConsoleRenderer.Info("L'IA réfléchit...");
                Thread.Sleep(AiDelayMs);

                aiValues = game.GetAiColumnValues();
                game.PlayAiTurn();
                if (game.LastMove.HasValue)
                {
                    aiColumn = game.LastMove.Value.Column;
                    message = $"L'IA ({ConsoleRenderer.PlayerName(game.AiPlayer)}) a joué la colonne {aiColumn + 1}.";
                }

                continue;
            }

            int columns = game.Board.Columns;
            int? choice = ConsoleInput.ReadInt($"Colonne (1-{columns}, q pour quitter) : ", 1, columns);
            if (choice == null)
            {
                return true;
            }

            MoveResult result = game.Play(choice.Value - 1);
            if (result == MoveResult.ColumnFull)
            {
                message = "Cette colonne est pleine, choisis-en une autre.";
                messageIsError = true;
            }
        }

        Draw(game, title, message, messageIsError, aiValues, aiColumn);
        ShowResult(game);
        return false;
    }

    private static void Draw(Game game, string title, string? message, bool messageIsError,
        IReadOnlyList<(int Column, double Value)>? aiValues, int aiColumn)
    {
        ConsoleRenderer.Clear();
        ConsoleRenderer.Title(title);
        ConsoleRenderer.DrawBoard(game.Board, game.LastMove);

        if (aiValues != null)
        {
            ConsoleRenderer.ShowAiValues(aiValues, aiColumn);
        }

        if (message != null)
        {
            if (messageIsError)
            {
                ConsoleRenderer.Error(message);
            }
            else
            {
                ConsoleRenderer.Info(message);
            }
        }

        if (game.State == GameState.InProgress)
        {
            Console.Write("Tour de ");
            ConsoleRenderer.WritePlayer(game.CurrentPlayer);
            if (game.IsAiTurn)
            {
                Console.Write(" (IA)");
            }

            Console.WriteLine();
        }
    }

    private static void ShowResult(Game game)
    {
        Console.WriteLine();
        if (game.State == GameState.Draw)
        {
            ConsoleRenderer.Info("Match nul !");
            return;
        }

        if (game.AiPlayer == CellPlayer.Empty)
        {
            ConsoleRenderer.WritePlayer(game.Winner);
            Console.WriteLine(" gagne !");
            return;
        }

        if (game.Winner == game.AiPlayer)
        {
            ConsoleRenderer.Error("L'IA gagne !");
        }
        else
        {
            ConsoleRenderer.Success("Tu gagnes !");
        }
    }
}
