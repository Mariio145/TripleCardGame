using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class KittensShuffle : KittensAction
{
    public KittensShuffle(int cardIndex)
    {
        CardIndex = cardIndex;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);
        KittensPlayerStatus player = kittensGs.PlayersStatus[kittensGs.GetPlayerTurnIndex()];
        
        kittensGs.DrawDeck.ShuffleDeck();
        kittensGs.TopVisibleCards = new List<KittensCard>[kittensGs.TopVisibleCards.Length];
        
        Debug.Log("Antes: " + player.Hand.Count);
        Card card = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex);
        kittensGs.DiscardCard(card);
        Debug.Log("Despues: " + player.Hand.Count);
        
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not KittensObservation kittensOb) return false;
        KittensPlayerStatus player = kittensOb.PlayersStatus[kittensOb.GetPlayerTurnIndex()];
        
        kittensOb.MixedDrawDeck.ShuffleDeck();
        kittensOb.TopVisibleCards = new List<KittensCard>[kittensOb.TopVisibleCards.Length];
        
        kittensOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex));

        return true;
    }
}