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
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusGs.PlayersStatus[PlayerTarget];
        
        VirusOrgan organToCure = playerSelf.SearchOrganColor(ColorSelf);
        VirusOrgan organToInfect = playerTarget.SearchOrganColor(ColorTarget);
        
        await virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        //Debug.Log("ColorSelf: " + ColorSelf + " | ColorTarget: " + ColorTarget + "  | OrganSelf: " + organToCure + "  | OrganTarget:  " + organToInfect);

        organToInfect.Status = Status.Infected;
        organToInfect.VirusColor = organToCure.VirusColor;
        organToCure.Status = Status.Normal;
        organToCure.VirusColor = VirusColor.None;
        await playerSelf.VisualBody.RemoveVirusAnimation(ColorSelf);
        await playerTarget.VisualBody.PlaceVirusAnimation(ColorTarget, organToInfect.VirusColor);
        
        return true;
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
