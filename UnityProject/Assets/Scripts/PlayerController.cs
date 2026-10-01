using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Subsistence
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Motion")]
        public float MoveSpeed=5.4f;
        public float JumpVelocity=8.8f;
        public float Acceleration=34f;
        public float Deceleration=48f;
        public float AirControl=.55f;
        public float CoyoteTime=.08f;
        public float JumpBuffer=.09f;

        Rigidbody2D body;
        InventorySystem inventory;
        CharacterVisual2D character;
        SpriteRenderer beamRenderer,handFlashlightRenderer;
        bool grounded,flashlightOn=true,insideElevator;
        float invulnerable,attackCooldown,footstepClock,horizontalInput,elevatorMinX,elevatorMaxX;
        float coyoteClock,jumpBufferClock;
        int facing=1;
        Vector2 flashlightOffset=new Vector2(.31f,.98f);
        #if UNITY_2019_3_OR_NEWER
        Light2D flashlightLight;
        #endif
        Transform flashlightPivot;

        public bool FlashlightOn => flashlightOn && inventory != null && inventory.Count(ItemId.Flashlight) > 0;
        public InventorySystem Inventory => inventory;
        public CharacterVisual2D Character => character;

        void Awake()
        {
            body=GetComponent<Rigidbody2D>();
            body.gravityScale=3.1f;
            body.freezeRotation=true;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            character=GetComponent<CharacterVisual2D>();if(character==null)character=gameObject.AddComponent<CharacterVisual2D>();
            inventory=GetComponent<InventorySystem>();if(inventory==null)inventory=gameObject.AddComponent<InventorySystem>();
            inventory.Character=character;

            flashlightPivot=new GameObject("Flashlight rig").transform;
            flashlightPivot.SetParent(transform,false);
            flashlightPivot.localPosition=new Vector3(flashlightOffset.x,flashlightOffset.y,0);

            var beam=new GameObject("Visible flashlight cone");
            beam.transform.SetParent(flashlightPivot,false);
            beamRenderer=beam.AddComponent<SpriteRenderer>();beamRenderer.sprite=PixelArtFactory.FlashlightBeam();
            beamRenderer.sortingOrder=9;beamRenderer.color=new Color(1f,.94f,.74f,.78f);

            var heldLight=new GameObject("Handheld flashlight sprite");
            heldLight.transform.SetParent(flashlightPivot,false);
            handFlashlightRenderer=heldLight.AddComponent<SpriteRenderer>();handFlashlightRenderer.sprite=PixelArtFactory.HandFlashlight();
            handFlashlightRenderer.sortingOrder=11;
            handFlashlightRenderer.transform.localPosition=new Vector3(0f,-.38f,0);

            #if UNITY_2019_3_OR_NEWER
            var lightObject=new GameObject("Flashlight URP point light");
            lightObject.transform.SetParent(flashlightPivot,false);
            flashlightLight=lightObject.AddComponent<Light2D>();
            flashlightLight.lightType=Light2D.LightType.Point;
            flashlightLight.color=new Color(1f,.95f,.78f,1f);
            flashlightLight.intensity=1.75f;
            flashlightLight.pointLightInnerRadius=1.2f;
            flashlightLight.pointLightOuterRadius=6.2f;
            flashlightLight.pointLightInnerAngle=54f;
            flashlightLight.pointLightOuterAngle=88f;
            flashlightLight.shadowsEnabled=true;
            flashlightLight.shadowIntensity=.82f;
            flashlightLight.volumeIntensityEnabled=true;
            flashlightLight.volumeIntensity=.12f;
            #endif
        }

        void Start(){inventory.BeginRun();}

        void Update()
        {
            if(RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped){horizontalInput=0f;return;}
            horizontalInput=Input.GetAxisRaw("Horizontal");
            bool jumpPressed=Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow);
            if(jumpPressed)jumpBufferClock=JumpBuffer;
            if(!insideElevator&&jumpBufferClock>0f&&coyoteClock>0f&&grounded)
            {
                body.velocity=new Vector2(body.velocity.x,JumpVelocity);
                grounded=false;coyoteClock=0f;jumpBufferClock=0f;
            }
            if(Input.GetKeyDown(KeyCode.E))Interact();
            if(Input.GetKeyDown(KeyCode.Q)&&attackCooldown<=0)Attack();
            if(Input.GetKeyDown(KeyCode.H))inventory.UseQuickMed();
            if(Input.GetKeyDown(KeyCode.C))inventory.CraftBandage();
            if(Input.GetKeyDown(KeyCode.F))ToggleFlashlight();
            for(int i=0;i<6;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i)))inventory.SelectBelt(i);
            float wheel=Input.mouseScrollDelta.y;
            if(Mathf.Abs(wheel)>.01f)inventory.SelectBelt((inventory.SelectedBeltSlot+(wheel>0?5:1))%InventorySystem.BeltSize);
            if(invulnerable>0)invulnerable-=Time.deltaTime;
            if(attackCooldown>0)attackCooldown-=Time.deltaTime;
            if(coyoteClock>0)coyoteClock-=Time.deltaTime;
            if(jumpBufferClock>0)jumpBufferClock-=Time.deltaTime;

            float absInput=Mathf.Abs(horizontalInput);
            if(absInput>.05f){facing=horizontalInput<0?-1:1;footstepClock+=Time.deltaTime;if(footstepClock>.36f){footstepClock=0;AudioDirector.Instance?.Play("step",.27f);}}
            else footstepClock=0;
            if(character!=null)character.SetMotion(absInput,grounded,!grounded,facing,false);
            bool ownsFlashlight=inventory!=null&&inventory.Count(ItemId.Flashlight)>0;
            if(beamRenderer!=null){beamRenderer.flipX=facing<0;beamRenderer.enabled=FlashlightOn;}
            if(handFlashlightRenderer!=null){handFlashlightRenderer.flipX=facing<0;handFlashlightRenderer.enabled=ownsFlashlight;handFlashlightRenderer.color=FlashlightOn?Color.white:new Color(.55f,.58f,.53f,1f);}
            flashlightPivot.localPosition=new Vector3(facing*flashlightOffset.x,flashlightOffset.y,0);
            flashlightPivot.localRotation=Quaternion.Euler(0f,0f,facing<0?4f:-4f);
            #if UNITY_2019_3_OR_NEWER
            if(flashlightLight!=null)
            {
                flashlightLight.enabled=FlashlightOn;
                flashlightLight.transform.localPosition=new Vector3(facing*.85f,0f,0f);
                flashlightLight.pointLightInnerAngle=48f;
                flashlightLight.pointLightOuterAngle=84f;
                flashlightLight.pointLightInnerRadius=FlashlightOn?0.6f:0f;
                flashlightLight.pointLightOuterRadius=FlashlightOn?6.6f+Mathf.Sin(Time.time*7.7f)*.08f:0f;
                flashlightLight.intensity=FlashlightOn?1.78f+Mathf.Sin(Time.time*8.3f)*.06f:0f;
            }
            #endif
        }

        void FixedUpdate()
        {
            if(RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped){body.velocity=new Vector2(0,body.velocity.y);return;}
            if(insideElevator)
            {
                float nextX=body.position.x+horizontalInput*MoveSpeed*.45f*Time.fixedDeltaTime;
                body.velocity=Vector2.zero;
                body.MovePosition(new Vector2(Mathf.Clamp(nextX,elevatorMinX,elevatorMaxX),body.position.y));
                return;
            }
            Vector2 velocity=body.velocity;
            float control=grounded?1f:AirControl;
            float desiredHorizontal=horizontalInput*MoveSpeed;
            float acceleration=Mathf.Abs(horizontalInput)>.05f?Acceleration:Deceleration;
            velocity.x=Mathf.MoveTowards(velocity.x,desiredHorizontal,acceleration*control*Time.fixedDeltaTime);
            body.velocity=velocity;
            if(transform.position.x<-23f)body.position=new Vector2(-23f,body.position.y);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            for(int i=0;i<collision.contactCount;i++)if(collision.GetContact(i).normal.y>.55f){grounded=true;coyoteClock=CoyoteTime;}
        }
        void OnCollisionExit2D(Collision2D collision){grounded=false;}

        void Interact()
        {
            LootContainer crate=null;float best=1.75f;
            foreach(var candidate in FindObjectsOfType<LootContainer>()){float d=Vector2.Distance(transform.position,candidate.transform.position);if(d<best){best=d;crate=candidate;}}
            if(crate!=null){crate.Open(inventory);return;}
            WorldItem worldItem=null;best=1.55f;
            foreach(var item in FindObjectsOfType<WorldItem>()){float d=Vector2.Distance(transform.position,item.transform.position);if(d<best){best=d;worldItem=item;}}
            if(worldItem!=null){worldItem.Collect(inventory);return;}
            SupplyPickup pickup=null;best=1.55f;
            foreach(var p in FindObjectsOfType<SupplyPickup>()){float d=Vector2.Distance(transform.position,p.transform.position);if(!p.Collected&&d<best){best=d;pickup=p;}}
            if(pickup!=null){pickup.Collect();return;}
            TraderNPC trader=null;best=1.9f;
            foreach(var candidate in FindObjectsOfType<TraderNPC>()){float d=Vector2.Distance(transform.position,candidate.transform.position);if(d<best){best=d;trader=candidate;}}
            if(trader!=null){trader.Interact(inventory);return;}
            LevelElevator elevator=null;best=2.15f;
            foreach(var candidate in FindObjectsOfType<LevelElevator>()){float d=Vector2.Distance(transform.position,candidate.transform.position);if(d<best){best=d;elevator=candidate;}}
            if(elevator!=null){elevator.TryUse(this);return;}
            RunState.Instance.Notify("Здесь больше ничего нет.",1.1f);
        }

        void Attack()
        {
            attackCooldown=.42f;character?.Attack();ItemId weapon=inventory.ActiveItem.id;bool ranged=weapon==ItemId.Pistol||weapon==ItemId.Rifle;
            if(ranged){ItemId ammo=weapon==ItemId.Pistol?ItemId.PistolAmmo:ItemId.RifleAmmo;if(!inventory.ConsumeAmmo(ammo,1)){RunState.Instance.Notify("Нет подходящих патронов.");return;}}
            else AudioDirector.Instance?.Play("swipe",.70f);
            WatcherAI nearest=null;float best=ranged?10f:2.2f;
            foreach(var candidate in FindObjectsOfType<WatcherAI>()){float d=Mathf.Abs(candidate.transform.position.x-transform.position.x);if(d<best){best=d;nearest=candidate;}}
            if(nearest!=null){nearest.Stun(weapon==ItemId.Pipe?2.1f:ranged?3.8f:1.1f);return;}
            RunState.Instance.Notify(ranged?"Выстрел растворился в коридоре.":"Удар не достиг цели.",.8f);
        }

        public void ReceiveHit(float damage)
        {
            if(invulnerable>0||RunState.Instance==null||!GameHUD.IsPlaying)return;invulnerable=.78f;
            float reduction=Mathf.Clamp(inventory.ArmorMelee/100f,0,.72f);RunState.Instance.TakeDamage(damage*(1-reduction));
            AudioDirector.Instance?.Play("hit",.8f);RunState.Instance.Notify(reduction>0?"Броня смягчила удар. Q — отбиться.":"Оно тебя зацепило — оттолкни его клавишей Q.",1.3f);
        }

        void ToggleFlashlight()
        {
            if(inventory==null||inventory.Count(ItemId.Flashlight)<=0){RunState.Instance?.Notify("Фонарь потерян — найди новый.",1.4f);return;}
            flashlightOn=!flashlightOn;
            if(beamRenderer!=null)beamRenderer.enabled=FlashlightOn;
            if(handFlashlightRenderer!=null)handFlashlightRenderer.color=FlashlightOn?Color.white:new Color(.55f,.58f,.53f,1f);
            #if UNITY_2019_3_OR_NEWER
            if(flashlightLight!=null)flashlightLight.enabled=FlashlightOn;
            #endif
            AudioDirector.Instance?.Play("ui",.45f);RunState.Instance.Notify(flashlightOn?"Фонарь включён.":"Фонарь выключен.",1.4f);
        }

        public void EnterElevator(float centerX,float halfWidth){insideElevator=true;elevatorMinX=centerX-halfWidth;elevatorMaxX=centerX+halfWidth;horizontalInput=0;body.velocity=Vector2.zero;body.position=new Vector2(centerX,0);transform.position=new Vector3(centerX,0,0);grounded=true;coyoteClock=0f;jumpBufferClock=0f;}
        public void ExitElevator(Vector2 destination){insideElevator=false;horizontalInput=0;body.velocity=Vector2.zero;body.position=destination;transform.position=new Vector3(destination.x,destination.y,0);grounded=true;}
        public void ResetForNewRun()=>ResetForNewRun(-19f);
        public void ResetForNewRun(float startX)
        {
            insideElevator=false;transform.position=new Vector3(startX,0,0);body.velocity=Vector2.zero;grounded=false;horizontalInput=0;invulnerable=0;attackCooldown=0;footstepClock=0;facing=1;flashlightOn=true;coyoteClock=0f;jumpBufferClock=0f;
            if(beamRenderer!=null){beamRenderer.enabled=true;beamRenderer.flipX=false;}
            #if UNITY_2019_3_OR_NEWER
            if(flashlightLight!=null)flashlightLight.enabled=true;
            #endif
            if(inventory!=null){inventory.BeginRun();character?.SetGear(inventory.Gear);}
        }
    }
}
