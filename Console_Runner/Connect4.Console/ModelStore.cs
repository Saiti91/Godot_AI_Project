namespace Connect4.ConsoleApp;

/// <summary>Emplacement des modèles d'IA : un fichier par taille de plateau, à côté de l'exécutable.</summary>
public static class ModelStore
{
    public static string ModelsFolder => Path.Combine(AppContext.BaseDirectory, "models");

    public static string PathFor(int columns, int rows)
    {
        return Path.Combine(ModelsFolder, $"connect4_{columns}x{rows}.txt");
    }
}
