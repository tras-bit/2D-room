using UnityEngine;

namespace Subsistence
{
    /// <summary>Solo attendant in Level 0's rare Manila Room; exchanges scrap for a Level 1 lift pass.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TraderNPC : MonoBehaviour
    {
        public const int GreenCardScrapCost=15;

        public void Initialize()
        {
            var sprite=GetComponent<SpriteRenderer>();
            sprite.sprite=PixelArtFactory.TraderSprite();
            sprite.sortingOrder=11;
            var trigger=GetComponent<CircleCollider2D>();
            if(trigger==null)trigger=gameObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger=true;trigger.radius=1.1f;trigger.offset=new Vector2(0,.72f);
        }

        public string GetHint(InventorySystem inventory)
        {
            if(inventory!=null&&inventory.Count(ItemId.Keycard)>0)return "ТОРГОВЕЦ  ·  ЗЕЛЁНАЯ КАРТА УЖЕ ЕСТЬ";
            return "E  ·  ОБМЕНЯТЬ 15 МЕТАЛЛОЛОМА НА ЗЕЛЁНУЮ КАРТУ";
        }

        public bool Interact(InventorySystem inventory)
        {
            if(inventory==null)return false;
            return inventory.TradeForGreenCard(GreenCardScrapCost);
        }
    }
}
