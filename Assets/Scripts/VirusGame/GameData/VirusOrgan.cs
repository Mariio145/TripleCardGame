﻿public class VirusOrgan
{
    public readonly VirusColor OrganColor;
    public Status Status;
    public VirusColor MedicineColor, MedicineColor2;
    public VirusColor VirusColor;
    
    public VirusOrgan(VirusColor organ)
    {
        OrganColor = organ;
        Status = Status.Normal;
        MedicineColor = VirusColor.None;
        MedicineColor2 = VirusColor.None;
        VirusColor = VirusColor.None;
    }

    public VirusOrgan Clone()
    {
        VirusOrgan copyOrgan = new(OrganColor)
        {
            Status = Status,
            MedicineColor = MedicineColor,
            MedicineColor2 = MedicineColor2,
            VirusColor = VirusColor
        };
        return copyOrgan;
    }
}

public enum Status
{
    Normal,
    Infected,
    Vaccinated,
    Immune
}