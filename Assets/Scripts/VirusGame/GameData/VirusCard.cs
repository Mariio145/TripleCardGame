using UnityEngine;

public enum VirusType
{
    Organ,
    Medicine,
    Virus,
    Treatment
}
public enum VirusColor
{
    Red,
    Blue,
    Rainbow,
    Yellow,
    Green,
    None
}

public enum TreatmentType
{
    Transplant,
    OrganThief,
    Spreading,
    LatexGlove,
    MedicalError, 
    None
}

public class VirusCard: Card
{
    private readonly VirusType _type;
    private readonly VirusColor _color;
    private readonly TreatmentType _treatment;

    public VirusCard(VirusColor color, VirusType type, TreatmentType treatment = TreatmentType.None, GameObject parent = null)
    {
        _color = color;
        _type = type;
        _treatment = treatment;
        if (parent is not null) CreateVisualCard(parent.transform);
    }
    public new VirusType GetType()
    {
        return _type;
    }
    
    public VirusColor GetColor()
    {
        return _color;
    }
    
    public TreatmentType GetTreatmentType()
    {
        return _treatment;
    }
    
    public override string ToString()
    {
        return $"Tipo: {_type}, Color: {_color}, Tratamiento: {_treatment}";
    }
}
