using System.Threading.Tasks;

public class UnoAction : IAction
{
    public virtual Task<bool> PlayAction(IGameState gameState)
    {
        return Task.FromResult(false);
    }

    public virtual bool TestAction(IObservation gameState)
    {
        return false;
    }
}
