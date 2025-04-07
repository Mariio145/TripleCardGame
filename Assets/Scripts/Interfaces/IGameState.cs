using System.Collections.Generic;

public interface IGameState
{
    public void Reset(List<Player> players);
    public bool IsTerminal();
    public IObservation GetObservationFromPlayer(int index);
    public int GetPlayerTurnIndex();
    public void ChangeTurnIndex();
    public Player GetPlayer();
    public Player GetWinner();
    public List<PlayerStatus> GetPlayerStatus();
}