using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [HideInInspector] public GameObject handSlot;
    public string Name;
    [HideInInspector] public int index;
    [HideInInspector] public Sprite icon;
    protected IForwardModel ForwardModel;
    protected IHeuristic Heuristic;
    protected static readonly System.Random Random = new();
    private SpriteRenderer _turnBorder;

    void Awake()
    {
        handSlot = GetComponentInChildren<VisualHand>().gameObject;
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _turnBorder = spriteRenderers.First(spRenderer => spRenderer.gameObject.CompareTag("Border"));
        _turnBorder.enabled = false;
    }

    public Sprite GetIcon()
    {
        return GetComponentsInChildren<SpriteRenderer>().First(spRenderer => spRenderer.gameObject.CompareTag("Icon")).sprite;
    }

    public void StartTurn()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = true;
    }

    public void StopTurn()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
    

    public void SetForwardModel(IForwardModel forwardModel)
    {
        ForwardModel = forwardModel;
    }
    
    public virtual void SetHeuristic(GameToPlay gametoPlay)
    {
        Heuristic = gametoPlay switch
        {
            GameToPlay.VirusGame => new VirusSimpleHeuristic(),
            GameToPlay.UnoGame => new UnoSimpleHeuristic(),
            GameToPlay.KittensGame => new KittensSimpleHeuristic(),
            _ => throw new ArgumentOutOfRangeException(nameof(gametoPlay), gametoPlay, null)
        };
    }
    public virtual IAction Think(IObservation observable, float thinkingTime) => null;
}