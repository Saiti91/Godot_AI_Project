using System.Globalization;

namespace Connect4.ML;

/// <summary>
/// Agent Q-learning tabulaire. Ne connaît rien du jeu : il manipule des états (string),
/// des actions (int) et des récompenses (double) fournis par un IEnvironment.
/// </summary>
public class QLearningAgent
{
    private Dictionary<string, double[]> _qTable = new Dictionary<string, double[]>();

    private int _nbPossibleActions = 7;

    // Pas d'apprentissage (α)
    private double _alpha;

    // Facteur d'actualisation (γ)
    private double _gamma;

    // Taux d'exploration (ε)
    private double _epsilon;

    private Random _random = new Random();

    /// <summary>Crée un agent vierge (table vide) avec ses hyperparamètres.</summary>
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

    /// <summary>
    /// Renvoie l'action gloutonne parmi les actions possibles.
    /// État inconnu : toutes les valeurs valent 0. Égalité : la première action de la liste.
    /// </summary>
    public int Predict(string state, IReadOnlyList<int> actions)
    {
        if (actions.Count == 0)
        {
            throw new ArgumentException("Aucune action possible.", nameof(actions));
        }

        _qTable.TryGetValue(state, out var values);

        int bestAction = actions[0];
        double bestValue = values?[bestAction] ?? 0;

        foreach (var action in actions)
        {
            double q = values?[action] ?? 0;
            if (q > bestValue)
            {
                bestValue = q;
                bestAction = action;
            }
        }

        return bestAction;
    }

    /// <summary>Nombre d'états rencontrés pendant l'entraînement (taille de la table Q).</summary>
    public int StateCount => _qTable.Count;

    /// <summary>Copie des valeurs Q d'un état, ou null si l'état n'a jamais été vu.</summary>
    public double[]? GetQValues(string state)
    {
        if (_qTable.TryGetValue(state, out var values))
        {
            return values.ToArray();
        }

        return null;
    }

    /// <summary>Remplace les valeurs Q d'un état (utile pour les tests).</summary>
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
        if (_qTable.TryGetValue(state, out var values))
        {
            return values;
        }

        values = new double[_nbPossibleActions];
        _qTable[state] = values;
        return values;
    }

    /// <summary>ε-greedy : action au hasard avec la probabilité ε, gloutonne sinon.</summary>
    private int ChooseAction(string state, IReadOnlyList<int> actions)
    {
        double roll = _random.NextDouble();

        if (roll < _epsilon)
        {
            int index = _random.Next(actions.Count);
            return actions[index];
        }

        return Predict(state, actions);
    }

    /// <summary>
    /// Q-learning : Q(s,a) ← Q(s,a) + α [r + γ max Q(s',·) − Q(s,a)].
    /// Si s' est terminal, la cible est la récompense seule.
    /// </summary>
    public void Train(IEnvironment env, int nbEpisode)
    {
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
                    var nextActions = env.AvailableActions();
                    int bestNext = Predict(nextState, nextActions);
                    _qTable.TryGetValue(nextState, out var nextValues);
                    double maxNext = nextValues?[bestNext] ?? 0;

                    cible = reward + _gamma * maxNext;
                }

                var values = GetOrCreateValues(state);
                values[action] += _alpha * (cible - values[action]);

                state = nextState;
            }
        }
    }

    /// <summary>
    /// Sauvegarde la table Q dans un fichier texte : une ligne par état, "état;v0;v1;...".
    /// Les nombres utilisent toujours le point décimal (InvariantCulture).
    /// </summary>
    public void Save(string path)
    {
        var lines = new List<string>(_qTable.Count);

        foreach (var (state, values) in _qTable)
        {
            var parts = new string[values.Length + 1];
            parts[0] = state;
            for (int i = 0; i < values.Length; i++)
            {
                parts[i + 1] = values[i].ToString("R", CultureInfo.InvariantCulture);
            }

            lines.Add(string.Join(";", parts));
        }

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllLines(path, lines);
    }

    /// <summary>
    /// Recharge une table Q sauvegardée par Save et remplace la table actuelle.
    /// À appeler après Init : α, γ, ε et le nombre d'actions viennent d'Init, pas du fichier.
    /// </summary>
    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Aucun modèle trouvé à l'emplacement : {path}", path);
        }

        var table = new Dictionary<string, double[]>();
        string[] lines = File.ReadAllLines(path);

        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            string line = lines[lineNumber];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] parts = line.Split(';');
            if (parts.Length != _nbPossibleActions + 1)
            {
                throw new InvalidDataException(
                    $"Ligne {lineNumber + 1} : {parts.Length - 1} valeurs trouvées, {_nbPossibleActions} attendues. " +
                    "Le fichier ne correspond pas au nombre d'actions donné à Init.");
            }

            var values = new double[_nbPossibleActions];
            for (int i = 0; i < _nbPossibleActions; i++)
            {
                values[i] = double.Parse(parts[i + 1], CultureInfo.InvariantCulture);
            }

            table[parts[0]] = values;
        }

        _qTable = table;
    }
}
