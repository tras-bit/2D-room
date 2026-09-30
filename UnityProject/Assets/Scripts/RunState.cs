using UnityEngine;

namespace Subsistence
{
    public sealed class RunState : MonoBehaviour
    {
        public static RunState Instance{get;private set;}
        public float Health{get;private set;}=100;
        public float Hunger{get;private set;}=82;
        public float Thirst{get;private set;}=68;
        public int Scrap{get;private set;}
        public int Cloth{get;private set;}
        public int SuppliesFound{get;private set;}
        public float ShiftSeconds{get;private set;}
        public string Notice{get;private set;}="Выбери сектор. Найди ресурсы и переживи смену.";
        public float NoticeTime{get;private set;}=4;
        public bool IsDead{get;private set;}
        public bool HasEscaped{get;private set;}
        public const float ExitX=94.5f;
        void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;}
        void Update()
        {
            if(!GameHUD.IsPlaying||IsDead||HasEscaped)return;
            ShiftSeconds+=Time.deltaTime;Hunger=Mathf.Max(0,Hunger-Time.deltaTime*.14f);Thirst=Mathf.Max(0,Thirst-Time.deltaTime*.21f);
            if(Hunger<=0||Thirst<=0)TakeDamage(Time.deltaTime*(Hunger<=0&&Thirst<=0?1.8f:1f));NoticeTime=Mathf.Max(0,NoticeTime-Time.deltaTime);
        }
        public void Collect(SupplyPickup.Kind kind)
        {
            SuppliesFound++;var bag=FindObjectOfType<InventorySystem>();
            switch(kind)
            {
                case SupplyPickup.Kind.Scrap:Scrap+=5;bag?.Add(ItemId.Scrap,5);Notify("Металлолом ×5 помещён в рюкзак.");break;
                case SupplyPickup.Kind.Cloth:Cloth+=2;bag?.Add(ItemId.Cloth,2);Notify("Ткань ×2 помещена в рюкзак.");break;
                case SupplyPickup.Kind.Food:bag?.Add(ItemId.CannedFood,1);Eat(28);Notify("Подобраны консервы.");break;
                case SupplyPickup.Kind.Water:bag?.Add(ItemId.Water,1);Drink(34);Notify("Найдена вода.");break;
            }
        }
        public void RecordSupplyCrate(){SuppliesFound++;Notify("Запасы обнаружены · ящик осмотрен.");}
        public void TakeDamage(float amount)
        {if(IsDead||HasEscaped)return;Health=Mathf.Max(0,Health-amount);if(Health<=0){IsDead=true;Notify("Смена завершена. Сигнал прерван.",99);}}
        public void Heal(float amount){Health=Mathf.Min(100,Health+amount);}
        public void Eat(float amount){Hunger=Mathf.Min(100,Hunger+amount);}
        public void Drink(float amount){Thirst=Mathf.Min(100,Thirst+amount);}
        public bool CraftBandage(){var bag=FindObjectOfType<InventorySystem>();return bag!=null&&bag.CraftBandage();}
        public void ResetForNewRun()
        {Health=100;Hunger=82;Thirst=68;Scrap=0;Cloth=0;SuppliesFound=0;ShiftSeconds=0;IsDead=false;HasEscaped=false;Notify("Ты один в бесконечных комнатах. Найди выход.",4);}
        public void CheckExit(float x)
        {
            if(x<ExitX)return;
            var bag=FindObjectOfType<InventorySystem>();bool keycard=bag!=null&&bag.Count(ItemId.Keycard)>0;
            if(SuppliesFound>=3&&keycard){HasEscaped=true;Notify("Сектор покинут. Но за дверью — новый сигнал.",99);}
            else if(!keycard)Notify("Дверь заблокирована. Нужна карта доступа из ящика уровня 3.");
            else Notify("Сначала осмотри припасы в трёх ящиках.");
        }
        public void Notify(string message,float duration=2.2f){Notice=message;NoticeTime=duration;}
    }
}
