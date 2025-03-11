using System.Collections.Generic;
using System.Threading.Tasks;

public class KittensPair : KittensAction
{
    private readonly int _discardIndex1;
    private readonly int _discardIndex2;
    private readonly int _playerStealIndex;
    private readonly int _stealCardIndex;

    public KittensPair(int discardIndex1, int discardIndex2, int playerStealIndex , int stealCardIndex)
    {
        _discardIndex1 = discardIndex1;
        _discardIndex2 = discardIndex2;
        _playerStealIndex = playerStealIndex;
        _stealCardIndex = stealCardIndex;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) return Task.FromResult(false);

        KittensPlayerStatus playerSelf = kittensGs.PlayersStatus[kittensGs.GetPlayerTurnIndex()];
        KittensPlayerStatus playerTarget = kittensGs.PlayersStatus[_playerStealIndex];

        Card sndCardPlayed, cardPlayed;

        if (_discardIndex1 > _discardIndex2)
        {
            cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex1);
            kittensGs.DiscardCard(cardPlayed);
            
            sndCardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex2);
            kittensGs.DiscardCard(sndCardPlayed);
        }
        else
        {
            sndCardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex2);
            kittensGs.DiscardCard(sndCardPlayed);
            
            cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex1);
            kittensGs.DiscardCard(cardPlayed);
        }
        
        Card cardStolen = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerTarget.Hand, _stealCardIndex);

        bool isHumanPlayer = kittensGs.GetPlayerTurnIndex() == 0;
        cardStolen.VisualCard.ChangeParent(playerSelf.HandGObject.transform, isHumanPlayer);
        playerSelf.Hand.Enqueue(cardStolen);
        
        return Task.FromResult(true);
    }

    public override bool TestAction(IObservation observation) 
    {
        if (observation is not KittensObservation kittensOb) return false;
        
        KittensPlayerStatus playerSelf = kittensOb.PlayersStatus[kittensOb.GetPlayerTurnIndex()];
        KittensPlayerStatus playerTarget = kittensOb.PlayersStatus[_playerStealIndex];
        
        Card sndCardPlayed, cardPlayed;
        
        if (_discardIndex1 > _discardIndex2)
        {
            cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex1);
            kittensOb.DiscardCard(cardPlayed);
        
            sndCardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex2);
            kittensOb.DiscardCard(sndCardPlayed);
        }
        else
        {
            sndCardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex2);
            kittensOb.DiscardCard(sndCardPlayed);
            
            cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _discardIndex1);
            kittensOb.DiscardCard(cardPlayed);
        }
        
        Card cardStolen = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerTarget.Hand, _stealCardIndex);
        
        playerSelf.Hand.Enqueue(cardStolen);
        
        return true;
    }
}