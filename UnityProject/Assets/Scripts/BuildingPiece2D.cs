using UnityEngine;

namespace Subsistence
{
    public enum BuildPart2D { Foundation, Wall, Doorway, Floor }
    public enum BuildingGrade2D { Twig, Wood, Stone, Metal, Armored }

    /// <summary>A single player-placed, grid-snapped Rust-style building segment.</summary>
    [DisallowMultipleComponent]
    public sealed class BuildingPiece2D : MonoBehaviour
    {
        public BuildPart2D Part { get; private set; }
        public BuildingGrade2D Grade { get; private set; }
        public int GridX { get; private set; }
        public int Level { get; private set; }
        public float Condition { get; private set; }
        public float MaxCondition=>100f+(int)Grade*25f;
        float maintenanceDeadline;
        bool decayWarned;

        public void Initialize(BuildPart2D part,int gridX,int level,BuildingGrade2D grade)
        {
            Part=part;GridX=gridX;Level=level;Grade=grade;Condition=MaxCondition;
            maintenanceDeadline=Time.time+ToolCupboard2D.InitialGrace;
            name="Player Building · "+PartName(part)+" · "+GradeName(grade);
            var renderer=GetComponent<SpriteRenderer>();if(renderer==null)renderer=gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=BuildingArt2D.Get(part,grade);renderer.sortingOrder=6;RendererLibrary2D.Configure(renderer);
            if(part==BuildPart2D.Doorway)CreateDoorwayColliders();
            else
            {
                var collider=GetComponent<BoxCollider2D>();if(collider==null)collider=gameObject.AddComponent<BoxCollider2D>();
                collider.size=new Vector2(1.96f,PartHeight(part));collider.offset=Vector2.zero;
            }
        }

        void Update()
        {
            if(!GameHUD.IsPlaying||RunState.Instance==null||RunState.Instance.IsDead||Condition<=0||Time.time<=maintenanceDeadline)return;
            if(!decayWarned)
            {
                decayWarned=true;RunState.Instance.Notify("Постройка теряет прочность. Пополни шкаф строительных прав или почини её молотком.",4f);
            }
            Condition=Mathf.Max(0,Condition-Time.deltaTime*.35f);
            if(Condition<=0)
            {
                RunState.Instance.Notify(PartName(Part)+" разрушилась из-за отсутствия обслуживания.",4f);
                BuildingSystem2D.CollapseUnsupportedFrom(this);Destroy(gameObject);
            }
        }

        public void MarkUpkeepPaid(float deadline)
        {
            maintenanceDeadline=Mathf.Max(maintenanceDeadline,deadline);decayWarned=false;
        }

        bool Repair(InventorySystem inventory)
        {
            ItemId resource=UpkeepResource(Grade);
            if(!inventory.TrySpend(resource,1))return false;
            Condition=Mathf.Min(MaxCondition,Condition+25f);maintenanceDeadline=Time.time+ToolCupboard2D.InitialGrace;decayWarned=false;
            RunState.Instance?.Notify(PartName(Part)+" починена молотком · "+Mathf.CeilToInt(Condition)+"/"+Mathf.CeilToInt(MaxCondition)+".");
            AudioDirector.Instance?.Play("metal",.42f);return true;
        }

        public static ItemId UpkeepResource(BuildingGrade2D grade)
        {
            switch(grade)
            {
                case BuildingGrade2D.Twig:case BuildingGrade2D.Wood:return ItemId.Wood;
                case BuildingGrade2D.Stone:return ItemId.Stone;
                case BuildingGrade2D.Metal:return ItemId.MetalFragments;
                default:return ItemId.HighQualityMetal;
            }
        }

        void CreateDoorwayColliders()
        {
            AddDoorCollider("Left frame",new Vector2(-.77f,0),new Vector2(.42f,2.4f));
            AddDoorCollider("Right frame",new Vector2(.77f,0),new Vector2(.42f,2.4f));
            AddDoorCollider("Header",new Vector2(0,.99f),new Vector2(1.96f,.42f));
        }

        void AddDoorCollider(string childName,Vector2 offset,Vector2 size)
        {
            var child=new GameObject(childName);child.transform.SetParent(transform,false);child.transform.localPosition=offset;
            var collider=child.AddComponent<BoxCollider2D>();collider.size=size;
        }

        public bool HitTest(Vector2 point)
        {
            Vector2 center=transform.position;
            return Mathf.Abs(point.x-center.x)<=1.02f&&Mathf.Abs(point.y-center.y)<=PartHeight(Part)*.5f+.08f;
        }

        public bool TryUpgrade(InventorySystem inventory)
        {
            if(Condition<MaxCondition-.01f)return Repair(inventory);
            if((int)Grade>=(int)BuildingGrade2D.Armored){RunState.Instance?.Notify("Эта секция уже бронирована.");return false;}
            ItemId costItem;int amount;
            switch(Grade)
            {
                case BuildingGrade2D.Twig:costItem=ItemId.Wood;amount=50;break;
                case BuildingGrade2D.Wood:costItem=ItemId.Stone;amount=100;break;
                case BuildingGrade2D.Stone:costItem=ItemId.MetalFragments;amount=100;break;
                default:costItem=ItemId.HighQualityMetal;amount=25;break;
            }
            if(!inventory.TrySpend(costItem,amount))return false;
            Grade=(BuildingGrade2D)((int)Grade+1);Condition=MaxCondition;maintenanceDeadline=Time.time+ToolCupboard2D.InitialGrace;decayWarned=false;
            var renderer=GetComponent<SpriteRenderer>();if(renderer!=null){renderer.sprite=BuildingArt2D.Get(Part,Grade);RendererLibrary2D.Configure(renderer);}
            RunState.Instance?.Notify(PartName(Part)+" улучшена: "+GradeName(Grade)+".");
            AudioDirector.Instance?.Play("metal",.55f);return true;
        }

        public static float PartHeight(BuildPart2D part)
        {
            switch(part)
            {
                case BuildPart2D.Foundation:return .46f;
                case BuildPart2D.Floor:return .32f;
                default:return 2.4f;
            }
        }

        public static string PartName(BuildPart2D part)
        {
            switch(part)
            {
                case BuildPart2D.Foundation:return "ФУНДАМЕНТ";
                case BuildPart2D.Wall:return "СТЕНА";
                case BuildPart2D.Doorway:return "ДВЕРНОЙ ПРОЁМ";
                default:return "ПЕРЕКРЫТИЕ / КРЫША";
            }
        }

        public static string GradeName(BuildingGrade2D grade)
        {
            switch(grade)
            {
                case BuildingGrade2D.Twig:return "ветки";
                case BuildingGrade2D.Wood:return "дерево";
                case BuildingGrade2D.Stone:return "камень";
                case BuildingGrade2D.Metal:return "металл";
                default:return "броня";
            }
        }
    }

}
