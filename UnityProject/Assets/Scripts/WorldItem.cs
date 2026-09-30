using UnityEngine;

namespace Subsistence
{
    public sealed class WorldItem : MonoBehaviour
    {
        public ItemId Id { get; private set; }
        public int Count { get; private set; }
        Transform model;float bobSeed;
        public static WorldItem Spawn(ItemId id,int count,Vector3 position)
        {
            var go=new GameObject("Dropped item · "+ItemCatalog.Get(id).name);go.transform.position=position;
            var item=go.AddComponent<WorldItem>();item.Initialize(id,count);return item;
        }
        public void Initialize(ItemId id,int count)
        {
            Id=id;Count=count;bobSeed=Random.value*6.28f;RuntimeArtFactory.Initialize();
            var trigger=gameObject.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(.70f,.75f);trigger.offset=new Vector2(0,.36f);
            model=new GameObject("3D pickup model").transform;model.SetParent(transform,false);model.localPosition=new Vector3(0,.35f,2.25f);
            Material m=NewMat(ItemCatalog.Get(id).iconColor);Material metal=RuntimeArtFactory.Metal;
            switch(id)
            {
                case ItemId.Water: RuntimeArtFactory.Cube(model,"Bottle",Vector3.zero,new Vector3(.24f,.40f,.18f),m);RuntimeArtFactory.Cube(model,"Bottle neck",new Vector3(0,.22f,0),new Vector3(.09f,.09f,.09f),metal);break;
                case ItemId.CannedFood: RuntimeArtFactory.Cylinder(model,"Tin can",Vector3.zero,new Vector3(.25f,.30f,.25f),m);break;
                case ItemId.Pistol: case ItemId.Rifle: RuntimeArtFactory.Cube(model,"Weapon body",Vector3.zero,new Vector3(id==ItemId.Rifle ? .55f : .34f,.13f,.10f),metal);RuntimeArtFactory.Cube(model,"Weapon grip",new Vector3(-.12f,-.10f,0),new Vector3(.11f,.19f,.10f),m);break;
                case ItemId.Helmet: RuntimeArtFactory.Sphere(model,"Helmet",Vector3.zero,new Vector3(.42f,.24f,.34f),m);break;
                case ItemId.ArmorVest: case ItemId.HazmatSuit: RuntimeArtFactory.Cube(model,"Armour",Vector3.zero,new Vector3(.35f,.42f,.16f),m);break;
                case ItemId.Bandage: case ItemId.Cloth: RuntimeArtFactory.Cube(model,"Folded fabric",Vector3.zero,new Vector3(.30f,.19f,.18f),m);RuntimeArtFactory.Cube(model,"Fabric band",new Vector3(0,0,-.1f),new Vector3(.07f,.20f,.02f),RuntimeArtFactory.Trim);break;
                case ItemId.Scrap: case ItemId.MetalFragments: case ItemId.CircuitBoard: RuntimeArtFactory.Cube(model,"Metal salvage",Vector3.zero,new Vector3(.27f,.22f,.19f),m);RuntimeArtFactory.Cube(model,"Salvage edge",new Vector3(0,.13f,0),new Vector3(.33f,.055f,.22f),metal);break;
                default: RuntimeArtFactory.Cube(model,"Pickup",Vector3.zero,new Vector3(.26f,.24f,.18f),m);break;
            }
            var label=new GameObject("Pickup name").AddComponent<TextMesh>();label.transform.SetParent(transform,false);label.transform.localPosition=new Vector3(0,.83f,2.2f);label.transform.localRotation=Quaternion.Euler(0,180,0);label.text=count>1?ItemCatalog.Get(id).name+" ×"+count:ItemCatalog.Get(id).name;label.characterSize=.045f;label.fontSize=28;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.93f,.88f,.71f);
        }
        static Material NewMat(Color c){Shader s=Shader.Find("Standard");return new Material(s){color=c};}
        public string Hint=>"E  ·  ПОДОБРАТЬ "+ItemCatalog.Get(Id).name.ToUpperInvariant();
        public bool Collect(InventorySystem inventory)
        {
            if(!inventory.Add(Id,Count))return false;AudioDirector.Instance?.Play("metal",.55f);Destroy(gameObject);return true;
        }
        void Update(){if(model!=null){model.localRotation=Quaternion.Euler(0,Mathf.Sin(Time.time*1.2f+bobSeed)*6f,0);model.localPosition=new Vector3(0,.35f+Mathf.Sin(Time.time*2+bobSeed)*.055f,2.25f);}}
    }
}
