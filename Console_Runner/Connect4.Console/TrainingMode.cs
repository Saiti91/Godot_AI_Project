using System.Diagnostics;
using Connect4.Core;

namespace Connect4.ConsoleApp;

/// <summary>Entraîne l'IA par étapes, l'évalue contre un adversaire aléatoire après chacune, puis sauvegarde le modèle.</summary>
public static class TrainingMode
{
    private const int DefaultEpisodes = 100_000;
    private const int Steps = 10;
    private const int EvaluationGames = 500;

    public static void Run()
    {
        ConsoleRenderer.Clear();
        ConsoleRenderer.Title("Entraîner l'IA");

        var size = ConsoleInput.ChooseBoardSize();
        if (size == null)
        {
            return;
        }

        var (columns, rows) = size.Value;
        string path = ModelStore.PathFor(columns, rows);

        Game game;
        if (File.Exists(path)
            && ConsoleInput.ReadYesNo($"Un modèle {columns} x {rows} existe déjà. Continuer son entraînement ? (o = continuer, n = repartir de zéro) : "))
        {
            game = new Game(columns, rows, CellPlayer.Player2, trainAi: false, modelPath: path);
        }
        else
        {
            game = new Game(columns, rows, CellPlayer.Player2, trainAi: true, trainingEpisodes: 0);
        }

        int? total = ConsoleInput.ReadIntOrDefault(
            $"Nombre de parties d'entraînement (Entrée = {DefaultEpisodes:N0}) : ", 1, 100_000_000, DefaultEpisodes);
        if (total == null)
        {
            return;
        }

        int chunk = Math.Max(1, total.Value / Steps);

        Console.WriteLine();
        ConsoleRenderer.Info($"Après chaque étape : {EvaluationGames} parties sans exploration contre un adversaire aléatoire.");
        Console.WriteLine();

        PrintProgress(0, total.Value, game.EvaluateAi(EvaluationGames), game.AiKnownStates, TimeSpan.Zero);

        var stopwatch = Stopwatch.StartNew();
        int trained = 0;
        while (trained < total.Value)
        {
            int episodes = Math.Min(chunk, total.Value - trained);
            game.TrainAi(episodes);
            trained += episodes;

            PrintProgress(trained, total.Value, game.EvaluateAi(EvaluationGames), game.AiKnownStates, stopwatch.Elapsed);
        }

        game.SaveAi(path);

        Console.WriteLine();
        ConsoleRenderer.Success($"Modèle sauvegardé : {path}");
        ConsoleRenderer.Info("Le mode « Joueur contre IA » l'utilisera automatiquement pour ce plateau.");
        Console.WriteLine();
        ConsoleInput.Pause();
    }

    private static void PrintProgress(int trained, int total, AiEvaluation evaluation, int states, TimeSpan elapsed)
    {
        Console.Write($"{trained,11:N0} / {total:N0}  ");
        ConsoleRenderer.WriteProgressBar(evaluation.WinRate);
        Console.WriteLine(
            $"  victoires {evaluation.WinRate,6:P1} | nuls {evaluation.DrawRate,6:P1} | défaites {evaluation.LossRate,6:P1}" +
            $" | {states:N0} états | {elapsed.TotalSeconds:0.0} s");
    }
}
