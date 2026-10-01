using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float MoveSpeed=5.1f;
        public float JumpVelocity=8.4f;
        Rigidbody2D body;
        InventorySystem inventory;
        CharacterVisual2D character;
        SpriteRenderer beamRenderer,handFlashlightRenderer;
        bool grounded,flashlightOn=true,insideElevator;
        float invulnerable,attackCooldown,footstepClock,horizontal,elevatorMinX,elevatorMaxX;
        int facing=1;
        public bool FlashlightOn=>flashlightOn&&inventory!=null&&inventory.Count(ItemId.Flashlight)>0;
        public InventorySystem Inventory=>inventory;
        public CharacterVisual2D Character=>character;

        void Awake()
        {
            body=GetComponent<Rigidbody2D>();body.gravityScale=3.1f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            character=GetComponent<CharacterVisual2D>();if(character==null)character=gameObject.AddComponent<CharacterVisual2D>();
            inventory=GetComponent<InventorySystem>();if(inventory==null)inventory=gameObject.AddComponent<InventorySystem>();
            inventory.Character=character;
            var beam=new GameObject("Visible flashlight cone");beam.transform.SetParent(transform,false);
            beamRenderer=beam.AddComponent<SpriteRenderer>();beamRenderer.sprite=PixelArtFactory.FlashlightBeam();beamRenderer.sortingOrder=9;beamRenderer.color=new Color(1f,.94f,.74f,.78f);
            beam.transform.localPosition=new Vector3(.34f,1.02f,0);beamRenderer.enabled=flashlightOn;
            var heldLight=new GameObject("Handheld pixel flashlight");heldLight.transform.SetParent(transform,false);
            handFlashlightRenderer=heldLight.AddComponent<SpriteRenderer>();handFlashlightRenderer.sprite=PixelArtFactory.HandFlashlight();handFlashlightRenderer.sortingOrder=11;
            heldLight.transform.localPosition=new Vector3(.30f,.58f,0);
        }
        void Start(){inventory.BeginRun();}

        void Update()
        {
            if(RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped)return;
            horizontal=Input.GetAxisRaw("Horizontal");
            if(!insideElevator&&(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow)))
                if(grounded){body.velocity=new Vector2(body.velocity.x,JumpVelocity);grounded=false;}
            if(Input.GetKeyDown(KeyCode.E))Interact();
            if(Input.GetKeyDown(KeyCode.Q)&&attackCooldown<=0)Attack();
            if(Input.GetKeyDown(KeyCode.H))inventory.UseQuickMed();
            if(Input.GetKeyDown(KeyCode.C))inventory.CraftBandage();
            if(Input.GetKeyDown(KeyCode.F))ToggleFlashlight();
            for(int i=0;i<6;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i)))inventory.SelectBelt(i);
            float wheel=Input.mouseScrollDelta.y;
            if(Mathf.Abs(wheel)>.01f)inventory.SelectBelt((inventory.SelectedBeltSlot+(wheel>0?5:1))%InventorySystem.BeltSize);
            if(invulnerable>0)invulnerable-=Time.deltaTime;if(attackCooldown>0)attackCooldown-=Time.deltaTime;
            if(Mathf.Abs(horizontal)>.05f)
            {
                facing=horizontal<0?-1:1;footstepClock+=Time.deltaTime;
                if(footstepClock>.38f){footstepClock=0;AudioDirector.Instance?.Play("step",.27f);}
            }
            else footstepClock=0;
            if(character!=null)character.SetMotion(Mathf.Abs(horizontal),grounded,!grounded,facing,false);
            bool ownsFlashlight=inventory!=null&&inventory.Count(ItemId.Flashlight)>0;
            if(beamRenderer!=null)
            {
                beamRenderer.flipX=facing<0;beamRenderer.enabled=FlashlightOn;
                beamRenderer.transform.localPosition=new Vector3(facing*.34f,1.02f,0);
            }
            if(handFlashlightRenderer!=null)
            {
                handFlashlightRenderer.flipX=facing<0;handFlashlightRenderer.enabled=ownsFlashlight;
                handFlashlightRenderer.color=FlashlightOn?Color.white:new Color(.55f,.58f,.53f,1f);
                handFlashlightRenderer.transform.localPosition=new Vector3(facing*.31f,.59f,0);
            }
        }
        void FixedUpdate()
        {
            if(RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped){body.velocity=new Vector2(0,body.velocity.y);return;}
            if(insideElevator)
            {
                float nextX=body.position.x+horizontal*MoveSpeed*Time.fixedDeltaTime;
                body.velocity=Vector2.zero;body.MovePosition(new Vector2(Mathf.Clamp(nextX,elevatorMinX,elevatorMaxX),body.position.y));
                return;
            }
            body.velocity=new Vector2(horizontal*MoveSpeed,body.velocity.y);
            if(transform.position.x<-23f)body.position=new Vector2(-23f,body.position.y);
        }
        void OnCollisionStay2D(Collision2D collision){for(int i=0;i<collision.contactCount;i++)if(collision.GetContact(i).normal.y>.55f)grounded=true;}
        void OnCollisionExit2D(Collision2D collision){grounded=false;}

        void Interact()
        {
            LootContainer crate=null;float best=1.75f;
            foreach(var candidate in FindObjectsOfType<LootContainer>())
            {float d=Vector2.Distance(transform.position,candidate.transform.position);if(d<best){best=d;crate=candidate;}}
            if(crate!=null){crate.Open(inventory);return;}
            WorldItem worldItem=null;best=1.55f;
            foreach(var item in FindObjectsOfType<WorldItem>())
            {float d=Vector2.Distance(transform.position,item.transform.position);if(d<best){best=d;worldItem=item;}}
            if(worldItem!=null){worldItem.Collect(inventory);return;}
            SupplyPickup pickup=null;best=1.55f;
            foreach(var p in FindObjectsOfType<SupplyPickup>())
            {float d=Vector2.Distance(transform.position,p.transform.position);if(!p.Collected&&d<best){best=d;pickup=p;}}
            if(pickup!=null){pickup.Collect();return;}
            TraderNPC trader=null;best=1.9f;
            foreach(var candidate in FindObjectsOfType<TraderNPC>())
            {float d=Vector2.Distance(transform.position,candidate.transform.position);if(d<best){best=d;trader=candidate;}}
            if(trader!=null){trader.Interact(inventory);return;}
            LevelElevator elevator=null;best=2.15f;
            foreach(var candidate in FindObjectsOfType<LevelElevator>())
            {float d=Vector2.Distance(transform.position,candidate.transform.position);if(d<best){best=d;elevator=candidate;}}
            if(elevator!=null){elevator.TryUse(this);return;}
            RunState.Instance.Notify("Здесь больше ничего нет.",1.1f);
        }
        void Attack()
        {
            attackCooldown=.42f;character?.Attack();ItemId weapon=inventory.ActiveItem.id;bool ranged=weapon==ItemId.Pistol||weapon==ItemId.Rifle;
            if(ranged)
            {
                ItemId ammo=weapon==ItemId.Pistol?ItemId.PistolAmmo:ItemId.RifleAmmo;
                if(!inventory.ConsumeAmmo(ammo,1)){RunState.Instance.Notify("Нет подходящих патронов.");return;}
            }
            else AudioDirector.Instance?.Play("swipe",.70f);
            WatcherAI nearest=null;float best=ranged?10f:2.2f;
            foreach(var candidate in FindObjectsOfType<WatcherAI>())
            {float d=Mathf.Abs(candidate.transform.position.x-transform.position.x);if(d<best){best=d;nearest=candidate;}}
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
            if(inventory==null||inventory.Count(ItemId.Flashlight)<=0)
            {RunState.Instance?.Notify("Фонарь потерян — найди новый.",1.4f);return;}
            flashlightOn=!flashlightOn;
            if(beamRenderer!=null)beamRenderer.enabled=FlashlightOn;
            if(handFlashlightRenderer!=null)handFlashlightRenderer.color=FlashlightOn?Color.white:new Color(.55f,.58f,.53f,1f);
            AudioDirector.Instance?.Play("ui",.45f);RunState.Instance.Notify(flashlightOn?"Фонарь включён.":"Фонарь выключен.",1.4f);
        }
        public void EnterElevator(float centerX,float halfWidth)
        {
            insideElevator=true;elevatorMinX=centerX-halfWidth;elevatorMaxX=centerX+halfWidth;
            horizontal=0;body.velocity=Vector2.zero;body.position=new Vector2(centerX,0);transform.position=new Vector3(centerX,0,0);grounded=true;
        }
        public void ExitElevator(Vector2 destination)
        {
            insideElevator=false;horizontal=0;body.velocity=Vector2.zero;body.position=destination;transform.position=new Vector3(destination.x,destination.y,0);grounded=true;
        }
        public void ResetForNewRun()=>ResetForNewRun(-19f);
        public void ResetForNewRun(float startX)
        {
            insideElevator=false;transform.position=new Vector3(startX,0,0);body.velocity=Vector2.zero;grounded=false;horizontal=0;invulnerable=0;attackCooldown=0;footstepClock=0;facing=1;flashlightOn=true;
            if(beamRenderer!=null){beamRenderer.enabled=true;beamRenderer.flipX=false;}
            if(inventory!=null){inventory.BeginRun();character?.SetGear(inventory.Gear);}
        }
    }
}
