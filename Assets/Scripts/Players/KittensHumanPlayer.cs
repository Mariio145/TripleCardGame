using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class KittensHumanPlayer : Player
{
    private bool _playCard, _drawCard;
    public VisualKittensAction kittensVisualAction;
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        VisualKittensCard[] listCards = handSlot.GetComponentsInChildren<VisualKittensCard>();
        
        KittensObservation kittensOb = observable as KittensObservation;
        
        foreach (VisualKittensCard card in listCards)
        {
            card.selected = false;
        }
        
        while (true)
        {
            _playCard = false;
            _drawCard = false;

            if (kittensOb!.ActionIsPlaying)
            {
                int index = listCards.Select(visualKittensCard => visualKittensCard.MemoryCard as KittensCard).TakeWhile(card => card!.Type != KittensType.Nope).Count();
                
                /*foreach (VisualKittensCard kittensCard in listCards)
                {
                    KittensCard card = visualKittensCard.MemoryCard as KittensCard;

                    if (card!.Type == KittensType.Nope)
                    {
                        break;
                    }

                    index++;
                }*/

                return new KittensNope(index);
            }

            if (kittensOb.IsDoingFavor != -1)
            {
                //Elige carta momento
                Debug.Log("Seleccione carta a dar: ");
                return new KittensGiveCard(kittensOb.CurrentPlayerTurn, kittensOb.PlayerIndexPerspective,
                    kittensOb.IsDoingFavor, 1); //await kittensVisualAction.SelectCard(listCards));
                //TODO
            }
            
            if (observable.GetActions().All(action => action.GetType() == typeof(KittensAction))) return new KittensAction();

            while (!_playCard && !_drawCard && thinkingTime > 0)
            {
                //await Task.Yield();
            }
            
            if (!_playCard && !_drawCard) return new KittensAction();

            int cardSlot = 0;

            if (_drawCard)
                return new KittensNothing();
            
            //Seleccionar la accion dependiendo de la carta seleccionada
            KittensCard selectedCard = null;

            if (listCards.Count(card => card.selected) == 2)
            {
                List<VisualKittensCard> cards = new();
                int cardIndex1 = -1;
                int cardIndex2 = -1;
                int index = 0;

                foreach (VisualKittensCard visualKittensCard in listCards)
                {
                    if (visualKittensCard.selected)
                    {
                        cards.Add(visualKittensCard);
                        if (cardIndex1 == -1) cardIndex1 = index;
                        else
                        {
                            cardIndex2 = index;
                            break;
                        }
                    }

                    index++;
                }

                if (cards[0].MemoryCard is KittensCard && cards[1].MemoryCard is KittensCard)
                {
                    selectedCard = cards[0].MemoryCard as KittensCard;
                    KittensCard selectedCard2 = cards[1].MemoryCard as KittensCard;

                    if (selectedCard!.Type == selectedCard2!.Type)
                    {
                        //TODO
                        int playerTarget = 0;//await kittensVisualAction.SelectPlayerTarget(kittensOb);
                        
                        //Selecciona una carta aleatoria
                        int stolenCard = Random.Next(0, kittensOb.PlayersStatus[playerTarget].Hand.Count);
                        return new KittensPair(cardIndex1, cardIndex2, playerTarget, stolenCard);
                    }
                }

            }

            foreach (VisualKittensCard card in listCards)
            {
                if (card.selected)
                {
                    selectedCard = (KittensCard) card.MemoryCard;
                    break;
                }
                cardSlot++;
            }
            
            if (selectedCard == null) continue; // No ha seleccionado ninguna carta
            //TODO
            if (observable.IsCardPlayable(selectedCard)) return null;//await GetCardAction((KittensObservation)observable, selectedCard, cardSlot);
            Debug.Log("No se puede jugar la carta");

        }
    }
    
    private async Task<IAction> GetCardAction(KittensObservation observable, KittensCard card, int handIndex)
    {
        switch (card.Type)
        {
            case KittensType.Attack:
                return new KittensAttack(handIndex);
            case KittensType.Skip:
                return new KittensSkip(handIndex);
            case KittensType.Favor:
                int playerTargetIndex = await kittensVisualAction.SelectPlayerTarget(observable);
                return new KittensFavor(playerTargetIndex, handIndex);
            case KittensType.Shuffle:
                return new KittensShuffle(handIndex);
            case KittensType.SeeTheFuture:
                return new KittensSeeTheFuture(handIndex);
        }

        return null;
    }

    public void PlayCard()
    {
        _playCard = true;
    }

    public void DrawCard()
    {
        _drawCard = true;
    }
}
