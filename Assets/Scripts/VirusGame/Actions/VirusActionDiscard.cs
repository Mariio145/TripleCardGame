using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VirusActionDiscard : VirusAction
{
    private readonly List<int> _indexesToDiscard;
    public VirusActionDiscard(List<int> discardCards, int playerSelf)
    {
        _indexesToDiscard = discardCards;
        _indexesToDiscard.Reverse();
        PlayerSelf = playerSelf;
    }
    public override async Task<bool> PlayAction(IGameState gameState)
    {
        if (gameState is not VirusGameState virusGs) return await Task.FromResult(false);
        
        VirusPlayerStatus player = virusGs.PlayersStatus[PlayerSelf];
        
        List<Task> discardTasks = new();

        foreach (int i in _indexesToDiscard)
        {
            discardTasks.Add(virusGs.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, i)));
        }
        
        await Task.WhenAll(discardTasks);
        
        return await Task.FromResult(true);
    }
    
    public override bool TestAction(IObservation observation)
    {
        if (observation is not VirusObservation virusOb) return false;
        
        VirusPlayerStatus player = virusOb.PlayersStatus[PlayerSelf];

        if (player.Hand.Count < 3)
        {
            Debug.LogWarning("Descartado");
            return false;
        }

        foreach (int i in _indexesToDiscard)
        {
            virusOb.DiscardCard(Auxiliar<Card>.GetAndRemoveCardFromQueue(ref player.Hand, i));
        }
        
        return true;
    }
}