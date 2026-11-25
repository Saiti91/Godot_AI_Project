namespace Connect4.ML;

/// <summary>
/// Environnement d'apprentissage vu par l'agent (interface des méthodes sans modèle du cours de RL).
/// </summary>
public interface IEnvironment
{
    /// <summary>Démarre un nouvel épisode et renvoie l'état initial.</summary>
    string Reset();

    /// <summary>Actions possibles dans l'état courant, par ordre croissant (vide si terminal).</summary>
    IReadOnlyList<int> AvailableActions();

    /// <summary>Joue l'action et renvoie l'état suivant, la récompense et la fin de l'épisode.</summary>
    (string NextState, double Reward, bool Done) Step(int action);
}
