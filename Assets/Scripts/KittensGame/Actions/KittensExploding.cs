using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class KittensExploding : KittensAction
{
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);

        KittensPlayerStatus player = kittensGs.PlayersStatus[kittensGs.GetPlayerTurnIndex()];

        int cardIndex = -1;
        foreach (KittensCard card in player.Hand.Select(card => card as KittensCard))
        {
            cardIndex++;
            if (card!.Type != KittensType.Defuse) continue;
            //TODO: Seleccionar donde poner el explodingKitten en el mazo
            kittensGs.DrawDeck.Add(new KittensCard(KittensType.Exploding, kittensGs.DeckGo.transform));
            kittensGs.DrawDeck.ShuffleDeck();
            kittensGs.DiscardCard(card); //Se descarta el explodingKitten
            Debug.Log("Antes: " + player.Hand.Count);
            Card defuseCard = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, cardIndex);
            Debug.Log("Despues: " + player.Hand.Count);
            kittensGs.DiscardCard(defuseCard); //Se descarta el defuse
            
            return Task.FromResult(true);
        }
        
        kittensGs.KillPlayer(player);
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not KittensObservation kittensOb) return false;
        
        KittensPlayerStatus player = kittensOb.PlayersStatus[kittensOb.GetPlayerTurnIndex()];
        
        int cardIndex = -1;
        foreach (KittensCard card in player.Hand.Select(card => card as KittensCard))
        {
            cardIndex++;
            if (card!.Type != KittensType.Defuse) continue;
            //TODO: Seleccionar donde poner el explodingKitten en el mazo
            kittensOb.MixedDrawDeck.Add(new KittensCard(KittensType.Exploding));
            kittensOb.DiscardCard(card);
            
            Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, cardIndex);
            
            return true;
        }
        
        kittensOb.KillPlayer(player);
        
        return true;
    }
}