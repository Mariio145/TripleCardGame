using System.Threading.Tasks;

public class UnoForcedDrawCard : UnoAction
{
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return false;
        
        UnoPlayerStatus playerSelf = unoGs.PlayersStatus[unoGs.GetPlayerTurnIndex()];
        UnoCard card = await unoGs.DrawCardFromDrawDeck(playerSelf.HandGObject);
        playerSelf.Hand.Enqueue(card);
        
        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return false;
        
        UnoPlayerStatus playerSelf = unoObs.PlayersStatus[unoObs.GetPlayerTurnIndex()];
        playerSelf.Hand.Enqueue(unoObs.DrawCardFromMixedDrawDeck());
        
        return true;
    }
}