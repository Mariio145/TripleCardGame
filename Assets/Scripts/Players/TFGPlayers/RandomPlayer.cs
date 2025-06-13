using System.Collections.Generic;

public class RandomPlayer: Player
{   
    public override void SetHeuristic(GameToPlay gametoPlay)
    {
        Heuristic = gametoPlay switch
        {
            GameToPlay.VirusGame => new VirusSimpleHeuristic(),
            GameToPlay.UnoGame => new UnoSimpleHeuristic(),
            GameToPlay.KittensGame => new KittensSimpleHeuristic(),
        };
    }
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        List<IAction> actions = observable.GetActions();
        
        return actions[Random.Next(0, actions.Count)];;
    }
}