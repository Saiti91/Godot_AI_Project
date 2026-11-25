namespace Connect4.ML;

public class QLearningAgent
{
    private Dictionary<string, double[]> _qTable = new Dictionary<string, double[]>();

    private int _nbPossibleActions = 7;

    //le pas d'apprentissage
    private double _alpha;

    //le facteur d'actualisation
    private double _gamma;

    //le taux d'exploration
    private double _epsilon;

    //Générateur
    private Random _random = new Random();

    public void Init(int nbPossibleActions, double alpha, double gamma, double epsilon, int? seed = null)
    {
        _nbPossibleActions = nbPossibleActions;
        _alpha = alpha;
        _gamma = gamma;
        _epsilon = epsilon;
        _qTable = new Dictionary<string, double[]>();

        if (seed.HasValue)
        {
            _random = new Random(seed.Value);
        }
        else
        {
            _random = new Random();
        }
    }

    public int Predict(string state, IReadOnlyList<int> actions)
    {
        if (actions.Count == 0)
        {
            throw new ArgumentException(nameof(actions));
        }

        _qTable.TryGetValue(state, out var values); // null si l'état est inconnu

        int bestAction = actions[0];
        double bestValue = values?[bestAction] ?? 0;

        foreach (var action in actions)
        {
            double q = values?[action] ?? 0;
            // si q est strictement plus grand que bestValue : mettre à jour les deux
            if (q > bestValue)
            {
                bestValue = q;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public void SetQValues(string state, double[] values)
    {
        if (values.Length != _nbPossibleActions)
        {
            throw new ArgumentException($"Il faut {_nbPossibleActions} valeurs, reçu {values.Length}.", nameof(values));
        }

        _qTable[state] = values.ToArray();
    }
}