using System;
using System.Collections.Generic;
using UnityEngine;

public static class Auxiliar<T>
{
    public static List<List<int>> GetCombinations(int[] nums)
    {
        List<List<int>> result = new();
        int n = nums.Length;
        
        int totalCombinations = (1 << n); // 2^n combinaciones posibles

        for (int i = 1; i < totalCombinations; i++) 
        {
            List<int> subset = new();
            
            for (int j = 0; j < n; j++)
            {
                if ((i & (1 << j)) != 0) 
                {
                    subset.Add(nums[j]);
                }
            }
            result.Add(subset);
        }
        return result;
    }

    public static T GetAndRemoveCardFromQueue(ref Queue<T> hand, int cardPos)
    {
        if (cardPos < 0 || cardPos >= hand.Count)
        {
            Debug.Log(cardPos);
            if (hand.Count == 0) Debug.LogError("Empieza en 0");
            if (cardPos < 0)
                throw new ArgumentOutOfRangeException(nameof(cardPos), "Posición menor que 0");
            
            throw new ArgumentOutOfRangeException(nameof(cardPos), "Posición fuera de rango");
        }
        

        Queue<T> newHand = new ();
        T removedCard = default;
        int index = 0;

        while (hand.Count > 0)
        {
            T card = hand.Dequeue();
            if (index != cardPos)
                newHand.Enqueue(card);
            else
                removedCard = card;
            
            index++;
        }

        hand = new Queue<T>(newHand);
        return removedCard;
    }

    
}