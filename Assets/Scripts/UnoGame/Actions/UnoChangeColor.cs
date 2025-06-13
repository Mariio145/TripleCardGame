using System.Threading.Tasks;

public class UnoChangeColor : UnoAction
{
    public UnoColor ColorChange { get;}

    public UnoChangeColor(UnoColor colorChange)
    {
        ColorChange = colorChange;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return Task.FromResult(false);
        
        unoGs.ChangeColor(ColorChange);

        return Task.FromResult(true);
    }

    public override bool TestAction(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return false;
        
        unoObs.TopCard.ChangeColor(ColorChange);

        return true;
    }
}
