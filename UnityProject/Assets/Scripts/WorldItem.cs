using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(SpriteRenderer),typeof(BoxCollider2D))]
    public sealed class WorldItem : MonoBehaviour
    {
        public ItemId Id{get;private set;}
        public int Count{get;private set;}
        float bobSeed;
        public static WorldItem Spawn(ItemId id,int count,Vector3 position)
        {
            var go=new GameObject("Dropped item · "+ItemCatalog.Get(id).name);go.transform.position=position;
            var item=go.AddComponent<WorldItem>();item.Initialize(id,count);return item;
        }
        public void Initialize(ItemId id,int count)
        {
            Id=id;Count=count;bobSeed=Random.value*6.28f;
            var renderer=GetComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.ItemSprite(id);renderer.sortingOrder=12;
            var collider=GetComponent<BoxCollider2D>();collider.isTrigger=true;collider.size=new Vector2(.75f,.75f);collider.offset=new Vector2(0,.36f);
        }
        public string Hint=>"E  ·  ПОДОБРАТЬ "+ItemCatalog.Get(Id).name.ToUpperInvariant();
        public bool Collect(InventorySystem inventory)
        {
            if(!inventory.Add(Id,Count))return false;AudioDirector.Instance?.Play("metal",.55f);Destroy(gameObject);return true;
        }
        void Update(){transform.localPosition=new Vector3(transform.localPosition.x,Mathf.Sin(Time.time*2+bobSeed)*.045f,transform.localPosition.z);}
    }
}
