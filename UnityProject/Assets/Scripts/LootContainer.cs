using System;
using UnityEngine;

namespace Subsistence
{
    public sealed class LootContainer : MonoBehaviour
    {
        public int Tier { get; private set; }
        public string DisplayName { get; private set; }
        public ItemStack[] Items { get; }=new ItemStack[12];
        public bool WasOpened { get; private set; }
        public bool Empty { get {foreach(var s in Items)if(!s.Empty)return false;return true;} }
        bool initialized;

        public void Initialize(int tier,string displayName)
        {
            if(initialized)return;initialized=true;Tier=Mathf.Clamp(tier,1,3);DisplayName=displayName;RuntimeArtFactory.Initialize();
            var trigger=GetComponent<BoxCollider2D>();if(trigger==null)trigger=gameObject.AddComponent<BoxCollider2D>();trigger.isTrigger=false;trigger.size=new Vector2(1.35f,.90f);trigger.offset=new Vector2(0,.45f);
            BuildModel();FillLoot();
        }
        void BuildModel()
        {
            Material body=Tier==1?RuntimeArtFactory.CrateTier1:(Tier==2?RuntimeArtFactory.CrateTier2:RuntimeArtFactory.CrateTier3);
            var root=new GameObject("3D loot crate · tier "+Tier).transform;root.SetParent(transform,false);root.localPosition=new Vector3(0,0,2.3f);
            RuntimeArtFactory.Cube(root,"Crate base",new Vector3(0,.42f,0),new Vector3(1.22f,.78f,.88f),body);
            RuntimeArtFactory.Cube(root,"Crate lid",new Vector3(0,.83f,0),new Vector3(1.30f,.16f,.94f),Tier==3?RuntimeArtFactory.Metal:RuntimeArtFactory.Wood);
            for(int i=-1;i<=1;i++)RuntimeArtFactory.Cube(root,"Reinforcement band",new Vector3(i*.45f,.43f,-.448f),new Vector3(.075f,.75f,.035f),RuntimeArtFactory.Metal);
            for(int i=-1;i<=1;i++)RuntimeArtFactory.Cube(root,"Lid strap",new Vector3(i*.44f,.93f,0),new Vector3(.07f,.035f,.95f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(root,"Lock plate",new Vector3(0,.54f,-.48f),new Vector3(.22f,.25f,.055f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(root,"Lock light",new Vector3(0,.57f,-.516f),new Vector3(.07f,.06f,.018f),Tier==3?RuntimeArtFactory.Fluorescent:RuntimeArtFactory.Trim);
            var label=new GameObject("Crate identification").AddComponent<TextMesh>();label.transform.SetParent(root,false);label.transform.localPosition=new Vector3(0,.70f,-.505f);label.transform.localRotation=Quaternion.Euler(0,180,0);label.text="SUPPLY  /  "+Tier;label.characterSize=.055f;label.fontSize=36;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new Color(.90f,.81f,.55f);
            RuntimeArtFactory.Cube(root,"Warning label",new Vector3(-.33f,.66f,-.47f),new Vector3(.25f,.11f,.02f),Tier==3?RuntimeArtFactory.Trim:RuntimeArtFactory.Metal);
            var glow=new GameObject("Container status").transform;glow.SetParent(root,false);glow.localPosition=new Vector3(0,1.28f,0);
            RuntimeArtFactory.PointLight(glow,"Status light",Vector3.zero,Tier==3?new Color(1,.36f,.12f):new Color(.78f,.70f,.38f),.22f,2.2f);
        }
        void FillLoot()
        {
            int seed=unchecked((int)(transform.position.x*1000))+Tier*9431;var rng=new System.Random(seed);
            if(Tier==1)
            {
                Put(ItemId.Scrap,rng.Next(8,21));Put(ItemId.Cloth,rng.Next(2,6));Put(ItemId.Water,rng.Next(1,3));
                if(rng.NextDouble()<.62)Put(ItemId.CannedFood,rng.Next(1,3));
                if(rng.NextDouble()<.32)Put(ItemId.Wood,rng.Next(4,12));
                if(rng.NextDouble()<.12)Put(ItemId.Blueprint,1);
                if(rng.NextDouble()<.12)Put(ItemId.WorkbenchI,1);
            }
            else if(Tier==2)
            {
                Put(ItemId.Scrap,rng.Next(18,36));Put(ItemId.MetalFragments,rng.Next(7,17));Put(ItemId.PistolAmmo,rng.Next(8,19));
                if(rng.NextDouble()<.58)Put(ItemId.Pipe,1);
                if(rng.NextDouble()<.46)Put(ItemId.Pistol,1);
                if(rng.NextDouble()<.65)Put(ItemId.CircuitBoard,rng.Next(1,4));
                if(rng.NextDouble()<.56)Put(ItemId.WorkbenchI,1);
                if(rng.NextDouble()<.50)Put(ItemId.Medkit,1);
                if(rng.NextDouble()<.34)Put(ItemId.Helmet,1);
            }
            else
            {
                Put(ItemId.MetalFragments,rng.Next(20,45));Put(ItemId.Scrap,rng.Next(35,71));Put(ItemId.RifleAmmo,rng.Next(14,32));Put(ItemId.Medkit,rng.Next(1,3));
                Put(ItemId.WorkbenchII,1);Put(ItemId.Keycard,1);
                if(rng.NextDouble()<.80)Put(ItemId.Rifle,1);
                if(rng.NextDouble()<.72)Put(ItemId.ArmorVest,1);
                if(rng.NextDouble()<.60)Put(ItemId.HazmatSuit,1);
                if(rng.NextDouble()<.70)Put(ItemId.Helmet,1);
                if(rng.NextDouble()<.60)Put(ItemId.Keycard,1);
                if(rng.NextDouble()<.65)Put(ItemId.CircuitBoard,rng.Next(3,8));
            }
        }
        void Put(ItemId id,int count)
        {
            for(int i=0;i<Items.Length&&count>0;i++)if(Items[i].Empty)
            {int n=Mathf.Min(count,ItemCatalog.Get(id).maxStack);Items[i]=new ItemStack(id,n);count-=n;}
        }
        public void Open(InventorySystem inventory)
        {
            if(!initialized)Initialize(1,"Supply Crate");
            if(!WasOpened){WasOpened=true;RunState.Instance?.RecordSupplyCrate();}
            inventory.SetOpenContainer(this);
            AudioDirector.Instance?.Play("metal",.48f);
        }
        public string GetHint(){return WasOpened?"E  ·  ОСМОТРЕТЬ ЯЩИК":"E  ·  ОТКРЫТЬ "+DisplayName.ToUpperInvariant();}
        public void ResetForNewRun(){Array.Clear(Items,0,Items.Length);WasOpened=false;FillLoot();}
    }
}
