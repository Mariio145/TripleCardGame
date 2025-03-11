using System.Threading.Tasks;

public interface IAction
{
    //public bool CanBeInterrupted(); //Indica si esta jugada puede ser interrumpida por otras jugadas en el mismo turno
    public Task<bool> PlayAction(IGameState gameState);
    public bool TestAction(IObservation gameState);
}