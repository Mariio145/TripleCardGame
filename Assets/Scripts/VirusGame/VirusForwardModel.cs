using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class VirusForwardModel : IForwardModel
{
    public async Task<bool> PlayAction(IGameState gameState, IAction action)
    {
        bool result;
        if (action is not null) result = await action.PlayAction(gameState);
        else result = await new VirusActionDiscard(new List<int> {1, 2, 3}, gameState.GetPlayerTurnIndex()).PlayAction(gameState);
        await Task.Delay(1000); //Tiempo entre acciones
        await EndTurn(gameState);
        await Task.Delay(1000); //Tiempo entre acciones
        return result;
    }

    private async Task EndTurn(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) throw new NullReferenceException("gameState is null");
        VirusPlayerStatus player = virusGs.PlayersStatus[virusGs.GetPlayerTurnIndex()];
        
        while (player.Hand.Count != player.HandGObject.GetComponentsInChildren<VisualVirusCard>().Length)
        {
            await Task.Yield(); //Sincronizacion
        }
        
        for (int i = player.HandGObject.GetComponentsInChildren<VisualVirusCard>().Length; i < 3; i++)
        {
            player.Hand.Enqueue(await virusGs.DrawCardFromDrawDeck(player.HandGObject));
            virusGs.UpdateHands();
            await Task.Delay(200);
        }
        
        gameState.ChangeTurnIndex();
    }

    public Task<bool> TestAction(IObservation observation, IAction action)
    {
        bool result = false;
        if (action is not null) result = action.TestAction(observation);
        else result = new VirusActionDiscard(new List<int> {1, 2, 3}, observation.GetPlayerTurnIndex()).TestAction(observation);
        
        EndObTurn(observation);
        
        return Task.FromResult(result);
    }

    private void EndObTurn(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) throw new NullReferenceException("observation is null");

        //Robar tantas cartas como te falten para tener 3
        VirusPlayerStatus player = virusOb.PlayersStatus[virusOb.GetPlayerTurnIndex()];
        
        for (int i = player.Hand.Count; i < 3; i++)
        {
            player.Hand.Enqueue(virusOb.DrawCardFromMixedDrawDeck());
        }
        
        observation.ChangeTurnIndex();
    }
}