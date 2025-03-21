using System.Threading.Tasks;
using UnityEngine;
public class UnoPlayCard : UnoAction
{
    private readonly UnoType _cardType;
    private readonly int _cardIndex;
    private readonly UnoColor _changeTo;

    public UnoPlayCard(UnoCard card, int handIndex)
    {
        
        _cardType = card.Type;
        _cardIndex = handIndex;
    }

    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return false;
        
        UnoPlayerStatus playerSelf = unoGs.PlayersStatus[unoGs.GetPlayerTurnIndex()];
        Card cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _cardIndex);
        cardPlayed.VisualCard.ChangeUnoParent(unoGs.discardGo.transform, false, unoGs.topCard.VisualCard.transform.localPosition - new Vector3(0, 0.002f, 0));
        unoGs.topCard = (UnoCard)cardPlayed;

        switch (_cardType)
        {
            case UnoType.Reverse:
                await unoGs.ShowReverseObject();
                unoGs.IsReversed = !unoGs.IsReversed;
                if (unoGs.PlayersStatus.Count <= 2) 
                    unoGs.blockNextTurn = true;
                break;
            case UnoType.Block:
                await unoGs.ShowBlockObject();
                unoGs.blockNextTurn = true;
                break;
            case UnoType.Draw2:
                unoGs.quantityToDraw += 2;
                break;
            case UnoType.Draw4:
                unoGs.quantityToDraw += 4;
                break;
        }
        
        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return false;
        
        UnoPlayerStatus playerSelf = unoObs.PlayersStatus[unoObs.GetPlayerTurnIndex()];
        
        Card cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, _cardIndex);
        
        unoObs.TopCard = (UnoCard)cardPlayed;

        switch (_cardType)
        {
            case UnoType.Reverse:
                unoObs.IsReversed = !unoObs.IsReversed;
                if (unoObs.PlayersStatus.Count <= 2) 
                    unoObs.blockNextTurn = true;
                break;
            case UnoType.Block:
                unoObs.blockNextTurn = true;
                break;
            case UnoType.Draw2:
                unoObs.quantityToDraw += 2;
                break;
            case UnoType.Draw4:
                unoObs.quantityToDraw += 4;
                break;
        }
        
        return true;
    }
}
