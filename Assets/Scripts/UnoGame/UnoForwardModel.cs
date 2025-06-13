using System.Threading.Tasks;

public class UnoForwardModel : IForwardModel
{
    public async Task<bool> PlayAction(IGameState gameState, IAction action)
    {
        bool result;
        if (gameState is not UnoGameState unoGs) return false;
        
        if (action is not null) result = await action.PlayAction(unoGs);
        else
        {
            if (unoGs.TopCard.Color == UnoColor.Wild) result = await new UnoChangeColor(UnoColor.Red).PlayAction(unoGs);
            else result = await new UnoForcedDrawCard().PlayAction(unoGs);
        }
        
        unoGs.UpdateHands();

        //await Task.Delay(500); //Tiempo entre acciones
        
        await ChangeTurn(unoGs);
        
        //await Task.Delay(100); //Tiempo entre acciones
        return result;
    }

    private async Task ChangeTurn(IGameState gameState)
    {
        if (gameState is not UnoGameState unoGs) return ;

        if (unoGs.TopCard.Color == UnoColor.Wild) return;

        unoGs.ChangeTurnIndex();
        
        // Si hay cartas pendientes por robar, forzar la acción
        if (unoGs.QuantityToDraw > 0)
        {
            while (unoGs.QuantityToDraw > 0)
            {
                await new UnoForcedDrawCard().PlayAction(unoGs);
                unoGs.UpdateHands();
                unoGs.QuantityToDraw--;
                await Task.Delay(10);
            }
            
            unoGs.ChangeTurnIndex();
        }
        // Si hay que bloquear el turno
        else if (unoGs.BlockNextTurn)
        {
            unoGs.BlockNextTurn = false;
            unoGs.ChangeTurnIndex();
        }
    }
    
    public bool TestAction(IObservation observation, IAction action)
    {
        action.TestAction(observation);
        ChangeObTurn(observation);
        return true;
    }

    private void ChangeObTurn(IObservation observation)
    {
        if (observation is not UnoObservation unoObs) return;

        if (unoObs.TopCard.Color == UnoColor.Wild) return;

        unoObs.ChangeTurnIndex();
        
        // Si hay cartas pendientes por robar, forzar la acción
        if (unoObs.QuantityToDraw > 0)
        {
            while (unoObs.QuantityToDraw > 0)
            {
                //Mejor hacerlo asi por si hay que animarlo
                new UnoForcedDrawCard().TestAction(observation);
                unoObs.QuantityToDraw--;
            }
            unoObs.ChangeTurnIndex();
        }
        // Si hay que bloquear el turno
        else if (unoObs.BlockNextTurn)
        {
            unoObs.BlockNextTurn = false;
            unoObs.ChangeTurnIndex();
        }
    }
}
