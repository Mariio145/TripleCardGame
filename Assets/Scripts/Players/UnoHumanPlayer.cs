using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class UnoHumanPlayer : Player
{
    private bool _playCard, _drawCard;
    private UnoColor _color;
    public VisualUnoAction unoVisualAction;
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        VisualUnoCard[] listCards = handSlot.GetComponentsInChildren<VisualUnoCard>();

        if (observable is UnoObservation unoObs && unoObs.TopCard.Color == UnoColor.Wild)
        {
            _ = unoVisualAction.SelectColor();
            return new UnoChangeColor(_color);
        }
        
        foreach (VisualUnoCard card in listCards)
        {
            card.selected = false;
        }
        
        while (true)
        {
            _playCard = false;
            _drawCard = false;
            
            if (observable.GetActions().All(action => action.GetType() == typeof(UnoAction))) return new UnoAction();

            while (!_playCard && !_drawCard && thinkingTime > 0)
            {
                //TODO
                //await Task.Yield();
            }
            
            if (!_playCard && !_drawCard) return new UnoAction();

            int cardSlot = 0;

            if (_drawCard)
                return new UnoDrawCard();
            
            //Seleccionar la accion dependiendo de la carta seleccionada
            UnoCard selectedCard = null;

            foreach (VisualUnoCard card in listCards)
            {
                if (card.selected)
                {
                    selectedCard = (UnoCard)card.MemoryCard;
                    break;
                }
                cardSlot++;
            }

            if (!observable.IsCardPlayable(selectedCard)) 
            {
                Debug.Log("No se puede jugar la carta");
                continue;
            }

            return new UnoPlayCard(selectedCard, cardSlot);
        }
    }

    public void PlayCard()
    {
        int cardsSelected = handSlot.GetComponentsInChildren<VisualUnoCard>().Count(card => card.selected);

        if (cardsSelected == 1) _playCard = true;
        Debug.Log(_playCard);
    }

    public void DrawCard()
    {
        _drawCard = true;
    }
}
