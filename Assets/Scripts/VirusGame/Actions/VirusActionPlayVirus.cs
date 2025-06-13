using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VirusActionPlayVirus : VirusAction
{
    public readonly VirusColor VirusColor;
    public VirusActionPlayVirus(VirusColor colorTarget, int playerTarget, VirusColor virusColor, int indexCard)
    {
        ColorTarget = colorTarget;
        PlayerTarget = playerTarget;
        VirusColor = virusColor;
        CardIndex = indexCard;
    }
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        
        VirusPlayerStatus playerTarget = virusGs.PlayersStatus[PlayerTarget];
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[virusGs.GetPlayerTurnIndex()];
        VirusOrgan organ = playerTarget.SearchOrganColor(ColorTarget);
        
        List<Task> tasks = new();
        
        Object.Destroy(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex).VisualCard.gameObject);
        switch (organ.Status)
        {
            case Status.Infected:
                virusGs.DiscardDeck.Add(new VirusCard(organ.VirusColor, VirusType.Virus, TreatmentType.None, virusGs.DiscardGo));
                virusGs.DiscardDeck.Add(new VirusCard(VirusColor, VirusType.Virus, TreatmentType.None, virusGs.DiscardGo));
                virusGs.DiscardDeck.Add(new VirusCard(ColorTarget, VirusType.Organ, TreatmentType.None, virusGs.DiscardGo));
                playerTarget.RemoveOrgan(ColorTarget);
                tasks.Add(playerTarget.VisualBody.RemoveVirusAnimation(ColorTarget));
                tasks.Add(playerTarget.VisualBody.RemoveOrganAnimation(ColorTarget));
                await Task.WhenAll(tasks);
                break;
            case Status.Normal:
                organ.Status = Status.Infected;
                organ.VirusColor = VirusColor;
                await playerTarget.VisualBody.PlaceVirusAnimation(ColorTarget, VirusColor);
                break;
            case Status.Vaccinated:
                organ.Status = Status.Normal;
                virusGs.DiscardDeck.Add(new VirusCard(organ.MedicineColor, VirusType.Medicine, TreatmentType.None, virusGs.DiscardGo));
                virusGs.DiscardDeck.Add(new VirusCard(VirusColor, VirusType.Virus, TreatmentType.None, virusGs.DiscardGo));
                organ.MedicineColor = VirusColor.None;
                await playerTarget.VisualBody.RemoveMedicine1Animation(ColorTarget);
                break;
        }
        
        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        
        VirusOrgan organ = virusOb.PlayersStatus[PlayerTarget].SearchOrganColor(ColorTarget);
        VirusPlayerStatus playerSelf = virusOb.PlayersStatus[virusOb.GetPlayerTurnIndex()];
        VirusPlayerStatus playerTarget = virusOb.PlayersStatus[PlayerTarget];
        
        switch (organ.Status)
        {
            case Status.Infected:
                virusOb.DiscardDeck.Add(new VirusCard(organ.VirusColor, VirusType.Virus));
                virusOb.DiscardDeck.Add(new VirusCard(VirusColor, VirusType.Virus));
                virusOb.DiscardDeck.Add(new VirusCard(ColorTarget, VirusType.Organ));
                playerTarget.RemoveOrgan(ColorTarget);
                return true;
            case Status.Normal:
                organ.Status = Status.Infected;
                organ.VirusColor = VirusColor;
                return true;
            case Status.Vaccinated:
                virusOb.DiscardDeck.Add(new VirusCard(organ.MedicineColor, VirusType.Medicine));
                virusOb.DiscardDeck.Add(new VirusCard(VirusColor, VirusType.Virus));
                organ.Status = Status.Normal;
                organ.MedicineColor = VirusColor.None;
                return true;
        }
        
        Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);
        
        return false;
    }
}