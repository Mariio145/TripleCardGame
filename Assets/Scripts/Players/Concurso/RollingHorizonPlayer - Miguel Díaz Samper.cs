using System;
using System.Collections.Generic;

public class RollingHorizonPlayer : Player
{
    private const int RolloutCount = 5;
    private const int MaxDepth = 3;

    public override IAction Think(IObservation observation, float thinkingTime)
    {
        List<IAction> possibleActions = observation.GetActions();
        IAction bestAction = null;
        float bestAverageValue = float.MinValue;

        foreach (IAction action in possibleActions)
        {
            float totalValue = 0;

            for (int i = 0; i < RolloutCount; i++)
            {
                IObservation simulatedObs = observation.Clone();
                ForwardModel.TestAction(simulatedObs, action);

                totalValue += SimulateRollout(simulatedObs, MaxDepth - 1);
            }

            float averageValue = totalValue / RolloutCount;

            if (averageValue > bestAverageValue)
            {
                bestAverageValue = averageValue;
                bestAction = action;
            }
        }

        return bestAction ?? possibleActions[0];
    }

    private float SimulateRollout(IObservation obs, int depth)
    {
        if (depth == 0 || obs.IsTerminal())
            return Heuristic.Evaluate(obs);

        List<IAction> actions = obs.GetActions();
        if (actions.Count == 0)
            return Heuristic.Evaluate(obs);

        IAction randomAction = actions[Random.Next(actions.Count)];
        ForwardModel.TestAction(obs, randomAction);

        return SimulateRollout(obs, depth - 1);
    }
}
