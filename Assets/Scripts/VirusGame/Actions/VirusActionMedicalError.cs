using System.Threading.Tasks;

public class VirusActionMedicalError : VirusAction
{
    public VirusActionMedicalError(int playerSelf, int playerTarget, int indexCard)
    {
        PlayerSelf = playerSelf;
        PlayerTarget = playerTarget;
        CardIndex = indexCard;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return Task.FromResult(false);
        
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusGs.PlayersStatus[PlayerTarget];

        //Se añade el organo y se elimina del otro jugador
        (playerTarget.Body, playerSelf.Body) = (playerSelf.Body, playerTarget.Body);
        
        playerSelf.VisualBody.UpdateBody();
        playerTarget.VisualBody.UpdateBody();
        
        virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        
        VirusPlayerStatus playerSelf = virusOb.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusOb.PlayersStatus[PlayerTarget];
        
        //Se añade el organo y se elimina del otro jugador
        (playerTarget.Body, playerSelf.Body) = (playerSelf.Body, playerTarget.Body);
        
        virusOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        return true;
    }
}
