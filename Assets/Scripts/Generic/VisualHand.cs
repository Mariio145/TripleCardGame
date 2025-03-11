using System;
using System.Collections.Generic;
using UnityEngine;

public class VisualHand : MonoBehaviour
{
    private List<VisualCard> _cards = new ();
    [SerializeField] private float handSize;
    private const float CardWidth = 0.05f;


    private void Start()
    {
        UpdateHandPosition();
    }

    public void UpdateHandPosition()
    {
        _cards = new List<VisualCard>();
        
        foreach (VisualCard card in GetComponentsInChildren<VisualCard>())
        {
            _cards.Add(card);
        }

        float cardSpacing = handSize / (_cards.Count + 1);
        float startPosition = -handSize / 2;

        for (int i = 0; i < _cards.Count; i++)
        {
            float xPos = startPosition + cardSpacing * (i + 1);
            _cards[i].SetPosition(new Vector3(xPos, 0, -CardWidth * i));
        }
    }
}
