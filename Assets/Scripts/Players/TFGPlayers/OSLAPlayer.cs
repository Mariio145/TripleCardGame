using System;
using System.Collections.Generic;
public class OSLAPlayer : Player
{
    public override IAction Think(IObservation observation, float thinkingTime)
    {
        List<IAction> possibleActions = observation.GetActions();
        IAction bestAction = null;
        float bestValue = float.MinValue;

        foreach (IAction action in possibleActions)
        {
            IObservation simulatedObservation = observation.Clone();
            ForwardModel.TestAction(simulatedObservation, action);
            
            float value = Heuristic.Evaluate(simulatedObservation);
            
            if (value > bestValue)
            {
                bestValue = value;
                bestAction = action;
            }
        }

        return bestAction ?? possibleActions[0];
    }
}