using System;
using System.Threading.Tasks;

public class VirusActionOrganThief : VirusAction
{
    public VirusActionOrganThief(VirusColor colorTarget, int playerSelf, int playerTarget, int indexCard)
    {
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

        VirusOrgan organToReceive = playerTarget.SearchOrganColor(ColorTarget);
        
        string cardEffect = "Card" + new Random().Next(1, 5);
        SoundManager.Instance.PlaySfx(cardEffect);
        
        await virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        //Se añade el organo y se elimina del otro jugador
        playerSelf.AddOrgan(organToReceive);
        playerTarget.RemoveOrgan(ColorTarget);
        
        await playerTarget.VisualBody.RemoveOrganComplete(organToReceive.OrganColor);
        await playerSelf.VisualBody.PlaceOrganComplete(organToReceive.OrganColor, organToReceive.MedicineColor, organToReceive.MedicineColor2, organToReceive.VirusColor);

        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        
        VirusPlayerStatus playerSelf = virusOb.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusOb.PlayersStatus[PlayerTarget];
        
        VirusOrgan organToReceive = playerTarget.SearchOrganColor(ColorTarget);
        
        //Se añade el organo y se elimina del otro jugador
        playerSelf.AddOrgan(organToReceive);
        playerTarget.RemoveOrgan(ColorTarget);

        virusOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        return true;
    }
}
