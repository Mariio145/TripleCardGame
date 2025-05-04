using System.Threading.Tasks;

public interface IForwardModel
{
    public Task<bool> PlayAction(IGameState gameState, IAction action);
    public bool TestAction(IObservation observation, IAction action);
}
