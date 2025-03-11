using UnityEngine;
using UnityEngine.Serialization;

public class ResourcesLoader : MonoBehaviour
{
    public static ResourcesLoader Instance; // Singleton
    
    //Para todos los juegos
    [Header("Para todos los juegos")]
    public Mesh cardMesh; // Asigna el prefab en el Inspector

    [Header("Para el Virus")] 
    public Material redMedMat;
    public Material blueMedMat;
    public Material greenMedMat;
    public Material yellowMedMat;
    public Material rainbowMedMat;
    
    public Material redVirMat;
    public Material blueVirMat;
    public Material greenVirMat;
    public Material yellowVirMat;
    public Material rainbowVirMat;
    
    [Header("Para el Uno")]
    public Material tableMaterial; // Asigna el prefab en el Inspector
    public GameObject reverse;
    public GameObject block;
    
    //[Header("Para el ExplodingKittens")]

    private void OnEnable()
    {
        if (Instance is null) Instance = this;
        else Destroy(this);
    }
}