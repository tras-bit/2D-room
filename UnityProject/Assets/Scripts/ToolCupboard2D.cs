using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>
    /// Offline building privilege and upkeep. The cupboard pays one matching
    /// material per owned building piece every five game minutes; unpaid pieces
    /// eventually decay, with a grace period to restock the cupboard.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ToolCupboard2D : MonoBehaviour
    {
        public const float PrivilegeRadius=12f;
        public const float UpkeepInterval=300f;
        public const float InitialGrace=300f;
        public const float PaidGrace=600f;

        LootContainer storage;
        float upkeepClock=UpkeepInterval-30f;
        int lastPaid,lastRequired,lastOwned;
        string lastResult="Добавь ресурс для грейда; первое списание через 30 секунд.";

        public string Status
        {
            get
            {
                string timer=Mathf.CeilToInt(Mathf.Max(0,UpkeepInterval-upkeepClock))+" c";
                if(lastRequired<=0)return "ЗОНА 12 м · "+lastResult+" · СЛЕДУЮЩЕЕ СПИСАНИЕ ЧЕРЕЗ "+timer;
                return "ЗОНА 12 м · СЕКЦИЙ "+lastOwned+" · UPKEEP "+lastPaid+"/"+lastRequired+" · "+timer;
            }
        }

        public void Initialize(LootContainer cupboardStorage)
        {
            storage=cupboardStorage!=null?cupboardStorage:GetComponent<LootContainer>();
            upkeepClock=UpkeepInterval-30f;
        }

        void Update()
        {
            if(!GameHUD.IsPlaying||RunState.Instance==null||RunState.Instance.IsDead||RunState.Instance.HasEscaped||storage==null)return;
            upkeepClock+=Time.deltaTime;
            if(upkeepClock<UpkeepInterval)return;
            bool fullyPaid=PayUpkeep();
            // If stock is short, retry every 30 seconds so a player can restock
            // before the grace period expires instead of waiting another cycle.
            upkeepClock=fullyPaid?0f:UpkeepInterval-30f;
        }

        public bool PayUpkeepNow()
        {
            if(storage==null)storage=GetComponent<LootContainer>();
            return PayUpkeep();
        }

        bool PayUpkeep()
        {
            var owned=new List<BuildingPiece2D>();
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())
                if(NearestFor(piece)==this)owned.Add(piece);
            owned.Sort((a,b)=>Vector2.Distance(transform.position,a.transform.position).CompareTo(Vector2.Distance(transform.position,b.transform.position)));
            lastOwned=owned.Count;lastRequired=owned.Count;lastPaid=0;
            if(owned.Count==0)
            {
                lastRequired=0;lastResult="ПОСТРОЕК В РАДИУСЕ НЕТ";return false;
            }

            ItemId[] resources={ItemId.Wood,ItemId.Stone,ItemId.MetalFragments,ItemId.HighQualityMetal};
            foreach(ItemId resource in resources)
            {
                var matching=new List<BuildingPiece2D>();
                foreach(var piece in owned)if(BuildingPiece2D.UpkeepResource(piece.Grade)==resource)matching.Add(piece);
                int paid=Mathf.Min(CountStored(resource),matching.Count);
                if(paid<=0)continue;
                ConsumeStored(resource,paid);
                for(int i=0;i<paid;i++)matching[i].MarkUpkeepPaid(Time.time+PaidGrace);
                lastPaid+=paid;
            }

            if(lastPaid==lastRequired)
            {
                lastResult="ОБСЛУЖИВАНИЕ ОПЛАЧЕНО";
                RunState.Instance?.Notify("Шкаф: upkeep оплачен на 5 минут для "+lastPaid+" секций.",3f);
                return true;
            }
            lastResult="НЕ ХВАТАЕТ РЕСУРСОВ · "+lastPaid+"/"+lastRequired;
            RunState.Instance?.Notify("Шкаф: upkeep оплачен частично ("+lastPaid+"/"+lastRequired+"). Пополни запас.",4f);
            return false;
        }

        int CountStored(ItemId id)
        {
            int count=0;if(storage==null)return count;
            foreach(var stack in storage.Items)if(stack.id==id)count+=stack.count;
            return count;
        }

        void ConsumeStored(ItemId id,int amount)
        {
            for(int i=0;i<storage.Items.Length&&amount>0;i++)
            {
                if(storage.Items[i].id!=id)continue;
                ItemStack stack=storage.Items[i];int used=Mathf.Min(amount,stack.count);
                stack.count-=used;amount-=used;if(stack.count<=0)stack.Clear();storage.Items[i]=stack;
            }
        }

        public static ToolCupboard2D NearestFor(BuildingPiece2D piece)
        {
            if(piece==null)return null;
            ToolCupboard2D nearest=null;float best=PrivilegeRadius;
            foreach(var cupboard in FindObjectsOfType<ToolCupboard2D>())
            {
                float distance=Vector2.Distance(piece.transform.position,cupboard.transform.position);
                if(distance<best){best=distance;nearest=cupboard;}
            }
            return nearest;
        }
    }
}
