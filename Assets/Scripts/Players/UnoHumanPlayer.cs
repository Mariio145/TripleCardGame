using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UnoHumanPlayer : Player
{
    private bool _playCard, _drawCard;
    internal UnoColor _color;
    public VisualUnoAction unoVisualAction;
    
    private static SynchronizationContext _mainThreadContext;
    
    void Awake()
    {
        _mainThreadContext = SynchronizationContext.Current;
    }
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        VisualUnoCard[] listCards = null;
        
        _mainThreadContext.Send(_ =>
        {
            listCards = handSlot.GetComponentsInChildren<VisualUnoCard>();
            foreach (VisualUnoCard card in listCards)
            {
                card.selected = false;
                card.SetOutline(false);
            }
        }, null);

        if (observable is UnoObservation unoObs && unoObs.TopCard.Color == UnoColor.Wild)
        {
            _color = UnoColor.Wild;
            _ = unoVisualAction.SelectColor(this);
            while (_color == UnoColor.Wild)
            { }
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
            
            foreach (VisualUnoCard card in listCards)
                _mainThreadContext.Send(_ => { card.ActivateCollider(); }, null);
            
            if (observable.GetActions().All(action => action.GetType() == typeof(UnoAction))) return new UnoAction();

            while (!_playCard && !_drawCard && thinkingTime > 0)
            { }
            
            foreach (VisualUnoCard card in listCards)
                _mainThreadContext.Send(_ => { card.DeactivateCollider(); }, null);
            
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
                    _mainThreadContext.Send(_ =>
                    {
                        card.SetOutline(false);
                    }, null);
                    break;
                }
                cardSlot++;
            }

            if (!observable.IsCardPlayable(selectedCard)) 
            {
                _mainThreadContext.Send(_ =>
                {
                    GameManager.EndText.text = "La carta seleccionada \nno puede ser jugada";
                    GameManager.EndTextShadow.text = "La carta seleccionada \nno puede ser jugada";
                }, null);
                        
                float timer = 0;
                while (timer < 3f)
                {
                    _mainThreadContext.Send(_ =>
                    {
                        timer += Time.deltaTime;
                    }, null);
                }
                        
                _mainThreadContext.Send(_ =>
                {
                    GameManager.EndText.text = "";
                    GameManager.EndTextShadow.text = "";
                }, null);
                
                continue;
            }
            
            _mainThreadContext.Send(_ => { ((VisualUnoCard)selectedCard!.VisualCard).SetOutline(false); }, null);

            return new UnoPlayCard(selectedCard, cardSlot);
        }
    }

    public void PlayCard()
    {
        int cardsSelected = handSlot.GetComponentsInChildren<VisualUnoCard>().Count(card => card.selected);

        if (cardsSelected == 1) _playCard = true;
    }

    public void DrawCard()
    {
        _drawCard = true;
    }
    
    public void SetColor(int numColor)
    {
        Debug.Log("Holaaaa");
        /*  0 == Red,
            1 == Blue,
            2 == Yellow,
            3 == Green,
            4 == Wild   */
        _color = (UnoColor)numColor;
    }
}
