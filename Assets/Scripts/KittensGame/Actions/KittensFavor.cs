using System.Threading.Tasks;

public class KittensFavor : KittensAction
{
    public int PlayerTarget { get;}

    public KittensFavor(int playerTarget, int cardIndex)
    {
        PlayerTarget = playerTarget;
        CardIndex = cardIndex;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        return true;
    }
}