using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VirusActionPlayVirus : VirusAction
{
    private readonly VirusColor _virusColor;
    public VirusActionPlayVirus(VirusColor colorTarget, int playerTarget, VirusColor virusColor, int indexCard)
    {
        ColorTarget = colorTarget;
        PlayerTarget = playerTarget;
        _virusColor = virusColor;
        CardIndex = indexCard;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return Task.FromResult(false);
        
        VirusPlayerStatus playerTarget = virusGs.PlayersStatus[PlayerTarget];
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[virusGs.GetPlayerTurnIndex()];
        VirusOrgan organ = playerTarget.SearchOrganColor(ColorTarget);

        switch (organ.Status)
        {
            case Status.Infected:
                virusGs.DiscardDeck.Add(new VirusCard(organ.VirusColor, VirusType.Virus, TreatmentType.None, virusGs.discardGo));
                virusGs.DiscardDeck.Add(new VirusCard(_virusColor, VirusType.Virus, TreatmentType.None, virusGs.discardGo));
                virusGs.DiscardDeck.Add(new VirusCard(ColorTarget, VirusType.Organ, TreatmentType.None, virusGs.discardGo));
                playerTarget.RemoveOrgan(ColorTarget);
                playerTarget.VisualBody.RemoveOrgan(ColorTarget);
                break;
            case Status.Normal:
                organ.Status = Status.Infected;
                organ.VirusColor = _virusColor;
                playerTarget.VisualBody.AddVirusToOrgan(ColorTarget, _virusColor);
                break;
            case Status.Vaccinated:
                organ.Status = Status.Normal;
                virusGs.DiscardDeck.Add(new VirusCard(organ.MedicineColor, VirusType.Medicine, TreatmentType.None, virusGs.discardGo));
                virusGs.DiscardDeck.Add(new VirusCard(_virusColor, VirusType.Virus, TreatmentType.None, virusGs.discardGo));
                organ.MedicineColor = VirusColor.None;
                playerTarget.VisualBody.RemoveMedicineFromOrgan(ColorTarget);
                break;
        }
        
        Object.Destroy(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex).VisualCard.gameObject);
        
        return Task.FromResult(true);
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
                virusOb.DiscardDeck.Add(new VirusCard(_virusColor, VirusType.Virus));
                virusOb.DiscardDeck.Add(new VirusCard(ColorTarget, VirusType.Organ));
                playerTarget.RemoveOrgan(ColorTarget);
                return true;
            case Status.Normal:
                organ.Status = Status.Infected;
                organ.VirusColor = _virusColor;
                return true;
            case Status.Vaccinated:
                virusOb.DiscardDeck.Add(new VirusCard(organ.MedicineColor, VirusType.Medicine));
                virusOb.DiscardDeck.Add(new VirusCard(_virusColor, VirusType.Virus));
                organ.Status = Status.Normal;
                organ.MedicineColor = VirusColor.None;
                return true;
        }
        
        Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);
        
        return false;
    }
}