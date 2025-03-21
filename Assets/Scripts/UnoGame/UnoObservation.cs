using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class UnoObservation : IObservation
{
    private Deck<UnoCard> _mixedDrawDeck;
    private Deck<UnoCard> _discardDeck;
    public bool blockNextTurn { get; set; }
    public int quantityToDraw { get; set; }
    public bool IsReversed = false;
    
    // Contiene los datos del player, tanto de su mano como de su cuerpo
    public readonly List<UnoPlayerStatus> PlayersStatus;
    private int _currentPlayerTurn;
    public int playerIndexPerspective {get;}

    private UnoCard _topCard; 
    public UnoCard TopCard
    {
        get => _topCard;
        set
        {
            if (_topCard != null)
            {
                _discardDeck.Add(_topCard);
            }
            _topCard = value;
        }
    }

    public UnoObservation(Deck<UnoCard> mixedDrawDeck, Deck<UnoCard> discardDeck, List<UnoPlayerStatus> playersStatus, int currentPlayerTurn, UnoCard topCard, int playerPerspective)
    {
        _mixedDrawDeck = mixedDrawDeck;
        _discardDeck = discardDeck;
        PlayersStatus = playersStatus;
        _currentPlayerTurn = currentPlayerTurn;
        TopCard = topCard;
        playerIndexPerspective = playerPerspective;
    }

    public IObservation Clone()
    {
        List<UnoPlayerStatus> playerStatus = PlayersStatus.Select(player => player.Clone()).ToList();
        Deck<UnoCard> mixedDrawDeck = new(_mixedDrawDeck); 
        Deck<UnoCard> discardDeck = new(_discardDeck);
        UnoCard topCard = new(TopCard.Type, TopCard.Color, TopCard.Number);

        return new UnoObservation(mixedDrawDeck, discardDeck, playerStatus, _currentPlayerTurn, topCard, playerIndexPerspective);
    }
    
    public List<IAction> GetActions()
    {
        List<IAction> actions = new();

        if (TopCard.Color == UnoColor.Wild)
        {
            actions.Add(new UnoChangeColor(UnoColor.Red));
            actions.Add(new UnoChangeColor(UnoColor.Blue));
            actions.Add(new UnoChangeColor(UnoColor.Green));
            actions.Add(new UnoChangeColor(UnoColor.Yellow));
            return actions;
        }

        /*int index = 0;

        // Obtener las cartas jugables de la mano del jugador actual
        UnoPlayerStatus currentPlayer = PlayersStatus[_currentPlayerTurn];
        foreach (UnoCard card in currentPlayer.Hand.Cast<UnoCard>())
        {
            if (IsCardPlayable(card))
                actions.Add(new UnoPlayCard(card, index));

            index++;
        }*/
        // Siempre se puede robar una carta
        actions.Add(new UnoDrawCard());
        
        return actions;
    }

    public int GetPlayerTurnIndex()
    {
        return _currentPlayerTurn;
    }

    public bool IsTerminal()
    {
        // El juego termina cuando un jugador se queda sin cartas
        return PlayersStatus.Any(player => player.Hand.Count == 0);
    }
    
    public IObservation GetCloneRandomized()
    {
        Deck<UnoCard> drawDeck = new(_mixedDrawDeck);

        foreach (UnoPlayerStatus player in PlayersStatus)
        {
            if (player == PlayersStatus[playerIndexPerspective]) continue;

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
            if (player == PlayersStatus[playerIndexPerspective])
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

        return new UnoObservation(drawDeck, discardDeck, newPlayersStatus, _currentPlayerTurn, TopCard, playerIndexPerspective);
    }

    public bool IsCardPlayable(Card card)
    {
        if (card is not UnoCard unoCard) return false;
        
        /* De momento no implementado
        // Si hay que robar cartas, solo se pueden jugar cartas de robar
        if (QuantityToDraw > 0)
        {
            return unoCard.Type == UnoType.Draw2 || unoCard.Type == UnoType.Draw4;
        }*/

        // Las cartas de cambio de color siempre se pueden jugar
        if (unoCard.Color == UnoColor.Wild)
            return true;

        // Para el resto de cartas, debe coincidir el color o el tipo/número
        return unoCard.Color == TopCard.Color || 
               (unoCard.Type == UnoType.Number && TopCard.Type == UnoType.Number && unoCard.Number == TopCard.Number) ||
               (unoCard.Type != UnoType.Number && unoCard.Type == TopCard.Type);
    }

    public void ChangeTurnIndex()
    {
        int direction = IsReversed ? -1 : 1;
        _currentPlayerTurn = (_currentPlayerTurn + direction + PlayersStatus.Count) % PlayersStatus.Count;
    }
    
    public UnoCard DrawCardFromMixedDrawDeck()
    {
        UnoCard card = _mixedDrawDeck.DrawCard();
        if (card is null) Debug.LogError("Draw card failed observation");

        if (_mixedDrawDeck.RemainingCards() > 0) return card;
        
        _mixedDrawDeck = new Deck<UnoCard>(_discardDeck);
        _discardDeck = new Deck<UnoCard>();
        return card;
    }
}
