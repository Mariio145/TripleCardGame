using UnityEngine;

public enum KittensType
{
    Exploding,
    Defuse,
    Nope,
    Attack,
    Skip,
    Favor,
    Shuffle,
    SeeTheFuture,
    Cat1,
    Cat2,
    Cat3,
    Cat4,
    Cat5
}

public class KittensCard : Card
{
    public KittensType Type { get; }

    public KittensCard(KittensType type, Transform parent = null)
    {
        Type = type;
        if (parent is not null) CreateVisualCard(parent);
    }
    
    public override string ToString()
    {
        return $"Tipo: {Type}";
    }
}