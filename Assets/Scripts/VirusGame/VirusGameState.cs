using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class VirusGameState : IGameState
{
    private Deck<VirusCard> _drawDeck;
    public Deck<VirusCard> DiscardDeck;
    private readonly GameObject _deckGo;
    public GameObject discardGo { get; }
    public List<VirusPlayerStatus> PlayersStatus;
    private int _currentPlayerTurn;

    public VirusGameState(GameObject deck, GameObject discard)
    {
        PlayersStatus = new List<VirusPlayerStatus>();
        _deckGo = deck;
        discardGo = discard;
    }
    public bool IsTerminal() => PlayersStatus.Any(playerInfo => playerInfo.HasWon());
    
    public void ChangeTurnIndex()
    {
        _currentPlayerTurn = (_currentPlayerTurn + 1) % PlayersStatus.Count;
    }

    public int GetPlayerTurnIndex() => _currentPlayerTurn;
    public Player GetPlayer() => PlayersStatus[_currentPlayerTurn].Player;
    public void Reset(List<Player> players)
    {
        _drawDeck = GetNewDeck();
        _drawDeck.ShuffleDeck();
        DiscardDeck = new Deck<VirusCard>();
        PlayersStatus = new List<VirusPlayerStatus>();
        int index = 0;
        foreach (Player player in players)
        {
            PlayersStatus.Add(new VirusPlayerStatus(index, player.handSlot, player, true));
            index++;
        }
        
        _currentPlayerTurn = 0;
        DrawStartCards();
    }

    private void DrawStartCards()
    {
        foreach (VirusPlayerStatus player in PlayersStatus)
            for (int j = 0; j < VirusGameParameters.NCardsPlayerHand; j++)
                player.Hand.Enqueue(DrawCardFromDrawDeck(player.HandGObject).Result);
    }
    
    public async Task<VirusCard> DrawCardFromDrawDeck(GameObject hand)
    {
        if (_drawDeck.RemainingCards() <= 0)
        {
            _drawDeck = new Deck<VirusCard>(DiscardDeck);
            _drawDeck.ShuffleDeck();
            await RefreshDrawCardAnim();
            
            // for (int i = 0; i < _drawDeck.RemainingCards(); i++)
            // {
            //     _discardDeck.DrawCard();
            //     //card.VisualCard.ChangeParent(_deckGo.transform, false);
            // }
        
            DiscardDeck = new Deck<VirusCard>();
        }

        VirusCard card = _drawDeck.DrawCard();
        
        card.VisualCard.ChangeParent(hand.transform, PlayersStatus[0].HandGObject == hand);

        return card;
    }
    
    public async Task DiscardCard(Card card)
    {
        await DiscardCard((VirusCard)card);
    }

    private async Task DiscardCard(VirusCard card)
    {
        card.VisualCard.ChangeParent(discardGo.transform, false);
        DiscardDeck.Add(card);
        await card.VisualCard.SetPosition(new Vector3(Random.Range(-0.1f, 0.1f), 0.005f * DiscardDeck.RemainingCards(), Random.Range(-0.1f, 0.1f)), true);
    }
    

    public void UpdateHands()
    {
        foreach (VirusPlayerStatus playerStatus in PlayersStatus)
        {
            playerStatus.HandGObject.GetComponent<VisualHand>().UpdateHandPosition();
        }
    }
    
    private Deck<VirusCard> GetNewDeck()
    {
        List<VirusCard> newDeck = new ();

        foreach (VirusColor color in Enum.GetValues(typeof(VirusColor)))
        {
            if (color is VirusColor.Rainbow or VirusColor.None) continue;
            
            for (int i = 0; i < VirusGameParameters.NOrganCards; i++)
            {
                newDeck.Add(new VirusCard(color, VirusType.Organ, TreatmentType.None, _deckGo));
            }
            
            for (int i = 0; i < VirusGameParameters.NVirusCards; i++)
            {
                newDeck.Add(new VirusCard(color, VirusType.Virus, TreatmentType.None, _deckGo));
            }
            
            for (int i = 0; i < VirusGameParameters.NMedicineCards; i++)
            {
                newDeck.Add(new VirusCard(color, VirusType.Medicine, TreatmentType.None, _deckGo));
            }
        }

        for (int i = 0; i < VirusGameParameters.NWildOrganCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.Rainbow, VirusType.Organ, TreatmentType.None, _deckGo));
        }

        for (int i = 0; i < VirusGameParameters.NWildVirusCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.Rainbow, VirusType.Virus, TreatmentType.None, _deckGo));
        }

        for (int i = 0; i < VirusGameParameters.NWildMedicineCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.Rainbow, VirusType.Medicine, TreatmentType.None, _deckGo));
        }

        for (int i = 0; i < VirusGameParameters.NTreatmentSpreadingCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.None, VirusType.Treatment, TreatmentType.Spreading, _deckGo));
        }

        for (int i = 0; i < VirusGameParameters.NTreatmentTransplantCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.None, VirusType.Treatment, TreatmentType.Transplant, _deckGo));
        }
        
        for (int i = 0; i < VirusGameParameters.NTreatmentLatexGloveCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.None, VirusType.Treatment, TreatmentType.LatexGlove, _deckGo));
        }
        
        for (int i = 0; i < VirusGameParameters.NTreatmentMedicalErrorCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.None, VirusType.Treatment, TreatmentType.MedicalError, _deckGo));
        }
        
        for (int i = 0; i < VirusGameParameters.NTreatmentOrganThiefCards; i++)
        {
            newDeck.Add(new VirusCard(VirusColor.None, VirusType.Treatment, TreatmentType.OrganThief, _deckGo));
        }

        foreach (VirusCard card in newDeck.Cast<VirusCard>())
        {
            card.VisualCard.ChangeParent(_deckGo.transform, false);
        }

        return new Deck<VirusCard>(newDeck);
    }
    
    public IObservation GetObservationFromPlayer(int index)
    {
        Deck<VirusCard> drawDeck = new (_drawDeck);
        foreach (VirusPlayerStatus player in PlayersStatus)
        {
            if (player == PlayersStatus[index]) continue;

            foreach (VirusCard card in player.Hand.Cast<VirusCard>())
            {
                drawDeck.Add(card);
            }
        }
        
        drawDeck.ShuffleDeck();
        
        List<VirusPlayerStatus> playersStatus = new();
        
        foreach (VirusPlayerStatus player in PlayersStatus)
        {
            VirusPlayerStatus playerCopy = player.Clone();
            playersStatus.Add(playerCopy);
            if (player == PlayersStatus[index]) continue;
            if (playerCopy.Hand.All(card => card is null)) continue;
            playerCopy.Hand.Clear();
            for (int i = 0; i < VirusGameParameters.NCardsPlayerHand; i++)
            {
                // No hay que preocuparse por que la baraja de robo se vacíe, ya que antes se llena con las cartas de la mano de los otros jugadores
                playerCopy.Hand.Enqueue(drawDeck.DrawCard());
            }
        }
        
        drawDeck.ShuffleDeck();
        Deck<VirusCard> discardDeck = new (DiscardDeck);

        return new VirusObservation(drawDeck, discardDeck, playersStatus, _currentPlayerTurn, index);
    }
    
    /*
     *
     * ANIMATIONS
     * 
     */
    
    private async Task RefreshDrawCardAnim()
    {
        const float animDuration = 0.5f;
        
        const int gridColumns = 10;
        const int gridRows = 10;
        
        List<Task> tasks = new();
        
        List<Transform> cards = discardGo.GetComponentsInChildren<Transform>().Where(card => card != discardGo.transform).ToList();
        Renderer renderer = cards[0].GetComponent<Renderer>();
        
        float cardLenght = renderer.bounds.size.x;
        const float cardHeight = 0.005f;
        float cardWidth = renderer.bounds.size.z;

        int index = 0;
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                if (index >= cards.Count) break;
                Vector3 targetPos = discardGo.transform.position + new Vector3(j * cardLenght, 0.25f, i * cardWidth);
                tasks.Add(cards[index].DOMove(targetPos, animDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion());
                index++;
            }
        }
        
        await Task.WhenAll(tasks);
        GameObject temp = new()
        {
            transform =
            {
                position = discardGo.transform.position + new Vector3(-renderer.bounds.size.x / 2, 0.15f, 0),
            }
        };
        
        cards.Clear();
        cards.AddRange(_drawDeck.GetList().Select(unoCard => unoCard.VisualCard.transform));
        //cards = cards.OrderBy(_ => new int()).ToList();
        
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 targetPos = temp.transform.position + new Vector3(0, i * cardHeight, 0);
            cards[i].transform.parent = temp.transform;
            tasks.Add(cards[i].DOMove(targetPos, animDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion());
        }
        await Task.WhenAll(tasks);
        
        await temp.transform.DOMove(_deckGo.transform.position, animDuration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

        foreach (Transform cardTransform in cards)
        {
            cardTransform.parent = _deckGo.transform;
        }
    }
}