using System.Collections.Generic;

public interface IObservation
{
    public IObservation Clone();
    public List<IAction> GetActions();
    public int GetPlayerTurnIndex();
    public bool IsTerminal();
    public bool IsCardPlayable(Card card);
    public void ChangeTurnIndex();
    public IObservation GetCloneRandomized();
}
