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
        if (MeshRenderer.material is null)
        {
            Debug.Log("Color de carta: " + color);
            Debug.Log("Tipo de carta: " + type);
            Debug.Log("Tratamiento de carta: " + treatment);
        }
    }

    protected override void OnMouseDown()
    {
        selected = !selected;
        SetOutline(selected);
    }

    public void SetOutline(bool show)
    {
        OutlineMaterial.SetFloat("_Scale", show ? 1.1f : 1f);
    }

    protected override void ShowCard()
    {
        VirusCard card = (VirusCard)MemoryCard;
        VirusColor color = card.GetColor();
        VirusType type = card.GetType();
        TreatmentType treatment = card.GetTreatmentType();
        
        string asset = "Sprites/Virus/Cards/";
        
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

        ShowCardRenderer.sprite = Resources.Load<Sprite>(asset);
        ShowCardRenderer.enabled = true;
    }

    protected override void HideCard()
    {
        ShowCardRenderer.enabled = false;
    }
}
