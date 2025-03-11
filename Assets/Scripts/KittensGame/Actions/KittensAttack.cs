using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class KittensAttack : KittensAction
{
    public KittensAttack(int cardIndex)
    {
        CardIndex = cardIndex;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);

        KittensPlayerStatus player = kittensGs.PlayersStatus[kittensGs.GetPlayerTurnIndex()];

        kittensGs.DrawAtEnd = false;
        kittensGs.TurnsToPlay = 0;
        kittensGs.NextTurnsToPlay = 2;
        kittensGs.TurnEnded = true;

        Debug.Log("Antes: " + player.Hand.Count);
        Card card = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex);
        kittensGs.DiscardCard(card);
        Debug.Log("Después: " + player.Hand.Count);
        
        
        
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not KittensObservation kittensOb) return false;
        
        KittensPlayerStatus player = kittensOb.PlayersStatus[kittensOb.GetPlayerTurnIndex()];
        
        kittensOb.DrawAtEnd = false;
        kittensOb.TurnsToPlay = 1;
        kittensOb.NextTurnsToPlay = 2;
        kittensOb.TurnEnded = true;
        
        kittensOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex));
        
        return true;
    }
}