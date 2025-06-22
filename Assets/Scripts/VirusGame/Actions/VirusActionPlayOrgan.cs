using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VirusActionPlayOrgan : VirusAction
{
    public VirusActionPlayOrgan(VirusColor colorSelf, int playerSelf, int indexCard)
    {
        ColorSelf = colorSelf;
        PlayerSelf = playerSelf;
        CardIndex = indexCard;
    }
    
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        VirusPlayerStatus player = virusGs.PlayersStatus[PlayerSelf];
        
        
        Object.Destroy(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex).VisualCard.gameObject);
        
        player.AddOrgan(ColorSelf);
        await player.VisualBody.PlaceOrganAnimation(ColorSelf);
        SoundManager.Instance.PlaySfx("Medicine");
        
        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        VirusPlayerStatus playerSelf = virusOb.PlayersStatus[PlayerSelf];
        
        playerSelf.AddOrgan(ColorSelf);
        
        Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);
        
        return true;
    }
}