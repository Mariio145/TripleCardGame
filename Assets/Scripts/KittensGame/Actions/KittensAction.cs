using System.Threading.Tasks;

public class KittensAction : IAction
{
    protected internal int CardIndex;
    public virtual Task<bool> PlayAction(IGameState gameState)
    {
        return Task.FromResult(true);
    }
    
    public virtual bool TestAction(IObservation gameState)
    {
        return true;
    }
}
