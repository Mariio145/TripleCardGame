using System.Threading.Tasks;

public class VirusAction: IAction
{
    protected int CardIndex;
    public VirusColor ColorSelf;
    public VirusColor ColorTarget;
    public int PlayerSelf;
    public int PlayerTarget;

    protected internal VirusAction(VirusColor selfColor = VirusColor.None, VirusColor targetColor = VirusColor.None, int playerSelf = -1, int playerTarget = -1)
    {
        ColorSelf = selfColor;
        ColorTarget = targetColor;
        PlayerSelf = playerSelf;
        PlayerTarget = playerTarget;
    }

    public virtual Task<bool> PlayAction(IGameState gameState)
    {
        return Task.FromResult(false);
    }

    public virtual bool TestAction(IObservation gameState)
    {
        return false;
    }
}