using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class VisualVirusBody : MonoBehaviour
{
    [Serializable]
    public struct OrganGameObject
    {
        public GameObject gameObject;
        public VisualVirusOrgan visualOrgan;
        public MeshRenderer medicineMesh, medicineMesh2, virusMesh;
        public Light organLight;
    }
    
    public OrganGameObject redOrgan, blueOrgan, greenOrgan, yellowOrgan, rainbowOrgan;

    private static readonly Vector3 TokenOutOfCamera = new(0, 0, 0.25f);
    private static readonly Vector3 VirusScale = new(0.1f, 0.1f, 0.1f);
    
    private static SynchronizationContext _mainThreadContext;

    public void Awake()
    {
        redOrgan.gameObject.transform.localPosition = TokenOutOfCamera;
        blueOrgan.gameObject.transform.localPosition = TokenOutOfCamera;
        greenOrgan.gameObject.transform.localPosition = TokenOutOfCamera;
        yellowOrgan.gameObject.transform.localPosition = TokenOutOfCamera;
        rainbowOrgan.gameObject.transform.localPosition = TokenOutOfCamera;
        
        redOrgan.medicineMesh.gameObject.transform.localScale = Vector3.zero;
        redOrgan.medicineMesh2.gameObject.transform.localScale = Vector3.zero;
        redOrgan.virusMesh.gameObject.transform.localScale = Vector3.zero;
        blueOrgan.medicineMesh.gameObject.transform.localScale = Vector3.zero;
        blueOrgan.medicineMesh2.gameObject.transform.localScale = Vector3.zero;
        blueOrgan.virusMesh.gameObject.transform.localScale = Vector3.zero;
        greenOrgan.medicineMesh.gameObject.transform.localScale = Vector3.zero;
        greenOrgan.medicineMesh2.gameObject.transform.localScale = Vector3.zero;
        greenOrgan.virusMesh.gameObject.transform.localScale = Vector3.zero;
        yellowOrgan.medicineMesh.gameObject.transform.localScale = Vector3.zero;
        yellowOrgan.medicineMesh2.gameObject.transform.localScale = Vector3.zero;
        yellowOrgan.virusMesh.gameObject.transform.localScale = Vector3.zero;
        rainbowOrgan.medicineMesh.gameObject.transform.localScale = Vector3.zero;
        rainbowOrgan.medicineMesh2.gameObject.transform.localScale = Vector3.zero;
        rainbowOrgan.virusMesh.gameObject.transform.localScale = Vector3.zero;
        
        redOrgan.visualOrgan.Deactivate();
        blueOrgan.visualOrgan.Deactivate();
        greenOrgan.visualOrgan.Deactivate();
        yellowOrgan.visualOrgan.Deactivate();
        rainbowOrgan.visualOrgan.Deactivate();
        
        _mainThreadContext = SynchronizationContext.Current;
        
    
    }
    
    /*
     *
     * ANIMATIONS
     *
     */
    public void ObscureOrgans()
    {
        _mainThreadContext.Send(_ =>
        {
            redOrgan.organLight.enabled = false;
            blueOrgan.organLight.enabled = false;
            greenOrgan.organLight.enabled = false;
            yellowOrgan.organLight.enabled = false;
            rainbowOrgan.organLight.enabled = false;

            redOrgan.visualOrgan.Deactivate();
            blueOrgan.visualOrgan.Deactivate();
            greenOrgan.visualOrgan.Deactivate();
            yellowOrgan.visualOrgan.Deactivate();
            rainbowOrgan.visualOrgan.Deactivate();
        }, null);
    }

    public void IluminateOrgans(List<VirusColor> colors)
    {
        foreach (VirusColor color in colors)
        {
            switch (color)
            {
                case VirusColor.Red:
                    _mainThreadContext.Send(_ =>
                    {
                        redOrgan.visualOrgan.Activate();
                        redOrgan.organLight.enabled = true;
                    }, null);
                    
                    break;
                case VirusColor.Blue:
                    
                    _mainThreadContext.Send(_ =>
                    {
                        blueOrgan.visualOrgan.Activate();
                        blueOrgan.organLight.enabled = true;
                    }, null);
                    
                    break;
                case VirusColor.Rainbow:
                    _mainThreadContext.Send(_ =>
                    {
                        rainbowOrgan.visualOrgan.Activate();
                        rainbowOrgan.organLight.enabled = true;
                    }, null);
                    
                    break;
                case VirusColor.Yellow:
                    _mainThreadContext.Send(_ =>
                    {
                        yellowOrgan.visualOrgan.Activate();
                        yellowOrgan.organLight.enabled = true;
                    }, null);
                   
                    break;
                case VirusColor.Green:
                    _mainThreadContext.Send(_ =>
                    {
                        greenOrgan.visualOrgan.Activate();
                        greenOrgan.organLight.enabled = true;
                    }, null);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

    }

    public async Task PlaceOrganAnimation(VirusColor color)
    {
        OrganGameObject organ = GetOrganObject(color);
        
        await organ.gameObject.transform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InQuint).AsyncWaitForCompletion();
    }

    public async Task RemoveOrganAnimation(VirusColor color)
    {
        OrganGameObject organ = GetOrganObject(color);
        
        await organ.gameObject.transform.DOLocalMove(TokenOutOfCamera, 1f).SetEase(Ease.InQuint).AsyncWaitForCompletion();
    }

    public async Task PlaceMedicine1Animation(VirusColor organTarget, VirusColor medicineColor)
    {
        Material medColor = GetMedicineMaterial(medicineColor);
        
        OrganGameObject organ = GetOrganObject(organTarget);
        
        organ.medicineMesh.material = medColor;

        await organ.medicineMesh.gameObject.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic).AsyncWaitForCompletion();
    }
    
    public async Task PlaceMedicine2Animation(VirusColor organTarget, VirusColor medicineColor)
    {
        Material medColor = GetMedicineMaterial(medicineColor);
        
        OrganGameObject organ = GetOrganObject(organTarget);
        
        organ.medicineMesh2.material = medColor;

        await organ.medicineMesh2.gameObject.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic).AsyncWaitForCompletion();
    }
    
    public async Task RemoveMedicine1Animation(VirusColor organTarget)
    {
        OrganGameObject organ = GetOrganObject(organTarget);

        await organ.medicineMesh.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic).AsyncWaitForCompletion();
    }

    private async Task RemoveMedicine2Animation(VirusColor organTarget)
    {
        OrganGameObject organ = GetOrganObject(organTarget);

        await organ.medicineMesh2.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic).AsyncWaitForCompletion();
    }

    public async Task PlaceVirusAnimation(VirusColor organTarget, VirusColor virusColor)
    {
        Material virColor = GetVirusMaterial(virusColor);
        
        OrganGameObject organ = GetOrganObject(organTarget);
        
        organ.virusMesh.material = virColor;

        await organ.virusMesh.gameObject.transform.DOScale(VirusScale, 1f).SetEase(Ease.OutElastic).AsyncWaitForCompletion();
    }

    public async Task RemoveVirusAnimation(VirusColor organTarget)
    {
        OrganGameObject organ = GetOrganObject(organTarget);
        
        await organ.virusMesh.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic).AsyncWaitForCompletion();
    }

    public async Task RemoveOrganComplete(VirusColor organColor)
    {
        List<Task> tasks = new()
        {
            RemoveVirusAnimation(organColor),
            RemoveMedicine1Animation(organColor),
            RemoveMedicine2Animation(organColor),
        };

        await Task.WhenAll(tasks);
        await RemoveOrganAnimation(organColor);
    }
    
    public async Task PlaceOrganComplete(VirusColor organColor, VirusColor medColor = VirusColor.None, VirusColor medColor2 = VirusColor.None, VirusColor virColor = VirusColor.None)
    {
        List<Task> tasks = new();
        
        await PlaceOrganAnimation(organColor);

        if (virColor is not VirusColor.None) tasks.Add(PlaceVirusAnimation(organColor, virColor));
        if (medColor is not VirusColor.None) tasks.Add(PlaceMedicine1Animation(organColor, medColor));
        if (medColor2 is not VirusColor.None) tasks.Add(PlaceMedicine2Animation(organColor, medColor2));
        
        await Task.WhenAll(tasks);
    }
    
    

    private OrganGameObject GetOrganObject(VirusColor color)
    {
        OrganGameObject organ = color switch
        {
            VirusColor.Red => redOrgan,
            VirusColor.Blue => blueOrgan,
            VirusColor.Green => greenOrgan,
            VirusColor.Yellow => yellowOrgan,
            VirusColor.Rainbow => rainbowOrgan,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
        
        return organ;
    }

    private Material GetMedicineMaterial(VirusColor color)
    {
        Material material = color switch
        {
            VirusColor.Red => ResourcesLoader.Instance.redMedMat,
            VirusColor.Blue => ResourcesLoader.Instance.blueMedMat,
            VirusColor.Green => ResourcesLoader.Instance.greenMedMat,
            VirusColor.Yellow => ResourcesLoader.Instance.yellowMedMat,
            VirusColor.Rainbow => ResourcesLoader.Instance.rainbowMedMat,
            _ => new Material(Shader.Find("Standard"))
        };

        return material;
    }

    private Material GetVirusMaterial(VirusColor color)
    {
        Material material = color switch
        {
            VirusColor.Red => ResourcesLoader.Instance.redVirMat,
            VirusColor.Blue => ResourcesLoader.Instance.blueVirMat,
            VirusColor.Green => ResourcesLoader.Instance.greenVirMat,
            VirusColor.Yellow => ResourcesLoader.Instance.yellowVirMat,
            VirusColor.Rainbow => ResourcesLoader.Instance.rainbowVirMat,
            _ => new Material(Shader.Find("Standard"))
        };
    
        return material;
    }
    
}
