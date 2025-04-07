    using System;
    using UnityEngine;
    using Object = System.Object;

    public class VisualUnoCard : VisualCard
    {
        public override void ChangeSprite()
        {
            UnoCard card = (UnoCard)MemoryCard;
            UnoColor color = card.Color;
            UnoType type = card.Type;
            int number = card.Number;
            
            string asset = "Materials/UnoCards/";
        
            switch (type)
            {
                case UnoType.Block:
                    asset += "Block" + color;
                    break;
                case UnoType.Reverse:
                    asset += "Reverse" + color;
                    break;
                case UnoType.Draw2:
                case UnoType.Draw4:
                    asset += "Draw" + color;
                    break;
                case UnoType.Number:
                    asset += number + color.ToString();
                    break;
                case UnoType.Change:
                    asset += "Change";
                    break;
            }
            
            MeshRenderer.material = Resources.Load<Material>(asset);
        }
        
        protected override void ShowCard()
        {
            UnoCard card = (UnoCard)MemoryCard;
            UnoColor color = card.Color;
            UnoType type = card.Type;
            int number = card.Number;
            
            string asset = "Sprites/Uno/";
        
            switch (type)
            {
                case UnoType.Block:
                    asset += "Block" + color;
                    break;
                case UnoType.Reverse:
                    asset += "Reverse" + color;
                    break;
                case UnoType.Draw2:
                case UnoType.Draw4:
                    asset += "Draw" + color;
                    break;
                case UnoType.Number:
                    asset += number + color.ToString();
                    break;
                case UnoType.Change:
                    asset += "Change";
                    break;
            }
            
            CardRenderer.sprite = Resources.Load<Sprite>(asset);
            CardRenderer.enabled = true;
        }
    }
