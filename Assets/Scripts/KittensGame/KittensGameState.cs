using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class KittensGameState : IGameState
{
    public readonly GameObject DeckGo;
    private readonly GameObject _discardGo;
    public readonly List<KittensPlayerStatus> PlayersStatus = new();
    private int _currentPlayerTurn;
    public Deck<KittensCard> DrawDeck;
    private Deck<KittensCard> _discardDeck;
    public List<KittensCard>[] TopVisibleCards;

    public bool DrawAtEnd;
    public int TurnsToPlay;
    public int NextTurnsToPlay;
    public bool TurnEnded;
    public bool CanBlock;
    public int IsDoingFavor = -1;

    public KittensGameState(GameObject deckHolder, GameObject discardHolder)
    {
        DeckGo = deckHolder;
        _discardGo = discardHolder;
        DrawDeck = new Deck<KittensCard>();
        _discardDeck = new Deck<KittensCard>();
    }
    

    public bool IsTerminal() => PlayersStatus.Count(player => player.Alive) == 1;

    public void ChangeTurnIndex()
    {
        _currentPlayerTurn = (_currentPlayerTurn + 1) % PlayersStatus.Count;
    }
    
    public int GetPlayerTurnIndex() => _currentPlayerTurn;
    
    public Player GetPlayer() => PlayersStatus[_currentPlayerTurn].Player;

    public void KillPlayer(KittensPlayerStatus player)
    {
        player.Alive = false;
        PlayersStatus.Remove(player);
        _currentPlayerTurn--;
    }

    public void Reset(List<Player> players)
    {
        DrawAtEnd = true;
        
        // Inicializar mazos
        DrawDeck = GetNewDeck();
        DrawDeck.ShuffleDeck();
        _discardDeck = new Deck<KittensCard>();

        PlayersStatus.Clear();
        TopVisibleCards = new List<KittensCard>[players.Count];

        for (int index = 0; index < players.Count; index++)
            TopVisibleCards[index] = new List<KittensCard>();

        foreach (Player player in players)
            PlayersStatus.Add(new KittensPlayerStatus(new Queue<Card>(), player.handSlot, player));
        
        _currentPlayerTurn = 0;
        DrawStartCards();
        
        DrawAtEnd = true;
        TurnsToPlay = 1;
        NextTurnsToPlay = 1;
        TurnEnded = false;
        IsDoingFavor = -1;
    }
    
    private void DrawStartCards()
    {
        foreach (KittensPlayerStatus player in PlayersStatus)
        {
            for (int j = 0; j < KittensGameParameters.InitialHandSize; j++)
                player.Hand.Enqueue(DrawCardFromDrawDeck(player.HandGObject));
            
            player.Hand.Enqueue(new KittensCard(KittensType.Defuse, player.HandGObject.transform));
        }

        //Una vez repartidas las cartas iniciales, se añaden los ExplodingKittens y los defuses segun el numero de jugadores
        for (int i = PlayersStatus.Count == 2 ? 4 : PlayersStatus.Count; i < KittensGameParameters.DefuseCount; i++)
        {
            DrawDeck.Add(new KittensCard(KittensType.Defuse, DeckGo.transform));
        }
        
        for (int i = 0; i < PlayersStatus.Count - 1; i++)
        {
            DrawDeck.Add(new KittensCard(KittensType.Exploding, DeckGo.transform));
        }
        
        DrawDeck.ShuffleDeck();
    }
    
    public Card DrawCardFromDrawDeck(GameObject hand)
    {
        Card card = DrawDeck.DrawCard();

        foreach (List<KittensCard> visibleCard in TopVisibleCards)
            if (visibleCard is not null && visibleCard.Count > 0) visibleCard.RemoveAt(0);
        
        card.VisualCard.ChangeParent(hand.transform, PlayersStatus[0].HandGObject == hand);

        return card;
    }
    
    public void DiscardCard(Card card)
    {
        DiscardCard((KittensCard)card);
    }
    
    public void DiscardCard(KittensCard card)
    {
        _discardDeck.Add(card);
        card.VisualCard.ChangeParent(_discardGo.transform, false);
    }
    
    public async void UpdateHands()
    {
        await Task.Delay(1);
        foreach (KittensPlayerStatus playerStatus in PlayersStatus)
        {
            playerStatus.HandGObject.GetComponent<VisualHand>().UpdateHandPosition();
        }
    }

    private Deck<KittensCard> GetNewDeck()
    {
        List<KittensCard> newDeck = new ();
        
        for (int i = 0; i < KittensGameParameters.NopeCount; i++)
        {
            newDeck.Add(new KittensCard(KittensType.Nope, DeckGo.transform)); 
        }

        for (int i = 0; i < KittensGameParameters.AttackCount; i++)
        {
            newDeck.Add(new KittensCard(KittensType.Attack, DeckGo.transform));
        }
        
        for (int i = 0; i < KittensGameParameters.SkipCount; i++)
        {
            newDeck.Add(new KittensCard(KittensType.Skip, DeckGo.transform));
        }
        
        for (int i = 0; i < KittensGameParameters.FavorCount; i++)
        {
            newDeck.Add(new KittensCard(KittensType.Favor, DeckGo.transform));
        }
        
        for (int i = 0; i < KittensGameParameters.ShuffleCount; i++)
        {
            newDeck.Add(new KittensCard(KittensType.Shuffle, DeckGo.transform));
        }
        
        for (int i = 0; i < KittensGameParameters.SeeTheFutureCount; i++)
        {
            newDeck.Add(new KittensCard(KittensType.SeeTheFuture, DeckGo.transform));
        }
        for (int i = 0; i < KittensGameParameters.CatCardsPerType; i++)
        {
            newDeck.Add(new KittensCard(KittensType.Cat1, DeckGo.transform));
            newDeck.Add(new KittensCard(KittensType.Cat2, DeckGo.transform));
            newDeck.Add(new KittensCard(KittensType.Cat3, DeckGo.transform));
            newDeck.Add(new KittensCard(KittensType.Cat4, DeckGo.transform));
            newDeck.Add(new KittensCard(KittensType.Cat5, DeckGo.transform));
        }
        
        foreach (KittensCard card in newDeck)
        {
            card.VisualCard.ChangeParent(DeckGo.transform, false);
        }
        
        return new Deck<KittensCard>(newDeck);
    }

    public IObservation GetObservationFromPlayer(int index)
    {
        Deck<KittensCard> drawDeck = new(DrawDeck);

        foreach (KittensPlayerStatus player in PlayersStatus)
        {
            if (player == PlayersStatus[index] || !player.IsAlive()) continue;

            foreach (KittensCard card in player.Hand.Cast<KittensCard>())
            {
                drawDeck.Add(card);
            }
        }
        
        drawDeck.ShuffleDeck();
        
        List<KittensPlayerStatus> playerStatus = new ();
        
        foreach (KittensPlayerStatus player in PlayersStatus)
        {
            KittensPlayerStatus playerCopy = player.Clone();
            playerStatus.Add(playerCopy);

            if (player == PlayersStatus[index])
            {
                continue;
            }

            if (!player.IsAlive())
            {
                Debug.LogError("Muertote");
                continue;
            }

            if (playerCopy.Hand.Count == 0)
            {
                Debug.LogError("NuloNuloNulo");
                continue;
            }
            
            playerCopy.Hand.Clear();
            
            for (int i = 0; i < player.Hand.Count; i++)
                playerCopy.Hand.Enqueue(drawDeck.DrawCard());
        }
        
        drawDeck.ShuffleDeck();
        Deck<KittensCard> discardDeck = new(_discardDeck);
        
        List<KittensCard>[] topVisibleCardsCopy = new List<KittensCard>[playerStatus.Count];
        for (int i = 0; i < playerStatus.Count; i++)
        {
            if (TopVisibleCards[i] is not null) topVisibleCardsCopy[i] = new List<KittensCard>(TopVisibleCards[i]);
            else topVisibleCardsCopy[i] = new List<KittensCard>();
        }
        
        return new KittensObservation(drawDeck, discardDeck, playerStatus, _currentPlayerTurn, DrawAtEnd, TurnsToPlay, NextTurnsToPlay, TurnEnded, CanBlock, IsDoingFavor, index, topVisibleCardsCopy);
    }
}
