using System;
using System.Collections.Generic;
using UnityEngine;

public class MCTSPlayerAmper : Player
{
    private class Node
    {
        public Node Parent;
        public List<Node> Children = new();
        public IObservation State;
        public IAction Action;

        public float Value = 0;
        public int Visits = 0;

        public Node(IObservation state, IAction action = null, Node parent = null)
        {
            State = state;
            Action = action;
            Parent = parent;
        }

        public bool FullyExpanded => Children.Count >= State.GetActions().Count;

        public Node GetBestChild(float explorationParam = 1.41f)
        {
            Node best = Children[0];
            float bestUCB1 = float.MinValue;

            foreach (var child in Children)
            {
                float ucb1 = (child.Value / (child.Visits + 1e-4f)) +
                             explorationParam * Mathf.Sqrt(Mathf.Log(Visits + 1) / (child.Visits + 1e-4f));

                if (ucb1 > bestUCB1)
                {
                    bestUCB1 = ucb1;
                    best = child;
                }
            }

            return best;
        }
    }

    public override IAction Think(IObservation rootObservation, float thinkingTime)
    {
        var rootNode = new Node(rootObservation.Clone());
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        int iterations = 0;
        int maxIterations = 1000;

        while (stopwatch.Elapsed.TotalSeconds < thinkingTime && iterations < maxIterations)
        {
            Node node = Select(rootNode);
            Node expandedNode = Expand(node);
            float value = Simulate(expandedNode.State.Clone());
            Backpropagate(expandedNode, value);
            iterations++;
        }

        if (rootNode.Children.Count == 0)
        {
            var fallbackActions = rootObservation.GetActions();
            Debug.Log("Acción aleatoria");
            return fallbackActions.Count > 0 ? fallbackActions[Random.Next(fallbackActions.Count)] : null;
        }

        Node bestChild = rootNode.Children[0];
        int bestVisits = bestChild.Visits;

        foreach (var child in rootNode.Children)
        {
            if (child.Visits > bestVisits)
            {
                bestChild = child;
                bestVisits = child.Visits;
            }
        }

        return bestChild.Action;
    }

    private Node Select(Node node)
    {
        while (node.FullyExpanded && node.Children.Count > 0)
        {
            node = node.GetBestChild();
        }
        return node;
    }

    private Node Expand(Node node)
    {
        var actions = node.State.GetActions();
        if (actions == null || actions.Count == 0)
            return node;

        var triedActions = new HashSet<IAction>(node.Children.ConvertAll(c => c.Action));

        foreach (var action in actions)
        {
            if (!triedActions.Contains(action))
            {
                IObservation newState = node.State.Clone();
                ForwardModel.TestAction(newState, action);
                var child = new Node(newState, action, node);
                node.Children.Add(child);
                return child;
            }
        }

        return node;
    }

    private float Simulate(IObservation state)
    {
        var actions = state.GetActions();
        if (actions == null || actions.Count == 0)
            return 0;

        var randomAction = actions[Random.Next(actions.Count)];
        ForwardModel.TestAction(state, randomAction);
        return Heuristic.Evaluate(state);
    }

    private void Backpropagate(Node node, float value)
    {
        while (node != null)
        {
            node.Visits++;
            node.Value += value;
            node = node.Parent;
        }
    }
}
