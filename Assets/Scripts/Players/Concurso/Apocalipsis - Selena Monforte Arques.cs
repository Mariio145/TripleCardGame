using UnityEngine;
using System;
using System.Collections.Generic;


public class Apocalipsis : Player

{
    public override IAction Think(IObservation observation, float thinkingTime)
    {
        List<IAction> possibleActions = observation.GetActions();

        if (possibleActions.Count == 0)
            return null;

        IAction bestAction = null;
        float bestScore = float.MinValue;

        foreach (IAction action in possibleActions)
        {
            
            IObservation simulation = observation.Clone();

            
            ForwardModel.TestAction(simulation, action);

            
            float score = Heuristic.Evaluate(simulation);

            
            score += EvaluateBonus(observation, action);

            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        return bestAction ?? possibleActions[0];
    }

    private float EvaluateBonus(IObservation observation, IAction action)
    {
        float bonus = 0f;

        
        if (observation is UnoObservation)
        {
            if (action is UnoDrawCard)
            {
                bonus -= 10f; 
            }
            else if (action is UnoPlayCard playCard)
            {
                
                var typeField = typeof(UnoPlayCard).GetField("_cardType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (typeField != null)
                {
                    UnoType cardType = (UnoType)typeField.GetValue(playCard);
                    
                    if (cardType == UnoType.Draw4)
                        bonus += 15f;
                    else if (cardType == UnoType.Block || cardType == UnoType.Reverse)
                        bonus += 5f;
                }

            }
        }
        
        if (observation is VirusObservation)
        {
            if (action is VirusActionPlayOrgan)
                bonus += 10f;
            else if (action is VirusActionPlayMedicine)
                bonus += 7f;
            else if (action is VirusActionPlayVirus)
                bonus += 8f;
            else if (action is VirusActionDiscard)
                bonus -= 5f;
        }

        return bonus;
    }
}