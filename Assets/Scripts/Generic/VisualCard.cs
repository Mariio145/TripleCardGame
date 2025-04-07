using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VisualCard : MonoBehaviour
{
    public Card MemoryCard;
    protected MeshRenderer MeshRenderer;
    private MeshCollider _collider;
    public bool selected;
    private static readonly Vector3 CardSize = new (25f, 25f, 25f);
    
    public float hoverHeight = 0.5f;

    private Vector3 _originalPosition;
    [SerializeField]private Vector3 targetPosition;
    
    private Coroutine _hoverCoroutine;
    private bool _isHovering;
    
    protected Image CardRenderer;
    protected Material OutlineMaterial;

    public void Awake()
    {
        transform.localScale = CardSize;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer = gameObject.AddComponent<MeshRenderer>();
        _collider = gameObject.AddComponent<MeshCollider>();
        _collider.enabled = false;
        gameObject.name = "Card";
        _originalPosition = transform.localPosition;
        targetPosition = _originalPosition;
        meshFilter.mesh = ResourcesLoader.Instance.cardMesh;
        _collider.sharedMesh = ResourcesLoader.Instance.cardMesh;
        CardRenderer = GameManager.ShowCardRender;
        OutlineMaterial = new Material(Shader.Find("Shader Graphs/Outline"));
        OutlineMaterial.SetColor("_Color", Color.white);
        List<Material> materials = new()
        {
            OutlineMaterial,
            OutlineMaterial
        };
        MeshRenderer.SetMaterials(materials);
        MeshRenderer.renderingLayerMask = 1 << 1;
        DeactivateCollider();
    }

    public void ActivateCollider()
    {
        _collider.enabled = true;
    }

    public void DeactivateCollider()
    {
        _collider.enabled = false;
    }

    public async Task SetPosition(Vector3 position, bool randomRotate = false)
    {
        _originalPosition = position;
        targetPosition = _originalPosition;
        await MoveToTarget(randomRotate);
    }
    
    private async Task MoveToTarget(bool randomRotate = false)
    {
        if (randomRotate) transform.DOLocalRotate(new Vector3(0 , transform.localRotation.y + Random.Range(-15f, 15f), transform.localRotation.z), 0.5f).SetEase(Ease.OutQuad);
        else transform.DOLocalRotate(new Vector3(0, transform.localRotation.y, transform.localRotation.z), 0.5f).SetEase(Ease.OutQuad);
        // new Vector3 (0, transform.rotation.eulerAngles.y, 180)
        await transform.DOLocalMove(targetPosition, 0.5f).AsyncWaitForCompletion();
    }

    public virtual void ChangeSprite()
    { }

    public async void ChangeParent(Transform parent)
    {
        transform.SetParent(parent);
        //transform.DOKill();
        targetPosition = Vector3.zero;
        //transform.localRotation = Quaternion.Euler(-90, transform.localRotation.y, transform.localRotation.z);
        
        if (parent.GetComponent<VisualHand>() is not null)
            parent.GetComponent<VisualHand>().UpdateHandPosition();
        
        await MoveToTarget();
    }
    
    public async void ChangeUnoParent(Transform parent, bool enableCollider, Vector3 position = default)
    {
        transform.SetParent(parent);
        transform.DOKill();
        targetPosition = position;
        //transform.localRotation = Quaternion.Euler(-90, transform.localRotation.y, transform.localRotation.z);
        
        if (parent.GetComponent<VisualHand>() is not null)
            parent.GetComponent<VisualHand>().UpdateHandPosition();

        if (enableCollider) _collider.enabled = true;
        await UnoPlayCardAnim();
    }
    
    private async Task UnoPlayCardAnim()
    {
        transform.DOLocalRotate(new Vector3(0, 0, 0), 0.4f);
        await transform.DOLocalMove(targetPosition, 0.5f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.localPosition = targetPosition;
    }

    public void OnMouseEnter()
    {
        targetPosition = _originalPosition + Vector3.forward * hoverHeight;
        _ = MoveToTarget();
        _isHovering = true;
        _hoverCoroutine = StartCoroutine(HoverTimer());
    }

    public void OnMouseExit()
    {
        targetPosition = _originalPosition;
        _ = MoveToTarget();
        _isHovering = false;
        
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
        
        HideCard();
    }

    protected virtual void OnMouseDown()
    {
        if (selected)
        {
            SetOutline(false);
            selected = false;
            return;
        }

        VisualCard[] cards = transform.parent.GetComponentsInChildren<VisualCard>();
        foreach (VisualCard card in cards)
        {
            card.SetOutline(false);
            card.selected = false;
        }
        selected = true;
        SetOutline(true);
    }
    
    public void SetOutline(bool show)
    {
        OutlineMaterial.SetFloat("_Scale", show ? 1.1f : 1f);
    }

    private IEnumerator HoverTimer()
    {
        yield return new WaitForSeconds(1f);

        if (_isHovering) 
        {
            ShowCard();
        }
    }

    protected virtual void ShowCard()
    { }

    protected virtual void HideCard()
    {
        CardRenderer.enabled = false;
    }

}
