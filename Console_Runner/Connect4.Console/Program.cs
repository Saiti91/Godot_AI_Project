using System.Text;
using Connect4.ConsoleApp;

Console.OutputEncoding = Encoding.UTF8;

while (true)
{
    ConsoleRenderer.Clear();
    ConsoleRenderer.Title("Puissance 4");
    Console.WriteLine("  1. Joueur contre joueur");
    Console.WriteLine("  2. Joueur contre IA");
    Console.WriteLine("  3. Entraîner l'IA");
    Console.WriteLine("  q. Quitter");
    Console.WriteLine();
    Console.Write("Choix : ");

    string? choice = Console.ReadLine();
    if (choice == null)
    {
        return;
    }

    switch (choice.Trim().ToLowerInvariant())
    {
        case "1":
            PlayMode.PlayerVsPlayer();
            break;
        case "2":
            PlayMode.PlayerVsAi();
            break;
        case "3":
            TrainingMode.Run();
            break;
        case "q":
            return;
        default:
            ConsoleRenderer.Error("Choix inconnu.");
            ConsoleInput.Pause("Appuie sur Entrée pour réessayer...");
            break;
    }
}
