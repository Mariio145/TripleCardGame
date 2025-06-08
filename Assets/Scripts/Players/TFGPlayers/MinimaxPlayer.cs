using UnityEngine;

public class MinimaxPlayer : Player
{
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        int depth = 5;
        BoardNode startBoard = new()
        {
            Observation = observable,
            NodeHeuristic = Heuristic
        };
        return MinimaxAlphaBeta(startBoard, depth, float.NegativeInfinity, float.PositiveInfinity, true).Action;
    }
    

    private BoardNode MinimaxAlphaBeta(BoardNode boardState, int remainingDepth, float alpha, float beta, bool maximizing)
    {
        if (remainingDepth == 0 || boardState.Observation.IsTerminal())
        {
            if (boardState.Value == 0) boardState.Value = boardState.Evaluate();
            return boardState;
        }
        
        
 
        if (maximizing)
        {
            BoardNode maxEval = new();
            maxEval.Value = Mathf.NegativeInfinity;
            foreach (IAction action in boardState.Observation.GetActions())
            {
                IObservation copy = boardState.Observation.Clone();
                ForwardModel.TestAction(copy, action);
                BoardNode actionPlayed = new()
                {
                    Action = action,
                    Observation = copy,
                    NodeHeuristic = Heuristic
                };
                
                BoardNode eval = MinimaxAlphaBeta(actionPlayed, remainingDepth - 1, alpha, beta, false);
                if (maxEval.Value < eval.Value)
                    maxEval = eval;

                alpha = Mathf.Max(alpha, eval.Value);
                if (beta <= alpha)
                    break;
            }

            return maxEval;
        }
        else
        {
            BoardNode minEval =  new();
            minEval.Value = Mathf.Infinity;
            foreach (IAction action in boardState.Observation.GetActions())
            {
                IObservation copy = boardState.Observation.Clone();
                ForwardModel.TestAction(copy, action);
                BoardNode actionPlayed = new()
                {
                    Action = action,
                    Observation = copy,
                    NodeHeuristic = Heuristic
                };
                
                BoardNode eval = MinimaxAlphaBeta(actionPlayed, remainingDepth - 1, alpha, beta, true);
                if (minEval.Value > eval.Value)
                    minEval = eval;

                beta = Mathf.Min(beta, eval.Value);
                if (beta <= alpha)
                    break;
            }

            return minEval;
        }
    }
}

public class BoardNode
{
    public IObservation Observation;
    public IAction Action;
    public float Value;
    public IHeuristic NodeHeuristic;

    public float Evaluate()
    {
        Value = NodeHeuristic.Evaluate(Observation);
        return Value;
    }
}
