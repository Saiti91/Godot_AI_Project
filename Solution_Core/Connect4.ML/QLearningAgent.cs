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

    private double[] GetOrCreateValues(string state)
    {
        // si l'état est dans la table (TryGetValue) :
        // renvoyer son tableau
        //
        // sinon :
        // créer un tableau de _nbPossibleActions zéros
        // le ranger dans la table pour cet état
        // le renvoyer

        if (_qTable.TryGetValue(state, out var value))
        {
            return value;
        }
        else
        {
            var values = new double[_nbPossibleActions];
            _qTable[state] = values;
            return values;
        }
    }

    private int ChooseAction(string state, IReadOnlyList<int> actions)
    {
        // tirer un nombre au hasard entre 0 et 1
        //
        // si ce nombre est < _epsilon :      ← exploration
        //     choisir un index au hasard entre 0 et (nombre d'actions - 1)
        // renvoyer l'action à cet index dans la liste
        //
        // sinon :                            ← exploitation
        //     renvoyer Predict(state, actions)
        double random = _random.NextDouble();

        if (random<_epsilon)
        {
            int index = _random.Next(actions.Count);
            return actions[index];
        }
        else
        {
            return Predict(state, actions);
        }
    }

    public void Train(IEnvironment env, int nbEpisode)
    {
        // pour chaque épisode :
        // state = env.Reset()
        // done = false
        //
        // tant que la partie n'est pas finie :
        // actions = env.AvailableActions()
        // action  = ChooseAction(state, actions)          ← ε-greedy
        //
        //     (nextState, reward, done) = env.Step(action)
        //
        // si done :
        // cible = reward
        // sinon :
        // cible = reward + γ × (max des Q de nextState, parmi ses actions possibles)
        //
        // q = valeurs de state, à l'index action          ← avec GetOrCreateValues
        //     q = q + α × (cible − q)
        //
        // state = nextState

        for (int episode = 0; episode < nbEpisode; episode++)
        {
            string state = env.Reset(); 
            bool done = false;

            while (!done)
            {
                var actions = env.AvailableActions();
                var action = ChooseAction(state, actions);
                (string nextState, double reward, done) = env.Step(action);

                double cible;
                if (done)
                {
                    cible = reward;
                }
                else
                {
                    // ↓ le bloc va ici, à la place de l'ancienne ligne 159
                    var nextActions = env.AvailableActions();
                    int bestNext = Predict(nextState, nextActions);
                    _qTable.TryGetValue(nextState, out var nextValues);
                    double maxNext = nextValues?[bestNext] ?? 0;

                    cible = reward + _gamma * maxNext;
                }

                // mise à jour de Q : à toi (récupérer le tableau de state, modifier la case [action])
                var values = GetOrCreateValues(state);
                values[action] += _alpha * (cible - values[action]);
                state = nextState;
            }
        }
    }
}