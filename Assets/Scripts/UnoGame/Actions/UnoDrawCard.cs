using System.Threading.Tasks;

public class UnoDrawCard : UnoAction
{
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return false;
        
        UnoPlayerStatus playerSelf = unoGs.PlayersStatus[unoGs.GetPlayerTurnIndex()];
        UnoCard drawnCard = await unoGs.DrawCardFromDrawDeck(playerSelf.HandGObject);
        playerSelf.Hand.Enqueue(drawnCard);


        if (!unoGs.IsCardPlayable(drawnCard)) return true;
        
        //await Task.Delay(750);
        await new UnoPlayCard(drawnCard, playerSelf.Hand.Count - 1).PlayAction(unoGs);
        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return false;
        
        UnoPlayerStatus playerSelf = unoObs.PlayersStatus[unoObs.GetPlayerTurnIndex()];
        UnoCard drawnCard = unoObs.DrawCardFromMixedDrawDeck();
        playerSelf.Hand.Enqueue(drawnCard);
        
        if (!unoObs.IsCardPlayable(drawnCard)) return true;
        new UnoPlayCard(drawnCard, playerSelf.Hand.Count - 1).TestAction(unoObs);

        return true;
    }
}
