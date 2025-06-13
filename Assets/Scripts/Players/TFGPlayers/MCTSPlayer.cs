using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MctsPlayer : Player
{
    private MctsNode rootNode;

    public override IAction Think(IObservation observable, float thinkingTime)
    {
        rootNode = new MctsNode(null, null, observable);
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        ExpandNode(rootNode);

        int iteration = 1;
        const int iterMax = 100;


        while (stopwatch.Elapsed.Milliseconds < thinkingTime*1000-100 && iteration < iterMax)
        {
            MctsNode current = rootNode;
            
            while (!current.IsLeaf())
            {
                current = current.GetBestChild();
            }

            if (!current.HasBeenVisited())
            {
                float score = Rollout(current);
                Backpropagate(current, score);
            }
            else
            {
                ExpandNode(current);
            }
            iteration++;
        }
        
        MctsNode bestChild = rootNode.GetBestChild();
        return bestChild.GetAction();
    }

    private void ExpandNode(MctsNode node)
    {
        List<IAction> listPossibleActions = node.GetObservation().GetActions();
        
        foreach (IAction action in listPossibleActions)
        {
            IObservation newObservation = node.GetObservation().Clone();
            ForwardModel.TestAction(newObservation, action);
            
            if (!newObservation.IsTerminal())
            {
                node.AddChild(action, newObservation);
            }
        }
    }

    private float Rollout(MctsNode node)
    {
        const int maxIterations = 100;
        
        IObservation newObservation = node.GetObservation().Clone();
        
        int iterations = 1;
        while (iterations < maxIterations && !newObservation.IsTerminal())
        {
            List<IAction> possibleActions = newObservation.GetActions();
            int randomIndex = Random.Next(0, possibleActions.Count);
            IAction action = possibleActions[randomIndex];
            
            ForwardModel.TestAction(newObservation, action);
            iterations++;
        }
        
        return Heuristic.Evaluate(newObservation);
    }

    private void Backpropagate(MctsNode node, float score)
    {
        while (node != null)
        {
            node.UpdateStats(score);
            
            node = node.Parent;
        }
    }
}
public class MctsNode
{
    private static readonly System.Random random = new();
    private readonly IAction _action;
    internal readonly MctsNode Parent;
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
            return BigNumber + random.Next();
        }

        return _wins / _visits + Mathf.Sqrt(2 * Mathf.Log(Parent._visits) / _visits);
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
        
        int randomIndex = random.Next(0, _children.Count);
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

