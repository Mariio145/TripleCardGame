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

    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        
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
        
        await virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        List<Task> tasks = new();
        
        tasks.Add(playerSelf.VisualBody.RemoveOrganComplete(organToGive.OrganColor));
        tasks.Add(playerTarget.VisualBody.RemoveOrganComplete(organToReceive.OrganColor));

        await Task.WhenAll(tasks);
        
        tasks.Add(playerSelf.VisualBody.PlaceOrganComplete(organToReceive.OrganColor, organToReceive.MedicineColor, organToReceive.MedicineColor2, organToReceive.VirusColor));
        tasks.Add(playerTarget.VisualBody.PlaceOrganComplete(organToGive.OrganColor, organToGive.MedicineColor, organToGive.MedicineColor2, organToGive.VirusColor));
        
        await Task.WhenAll(tasks);
        
        return true;
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
