using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck<T> : IEnumerable
{
    private readonly List<T> _deckCard;
    private static readonly System.Random random = new();

    public Deck()
    {
        _deckCard = new List<T>();
    }
    public Deck(List<T> deckCard)
    {
        _deckCard = deckCard;
    }
    
    public Deck(Deck<T> deckCard)
    {
        _deckCard = new List<T>(deckCard._deckCard);
    }
    
    public List<T> GetList() => _deckCard;

    public void Add(T item)
    {
        _deckCard.Add(item);
    }

    public T DrawCard()
    {
        if (_deckCard.Count <= 0)
        {
            Debug.LogError("Mazo vacío");
            return default(T);
        }
        T card = _deckCard[^1];
        _deckCard.RemoveAt(_deckCard.Count - 1);
        return card;
    }
    
    public void ShuffleDeck()
    {
        for (int i = _deckCard.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (_deckCard[i], _deckCard[j]) = (_deckCard[j], _deckCard[i]);
        }
    }

    public int RemainingCards()
    {
        return _deckCard.Count;
    }

    public IEnumerator GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}