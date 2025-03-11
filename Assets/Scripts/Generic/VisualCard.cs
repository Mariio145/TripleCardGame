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
    [SerializeField]private Vector3 _targetPosition;

    public void Awake()
    {
        transform.localScale = CardSize;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer = gameObject.AddComponent<MeshRenderer>();
        _collider = gameObject.AddComponent<MeshCollider>();
        _collider.enabled = false;
        gameObject.name = "Card";
        _originalPosition = transform.localPosition;
        _targetPosition = _originalPosition;
        meshFilter.mesh = ResourcesLoader.Instance.cardMesh;
    }

    public void SetPosition(Vector3 position, bool randomRotate = false)
    {
        _originalPosition = position;
        _targetPosition = _originalPosition;
        MoveToTarget(randomRotate);
    }
    
    private void MoveToTarget(bool randomRotate = false)
    {
        transform.DOLocalMove(_targetPosition, 0.5f);
        if (randomRotate) transform.DOLocalRotate(new Vector3(0 , transform.localRotation.y + Random.Range(-15f, 15f), transform.localRotation.z), 0.5f).SetEase(Ease.OutQuad);
        else transform.DOLocalRotate(new Vector3(0, transform.localRotation.y, transform.localRotation.z), 0.5f).SetEase(Ease.OutQuad);
        // new Vector3 (0, transform.rotation.eulerAngles.y, 180)
    }

    public virtual void ChangeSprite()
    { }

    public void ChangeParent(Transform parent, bool enableCollider)
    {
        transform.SetParent(parent);
        _targetPosition = Vector3.zero;
        //transform.localRotation = Quaternion.Euler(-90, transform.localRotation.y, transform.localRotation.z);
        
        if (parent.GetComponent<VisualHand>() is not null)
            parent.GetComponent<VisualHand>().UpdateHandPosition();

        if (enableCollider) _collider.enabled = true;
        MoveToTarget();
    }

    public void OnMouseEnter()
    {
        _targetPosition = _originalPosition + Vector3.up * hoverHeight;
        MoveToTarget();
    }

    public void OnMouseExit()
    {
        _targetPosition = _originalPosition;
        MoveToTarget();
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
