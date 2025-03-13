using System.Collections.Generic;
using UnityEngine;

public class MctsPlayer : Player
{
    private readonly IForwardModel _forwardModel;

    public override IAction Think(IObservation observable, float thinkingTime)
    {
        MctsNode rootNode = new(null, null, observable);

        // Fill with children of the root node
        ExpandNode(rootNode);

        int iteration = 1;
        const int iterMax = 100;


        while (iteration < iterMax)
        {
            MctsNode current = rootNode;

            // Traverse the tree until a leaf node is reached
            while (!current.IsLeaf())
            {
                current = current.GetBestChild();
            }

            if (!current.HasBeenVisited())
            {
                // Rollout from this node and backpropagate (update the score of all the nodes in the path)
                float score = Rollout(current);
                Backpropagate(current, score);
            }
            else
            {
                // Expand the node
                ExpandNode(current);
            }
            iteration++;
        }

        // get child with best score
        MctsNode bestChild = rootNode.GetBestChild();
        return bestChild.GetAction();
    }

    private void ExpandNode(MctsNode node)
    {
        // Get the list of all possible actions from an observation of 'node'
        List<IAction> listPossibleActions = node.GetObservation().GetActions();

        // Expand using each possible action
        foreach (IAction action in listPossibleActions)
        {
            IObservation newObservation = node.GetObservation().Clone();
            _forwardModel.TestAction(newObservation, action);

            // If the action produces a change in the current position, and
            // it doesn't cause falling into a hole, add that action and observation clone
            // as a child of the current node
            if (!newObservation.IsTerminal())
            {
                node.AddChild(action, newObservation);
            }
        }
    }

    private float Rollout(MctsNode node)
    {
        // Consider a limit in the depth of this branch, that is,
        // in the number of nodes to be produced.
        int maxIterations = 50;

        // Get a clone of an observation of the current node as starting point
        IObservation newObservation = node.GetObservation().Clone();

        // Go producing nodes until a terminal node or the maximum number of
        // iterations is reached
        int iterations = 1;
        while (iterations < maxIterations && !newObservation.IsTerminal())
        {
            // From the list of possible actions, pick up one at random
            List<IAction> possibleActions = newObservation.GetActions();
            int randomIndex = Random.Next(0, possibleActions.Count);
            IAction action = possibleActions[randomIndex];

            // Use the forward model to test it on the cloned observation
            _forwardModel.TestAction(newObservation, action);
            iterations++;
        }

        // Return the score of the last observation
        return Heuristic.Evaluate(newObservation);
    }

    private void Backpropagate(MctsNode node, float score)
    {
        while (node != null)
        {
            // Update current node's score
            node.UpdateStats(score);

            // Go up to the parent of the current node
            node = node.Parent;
        }
    }

    public override string ToString()
    {
        return "MCTSPlayer";
    }
}
public class MctsNode
{
    private readonly IAction _action;  // the move that got us to this node - null for the root node
    internal readonly MctsNode Parent;  // null for the root node
    private readonly List<MctsNode> _children;
    private float _wins;
    private int _visits;
    private readonly IObservation _observation;
    private const float BigNumber = 100000f;

    public MctsNode(IAction action, MctsNode parent, IObservation observation)
    {
        _action = action;
        Parent = parent;
        _children = new List<MctsNode>();
        _wins = 0;
        _visits = 0;
        _observation = observation;
    }

    private float GetUcb()
    {
        if (_visits == 0)
        {
            return BigNumber + Random.value;
        }
        else
        {
            return _wins / _visits + Mathf.Sqrt(2 * Mathf.Log(Parent._visits) / _visits);
        }
    }

    public MctsNode GetBestChild()
    {
        MctsNode bestChild = null;
        float bestUcb = -BigNumber;
        
        foreach (MctsNode child in _children)
        {
            float ucb = child.GetUcb();
            if (ucb > bestUcb)
            {
                bestChild = child;
                bestUcb = ucb;
            }
        }

        if (bestChild != null) return bestChild;
        
        int randomIndex = Random.Range(0, _children.Count);
        bestChild = _children[randomIndex];

        return bestChild;
    }

    public void AddChild(IAction action, IObservation observation)
    {
        MctsNode newNode = new(action, this, observation);
        _children.Add(newNode);
    }

    public void UpdateStats(float result)
    {
        _visits++;
        _wins += result;
    }

    public bool IsLeaf()
    {
        return _children.Count == 0;
    }

    public bool HasBeenVisited()
    {
        return _visits > 0;
    }

    public IObservation GetObservation()
    {
        return _observation;
    }

    public IAction GetAction()
    {
        return _action;
    }
}

