using System.Collections.Generic;
using System.Threading.Tasks;

public class VirusActionMedicalError : VirusAction
{
    public VirusActionMedicalError(int playerSelf, int playerTarget, int indexCard)
    {
        PlayerSelf = playerSelf;
        PlayerTarget = playerTarget;
        CardIndex = indexCard;
    }
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[PlayerSelf];
        VirusPlayerStatus playerTarget = virusGs.PlayersStatus[PlayerTarget];
        
        await virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));

        List<Task> tasks = new();
        
        foreach (VirusOrgan organSelf in playerSelf.Body)
        {
            tasks.Add(playerSelf.VisualBody.RemoveOrganComplete(organSelf.OrganColor));
        }
        
        foreach (VirusOrgan organTarget in playerTarget.Body)
        {
            tasks.Add(playerTarget.VisualBody.RemoveOrganComplete(organTarget.OrganColor));
        }

        await Task.WhenAll(tasks);

        //Se intercambian los cuerpos de los jugadores
        (playerTarget.Body, playerSelf.Body) = (playerSelf.Body, playerTarget.Body);
        
        foreach (VirusOrgan organSelf in playerSelf.Body)
        {
            tasks.Add(playerSelf.VisualBody.PlaceOrganComplete(organSelf.OrganColor, organSelf.MedicineColor, organSelf.MedicineColor2, organSelf.VirusColor));
        }
        
        foreach (VirusOrgan organTarget in playerTarget.Body)
        {
            tasks.Add(playerTarget.VisualBody.PlaceOrganComplete(organTarget.OrganColor, organTarget.MedicineColor, organTarget.MedicineColor2, organTarget.VirusColor));
        }
        
        await Task.WhenAll(tasks);
        
        return true;
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
