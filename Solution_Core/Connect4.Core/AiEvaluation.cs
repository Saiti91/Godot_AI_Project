namespace Connect4.Core;

/// <summary>Résultat d'une évaluation de l'IA contre un adversaire aléatoire.</summary>
public readonly record struct AiEvaluation(int Wins, int Losses, int Draws)
{
    public int Games => Wins + Losses + Draws;

    public double WinRate => Rate(Wins);
    public double LossRate => Rate(Losses);
    public double DrawRate => Rate(Draws);

    private double Rate(int count)
    {
        if (Games == 0)
        {
            return 0;
        }

        return (double)count / Games;
    }
}
