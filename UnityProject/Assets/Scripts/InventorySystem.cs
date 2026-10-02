using System;
using UnityEngine;

namespace Subsistence
{
    public enum ItemId
    {
        None, Flashlight, Cloth, Scrap, MetalFragments, Wood, Water, CannedFood,
        Bandage, Pistol, PistolAmmo, Pipe, Blueprint, WorkbenchI, WorkbenchII,
        FieldJacket, FieldPants, WorkBoots, Backpack, Helmet, ArmorVest,
        Medkit, Keycard, Rifle, RifleAmmo, HazmatSuit, CircuitBoard,
        BuildingPlan, Hammer, Stone, HighQualityMetal, StorageBox,
        WorkbenchStationI, WorkbenchStationII, ToolCupboard
    }

    public enum GearSlot { Head, Chest, Legs, Feet, Back }

    [Serializable]
    public struct ItemStack
    {
        public ItemId id;
        public int count;
        public bool Empty => id == ItemId.None || count <= 0;
        public ItemStack(ItemId item, int amount) { id=item;count=amount; }
        public void Clear() { id=ItemId.None;count=0; }
    }

    public sealed class ItemDefinition
    {
        public ItemId id; public string name; public string code; public string description;
        public int maxStack; public int tier; public GearSlot? gearSlot;
        public float bullet, melee, cold, radiation;
        public Color iconColor;
        public ItemDefinition(ItemId id,string name,string code,string desc,int stack,int tier,Color color,GearSlot? slot=null,float bullet=0,float melee=0,float cold=0,float radiation=0)
        {this.id=id;this.name=name;this.code=code;description=desc;maxStack=stack;this.tier=tier;iconColor=color;gearSlot=slot;this.bullet=bullet;this.melee=melee;this.cold=cold;this.radiation=radiation;}
    }

    public static class ItemCatalog
    {
        static readonly ItemDefinition[] definitions =
        {
            new ItemDefinition(ItemId.Flashlight,"Ручной фонарь","TOOL","Старый фонарь. Заряд почти полный.",1,0,new Color(.82f,.77f,.47f)),
            new ItemDefinition(ItemId.Cloth,"Ткань","MAT","Чистая ткань для перевязки и крафта.",20,0,new Color(.70f,.68f,.54f)),
            new ItemDefinition(ItemId.Scrap,"Металлолом","MAT","Коррозионностойкие детали. Ценная валюта убежищ.",100,0,new Color(.72f,.66f,.46f)),
            new ItemDefinition(ItemId.MetalFragments,"Фрагменты металла","MAT","Переплавленный металл для оружия и брони.",100,1,new Color(.62f,.68f,.68f)),
            new ItemDefinition(ItemId.Wood,"Доски","MAT","Сухая древесина из старой мебели.",100,0,new Color(.62f,.43f,.27f)),
            new ItemDefinition(ItemId.Water,"Вода","FOOD","Запечатанная бутылка. Восстанавливает жажду.",10,0,new Color(.36f,.69f,.77f)),
            new ItemDefinition(ItemId.CannedFood,"Консервы","FOOD","Еда с долгим сроком хранения.",10,0,new Color(.78f,.57f,.32f)),
            new ItemDefinition(ItemId.Bandage,"Бинт","MED","Останавливает кровь и восстанавливает здоровье.",5,0,new Color(.78f,.77f,.65f)),
            new ItemDefinition(ItemId.Pistol,"Пистолет M9","WEAPON","Компактное оружие ближней обороны. Нужны патроны.",1,2,new Color(.56f,.58f,.55f)),
            new ItemDefinition(ItemId.PistolAmmo,"Патроны 9 мм","AMMO","Магазин стандартных патронов.",60,1,new Color(.76f,.67f,.39f)),
            new ItemDefinition(ItemId.Pipe,"Стальная труба","WEAPON","Тяжёлое оружие ближнего боя.",1,1,new Color(.59f,.63f,.60f)),
            new ItemDefinition(ItemId.Blueprint,"Чертёж","PLAN","Изучите, чтобы открыть новый рецепт.",1,1,new Color(.63f,.76f,.67f)),
            new ItemDefinition(ItemId.WorkbenchI,"Чертёж верстака I","PLAN","Открывает рецепты начального верстака.",1,1,new Color(.72f,.64f,.41f)),
            new ItemDefinition(ItemId.WorkbenchII,"Чертёж верстака II","PLAN","Открывает продвинутые рецепты.",1,2,new Color(.64f,.72f,.68f)),
            new ItemDefinition(ItemId.FieldJacket,"Поношенная куртка","Одежда","Надёжная ткань защищает от холода.",1,0,new Color(.43f,.47f,.34f),GearSlot.Chest,4,8,22,0),
            new ItemDefinition(ItemId.FieldPants,"Полевые штаны","Одежда","Старые штаны с усиленными коленями.",1,0,new Color(.39f,.42f,.35f),GearSlot.Legs,3,5,16,0),
            new ItemDefinition(ItemId.WorkBoots,"Рабочие ботинки","Одежда","Кожаные ботинки для долгих переходов.",1,0,new Color(.47f,.32f,.22f),GearSlot.Feet,2,7,18,0),
            new ItemDefinition(ItemId.Backpack,"Полевой рюкзак","Одежда","Потёртый, но вместительный рюкзак.",1,0,new Color(.39f,.36f,.27f),GearSlot.Back,1,3,8,0),
            new ItemDefinition(ItemId.Helmet,"Каска с фонарём","Броня","Стальная каска со старым налобным фонарём.",1,2,new Color(.62f,.57f,.34f),GearSlot.Head,24,14,30,8),
            new ItemDefinition(ItemId.ArmorVest,"Самодельный бронежилет","Броня","Пластины и плотная ткань снижают урон.",1,3,new Color(.43f,.40f,.32f),GearSlot.Chest,42,24,12,0),
            new ItemDefinition(ItemId.Medkit,"Аптечка","MED","Полный набор первой помощи.",2,2,new Color(.74f,.49f,.38f)),
            new ItemDefinition(ItemId.Keycard,"Зелёная карта","KEY","Карта-пропуск: активирует лифт на следующий уровень.",1,1,new Color(.38f,.72f,.43f)),
            new ItemDefinition(ItemId.Rifle,"Карабин","WEAPON","Старый карабин. Редкая находка.",1,3,new Color(.49f,.48f,.39f)),
            new ItemDefinition(ItemId.RifleAmmo,"Патроны 5.56","AMMO","Боеприпас для карабина.",40,3,new Color(.74f,.61f,.32f)),
            new ItemDefinition(ItemId.HazmatSuit,"Защитный костюм","Броня","Защита от холода и радиоактивной пыли.",1,3,new Color(.71f,.68f,.27f),GearSlot.Chest,18,20,38,55),
            new ItemDefinition(ItemId.CircuitBoard,"Плата управления","TECH","Сложная электроника из закрытой зоны.",20,2,new Color(.35f,.66f,.48f)),
            new ItemDefinition(ItemId.BuildingPlan,"Чертёж строительства","TOOL","Выбери секцию и поставь каркас из веток. R/колесо — секция, T — этаж, ЛКМ — установка.",1,0,new Color(.47f,.72f,.64f)),
            new ItemDefinition(ItemId.Hammer,"Строительный молоток","TOOL","ЛКМ чинит повреждённую или улучшает целую секцию, ПКМ разбирает постройку.",1,0,new Color(.68f,.49f,.29f)),
            new ItemDefinition(ItemId.Stone,"Камень","MAT","Каменные обломки для укрепления стен.",100,1,new Color(.57f,.58f,.52f)),
            new ItemDefinition(ItemId.HighQualityMetal,"Металл высокого качества","MAT","Редкий сплав для бронированных секций.",100,3,new Color(.55f,.71f,.72f)),
            new ItemDefinition(ItemId.StorageBox,"Домашний ящик","BUILD","Пустое личное хранилище. Ставится отдельно от мировых лут-кейсов.",1,0,new Color(.57f,.40f,.25f)),
            new ItemDefinition(ItemId.WorkbenchStationI,"Верстак I","BUILD","Размести рядом с собой, чтобы открыть базовые рецепты.",1,1,new Color(.63f,.52f,.34f)),
            new ItemDefinition(ItemId.WorkbenchStationII,"Верстак II","BUILD","Продвинутый верстак. Требует изученный чертёж и верстак I рядом.",1,2,new Color(.48f,.62f,.58f)),
            new ItemDefinition(ItemId.ToolCupboard,"Шкаф строительных прав","BUILD","Задаёт личную зону привилегии и оплачивает upkeep ресурсами из своего запаса.",1,1,new Color(.42f,.54f,.49f))
        };

        public static ItemDefinition Get(ItemId id)
        {
            for(int i=0;i<definitions.Length;i++)if(definitions[i].id==id)return definitions[i];
            return definitions[0];
        }
    }

    public sealed class InventorySystem : MonoBehaviour
    {
        public const int BackpackSize=30, BeltSize=6, GearSize=5;
        public ItemStack[] Backpack { get; }=new ItemStack[BackpackSize];
        public ItemStack[] Belt { get; }=new ItemStack[BeltSize];
        public ItemStack[] Gear { get; }=new ItemStack[GearSize];
        public LootContainer OpenContainer { get; private set; }
        public int SelectedBeltSlot { get; private set; }
        public int WorkbenchTier { get; set; }
        public bool InventoryOpen { get; set; }
        public CharacterVisual2D Character { get; set; }
        public float ArmorBullet { get; private set; }
        public float ArmorMelee { get; private set; }
        public float ColdProtection { get; private set; }
        public float RadiationProtection { get; private set; }
        public string LastCraftStatus { get; private set; }="";
        float statusTimer,stationScan;
        int observedWorkbench=-1;
        bool blueprintKnown,workbenchIKnown,workbenchIIKnown;
        public bool HasBlueprint(ItemId id)=>id==ItemId.Blueprint?blueprintKnown:id==ItemId.WorkbenchI?workbenchIKnown:id==ItemId.WorkbenchII?workbenchIIKnown:false;
        public bool IsLootOpen => OpenContainer!=null;

        void Update()
        {
            if(statusTimer>0)statusTimer-=Time.deltaTime;
            stationScan-=Time.deltaTime;
            if(stationScan<=0)
            {
                stationScan=.25f;int tier=0;float best=3.0f;
                foreach(var station in FindObjectsOfType<WorkbenchStation>())
                {float d=Vector2.Distance(transform.position,station.transform.position);if(d<best){best=d;tier=station.Tier;}}
                WorkbenchTier=tier;
                if(tier!=observedWorkbench){observedWorkbench=tier;if(tier>0)Notify("Верстак уровня "+tier+" доступен.");}
            }
        }
        public void Notify(string s){LastCraftStatus=s;statusTimer=2.7f;}
        public bool ShowNotice=>statusTimer>0;

        public void BeginRun()
        {
            Array.Clear(Backpack,0,Backpack.Length);Array.Clear(Belt,0,Belt.Length);Array.Clear(Gear,0,Gear.Length);
            OpenContainer=null;InventoryOpen=false;WorkbenchTier=0;SelectedBeltSlot=0;blueprintKnown=false;workbenchIKnown=false;workbenchIIKnown=false;
            Gear[(int)GearSlot.Chest]=new ItemStack(ItemId.FieldJacket,1);
            Gear[(int)GearSlot.Legs]=new ItemStack(ItemId.FieldPants,1);
            Gear[(int)GearSlot.Feet]=new ItemStack(ItemId.WorkBoots,1);
            Gear[(int)GearSlot.Back]=new ItemStack(ItemId.Backpack,1);
            Belt[0]=new ItemStack(ItemId.Flashlight,1);
            Backpack[0]=new ItemStack(ItemId.Water,2);
            Backpack[1]=new ItemStack(ItemId.CannedFood,2);
            Backpack[2]=new ItemStack(ItemId.Bandage,1);
            RecalculateArmor();Character?.SetGear(Gear);
        }

        public void SetOpenContainer(LootContainer container){OpenContainer=container;InventoryOpen=true;}
        public void CloseContainer(){OpenContainer=null;InventoryOpen=false;}
        public void SelectBelt(int index){if(index>=0&&index<BeltSize)SelectedBeltSlot=index;}
        public ItemStack ActiveItem=>Belt[SelectedBeltSlot];

        public bool Add(ItemId id,int amount)
        {
            if(id==ItemId.None||amount<=0)return false;
            if(!CanFitCombined(id,amount)){Notify("Недостаточно места в рюкзаке.");return false;}
            int remainder=InsertInto(Backpack,id,amount);if(remainder>0)remainder=InsertInto(Belt,id,remainder);
            AudioDirector.Instance?.Play("metal",.22f);RecalculateArmor();return remainder==0;
        }
        public bool TrySpend(ItemId id,int amount)
        {
            if(amount<=0)return true;
            if(Count(id)<amount)
            {
                string message="Недостаточно ресурса: "+ItemCatalog.Get(id).name+" ×"+amount+".";
                Notify(message);RunState.Instance?.Notify(message);return false;
            }
            Consume(id,amount);return true;
        }
        public bool TradeForGreenCard(int scrapCost)
        {
            if(Count(ItemId.Keycard)>0){Notify("Зелёная карта уже у тебя.");return false;}
            if(Count(ItemId.Scrap)<scrapCost){Notify("Торговец просит "+scrapCost+" металлолома за зелёную карту.");return false;}
            if(!CanFitCombined(ItemId.Keycard,1)){Notify("Освободи место в рюкзаке для зелёной карты.");return false;}
            Consume(ItemId.Scrap,scrapCost);
            if(!Add(ItemId.Keycard,1)){Add(ItemId.Scrap,scrapCost);return false;}
            Notify("Обмен завершён: зелёная карта получена. Лифты открыты.");
            return true;
        }
        static bool CanFit(ItemStack[] slots,ItemId id,int amount)
        {
            int max=ItemCatalog.Get(id).maxStack,capacity=0;
            for(int i=0;i<slots.Length;i++)capacity+=slots[i].Empty?max:slots[i].id==id?Mathf.Max(0,max-slots[i].count):0;
            return capacity>=amount;
        }
        bool CanFitCombined(ItemId id,int amount)
        {
            int max=ItemCatalog.Get(id).maxStack,capacity=0;
            foreach(var s in Backpack)capacity+=s.Empty?max:s.id==id?Mathf.Max(0,max-s.count):0;
            foreach(var s in Belt)capacity+=s.Empty?max:s.id==id?Mathf.Max(0,max-s.count):0;
            return capacity>=amount;
        }

        static int InsertInto(ItemStack[] slots,ItemId id,int amount)
        {
            int max=ItemCatalog.Get(id).maxStack;
            for(int i=0;i<slots.Length&&amount>0;i++)if(slots[i].id==id&&slots[i].count<max)
            {int put=Math.Min(max-slots[i].count,amount);slots[i].count+=put;amount-=put;}
            for(int i=0;i<slots.Length&&amount>0;i++)if(slots[i].Empty)
            {int put=Math.Min(max,amount);slots[i]=new ItemStack(id,put);amount-=put;}
            return amount;
        }

        public bool MoveStack(ItemStack[] from,int fromIndex,ItemStack[] to,int toIndex,int amount=0)
        {
            if(from==null||to==null||fromIndex<0||fromIndex>=from.Length||toIndex<0||toIndex>=to.Length||from[fromIndex].Empty)return false;
            ItemStack a=from[fromIndex],b=to[toIndex];
            if(amount>0&&amount<a.count)
            {
                if(b.Empty){to[toIndex]=new ItemStack(a.id,amount);a.count-=amount;from[fromIndex]=a;return true;}
                if(b.id==a.id){int put=Math.Min(amount,ItemCatalog.Get(a.id).maxStack-b.count);if(put<=0)return false;b.count+=put;a.count-=put;to[toIndex]=b;from[fromIndex]=a;return true;}
                return false;
            }
            if(b.Empty){to[toIndex]=a;from[fromIndex].Clear();return true;}
            if(a.id==b.id&&b.count<ItemCatalog.Get(a.id).maxStack)
            {int put=Math.Min(a.count,ItemCatalog.Get(a.id).maxStack-b.count);b.count+=put;a.count-=put;to[toIndex]=b;from[fromIndex]=a;if(a.count<=0)from[fromIndex].Clear();return true;}
            to[toIndex]=a;from[fromIndex]=b;return true;
        }

        public bool QuickMoveBackpack(int index)
        {
            if(index<0||index>=Backpack.Length||Backpack[index].Empty)return false;
            if(OpenContainer!=null)return TransferStack(Backpack,index,OpenContainer.Items);
            return TransferStack(Backpack,index,Belt);
        }
        public bool QuickMoveBelt(int index)
        {
            if(index<0||index>=Belt.Length||Belt[index].Empty)return false;
            return TransferStack(Belt,index,Backpack);
        }
        public bool QuickMoveContainer(int index)
        {
            if(OpenContainer==null||index<0||index>=OpenContainer.Items.Length)return false;
            return TransferStack(OpenContainer.Items,index,Backpack);
        }
        bool TransferStack(ItemStack[] from,int i,ItemStack[] to)
        {
            if(from[i].Empty)return false;
            ItemStack source=from[i];int remainder=InsertInto(to,source.id,source.count);int moved=source.count-remainder;
            if(moved<=0)return false;source.count=remainder;if(remainder<=0)source.Clear();from[i]=source;return true;
        }

        public bool EquipFromBackpack(int index)
        {
            if(index<0||index>=Backpack.Length||Backpack[index].Empty)return false;
            ItemDefinition d=ItemCatalog.Get(Backpack[index].id);if(!d.gearSlot.HasValue){Notify("Этот предмет нельзя надеть.");return false;}
            int slot=(int)d.gearSlot.Value;ItemStack old=Gear[slot];
            Gear[slot]=Backpack[index];Backpack[index]=old;
            RecalculateArmor();Character?.SetGear(Gear);AudioDirector.Instance?.Play("ui",.5f);return true;
        }
        public bool Unequip(int slot)
        {
            if(slot<0||slot>=Gear.Length||Gear[slot].Empty)return false;
            ItemStack item=Gear[slot];if(!CanFit(Backpack,item.id,item.count)){Notify("В рюкзаке нет свободной ячейки.");return false;}
            InsertInto(Backpack,item.id,item.count);Gear[slot].Clear();RecalculateArmor();Character?.SetGear(Gear);return true;
        }
        public void RefreshGear(){RecalculateArmor();Character?.SetGear(Gear);}
        void RecalculateArmor()
        {
            ArmorBullet=ArmorMelee=ColdProtection=RadiationProtection=0;
            foreach(ItemStack stack in Gear)if(!stack.Empty){var d=ItemCatalog.Get(stack.id);ArmorBullet+=d.bullet;ArmorMelee+=d.melee;ColdProtection+=d.cold;RadiationProtection+=d.radiation;}
        }

        public bool UseBackpack(int index)=>UseFrom(Backpack,index);
        public bool UseBelt(int index)=>UseFrom(Belt,index);
        public bool UseQuickMed()
        {
            int active=SelectedBeltSlot;
            if(Belt[active].id==ItemId.Bandage||Belt[active].id==ItemId.Medkit)return UseBelt(active);
            for(int i=0;i<Belt.Length;i++)if(Belt[i].id==ItemId.Medkit||Belt[i].id==ItemId.Bandage)return UseBelt(i);
            for(int i=0;i<Backpack.Length;i++)if(Backpack[i].id==ItemId.Medkit||Backpack[i].id==ItemId.Bandage)return UseBackpack(i);
            Notify("Нет бинтов или аптечек.");return false;
        }
        public bool StudyBlueprint(ItemId id)
        {
            if(id==ItemId.Blueprint){if(blueprintKnown){Notify("Этот чертёж уже изучен.");return false;}blueprintKnown=true;Notify("Чертёж изучен · открыт рецепт аптечки.");return true;}
            if(id==ItemId.WorkbenchI){if(workbenchIKnown){Notify("Чертёж верстака I уже изучен.");return false;}workbenchIKnown=true;Notify("Изучен чертёж верстака I · открыты базовые рецепты.");return true;}
            if(id==ItemId.WorkbenchII){if(workbenchIIKnown){Notify("Чертёж верстака II уже изучен.");return false;}workbenchIIKnown=true;Notify("Изучен чертёж верстака II · открыты продвинутые рецепты.");return true;}
            return false;
        }
        bool UseFrom(ItemStack[] slots,int index)
        {
            if(index<0||index>=slots.Length||slots[index].Empty)return false;
            ItemId id=slots[index].id;var state=RunState.Instance;if(state==null)return false;
            if(id==ItemId.Blueprint||id==ItemId.WorkbenchI||id==ItemId.WorkbenchII)
            {if(!StudyBlueprint(id))return false;ConsumeOne(slots,index);return true;}
            if(id==ItemId.Water){state.Drink(38);ConsumeOne(slots,index);Notify("Ты выпил воду · жажда восстановлена.");return true;}
            if(id==ItemId.CannedFood){state.Eat(32);ConsumeOne(slots,index);Notify("Ты съел консервы · голод отступил.");return true;}
            if(id==ItemId.Bandage){state.Heal(24);ConsumeOne(slots,index);Notify("Перевязка завершена · +24 здоровья.");return true;}
            if(id==ItemId.Medkit){state.Heal(62);ConsumeOne(slots,index);Notify("Аптечка использована · +62 здоровья.");return true;}
            Notify("Этот предмет нельзя использовать прямо сейчас.");return false;
        }
        static void ConsumeOne(ItemStack[] slots,int index){slots[index].count--;if(slots[index].count<=0)slots[index].Clear();}
        public bool ConsumeAmmo(ItemId id,int amount){if(Count(id)<amount)return false;Consume(id,amount);return true;}

        public void DropBackpack(int index)
        {
            if(index<0||index>=Backpack.Length||Backpack[index].Empty)return;
            var s=Backpack[index];Backpack[index].Clear();WorldItem.Spawn(s.id,s.count,transform.position+Vector3.right*1.1f);Notify("Предмет выброшен.");
        }

        public bool CraftBuildingPlan()
        {
            if(Count(ItemId.Wood)<20){Notify("Для чертежа строительства нужны 20 досок.");return false;}
            if(!CanFitCombined(ItemId.BuildingPlan,1)){Notify("Освободи место для чертежа строительства.");return false;}
            Consume(ItemId.Wood,20);Add(ItemId.BuildingPlan,1);Notify("Чертёж строительства готов. Выбери его на поясе.");return true;
        }
        public bool CraftHammer()
        {
            if(Count(ItemId.Wood)<100){Notify("Для строительного молотка нужны 100 досок.");return false;}
            if(!CanFitCombined(ItemId.Hammer,1)){Notify("Освободи место для строительного молотка.");return false;}
            Consume(ItemId.Wood,100);Add(ItemId.Hammer,1);Notify("Строительный молоток изготовлен.");return true;
        }
        public bool CraftStorageBox()
        {
            if(Count(ItemId.Wood)<40){Notify("Для домашнего ящика нужны 40 досок.");return false;}
            if(!CanFitCombined(ItemId.StorageBox,1)){Notify("Освободи место для домашнего ящика.");return false;}
            Consume(ItemId.Wood,40);Add(ItemId.StorageBox,1);Notify("Домашний ящик собран. Выбери его на поясе и поставь.");return true;
        }
        public bool CraftWorkbenchStationI()
        {
            if(Count(ItemId.Wood)<80||Count(ItemId.MetalFragments)<5){Notify("Для верстака I нужны 80 досок и 5 фрагментов металла.");return false;}
            if(!CanFitCombined(ItemId.WorkbenchStationI,1)){Notify("Освободи место для верстака I.");return false;}
            Consume(ItemId.Wood,80);Consume(ItemId.MetalFragments,5);Add(ItemId.WorkbenchStationI,1);Notify("Верстак I собран. Размести его, чтобы открыть базовый крафт.");return true;
        }
        public bool CraftWorkbenchStationII()
        {
            if(!workbenchIIKnown){Notify("Нужен изученный чертёж верстака II.");return false;}
            if(WorkbenchTier<1){Notify("Для сборки верстака II нужен верстак I поблизости.");return false;}
            if(Count(ItemId.Wood)<200||Count(ItemId.MetalFragments)<25){Notify("Для верстака II нужны 200 досок и 25 фрагментов металла.");return false;}
            if(!CanFitCombined(ItemId.WorkbenchStationII,1)){Notify("Освободи место для верстака II.");return false;}
            Consume(ItemId.Wood,200);Consume(ItemId.MetalFragments,25);Add(ItemId.WorkbenchStationII,1);Notify("Верстак II собран. Размести его рядом с базой.");return true;
        }
        public bool CraftToolCupboard()
        {
            if(Count(ItemId.Wood)<100||Count(ItemId.MetalFragments)<10){Notify("Для шкафа строительных прав нужны 100 досок и 10 фрагментов металла.");return false;}
            if(!CanFitCombined(ItemId.ToolCupboard,1)){Notify("Освободи место для шкафа строительных прав.");return false;}
            Consume(ItemId.Wood,100);Consume(ItemId.MetalFragments,10);Add(ItemId.ToolCupboard,1);
            Notify("Шкаф строительных прав собран. Размести его, затем пополни upkeep-ресурсы.");return true;
        }
        public bool CraftBandage()
        {
            if(Count(ItemId.Cloth)<2){Notify("Для бинта нужны 2 единицы ткани.");return false;}
            if(!CanFitCombined(ItemId.Bandage,1)){Notify("Нет места для бинта.");return false;}
            Consume(ItemId.Cloth,2);Add(ItemId.Bandage,1);Notify("Сделан бинт. Используй его из инвентаря.");return true;
        }
        public bool CraftPipe()
        {
            if(!workbenchIKnown){Notify("Нужен изученный чертёж верстака I.");return false;}
            if(WorkbenchTier<1){Notify("Подойди к верстаку I, чтобы изготовить трубу.");return false;}
            if(Count(ItemId.MetalFragments)<12||Count(ItemId.Cloth)<2){Notify("Нужно 12 металла и 2 ткани.");return false;}
            if(!CanFitCombined(ItemId.Pipe,1)){Notify("Нет места для трубы.");return false;}
            Consume(ItemId.MetalFragments,12);Consume(ItemId.Cloth,2);Add(ItemId.Pipe,1);Notify("Стальная труба изготовлена.");return true;
        }
        public bool CraftAmmo()
        {
            if(!workbenchIKnown){Notify("Нужен изученный чертёж верстака I.");return false;}
            if(WorkbenchTier<1||Count(ItemId.MetalFragments)<5){Notify("Нужны верстак I и 5 фрагментов металла.");return false;}
            if(!CanFitCombined(ItemId.PistolAmmo,8)){Notify("Нет места для патронов.");return false;}
            Consume(ItemId.MetalFragments,5);Add(ItemId.PistolAmmo,8);Notify("Собрано 8 патронов.");return true;
        }
        public bool CraftMedkit()
        {
            if(!blueprintKnown||!workbenchIIKnown){Notify("Нужны чертёж аптечки и чертёж верстака II.");return false;}
            if(WorkbenchTier<2){Notify("Подойди к верстаку II, чтобы собрать аптечку.");return false;}
            if(Count(ItemId.Cloth)<8||Count(ItemId.CircuitBoard)<2){Notify("Нужно 8 ткани и 2 платы управления.");return false;}
            if(!CanFitCombined(ItemId.Medkit,1)){Notify("Нет места для аптечки.");return false;}
            Consume(ItemId.Cloth,8);Consume(ItemId.CircuitBoard,2);Add(ItemId.Medkit,1);Notify("Аптечка изготовлена.");return true;
        }
        public int Count(ItemId id){int total=0;foreach(var s in Backpack)if(s.id==id)total+=s.count;foreach(var s in Belt)if(s.id==id)total+=s.count;foreach(var s in Gear)if(s.id==id)total+=s.count;return total;}
        void Consume(ItemId id,int amount){for(int i=0;i<Backpack.Length&&amount>0;i++)if(Backpack[i].id==id){int n=Math.Min(amount,Backpack[i].count);Backpack[i].count-=n;amount-=n;if(Backpack[i].count<=0)Backpack[i].Clear();}for(int i=0;i<Belt.Length&&amount>0;i++)if(Belt[i].id==id){int n=Math.Min(amount,Belt[i].count);Belt[i].count-=n;amount-=n;if(Belt[i].count<=0)Belt[i].Clear();}}
    }
}
