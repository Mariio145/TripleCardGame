using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class VisualCard : MonoBehaviour
{
    public Card MemoryCard;
    protected MeshRenderer MeshRenderer;
    private MeshCollider _collider;
    public bool selected;
    private static readonly Vector3 CardSize = new (17f, 17f, 17f);
    
    public float hoverHeight = 0.5f;

    private Vector3 _originalPosition;
    [SerializeField]private Vector3 targetPosition;

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

    public async void ChangeParent(Transform parent, bool enableCollider)
    {
        transform.SetParent(parent);
        targetPosition = Vector3.zero;
        //transform.localRotation = Quaternion.Euler(-90, transform.localRotation.y, transform.localRotation.z);
        
        if (parent.GetComponent<VisualHand>() is not null)
            parent.GetComponent<VisualHand>().UpdateHandPosition();

        if (enableCollider) _collider.enabled = true;
        await MoveToTarget();
    }
    
    public void ChangeUnoParent(Transform parent, bool enableCollider, Vector3 position = default)
    {
        transform.SetParent(parent);
        targetPosition = position;
        //transform.localRotation = Quaternion.Euler(-90, transform.localRotation.y, transform.localRotation.z);
        
        if (parent.GetComponent<VisualHand>() is not null)
            parent.GetComponent<VisualHand>().UpdateHandPosition();

        if (enableCollider) _collider.enabled = true;
        UnoPlayCardAnim();
    }
    
    private void UnoPlayCardAnim()
    {
        transform.DOLocalMove(targetPosition, 0.5f).SetEase(Ease.InQuad);
        transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f);
        // new Vector3 (0, transform.rotation.eulerAngles.y, 180)
    }

    public void OnMouseEnter()
    {
        targetPosition = _originalPosition + Vector3.up * hoverHeight;
        _ = MoveToTarget();
    }

    public void OnMouseExit()
    {
        targetPosition = _originalPosition;
        _ = MoveToTarget();
    }

    protected virtual void OnMouseDown()
    {
        if (selected)
        {
            selected = false;
            return;
        }

        VisualCard[] cards = transform.parent.GetComponentsInChildren<VisualCard>();
        foreach (VisualCard card in cards)
        {
            card.selected = false;
        }
        selected = true;
    }
}
