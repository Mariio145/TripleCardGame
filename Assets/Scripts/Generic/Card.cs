using UnityEngine;

public class Card
{
    public VisualCard VisualCard;
    
    protected void CreateVisualCard(Transform parent)
    {
        GameObject obj = new ("Card")
        {
            layer = LayerMask.NameToLayer("Card")
        };

        VisualCard = this switch
        {
            UnoCard => obj.AddComponent<VisualUnoCard>(),
            KittensCard => obj.AddComponent<VisualKittensCard>(),
            VirusCard => obj.AddComponent<VisualVirusCard>(),
            _ => VisualCard
        };

        VisualCard.MemoryCard = this;
        VisualCard.ChangeParent(parent);
        VisualCard.ChangeSprite();
    }
}