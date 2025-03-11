using System;
using System.Collections.Generic;
using UnityEngine;

public class VisualVirusBody : MonoBehaviour
{
    [Serializable]
    public struct OrganGameObject
    {
        public GameObject gameObject;
        public MeshRenderer medicineMesh, medicineMesh2, virusMesh;
    }
    
    public OrganGameObject redOrgan, blueOrgan, greenOrgan, yellowOrgan, rainbowOrgan;
    public VirusPlayerStatus PlayerStatus;

    public void Awake()
    {
        redOrgan.gameObject.SetActive(false);
        blueOrgan.gameObject.SetActive(false);
        greenOrgan.gameObject.SetActive(false);
        yellowOrgan.gameObject.SetActive(false);
        rainbowOrgan.gameObject.SetActive(false);
        
        redOrgan.medicineMesh.enabled = false;
        redOrgan.medicineMesh2.enabled = false;
        redOrgan.virusMesh.enabled = false;
        blueOrgan.medicineMesh.enabled = false;
        blueOrgan.medicineMesh2.enabled = false;
        blueOrgan.virusMesh.enabled = false;
        greenOrgan.medicineMesh.enabled = false;
        greenOrgan.medicineMesh2.enabled = false;
        greenOrgan.virusMesh.enabled = false;
        yellowOrgan.medicineMesh.enabled = false;
        yellowOrgan.medicineMesh2.enabled = false;
        yellowOrgan.virusMesh.enabled = false;
        rainbowOrgan.medicineMesh.enabled = false;
        rainbowOrgan.medicineMesh2.enabled = false;
        rainbowOrgan.virusMesh.enabled = false;
    }

    public void AddOrgan(VirusColor organColor)
    {
        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.gameObject.SetActive(true);
                break;
            case VirusColor.Blue:
                blueOrgan.gameObject.SetActive(true);
                break;
            case VirusColor.Green:
                greenOrgan.gameObject.SetActive(true);
                break;
            case VirusColor.Yellow:
                yellowOrgan.gameObject.SetActive(true);
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.gameObject.SetActive(true);
                break;
        }
    }

    public void RemoveOrgan(VirusColor organColor)
    {
        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.medicineMesh.enabled = false;
                redOrgan.virusMesh.enabled = false;
                redOrgan.gameObject.SetActive(false);
                break;
            case VirusColor.Blue:
                blueOrgan.medicineMesh.enabled = false;
                blueOrgan.virusMesh.enabled = false;
                blueOrgan.gameObject.SetActive(false);
                break;
            case VirusColor.Green:
                greenOrgan.medicineMesh.enabled = false;
                greenOrgan.virusMesh.enabled = false;
                greenOrgan.gameObject.SetActive(false);
                break;
            case VirusColor.Yellow:
                yellowOrgan.medicineMesh.enabled = false;
                yellowOrgan.virusMesh.enabled = false;
                yellowOrgan.gameObject.SetActive(false);
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.medicineMesh.enabled = false;
                rainbowOrgan.virusMesh.enabled = false;
                rainbowOrgan.gameObject.SetActive(false);
                break;
        }
    }

    public void AddMedicineToOrgan(VirusColor organColor, VirusColor medicineColor)
    {
        Material medColor = medicineColor switch
        {
            VirusColor.Red => ResourcesLoader.Instance.redMedMat,
            VirusColor.Blue => ResourcesLoader.Instance.blueMedMat,
            VirusColor.Green => ResourcesLoader.Instance.greenMedMat,
            VirusColor.Yellow => ResourcesLoader.Instance.yellowMedMat,
            VirusColor.Rainbow => ResourcesLoader.Instance.rainbowMedMat,
            _ => new Material(Shader.Find("Standard"))
        };

        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.medicineMesh.enabled = true;
                redOrgan.medicineMesh.material = medColor;
                break;
            case VirusColor.Blue:
                blueOrgan.medicineMesh.enabled = true;
                blueOrgan.medicineMesh.material = medColor;
                break;
            case VirusColor.Green:
                greenOrgan.medicineMesh.enabled = true;
                greenOrgan.medicineMesh.material = medColor;
                break;
            case VirusColor.Yellow:
                yellowOrgan.medicineMesh.enabled = true;
                yellowOrgan.medicineMesh.material = medColor;
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.medicineMesh.enabled = true;
                rainbowOrgan.medicineMesh.material = medColor;
                break;
        }
    }

    public void RemoveMedicineFromOrgan(VirusColor organColor)
    {
        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.medicineMesh.enabled = false;
                break;
            case VirusColor.Blue:
                blueOrgan.medicineMesh.enabled = false;
                break;
            case VirusColor.Green:
                greenOrgan.medicineMesh.enabled = false;
                break;
            case VirusColor.Yellow:
                yellowOrgan.medicineMesh.enabled = false;
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.medicineMesh.enabled = false;
                break;
        }
    }

    public void AddVirusToOrgan(VirusColor organColor, VirusColor virusColor)
    {
        Material virColor = virusColor switch
        {
            VirusColor.Red => ResourcesLoader.Instance.redMedMat,
            VirusColor.Blue => ResourcesLoader.Instance.blueMedMat,
            VirusColor.Green => ResourcesLoader.Instance.greenMedMat,
            VirusColor.Yellow => ResourcesLoader.Instance.yellowMedMat,
            VirusColor.Rainbow => ResourcesLoader.Instance.rainbowMedMat,
            _ => new Material(Shader.Find("Standard"))
        };

        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.virusMesh.enabled = true;
                redOrgan.virusMesh.material = virColor;
                break;
            case VirusColor.Blue:
                blueOrgan.virusMesh.enabled = true;
                blueOrgan.virusMesh.material = virColor;
                break;
            case VirusColor.Green:
                greenOrgan.virusMesh.enabled = true;
                greenOrgan.virusMesh.material = virColor;
                break;
            case VirusColor.Yellow:
                yellowOrgan.virusMesh.enabled = true;
                yellowOrgan.virusMesh.material = virColor;
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.virusMesh.enabled = true;
                rainbowOrgan.virusMesh.material = virColor;
                break;
        }
    }

    public void RemoveVirusFromOrgan(VirusColor organColor)
    {
        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.virusMesh.enabled = false;
                break;
            case VirusColor.Blue:
                blueOrgan.virusMesh.enabled = false;
                break;
            case VirusColor.Green:
                greenOrgan.virusMesh.enabled = false;
                break;
            case VirusColor.Yellow:
                yellowOrgan.virusMesh.enabled = false;
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.virusMesh.enabled = false;
                break;
        }
    }

    public void ImmunizeOrgan(VirusColor organColor, VirusColor medicineColor)
    {
        Material medColor = medicineColor switch
        {
            VirusColor.Red => ResourcesLoader.Instance.redMedMat,
            VirusColor.Blue => ResourcesLoader.Instance.blueMedMat,
            VirusColor.Green => ResourcesLoader.Instance.greenMedMat,
            VirusColor.Yellow => ResourcesLoader.Instance.yellowMedMat,
            VirusColor.Rainbow => ResourcesLoader.Instance.rainbowMedMat,
            _ => new Material(Shader.Find("Standard"))
        };

        switch (organColor)
        {
            case VirusColor.Red:
                redOrgan.medicineMesh2.enabled = true;
                redOrgan.medicineMesh2.material = medColor;
                break;
            case VirusColor.Blue:
                blueOrgan.medicineMesh2.enabled = true;
                blueOrgan.medicineMesh2.material = medColor;
                break;
            case VirusColor.Green:
                greenOrgan.medicineMesh2.enabled = true;
                greenOrgan.medicineMesh2.material = medColor;
                break;
            case VirusColor.Yellow:
                yellowOrgan.medicineMesh2.enabled = true;
                yellowOrgan.medicineMesh2.material = medColor;
                break;
            case VirusColor.Rainbow:
                rainbowOrgan.medicineMesh2.enabled = true;
                rainbowOrgan.medicineMesh2.material = medColor;
                break;
        }
    }

    public void UpdateBody()
    {
        List<VirusColor> allColors = new ((Enum.GetValues(typeof(VirusColor)) as VirusColor[])!);
        allColors.Remove(VirusColor.None);
        
        Debug.Log("Count pero en visualVirus: " + PlayerStatus.Body.Count);
        
        foreach (VirusOrgan organ in PlayerStatus.Body)
        {
            VirusColor color = organ.OrganColor;
            allColors.Remove(color);
            AddOrgan(color);
            
            switch (organ.Status)
            {
                case Status.Immune:
                    AddMedicineToOrgan(color, organ.MedicineColor);
                    ImmunizeOrgan(color, organ.MedicineColor2);
                    break;
                case Status.Vaccinated:
                    AddMedicineToOrgan(color, organ.MedicineColor);
                    break;
                case Status.Infected:
                    AddVirusToOrgan(color, organ.VirusColor);
                    break;
            }
        }
        
        foreach (VirusColor color in allColors)
        {
            RemoveOrgan(color);
        }
    }
}
