namespace Connect4.ConsoleApp;

/// <summary>Lecture des saisies clavier : aucune saisie invalide ne doit faire planter le programme.</summary>
public static class ConsoleInput
{
    /// <summary>
    /// Demande un entier entre min et max (inclus), jusqu'à obtenir une saisie valide.
    /// Renvoie null si l'utilisateur tape "q" (ou si l'entrée est fermée).
    /// </summary>
    public static int? ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (input == null)
            {
                return null;
            }

            input = input.Trim();
            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }

            ConsoleRenderer.Error($"Entre un nombre entre {min} et {max} (ou q pour quitter).");
        }
    }

    /// <summary>Comme ReadInt, mais une saisie vide renvoie la valeur par défaut.</summary>
    public static int? ReadIntOrDefault(string prompt, int min, int max, int defaultValue)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (input == null)
            {
                return null;
            }

            input = input.Trim().Replace(" ", "");
            if (input.Length == 0)
            {
                return defaultValue;
            }

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }

            ConsoleRenderer.Error($"Entre un nombre entre {min} et {max}, ou Entrée pour {defaultValue}.");
        }
    }

    /// <summary>Question oui / non. Accepte o, oui, y, yes, n, non, no.</summary>
    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (input == null)
            {
                return false;
            }

            string answer = input.Trim().ToLowerInvariant();
            if (answer == "o" || answer == "oui" || answer == "y" || answer == "yes")
            {
                return true;
            }

            if (answer == "n" || answer == "non" || answer == "no")
            {
                return false;
            }

            ConsoleRenderer.Error("Réponds par o (oui) ou n (non).");
        }
    }

    /// <summary>Choix de la taille du plateau. Renvoie null si l'utilisateur quitte.</summary>
    public static (int Columns, int Rows)? ChooseBoardSize()
    {
        Console.WriteLine("Taille du plateau :");
        Console.WriteLine("  1. Classique 7 x 6");
        Console.WriteLine("  2. Petit 5 x 4 (l'IA y apprend beaucoup plus vite)");

        int? choice = ReadInt("Choix (q pour revenir) : ", 1, 2);
        if (choice == null)
        {
            return null;
        }

        Console.WriteLine();
        if (choice == 1)
        {
            return (7, 6);
        }

        return (5, 4);
    }

    public static void Pause(string message = "Appuie sur Entrée pour revenir au menu...")
    {
        Console.Write(message);
        Console.ReadLine();
    }
}
