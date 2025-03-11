using UnityEngine;

public class VisualKittensCard : VisualCard
{
    public override void ChangeSprite()
    {
        KittensCard card = (KittensCard)MemoryCard;
        KittensType cardType = card.Type;

        // Aquí se debe cargar el sprite correspondiente al tipo de carta
        string asset = "Materials/KittensCards/";
        
        asset += cardType.ToString();

        MeshRenderer.material = Resources.Load<Material>(asset);
    }
}
