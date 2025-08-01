using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

public class UnoGameState : IGameState
{
    private Deck<UnoCard> _drawDeck;
    private readonly GameObject _deckGo;
    private Deck<UnoCard> _discardDeck;
    public GameObject DiscardGo { get; }
    public bool BlockNextTurn { get; set; }
    public int QuantityToDraw { get; set; }
    public bool IsReversed = false;
    public readonly List<UnoPlayerStatus> PlayersStatus;
    private int _currentPlayerTurn;
    private UnoCard _topCard;
    public UnoCard TopCard
    {
        get => _topCard;
        set
        {
            if (_topCard != null)
            {
                _discardDeck.Add(_topCard);
                if (value.Color == UnoColor.Wild || value.Color != _topCard.Color) ChangeTableColor(value.Color);
                if (_topCard.Number is -4 or -5) _topCard.ChangeColor(UnoColor.Wild);
            }
            else
            {
                ChangeTableColor(value.Color);
            }
            
            _topCard = value;
        }
    }

    public UnoGameState(GameObject deckGo, GameObject discardGo)
    {
        _drawDeck = new Deck<UnoCard>();
        _discardDeck = new Deck<UnoCard>();
        PlayersStatus = new List<UnoPlayerStatus>();
        _deckGo = deckGo;
        DiscardGo = discardGo;
    }
    
    public bool IsTerminal() => PlayersStatus.Any(playerInfo => playerInfo.Hand.Count == 0);

    public void ChangeTurnIndex()
    {
        int direction = IsReversed ? -1 : 1;
        _currentPlayerTurn = (_currentPlayerTurn + direction + PlayersStatus.Count) % PlayersStatus.Count;
    }

    public int GetPlayerTurnIndex() => _currentPlayerTurn;
    
    public Player GetPlayer() => PlayersStatus[_currentPlayerTurn].Player;
    public Player GetWinner() =>  PlayersStatus.Find(player => player.HasWon()).Player;
    
    public List<PlayerStatus> GetPlayerStatus() => new (PlayersStatus);

    public void Reset(List<Player> players)
    {
        _drawDeck = new Deck<UnoCard>();
        _discardDeck = new Deck<UnoCard>();
        _drawDeck = GetNewDeck();
        _drawDeck.ShuffleDeck();
        
        PlayersStatus.Clear();

        foreach (Player player in players)
        {
            PlayersStatus.Add(new UnoPlayerStatus(new Queue<Card>(), players[PlayersStatus.Count].handSlot, player));
        }
        
        _currentPlayerTurn = 0;
        DrawStartCards();
        UpdateHands();
        TopCard = DrawCardFromDrawDeck(DiscardGo).Result;

    }
    
    public bool IsCardPlayable(UnoCard card)
    {
        /* TODO:
         De momento no implementado
        // Si hay que robar cartas, solo se pueden jugar cartas de robar
        if (QuantityToDraw > 0)
        {
            return unoCard.Type == UnoType.Draw2 || unoCard.Type == UnoType.Draw4;
        }*/

        // Las cartas de cambio de color siempre se pueden jugar
        if (card.Color == UnoColor.Wild)
            return true;

        // Para el resto de cartas, debe coincidir el color o el tipo/número
        return card.Color == TopCard.Color || 
               (card.Type == UnoType.Number && TopCard.Type == UnoType.Number && card.Number == TopCard.Number) ||
               (card.Type != UnoType.Number && card.Type == TopCard.Type);
    }
    
    private void DrawStartCards()
    {
        foreach (UnoPlayerStatus player in PlayersStatus)
            for (int i = 0; i < UnoGameParameters.NStartCards; i++)
                player.Hand.Enqueue(DrawCardFromDrawDeck(player.HandGObject).Result);
    }
    
    public async Task<UnoCard> DrawCardFromDrawDeck(GameObject hand)
    {
        if (_drawDeck.RemainingCards() <= 0)
        {
            _drawDeck = new Deck<UnoCard>(_discardDeck);
            _drawDeck.ShuffleDeck();
            await RefreshDrawCardAnim();
            
            // for (int i = 0; i < _drawDeck.RemainingCards(); i++)
            // {
            //     _discardDeck.DrawCard();
            //     //card.VisualCard.ChangeParent(_deckGo.transform, false);
            // }
        
            _discardDeck = new Deck<UnoCard>();
        }

        UnoCard card = _drawDeck.DrawCard();
        
        SoundManager.Instance.PlaySfx("DrawCard");
        
        card.VisualCard.ChangeParent(hand.transform);
    
        return card;
    }

    private Deck<UnoCard> GetNewDeck()
    {
        List<UnoCard> newDeck = new ();
        foreach (UnoColor color in Enum.GetValues(typeof(UnoColor)))
        {
            if (color == UnoColor.Wild) continue;
            for (int i = 0; i < UnoGameParameters.NumberCards; i++)
            {
                newDeck.Add(new UnoCard(UnoType.Number, color, i, _deckGo.transform));
                if (i > 0) newDeck.Add(new UnoCard(UnoType.Number, color, i, _deckGo.transform)); //Solo hay una copia de la carta 0, mientras que de las demas hay dos
            }

            for (int i = 0; i < UnoGameParameters.NBlockCards; i++)
            {
                newDeck.Add(new UnoCard(UnoType.Block, color, -1, _deckGo.transform));
            }
            
            for (int i = 0; i < UnoGameParameters.NReverseCards; i++)
            {
                newDeck.Add(new UnoCard(UnoType.Reverse, color, -2, _deckGo.transform));
            }
            
            for (int i = 0; i < UnoGameParameters.NDraw2Cards; i++)
            {
                newDeck.Add(new UnoCard(UnoType.Draw2, color, -3, _deckGo.transform));
            }
        }
        
        for (int i = 0; i < UnoGameParameters.NWildCards; i++)
        {
            newDeck.Add(new UnoCard(UnoType.Change, UnoColor.Wild, -4, _deckGo.transform));
        }
        
        for (int i = 0; i < UnoGameParameters.NWildDrawCards; i++)
        {
            newDeck.Add(new UnoCard(UnoType.Draw4, UnoColor.Wild, -5, _deckGo.transform));
        }
        
        foreach (UnoCard card in newDeck)
        {
            card.VisualCard.ChangeParent(_deckGo.transform);
        }
        
        return new Deck<UnoCard>(newDeck);
    }
    
    public IObservation GetObservationFromPlayer(int index)
    {
        Deck<UnoCard> drawDeck = new(_drawDeck);

        foreach (UnoPlayerStatus player in PlayersStatus)
        {
            if (player == PlayersStatus[index]) continue;

            foreach (UnoCard card in player.Hand.Cast<UnoCard>())
            {
                drawDeck.Add(card);
            }
        }
        
        drawDeck.ShuffleDeck();
        
        List<UnoPlayerStatus> newPlayersStatus = new ();
        
        foreach (UnoPlayerStatus player in PlayersStatus)
        {
            UnoPlayerStatus playerCopy = player.Clone();
            newPlayersStatus.Add(playerCopy);
            if (player == PlayersStatus[index])
                continue;
            if (playerCopy.Hand.All(card => card is null))
                continue;
            
            playerCopy.Hand.Clear();
            for (int i = 0; i < player.Hand.Count; i++)
            {
                // No hay que preocuparse por que la baraja de robo se vacíe, ya que antes se llena con las cartas de la mano de los otros jugadores
                playerCopy.Hand.Enqueue(drawDeck.DrawCard());
            }
        }
        
        drawDeck.ShuffleDeck();
        Deck<UnoCard> discardDeck = new(_discardDeck);

        return new UnoObservation(drawDeck, discardDeck, newPlayersStatus, _currentPlayerTurn, TopCard, index);
    }

    public void UpdateHands()
    {
        foreach (UnoPlayerStatus player in PlayersStatus)
        {
            player.HandGObject.GetComponent<VisualHand>().UpdateHandPosition();
        }
    }
    
    /*
     *
     * ANIMATIONS
     * 
     */

    public async Task ShowReverseObject()
    {
        GameObject reverse = GameManager.Reverse;
        reverse.SetActive(true);
        await Task.Delay(250);
        if(IsReversed)
            await reverse.transform.DORotate(new Vector3(0, 180, 0), 0.5f).AsyncWaitForCompletion();
        else
            await reverse.transform.DORotate(new Vector3(0, 0, 180), 0.5f).AsyncWaitForCompletion();
        
        await Task.Delay(250);
        
        reverse.SetActive(false);
    }
    
    public async Task ShowBlockObject()
    {
        GameObject block = GameManager.Block;
        block.SetActive(true);
        await Task.Delay(250);

        await block.transform.DOScale(1.25f, 0.5f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        await block.transform.DOScale(1f, 0.5f ).SetEase(Ease.InQuad).AsyncWaitForCompletion();
        
        await Task.Delay(250);
        
        block.SetActive(false);
    }
    
    private async Task RefreshDrawCardAnim()
    {
        const float animDuration = 0.5f;
        
        const int gridColumns = 10;
        const int gridRows = 10;
        
        List<Task> tasks = new();
        
        List<Transform> cards = DiscardGo.GetComponentsInChildren<Transform>().Where(card => card != DiscardGo.transform && card != TopCard.VisualCard.transform).ToList();
        Renderer renderer = cards[0].GetComponent<Renderer>();
        
        float cardLenght = renderer.bounds.size.x;
        const float cardHeight = 0.0065f;
        float cardWidth = renderer.bounds.size.z;
        
        SoundManager.Instance.PlaySfx("Shuffle");

        int index = 0;
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                if (index >= cards.Count) break;
                Vector3 targetPos = DiscardGo.transform.position + new Vector3(j * cardLenght, 0.25f, i * cardWidth);
                tasks.Add(cards[index].DOMove(targetPos, animDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion());
                tasks.Add(cards[index].DOLocalRotate(new Vector3(0, 0, 180), animDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion());
                index++;
            }
        }
        
        await Task.WhenAll(tasks);
        GameObject temp = new()
        {
            transform =
            {
                position = DiscardGo.transform.position + new Vector3(-renderer.bounds.size.x / 2, 0.15f, 0),
            }
        };
        
        cards.Clear();
        cards.AddRange(_drawDeck.GetList().Select(unoCard => unoCard.VisualCard.transform));
        
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

        _topCard.VisualCard.transform.DOLocalMove(Vector3.zero, 0.5f);
    }
    
    private void ChangeTableColor(UnoColor valueColor)
    {
        Color color = default;
        Material tableMaterial = ResourcesLoader.Instance.tableMaterial;
        Material tokenMaterial = ResourcesLoader.Instance.tokenMaterial;
        
        Debug.Log(valueColor);
        switch (valueColor)
        {
            case UnoColor.Red:
                color = Color.red;
                break;
            case UnoColor.Blue:
                color = Color.blue;
                break;
            case UnoColor.Yellow:
                color = Color.yellow;
                break;
            case UnoColor.Green:
                color = Color.green;
                break;
            case UnoColor.Wild:
                color = Color.gray;
                break;
        }
        tableMaterial.color = color;
        tokenMaterial.color = color;
    }

    public void ChangeColor(UnoColor colorChange)
    {
        ChangeTableColor(colorChange);
        TopCard.ChangeColor(colorChange);
    }
}