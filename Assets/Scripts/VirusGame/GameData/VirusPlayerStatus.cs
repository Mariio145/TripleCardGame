using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirusPlayerStatus : PlayerStatus
{
    //private readonly Player _player;
    public VisualVirusBody VisualBody;
    public List<VirusOrgan> Body = new();
    public readonly int Index;

    public VirusPlayerStatus(int index, GameObject gObject, Player player, bool isNew = false) : base(new Queue<Card>(), gObject, player)
    {
        Index = index;
        HandGObject = gObject;

        if (!isNew) return;
        VisualBody = gObject.transform.parent.GetComponentInChildren<VisualVirusBody>();
    }

    public VirusPlayerStatus Clone()
    {
        VirusPlayerStatus copy = new (Index, HandGObject, Player)
        {
            Body = new List<VirusOrgan>()
        };
        
        foreach (Card card in Hand)
            copy.Hand.Enqueue(card);

        foreach (VirusOrgan organ in Body)
            copy.Body.Add(organ.Clone());
        
        copy.VisualBody = VisualBody;

        return copy;
    }
    public VirusOrgan SearchOrganColor(VirusColor color)
    {
        return Body.Find(x => x.OrganColor == color);
    }

    public void AddOrgan(VirusColor organ)
    {
        Body.Add(new VirusOrgan(organ));
    }
    
    public void AddOrgan(VirusOrgan organ)
    {
        Body.Add(organ);
    }

    public void RemoveOrgan(VirusColor color)
    {
        Body.Remove(Body.Find(x => x.OrganColor == color));
    }

    public override bool HasWon()
    {
        int saneOrgans = Body.Count(organ => organ.Status != Status.Infected);

        return saneOrgans >= 4;
    }
    
    public override int GetPunctuation()
    {
        return Body.Count(organ => organ.Status != Status.Infected);
    }
}
