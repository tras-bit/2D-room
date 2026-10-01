using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Offline two-stage elevator ride: 15 seconds boarding, then a 60-second trip.</summary>
    [RequireComponent(typeof(SpriteRenderer),typeof(BoxCollider2D))]
    public sealed class LevelElevator : MonoBehaviour
    {
        public enum RidePhase { Idle, Boarding, Travelling }
        public const float BoardingDuration=15f;
        public const float TravelDuration=60f;
        const float CabinHalfWidth=.48f;

        readonly List<PlayerController> passengers=new List<PlayerController>();
        SpriteRenderer doorRenderer,indicatorRenderer;
        RidePhase phase;
        float remaining;
        float destinationX;
        int destinationLevel=1,elevatorNumber=1;
        public RidePhase Phase=>phase;
        public float SecondsRemaining=>Mathf.Max(0,remaining);
        public int ElevatorNumber=>elevatorNumber;

        public void Initialize(int number,int level,float arrivalX)
        {
            elevatorNumber=number;destinationLevel=level;destinationX=arrivalX;
            doorRenderer=GetComponent<SpriteRenderer>();doorRenderer.sprite=PixelArtFactory.ElevatorDoor(false);doorRenderer.sortingOrder=6;
            var area=GetComponent<BoxCollider2D>();area.isTrigger=true;area.size=new Vector2(1.75f,2.45f);area.offset=new Vector2(0,1.18f);
            var indicator=new GameObject("Lift call/status lamp "+number);indicator.transform.SetParent(transform,false);
            indicator.transform.localPosition=new Vector3(.48f,1.96f,-.01f);
            indicator.transform.localScale=new Vector3(.14f,.14f,1);
            indicatorRenderer=indicator.AddComponent<SpriteRenderer>();indicatorRenderer.sprite=PixelArtFactory.Block(Color.white);indicatorRenderer.sortingOrder=8;
            indicatorRenderer.color=new Color(.46f,.75f,.38f);
            UpdateDoor();
        }

        public void ResetForNewRun()
        {
            passengers.Clear();phase=RidePhase.Idle;remaining=0;UpdateDoor();
        }

        public bool TryUse(PlayerController player)
        {
            if(player==null||player.Inventory==null)return false;
            if(phase!=RidePhase.Idle)
            {
                if(!passengers.Contains(player))player.Inventory.Notify("Лифт уже закрывается или едет. Жди у дверей.");
                return false;
            }
            if(destinationLevel>(RunState.Instance!=null?RunState.Instance.CurrentLevel:0)&&player.Inventory.Count(ItemId.Keycard)<=0)
            {
                player.Inventory.Notify("Для лифта нужна зелёная карта. Купи её у торговца за 15 металлолома.");
                return false;
            }
            phase=RidePhase.Boarding;remaining=BoardingDuration;passengers.Clear();RegisterPassenger(player);
            RunState.Instance?.Notify("ЛИФТ "+elevatorNumber+" · ДВЕРИ ЗАКРОЮТСЯ ЧЕРЕЗ 15 СЕКУНД.",3f);
            UpdateDoor();return true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if(phase!=RidePhase.Boarding)return;
            var player=other.GetComponent<PlayerController>();
            if(player!=null)RegisterPassenger(player);
        }

        void RegisterPassenger(PlayerController player)
        {
            if(player==null||passengers.Contains(player))return;
            passengers.Add(player);
            player.EnterElevator(transform.position.x,CabinHalfWidth);
        }

        void Update()
        {
            if(phase==RidePhase.Idle)return;
            remaining-=Time.deltaTime;
            if(phase==RidePhase.Boarding&&remaining<=0)
            {
                if(passengers.Count==0){phase=RidePhase.Idle;UpdateDoor();return;}
                phase=RidePhase.Travelling;remaining=TravelDuration;
                RunState.Instance?.Notify("ДВЕРИ ЗАКРЫТЫ · ПЕРЕХОД НА УРОВЕНЬ "+destinationLevel+" · 60 СЕКУНД.",3f);
                UpdateDoor();
            }
            else if(phase==RidePhase.Travelling)
            {
                if(indicatorRenderer!=null)indicatorRenderer.color=Mathf.Repeat(Time.time*2f,1f)>.5f?new Color(.84f,.53f,.23f):new Color(.42f,.67f,.35f);
                if(remaining<=0)Arrive();
            }
        }

        void Arrive()
        {
            int count=passengers.Count;
            for(int i=0;i<count;i++)
            {
                PlayerController player=passengers[i];
                if(player!=null)player.ExitElevator(new Vector2(destinationX+i*.5f,0));
            }
            RunState.Instance?.EnterLevel(destinationLevel);
            passengers.Clear();phase=RidePhase.Idle;remaining=0;UpdateDoor();
        }

        void UpdateDoor()
        {
            if(doorRenderer!=null)doorRenderer.sprite=PixelArtFactory.ElevatorDoor(phase==RidePhase.Boarding);
            if(indicatorRenderer!=null&&phase!=RidePhase.Travelling)
                indicatorRenderer.color=phase==RidePhase.Boarding?new Color(.90f,.62f,.26f):new Color(.46f,.75f,.38f);
        }

        public string GetHint(PlayerController player)
        {
            if(phase==RidePhase.Boarding)return "ЛИФТ "+elevatorNumber+" · ЗАКРОЕТСЯ ЗА "+Mathf.CeilToInt(remaining)+" С · ХОДИ ВНУТРИ";
            if(phase==RidePhase.Travelling)return "ЛИФТ "+elevatorNumber+" · ПОЕЗДКА "+Mathf.CeilToInt(remaining)+" С · LEVEL "+destinationLevel;
            if(player!=null&&player.Inventory!=null&&player.Inventory.Count(ItemId.Keycard)>0)return "E  ·  ВОЙТИ В ЛИФТ "+elevatorNumber+"  /  УРОВЕНЬ "+destinationLevel;
            return "ЛИФТ "+elevatorNumber+"  ·  НУЖНА ЗЕЛЁНАЯ КАРТА ОТ ТОРГОВЦА";
        }
    }
}
