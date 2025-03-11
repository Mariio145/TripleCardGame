using UnityEngine;

public enum UnoType
{
    Number,
    Reverse,
    Block,
    Draw2,
    Change,
    Draw4
}

public enum UnoColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Wild
}

public class UnoCard : Card
{
    public UnoType Type { get; }

    public UnoColor Color { get; private set; }

    public int Number { get; }

    public UnoCard(UnoType type, UnoColor color, int number, Transform parent = null)
    {
        Type = type;
        Color = color;
        Number = number;
        if (parent is not null) CreateVisualCard(parent.transform);
    }

    public void ChangeColor(UnoColor color)
    {
        Color = color;
    }

    public override string ToString()
    {
        return $"Tipo: {Type}, Color: {Color}, Numero: {Number}";
    }
}
