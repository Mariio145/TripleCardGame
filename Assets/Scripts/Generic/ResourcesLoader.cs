using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ResourcesLoader : MonoBehaviour
{
    public static ResourcesLoader Instance; // Singleton
    
    //Para todos los juegos
    [Header("Para todos los juegos")]
    public Mesh cardMesh;

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
    public Material tableMaterial;
    public Material tokenMaterial;
    
    //[Header("Para el ExplodingKittens")]

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(this);
    }
}