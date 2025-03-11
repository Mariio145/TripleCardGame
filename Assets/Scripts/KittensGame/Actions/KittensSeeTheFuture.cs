using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class KittensSeeTheFuture : KittensAction
{
    public KittensSeeTheFuture(int cardIndex)
    {
        CardIndex = cardIndex;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);
        KittensPlayerStatus player = kittensGs.PlayersStatus[kittensGs.GetPlayerTurnIndex()];
        
        Deck<KittensCard> deckCopy = new (kittensGs.DrawDeck);
        List<KittensCard> threeCards = new();

        for (int i = 0; i < 3; i++)
        {
            threeCards.Add(deckCopy.DrawCard());
        }
        
        kittensGs.TopVisibleCards[kittensGs.GetPlayerTurnIndex()] = threeCards;
        
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
        
        Deck<KittensCard> deckCopy = new (kittensOb.MixedDrawDeck);
        List<KittensCard> threeCards = new();

        for (int i = 0; i < 3; i++)
        {
            threeCards.Add(deckCopy.DrawCard());
        }
        
        kittensOb.TopVisibleCards[kittensOb.GetPlayerTurnIndex()] = threeCards;
        
        kittensOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex));
        
        return true;
    }
}