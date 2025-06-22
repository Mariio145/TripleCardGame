using System;
using System.Threading.Tasks;
using UnityEngine;

public class VirusActionPlayMedicine : VirusAction
{
    public readonly VirusColor MedicineColor;

    public VirusActionPlayMedicine(VirusColor colorSelf, int playerSelf, VirusColor medicineColor, int indexCard)
    {
        ColorSelf = colorSelf;
        PlayerSelf = playerSelf;
        MedicineColor = medicineColor;
        CardIndex = indexCard;
    }
    
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[PlayerSelf];
        VirusOrgan organ = playerSelf.SearchOrganColor(ColorSelf);
        
        UnityEngine.Object.Destroy(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex).VisualCard.gameObject);
        
        SoundManager.Instance.PlaySfx("Medicine");

        switch (organ.Status)
        {
            case Status.Infected:
                organ.Status = Status.Normal;
                virusGs.DiscardDeck.Add(new VirusCard(organ.VirusColor, VirusType.Virus, TreatmentType.None, virusGs.DiscardGo));
                virusGs.DiscardDeck.Add(new VirusCard(MedicineColor, VirusType.Medicine, TreatmentType.None, virusGs.DiscardGo));
                organ.VirusColor = VirusColor.None;
                await playerSelf.VisualBody.RemoveVirusAnimation(ColorSelf);
                break;
            case Status.Normal:
                organ.Status = Status.Vaccinated;
                organ.MedicineColor = MedicineColor;
                await playerSelf.VisualBody.PlaceMedicine1Animation(ColorSelf, MedicineColor);
                break;
            case Status.Vaccinated:
                organ.Status = Status.Immune;
                organ.MedicineColor2 = MedicineColor;
                await playerSelf.VisualBody.PlaceMedicine2Animation(ColorSelf, MedicineColor);
                break;
        }

        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        VirusPlayerStatus playerSelf = virusOb.PlayersStatus[PlayerSelf];
        VirusOrgan organ = virusOb.PlayersStatus[PlayerSelf].SearchOrganColor(ColorSelf);

        switch (organ.Status)
        {
            case Status.Infected:
                organ.Status = Status.Normal;
                virusOb.DiscardDeck.Add(new VirusCard(organ.VirusColor, VirusType.Virus));
                virusOb.DiscardDeck.Add(new VirusCard(MedicineColor, VirusType.Medicine));
                organ.VirusColor = VirusColor.None;
                return true;
            case Status.Normal:
                organ.Status = Status.Vaccinated;
                organ.MedicineColor = MedicineColor;
                return true;
            case Status.Vaccinated:
                organ.Status = Status.Immune;
                organ.MedicineColor2 = MedicineColor;
                return true;
        }
        
        Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);

        return false;
    }
}