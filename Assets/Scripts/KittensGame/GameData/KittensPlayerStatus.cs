using System.Collections.Generic;
using UnityEngine;

public class KittensPlayerStatus: PlayerStatus
{
    public KittensPlayerStatus(Queue<Card> hand, GameObject handGObject, Player player, bool isAlive = true) : base(hand, handGObject, player, isAlive)
    { }
    
    public KittensPlayerStatus Clone()
    {
        Queue<Card> newHand = new ();
        foreach (Card card in Hand)
        {
            newHand.Enqueue(card);
        }
        
        return new KittensPlayerStatus(newHand, HandGObject, Player, Alive);
    }
}