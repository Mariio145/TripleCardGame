using System.Threading.Tasks;
using UnityEngine;

public class VirusActionPlayMedicine : VirusAction
{
    private readonly VirusColor _medicineColor;

    public VirusActionPlayMedicine(VirusColor colorSelf, int playerSelf, VirusColor medicineColor, int indexCard)
    {
        ColorSelf = colorSelf;
        PlayerSelf = playerSelf;
        _medicineColor = medicineColor;
        CardIndex = indexCard;
    }
    
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return Task.FromResult(false);
        
        VirusPlayerStatus playerSelf = virusGs.PlayersStatus[PlayerSelf];
        VirusOrgan organ = playerSelf.SearchOrganColor(ColorSelf);

        switch (organ.Status)
        {
            case Status.Infected:
                organ.Status = Status.Normal;
                virusGs.DiscardDeck.Add(new VirusCard(organ.VirusColor, VirusType.Virus, TreatmentType.None, virusGs.discardGo));
                virusGs.DiscardDeck.Add(new VirusCard(_medicineColor, VirusType.Medicine, TreatmentType.None, virusGs.discardGo));
                organ.VirusColor = VirusColor.None;
                playerSelf.VisualBody.RemoveVirusFromOrgan(ColorSelf);
                break;
            case Status.Normal:
                organ.Status = Status.Vaccinated;
                organ.MedicineColor = _medicineColor;
                playerSelf.VisualBody.AddMedicineToOrgan(ColorSelf, _medicineColor);
                break;
            case Status.Vaccinated:
                organ.Status = Status.Immune;
                organ.MedicineColor2 = _medicineColor;
                playerSelf.VisualBody.ImmunizeOrgan(ColorSelf, _medicineColor);
                break;
        }
        
        Object.Destroy(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex).VisualCard.gameObject);

        return Task.FromResult(true);
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
                virusOb.DiscardDeck.Add(new VirusCard(_medicineColor, VirusType.Medicine));
                organ.VirusColor = VirusColor.None;
                return true;
            case Status.Normal:
                organ.Status = Status.Vaccinated;
                organ.MedicineColor = _medicineColor;
                return true;
            case Status.Vaccinated:
                organ.Status = Status.Immune;
                organ.MedicineColor2 = _medicineColor;
                return true;
        }
        
        Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);

        return false;
    }
}