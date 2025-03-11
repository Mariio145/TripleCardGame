using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KittensObservation : IObservation
{
    public readonly Deck<KittensCard> MixedDrawDeck;
    public readonly Deck<KittensCard> DiscardDeck;
    public readonly List<KittensPlayerStatus> PlayersStatus;
    public int CurrentPlayerTurn {get; private set;}

    public bool DrawAtEnd;
    public int TurnsToPlay;
    public int NextTurnsToPlay;
    public bool TurnEnded;
    public bool ActionIsPlaying;
    public int IsDoingFavor;
    public int PlayerIndexPerspective {get; private set;}

    public List<KittensCard>[] TopVisibleCards;

    public KittensObservation(Deck<KittensCard> mixedDrawDeck, Deck<KittensCard> discardDeck, List<KittensPlayerStatus> playersStatus, int currentPlayerTurn, bool drawAtEnd, int turnsToPlay, int nextTurnsToPlay, bool turnEnded, bool actionIsPlaying, int isDoingFavor, int playerObservationIndexPerspective, List<KittensCard>[] topVisibleCards)
    {
        MixedDrawDeck = mixedDrawDeck;
        DiscardDeck = discardDeck;
        PlayersStatus = playersStatus;
        CurrentPlayerTurn = currentPlayerTurn;

        /*foreach (KittensPlayerStatus variable in PlayersStatus)
            Debug.LogWarning("Cartitas: " + variable.Hand.Count);*/
        
        DrawAtEnd = drawAtEnd;
        TurnsToPlay = turnsToPlay;
        NextTurnsToPlay = nextTurnsToPlay;
        TurnEnded = turnEnded;
        ActionIsPlaying = actionIsPlaying;
        IsDoingFavor = isDoingFavor;
        PlayerIndexPerspective = playerObservationIndexPerspective;
        TopVisibleCards = topVisibleCards;
    }

    

    public IObservation Clone()
    {
        List<KittensPlayerStatus> playerStatus = new();
        foreach (KittensPlayerStatus newPlayer in PlayersStatus)
        {
            playerStatus.Add(newPlayer.Clone());
        }
        
        Deck<KittensCard> mixedDrawDeck = new(MixedDrawDeck); 
        Deck<KittensCard> discardDeck = new(DiscardDeck);
        List<KittensCard>[] topVisibleCardsCopy = new List<KittensCard>[playerStatus.Count];
        for (int i = 0; i < playerStatus.Count; i++)
        {
            topVisibleCardsCopy[i] = new List<KittensCard>(TopVisibleCards[i]);
        }

        return new KittensObservation(mixedDrawDeck, discardDeck, playerStatus, CurrentPlayerTurn, DrawAtEnd, TurnsToPlay, NextTurnsToPlay, TurnEnded,
            ActionIsPlaying, IsDoingFavor, PlayerIndexPerspective, topVisibleCardsCopy);
    }

    public List<IAction> GetActions()
    {
        List<IAction> actions = new();
        
        if (IsDoingFavor != -1)
        {
            Debug.LogError(PlayersStatus[PlayerIndexPerspective].Hand.Count);
            /*Debug.LogError(PlayerIndexPerspective);*/
            for (int i = 0; i < PlayersStatus[PlayerIndexPerspective].Hand.Count; i++) actions.Add(new KittensGiveCard(CurrentPlayerTurn, PlayerIndexPerspective, IsDoingFavor, i));
            return actions;
        }
        
        actions.Add(new KittensNothing());

        int cardIndex;
        if (ActionIsPlaying)
        {
            cardIndex = 0;
            foreach (KittensCard cardFromHand in PlayersStatus[PlayerIndexPerspective].Hand.Cast<KittensCard>())
            {
                if (cardFromHand.Type == KittensType.Nope) actions.Add(new KittensNope(cardIndex));
                cardIndex++;
            }
            return actions;
        }



        cardIndex = 0;
        
        foreach (KittensCard cardFromHandCard in PlayersStatus[PlayerIndexPerspective].Hand.Cast<KittensCard>())
        {
            switch (cardFromHandCard.Type)
            {
                case KittensType.Attack:
                    actions.Add(new KittensAttack(cardIndex));
                    break;
                case KittensType.Favor:
                    for (int i = 0; i < PlayersStatus.Count; i++)
                        if (i != PlayerIndexPerspective)
                            actions.Add(new KittensFavor(i, cardIndex));
                    break;
                case KittensType.Shuffle:
                    actions.Add(new KittensShuffle(cardIndex));
                    break;
                case KittensType.SeeTheFuture:
                    actions.Add(new KittensSeeTheFuture(cardIndex));
                    break;
                case KittensType.Skip:
                    actions.Add(new KittensSkip(cardIndex));
                    break;
            }

            cardIndex++;
        }
        
        
        // Comprobar pares de cartas de gato
        List<KittensCard> handCards = PlayersStatus[PlayerIndexPerspective].Hand.Cast<KittensCard>().ToList();
        
        for (int i = 0; i < handCards.Count - 1; i++)
        {
            for (int j = i + 1; j < handCards.Count; j++) 
            {
                if (handCards[i].Type == handCards[j].Type && 
                    (handCards[i].Type == KittensType.Cat1 ||
                     handCards[i].Type == KittensType.Cat2 ||
                     handCards[i].Type == KittensType.Cat3 ||
                     handCards[i].Type == KittensType.Cat4 ||
                     handCards[i].Type == KittensType.Cat5))
                {
                    for (int targetPlayer = 0; targetPlayer < PlayersStatus.Count; targetPlayer++)
                    {
                        if (targetPlayer == CurrentPlayerTurn || PlayersStatus[targetPlayer].Hand.Count <= 0) continue;
                        
                        for (cardIndex = 0; cardIndex < PlayersStatus[targetPlayer].Hand.Count; cardIndex++)
                        {
                            actions.Add(new KittensPair(i, j, targetPlayer, cardIndex));
                        }
                    }
                }
            }
        }
        
        return actions;
    }
    
    public Card DrawCardFromMixedDrawDeck()
    {
        Card card = MixedDrawDeck.DrawCard();

        foreach (List<KittensCard> visibleCard in TopVisibleCards)
            if (visibleCard.Count > 0) visibleCard.RemoveAt(0);

        return card;
    }

    public int GetPlayerTurnIndex()
    {
        return CurrentPlayerTurn;
    }

    public bool IsTerminal()
    {
        return PlayersStatus.Count(player => player.IsAlive()) == 1;
    }

    public bool IsCardPlayable(Card card)
    {
        return ((KittensCard)card).Type is not (KittensType.Defuse or KittensType.Nope or KittensType.Cat1 or KittensType.Cat2
            or KittensType.Cat3 or KittensType.Cat4 or KittensType.Cat5); // Esas cartas no se pueden jugar por si solas
    }

    public void ChangeTurnIndex()
    {
        CurrentPlayerTurn = (CurrentPlayerTurn + 1) % PlayersStatus.Count;
    }
    
    public void KillPlayer(KittensPlayerStatus player)
    {
        player.Alive = false;
        PlayersStatus.Remove(player);
        CurrentPlayerTurn--;
    }
    
    public void DiscardCard(Card card)
    {
        DiscardDeck.Add((KittensCard)card);
    }
    
    public void DiscardCard(KittensCard card)
    {
        DiscardDeck.Add(card);
    }

    public IObservation CopyFromPlayer(int player)
    {
        IObservation clone = Clone();
        ((KittensObservation)clone).PlayerIndexPerspective = player;
        return clone;
    }
}
