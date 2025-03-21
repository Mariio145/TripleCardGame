using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UnoForwardModel : IForwardModel
{
    public async Task<bool> PlayAction(IGameState gameState, IAction action)
    {
        bool result = false;
        if (action is not null) result = await action.PlayAction(gameState);
        
        ((UnoGameState)gameState)!.UpdateHands();

        await Task.Delay(500); //Tiempo entre acciones
        
        await ChangeTurn(gameState);
        
        await Task.Delay(100); //Tiempo entre acciones
        return result;
    }

    private async Task ChangeTurn(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return ;

        if (unoGs.topCard.Color == UnoColor.Wild) return;

        unoGs.ChangeTurnIndex();
        
        // Si hay cartas pendientes por robar, forzar la acción
        if (unoGs.quantityToDraw > 0)
        {
            while (unoGs.quantityToDraw > 0)
            {
                await new UnoForcedDrawCard().PlayAction(unoGs);
                unoGs.UpdateHands();
                unoGs.quantityToDraw--;
                await Task.Delay(200);
            }
            
            unoGs.ChangeTurnIndex();
        }
        // Si hay que bloquear el turno
        else if (unoGs.blockNextTurn)
        {
            await unoGs.ShowBlockObject();
            unoGs.blockNextTurn = false;
            unoGs.ChangeTurnIndex();
        }
    }
    
    public Task<bool> TestAction(IObservation observation, IAction action)
    {
        action.TestAction(observation);
        ChangeObTurn(observation);
        return Task.FromResult(true);
    }

    private void ChangeObTurn(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return;

        if (unoObs.TopCard.Color == UnoColor.Wild) return;

        unoObs.ChangeTurnIndex();
        
        // Si hay cartas pendientes por robar, forzar la acción
        if (unoObs.quantityToDraw > 0)
        {
            while (unoObs.quantityToDraw > 0)
            {
                //Mejor hacerlo asi por si hay que animarlo
                new UnoForcedDrawCard().TestAction(observation);
                unoObs.quantityToDraw--;
            }
            unoObs.ChangeTurnIndex();
        }
        // Si hay que bloquear el turno
        else if (unoObs.blockNextTurn)
        {
            unoObs.blockNextTurn = false;
            unoObs.ChangeTurnIndex();
        }
    }
}
