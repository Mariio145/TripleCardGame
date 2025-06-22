using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VirusActionLatexGlove : VirusAction
{
    public VirusActionLatexGlove(int indexCard)
    {
        CardIndex = indexCard;
    }
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return false;
        
        VirusPlayerStatus player = virusGs.PlayersStatus[virusGs.GetPlayerTurnIndex()];
        
        string cardEffect = "Card" + new Random().Next(1, 5);
        SoundManager.Instance.PlaySfx(cardEffect);
        await virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, CardIndex));
        
        List<Task> discardTasks = new();

        for (int i = 0; i < virusGs.PlayersStatus.Count; i++)
        {
            if (i == virusGs.GetPlayerTurnIndex()) continue;
            
            VirusPlayerStatus otherPlayer = virusGs.PlayersStatus[i];

            while (otherPlayer.Hand.Count > 0)
            {
                Card card = otherPlayer.Hand.Dequeue();
                discardTasks.Add(virusGs.DiscardCard(card));
                if (i == 0)
                    (card.VisualCard as VisualVirusCard)!.SetOutline(false);
            }
        }
        
        await Task.WhenAll(discardTasks);
        
        return true;
    }

    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        
        VirusPlayerStatus player = virusOb.PlayersStatus[virusOb.GetPlayerTurnIndex()];

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
