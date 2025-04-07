using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus
{
    public readonly Player Player;
    public Queue<Card> Hand;
    public GameObject HandGObject;
    protected bool Alive;

    protected PlayerStatus(Queue<Card> hand, GameObject handGObject, Player player, bool alive = true)
    {
        Hand = hand;
        HandGObject = handGObject;
        Player = player;
        Alive = alive;
    }

    public bool IsAlive()
    {
        return Alive;
    }

    public virtual bool HasWon()
    {
        return false;
    }

    public virtual int GetPunctuation()
    {
        return -1;
    }
}