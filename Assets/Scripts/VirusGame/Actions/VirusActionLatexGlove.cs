using System.Collections.Generic;
using System.Threading.Tasks;

public class VirusActionLatexGlove : VirusAction
{
    public VirusActionLatexGlove(int indexCard)
    {
        CardIndex = indexCard;
    }
    public override Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return Task.FromResult(false);
        
        VirusPlayerStatus player = virusGs.PlayersStatus[virusGs.GetPlayerTurnIndex()];

        for (int i = 0; i < virusGs.PlayersStatus.Count; i++)
        {
            if (i == virusGs.GetPlayerTurnIndex()) continue;
            
            VirusPlayerStatus otherPlayer = virusGs.PlayersStatus[i];

            while (otherPlayer.Hand.Count > 0)
            {
                virusGs.DiscardCard((VirusCard)otherPlayer.Hand.Dequeue());
            }
        }

        virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex));
        
        return Task.FromResult(true);
    }

    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        VirusPlayerStatus player = virusOb.PlayersStatus[PlayerSelf];

        for (int i = 0; i < virusOb.PlayersStatus.Count; i++)
        {
            if (i == virusOb.GetPlayerTurnIndex()) continue;
            
            VirusPlayerStatus otherPlayer = virusOb.PlayersStatus[i];

            for (int j = 0; j < otherPlayer.Hand.Count; j++)
            {
                virusOb.DiscardCard((VirusCard)otherPlayer.Hand.Dequeue());
            }
        }

        virusOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex));
        return true;
    }
}
