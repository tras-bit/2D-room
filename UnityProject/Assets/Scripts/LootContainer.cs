using System;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Abandoned world caches and player-built empty storage use the same inventory UI, but are kept distinct.</summary>
    public sealed class LootContainer : MonoBehaviour
    {
        public int Tier { get; private set; }
        public string DisplayName { get; private set; }
        public ItemStack[] Items { get; }=new ItemStack[12];
        public bool WasOpened { get; private set; }
        public bool IsPlayerStorage { get; private set; }
        public bool IsToolCupboard { get; private set; }
        public bool Empty { get {foreach(var s in Items)if(!s.Empty)return false;return true;} }
        bool initialized;

        public void Initialize(int tier,string displayName)
        {
            if(initialized)return;
            initialized=true;IsPlayerStorage=false;IsToolCupboard=false;Tier=Mathf.Clamp(tier,1,3);DisplayName=displayName;
            ConfigureVisuals();FillLoot();
        }

        /// <summary>Creates a crafted, empty home box. Unlike a supply cache, opening it never records found loot.</summary>
        public void InitializePlayerStorage(string displayName)
        {
            if(initialized)return;
            initialized=true;IsPlayerStorage=true;IsToolCupboard=false;Tier=0;DisplayName=displayName;
            ConfigureVisuals();Array.Clear(Items,0,Items.Length);
        }

        public void InitializeToolCupboard(string displayName)
        {
            if(initialized)return;
            initialized=true;IsPlayerStorage=false;IsToolCupboard=true;Tier=0;DisplayName=displayName;
            ConfigureVisuals();Array.Clear(Items,0,Items.Length);
        }

        void ConfigureVisuals()
        {
            var trigger=GetComponent<BoxCollider2D>();if(trigger==null)trigger=gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger=true;trigger.size=new Vector2(1.36f,1.16f);trigger.offset=new Vector2(0,.56f);
            var renderer=GetComponent<SpriteRenderer>();if(renderer==null)renderer=gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=PixelArtFactory.Crate2D(IsToolCupboard?3:1);renderer.sortingOrder=7;
            renderer.color=IsPlayerStorage?new Color(.77f,.92f,.72f,1f):IsToolCupboard?new Color(.72f,.83f,.76f,1f):Color.white;
            RendererLibrary2D.Configure(renderer);
        }

        void FillLoot()
        {
            int seed=unchecked((int)(transform.position.x*1000))+Tier*9431;var rng=new System.Random(seed);
            if(Tier==1)
            {
                Put(ItemId.Scrap,rng.Next(8,21));Put(ItemId.Cloth,rng.Next(2,6));Put(ItemId.Water,rng.Next(1,3));
                if(rng.NextDouble()<.62)Put(ItemId.CannedFood,rng.Next(1,3));
                // The first abandoned cache deliberately contains enough wood to craft the plan,
                // hammer, and a first twig piece. The later cache remains a smaller supplement.
                Put(ItemId.Wood,Mathf.Abs(transform.position.x+9f)<.2f?160:rng.Next(20,51));
                if(Mathf.Abs(transform.position.x+9f)<.2f)Put(ItemId.WorkbenchI,1);
                else if(rng.NextDouble()<.16)Put(ItemId.WorkbenchI,1);
                if(rng.NextDouble()<.12)Put(ItemId.Blueprint,1);
            }
            else if(Tier==2)
            {
                Put(ItemId.Scrap,rng.Next(18,36));Put(ItemId.MetalFragments,rng.Next(7,18));Put(ItemId.Stone,rng.Next(35,66));Put(ItemId.Wood,rng.Next(50,91));Put(ItemId.PistolAmmo,rng.Next(8,19));
                if(Mathf.Abs(transform.position.x-19f)<.2f)Put(ItemId.Blueprint,1);
                if(rng.NextDouble()<.58)Put(ItemId.Pipe,1);
                if(rng.NextDouble()<.46)Put(ItemId.Pistol,1);
                if(rng.NextDouble()<.65)Put(ItemId.CircuitBoard,rng.Next(1,4));
                if(rng.NextDouble()<.56)Put(ItemId.WorkbenchI,1);
                if(rng.NextDouble()<.50)Put(ItemId.Medkit,1);
                if(rng.NextDouble()<.34)Put(ItemId.Helmet,1);
            }
            else
            {
                Put(ItemId.MetalFragments,rng.Next(80,131));Put(ItemId.HighQualityMetal,rng.Next(30,51));
                Put(ItemId.Scrap,rng.Next(35,71));Put(ItemId.Stone,rng.Next(60,101));Put(ItemId.Wood,rng.Next(100,151));Put(ItemId.RifleAmmo,rng.Next(14,32));Put(ItemId.Medkit,rng.Next(1,3));
                Put(ItemId.WorkbenchII,1);
                if(rng.NextDouble()<.80)Put(ItemId.Rifle,1);
                if(rng.NextDouble()<.72)Put(ItemId.ArmorVest,1);
                if(rng.NextDouble()<.60)Put(ItemId.HazmatSuit,1);
                if(rng.NextDouble()<.70)Put(ItemId.Helmet,1);
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
            if(!initialized)Initialize(1,"Коробка с припасами");
            if(!WasOpened){WasOpened=true;if(!IsPlayerStorage&&!IsToolCupboard)RunState.Instance?.RecordSupplyCrate();}
            inventory.SetOpenContainer(this);
            AudioDirector.Instance?.Play("metal",.48f);
        }

        public string GetHint()
        {
            if(IsPlayerStorage)return "E  ·  ОТКРЫТЬ ДОМАШНИЙ ЯЩИК";
            if(IsToolCupboard)return "E  ·  ОТКРЫТЬ ШКАФ СТРОИТЕЛЬНЫХ ПРАВ";
            return WasOpened?"E  ·  ОСМОТРЕТЬ КОРОБКУ":"E  ·  ОТКРЫТЬ "+DisplayName.ToUpperInvariant();
        }

        public void ResetForNewRun()
        {
            Array.Clear(Items,0,Items.Length);WasOpened=false;
            if(!IsPlayerStorage&&!IsToolCupboard)FillLoot();
        }
    }
}
