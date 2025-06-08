using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnoHumanPlayer : Player
{
    private bool _playCard, _drawCard;
    internal UnoColor Color;
    public VisualUnoAction unoVisualAction;
    private float _timer, _time;
    private bool _cancelAnim;
    
    private static SynchronizationContext _mainThreadContext;
    
    void Awake()
    {
        _mainThreadContext = SynchronizationContext.Current;
    }
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        _cancelAnim = false;
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
            Color = UnoColor.Wild;
            Task.Run(StartTimer);
            _ = unoVisualAction.SelectColor(this);

            while (Color == UnoColor.Wild)
            {
                if (_cancelAnim)
                {
                    Color = UnoColor.Red;
                    break;
                }
            }
            return new UnoChangeColor(Color);
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

            
            if (observable.GetActions().All(action => action.GetType() == typeof(UnoAction)))
                return null;
            
            Task.Run(StartTimer);

            while (!_playCard && !_drawCard)
            {                 
                if (_cancelAnim)
                    break;
            }
            
            foreach (VisualUnoCard card in listCards)
                _mainThreadContext.Send(_ =>
                {
                    card.DeactivateCollider(); 
                    card.SetOutline(false);
                }, null);
            
            if (_cancelAnim)
            {
                _playCard = false;
                _drawCard = false;
            }
            
            if (!_playCard && !_drawCard) return null;

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
                Task.Run(() => ShowText("La carta seleccionada \nno puede ser jugada"));
                continue;
            }
            
            _mainThreadContext.Send(_ => { ((VisualUnoCard)selectedCard!.VisualCard).SetOutline(false); }, null);

            return new UnoPlayCard(selectedCard, cardSlot);
        }
    }
    
    private void ShowText(string text)
    {
        _mainThreadContext.Send(_ =>
        {
            GameManager.EndText.text = text;
            GameManager.EndTextShadow.text = text;
        }, null);
                        
        
        while (_timer < 3f) 
        {
            _mainThreadContext.Send(_ =>
            {
                _mainThreadContext.Send(_ => { _timer += Time.deltaTime; }, null);
            }, null);
        }
                        
        _mainThreadContext.Send(_ =>
        {
            GameManager.EndText.text = "";
            GameManager.EndTextShadow.text = "";
        }, null);
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
    
    private Task StartTimer()
    {
        _cancelAnim = false;
        
        while (GameManager.TimeRemaining > 0)
        {

        }
        _cancelAnim = true;
        
        return Task.CompletedTask;
    }
    
    public void SetColor(int numColor)
    {
        /*  0 == Red,
            1 == Blue,
            2 == Yellow,
            3 == Green,
            4 == Wild   */
        Color = (UnoColor)numColor;
    }
}
