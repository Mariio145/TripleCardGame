using System.Threading.Tasks;

public class UnoChangeColor : UnoAction
{
    private readonly UnoColor _colorChange;

    public UnoChangeColor(UnoColor colorChange)
    {
        _colorChange = colorChange;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return Task.FromResult(false);
        
        unoGs.ChangeColor(_colorChange);

        return Task.FromResult(true);
    }

    public override bool TestAction(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return false;
        
        unoObs.TopCard.ChangeColor(_colorChange);

        return true;
    }
}
