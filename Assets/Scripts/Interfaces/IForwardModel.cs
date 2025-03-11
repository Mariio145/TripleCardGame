using System.Threading.Tasks;

public interface IForwardModel
{
    public Task<bool> PlayAction(IGameState gameState, IAction action);
    public Task<bool> TestAction(IObservation observation, IAction action);
}
