using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VirusActionSpread : VirusAction
{
    public VirusActionSpread(VirusColor colorSelf, VirusColor colorTarget, int playerSelf, int playerTarget,
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
        
        VirusOrgan organToCure = playerSelf.SearchOrganColor(ColorSelf);
        VirusOrgan organToInfect = playerTarget.SearchOrganColor(ColorTarget);
        
        Debug.Log("ColorSelf: " + ColorSelf + " | ColorTarget: " + ColorTarget + "  | OrganSelf: " + organToCure + "  | OrganTarget:  " + organToInfect);

        organToInfect.Status = Status.Infected;
        organToInfect.VirusColor = organToCure.VirusColor;
        playerTarget.VisualBody.AddVirusToOrgan(ColorTarget, organToCure.VirusColor);
        
        organToCure.Status = Status.Normal;
        organToCure.VirusColor = VirusColor.None;
        playerSelf.VisualBody.RemoveVirusFromOrgan(ColorSelf);
        
        virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        
        VirusPlayerStatus playerSelf = virusOb.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusOb.PlayersStatus[PlayerTarget];

        VirusOrgan organToCure = playerSelf.SearchOrganColor(ColorSelf);
        VirusOrgan organToInfect = playerTarget.SearchOrganColor(ColorTarget);

        organToInfect.Status = Status.Infected;
        organToInfect.VirusColor = organToCure.VirusColor;
        organToCure.Status = Status.Normal;
        organToCure.VirusColor = VirusColor.None;
        
        virusOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
      
        return true;
    }
}
