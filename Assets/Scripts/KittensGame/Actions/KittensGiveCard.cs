using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class KittensGiveCard : KittensAction
{
    private readonly int _playerReceive;
    private readonly int _playerSend;
    private readonly int _stealIndex;

    public KittensGiveCard(int playerReceive, int playerSend, int discardIndex, int stealIndex)
    {
        _playerReceive = playerReceive;
        _playerSend = playerSend;
        CardIndex = discardIndex;
        _stealIndex = stealIndex;
    }
    
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);
        
        PlayerStatus playerTarget = kittensGs.PlayersStatus[_playerSend];
        PlayerStatus playerSelf = kittensGs.PlayersStatus[_playerReceive];

        Debug.Log("Antes: " + playerSelf.Hand.Count);
        Card discardCard = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);
        kittensGs.DiscardCard(discardCard);
        Debug.Log("Despues: " + playerSelf.Hand.Count);
        
        Card cardStolen = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerTarget.Hand, _stealIndex);
        
        cardStolen.VisualCard.ChangeParent(playerSelf.HandGObject.transform);
        playerSelf.Hand.Enqueue(cardStolen);
        
        return Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not KittensObservation kittensOb) return false;
        
        PlayerStatus playerTarget = kittensOb.PlayersStatus[_playerSend];
        PlayerStatus playerSelf = kittensOb.PlayersStatus[_playerReceive];
        
        kittensOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex));
        
        Card cardStolen = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerTarget.Hand, CardIndex);
        
        playerSelf.Hand.Enqueue(cardStolen);
        return true;
    }
}
