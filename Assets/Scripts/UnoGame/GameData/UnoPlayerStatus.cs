using System.Collections.Generic;
using UnityEngine;

public class UnoPlayerStatus : PlayerStatus
{
    public UnoPlayerStatus(Queue<Card> hand, GameObject handGObject, Player player) : base(hand, handGObject, player)
    {
        Hand = hand;
        HandGObject = handGObject;
    }
    
    public UnoPlayerStatus Clone()
    {
        Queue<Card> newHand = new ();
        foreach (Card card in Hand)
        {
            newHand.Enqueue(card);
        }
        
        return new UnoPlayerStatus(newHand, HandGObject, Player);
    }
}