using System.Threading.Tasks;

public class KittensNothing : KittensAction
{
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);

        kittensGs.TurnEnded = true;
        
        return Task.FromResult(true);
    }

    public override bool TestAction(IObservation observation) 
    {
        if (observation is not KittensObservation kittensOb) return false;
        
        kittensOb.TurnEnded = true;
        
        return true;
    }
}