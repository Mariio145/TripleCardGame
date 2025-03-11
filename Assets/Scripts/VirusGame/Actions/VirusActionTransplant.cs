using System.Collections.Generic;
using System.Threading.Tasks;

public class VirusActionTransplant : VirusAction
{
    public VirusActionTransplant(VirusColor colorSelf, VirusColor colorTarget, int playerSelf, int playerTarget,
        int indexCard)
    {
        ColorSelf = colorSelf;
        ColorTarget = colorTarget;
        PlayerSelf = playerSelf;
        PlayerTarget = playerTarget;
        CardIndex = indexCard;
    }

    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return Task.FromResult(false);
        
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusGs.PlayersStatus[PlayerTarget];
        
        VirusOrgan organToGive = playerSelf.SearchOrganColor(ColorSelf);
        VirusOrgan organToReceive = playerTarget.SearchOrganColor(ColorTarget);

        //Se añade el otro organo y se elimina el suyo
        playerSelf.AddOrgan(organToReceive);
        playerSelf.RemoveOrgan(ColorSelf);

        //Se añade el otro organo y se elimina el suyo
        playerTarget.AddOrgan(organToGive);
        playerTarget.RemoveOrgan(ColorTarget);
        
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
        
        VirusOrgan organToGive = playerSelf.SearchOrganColor(ColorSelf);
        VirusOrgan organToReceive = playerTarget.SearchOrganColor(ColorTarget);

        //Se añade el otro organo y se elimina el suyo
        playerSelf.AddOrgan(organToReceive);
        playerSelf.RemoveOrgan(ColorSelf);

        //Se añade el otro organo y se elimina el suyo
        playerTarget.AddOrgan(organToGive);
        playerTarget.RemoveOrgan(ColorTarget);
        
        virusOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));

        return true;
    }
}
