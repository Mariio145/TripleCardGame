using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] public GameObject handSlot;
    public string Name;
    [HideInInspector] public int index;
    protected IForwardModel ForwardModel;
    protected IHeuristic Heuristic;
    protected static readonly System.Random Random = new();
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteRenderer.enabled = false;
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