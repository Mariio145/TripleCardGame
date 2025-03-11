using UnityEngine;

public class VisualVirusCard : VisualCard
{
    public override void ChangeSprite()
    {
        VirusCard card = (VirusCard)MemoryCard;
        VirusColor color = card.GetColor();
        VirusType type = card.GetType();
        TreatmentType treatment = card.GetTreatmentType();
        
        string asset = "Materials/VirusCards/";
        
        switch (type)
        {
            case VirusType.Organ:
                asset += "Organs/O" + color;
                break;
            case VirusType.Medicine:
                asset += "Medicine/M" + color;
                break;
            case VirusType.Virus:
                asset += "Virus/V" + color;
                break;
            case VirusType.Treatment:
                asset += "Treatments/";
                switch (treatment)
                {
                    case TreatmentType.Transplant:
                        asset += "Transplant";
                        break;
                    case TreatmentType.OrganThief:
                        asset += "OrganThief";
                        break;
                    case TreatmentType.Spreading:
                        asset += "Spreading";
                        break;
                    case TreatmentType.LatexGlove:
                        asset += "LatexGlove";
                        break;
                    case TreatmentType.MedicalError:
                        asset += "MedicalError";
                        break;
                }
                break;
        }
        
        MeshRenderer.material = Resources.Load<Material>(asset);
    }

    protected override void OnMouseDown()
    {
        selected = !selected;
    }
}
