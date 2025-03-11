using System;
using System.Threading.Tasks;
using UnityEngine;

public class VirusForwardModel : IForwardModel
{
    public async Task<bool> PlayAction(IGameState gameState, IAction action)
    {
        bool result;
        /*if (action is not null)*/ result = await action.PlayAction(gameState);
        await Task.Delay(1000); //Tiempo entre acciones
        await EndTurn(gameState);
        await Task.Delay(1000); //Tiempo entre acciones
        return result;
    }

    private async Task EndTurn(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) throw new NullReferenceException("gameState is null");
        //Robar tantas cartas como te falten para tener 3
        VirusPlayerStatus player = virusGs.PlayersStatus[virusGs.GetPlayerTurnIndex()];
        
        while (player.Hand.Count != player.HandGObject.GetComponentsInChildren<VisualVirusCard>().Length)
        {
            await Task.Yield();
        }
        
        for (int i = player.HandGObject.GetComponentsInChildren<VisualVirusCard>().Length; i < 3; i++)
        {
            player.Hand.Enqueue(virusGs.DrawCardFromDrawDeck(player.HandGObject).Result);
            virusGs.UpdateHands();
            await Task.Delay(200); //Tiempo entre acciones
        }
        
        

        foreach (VirusPlayerStatus playerUpdate in virusGs.PlayersStatus)
        {
            Debug.Log("Count pero en forwardModel: " + playerUpdate.Body.Count);
            playerUpdate.VisualBody.UpdateBody();
        }
        
        gameState.ChangeTurnIndex();
    }

    public Task<bool> TestAction(IObservation observation, IAction action)
    {
        bool result = false;
        if (action is not null) result = action.TestAction(observation);
        
        EndObTurn(observation);
        
        return Task.FromResult(result);
    }

    private void EndObTurn(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) throw new NullReferenceException("observation is null");

        //Robar tantas cartas como te falten para tener 3
        VirusPlayerStatus player = virusOb.PlayersStatus[virusOb.GetPlayerTurnIndex()];
        
        for (int i = player.HandGObject.GetComponentsInChildren<VisualVirusCard>().Length; i < 3; i++)
        {
            player.Hand.Enqueue(virusOb.DrawCardFromMixedDrawDeck());
        }
        
        observation.ChangeTurnIndex();
    }
}