using System.Collections.Generic;

public class RandomPlayer: Player
{   
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        //Player de ejemplo que ejecuta una acción aleatoria de todas las opciones dadas.
        
        List<IAction> actions = observable.GetActions();
        
        IAction action = actions[Random.Next(0, actions.Count)];
        
        return action;
    }
}