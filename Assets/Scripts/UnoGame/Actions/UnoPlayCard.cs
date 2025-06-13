using System.Threading.Tasks;
using UnityEngine;
public class UnoPlayCard : UnoAction
{
    private UnoType _cardType;
    public UnoCard Card;
    public int CardIndex { get; }
    private readonly UnoColor _changeTo;

    public UnoPlayCard(UnoCard card, int handIndex)
    {
        Card = card;
        _cardType = card.Type;
        CardIndex = handIndex;
    }

    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return false;
        
        UnoPlayerStatus playerSelf = unoGs.PlayersStatus[unoGs.GetPlayerTurnIndex()];
        Card cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);
        cardPlayed.VisualCard.ChangeUnoParent(unoGs.DiscardGo.transform, false, unoGs.TopCard.VisualCard.transform.localPosition - new Vector3(0, 0.002f, 0));
        unoGs.TopCard = (UnoCard)cardPlayed;

        Debug.Log(CardIndex);
        Debug.Log("Tipo: " + Card.Type + "| Color: " + Card.Color);
        Debug.Log("Tipo: " + ((UnoCard)cardPlayed).Type + "| Color: " +((UnoCard)cardPlayed).Color);

        switch (_cardType)
        {
            case UnoType.Reverse:
                await unoGs.ShowReverseObject();
                unoGs.IsReversed = !unoGs.IsReversed;
                if (unoGs.PlayersStatus.Count <= 2) 
                    unoGs.BlockNextTurn = true;
                break;
            case UnoType.Block:
                await unoGs.ShowBlockObject();
                unoGs.BlockNextTurn = true;
                break;
            case UnoType.Draw2:
                unoGs.QuantityToDraw += 2;
                break;
            case UnoType.Draw4:
                unoGs.QuantityToDraw += 4;
                break;
        }
        
        return true;
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return false;
        
        UnoPlayerStatus playerSelf = unoObs.PlayersStatus[unoObs.GetPlayerTurnIndex()];
        
        Card cardPlayed = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref playerSelf.Hand, CardIndex);
        
        unoObs.TopCard = (UnoCard)cardPlayed;

        switch (_cardType)
        {
            case UnoType.Reverse:
                unoObs.IsReversed = !unoObs.IsReversed;
                if (unoObs.PlayersStatus.Count <= 2) 
                    unoObs.BlockNextTurn = true;
                break;
            case UnoType.Block:
                unoObs.BlockNextTurn = true;
                break;
            case UnoType.Draw2:
                unoObs.QuantityToDraw += 2;
                break;
            case UnoType.Draw4:
                unoObs.QuantityToDraw += 4;
                break;
        }
        
        return true;
    }
}
