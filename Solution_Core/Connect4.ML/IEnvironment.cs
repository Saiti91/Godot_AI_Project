namespace Connect4.ML;

public interface IEnvironment
{
     string Reset();
     IReadOnlyList<int> AvailableActions();
     (string NextState, double Reward, bool Done) Step(int action);
}