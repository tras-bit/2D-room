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
        SpriteRenderer beamRenderer;
        bool grounded,flashlightOn=true;
        float invulnerable,attackCooldown,footstepClock,horizontal;
        int facing=1;
        public bool FlashlightOn=>flashlightOn;
        public InventorySystem Inventory=>inventory;
        public CharacterVisual2D Character=>character;

        void Awake()
        {
            body=GetComponent<Rigidbody2D>();body.gravityScale=3.1f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            character=GetComponent<CharacterVisual2D>();if(character==null)character=gameObject.AddComponent<CharacterVisual2D>();
            inventory=GetComponent<InventorySystem>();if(inventory==null)inventory=gameObject.AddComponent<InventorySystem>();
            inventory.Character=character;
            var beam=new GameObject("Pixel flashlight beam");beam.transform.SetParent(transform,false);
            beamRenderer=beam.AddComponent<SpriteRenderer>();beamRenderer.sprite=PixelArtFactory.FlashlightBeam();beamRenderer.sortingOrder=8;beamRenderer.color=new Color(1f,.91f,.68f,.54f);
            beam.transform.localPosition=new Vector3(.3f,1.05f,0);beamRenderer.enabled=flashlightOn;
        }
        void Start(){inventory.BeginRun();}

        void Update()
        {
            if(RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped)return;
            horizontal=Input.GetAxisRaw("Horizontal");
            if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow))
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
            if(beamRenderer!=null)
            {
                beamRenderer.flipX=facing<0;
                beamRenderer.enabled=flashlightOn;
                beamRenderer.transform.localPosition=new Vector3(facing*.3f,1.05f,0);
            }
        }
        void FixedUpdate()
        {
            if(RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped){body.velocity=new Vector2(0,body.velocity.y);return;}
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
            if(Mathf.Abs(transform.position.x-RunState.ExitX)<2.5f){RunState.Instance.CheckExit(transform.position.x);return;}
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
            flashlightOn=!flashlightOn;if(beamRenderer!=null)beamRenderer.enabled=flashlightOn;
            AudioDirector.Instance?.Play("ui",.45f);RunState.Instance.Notify(flashlightOn?"Фонарь включён.":"Фонарь выключен.",1.4f);
        }
        public void ResetForNewRun()=>ResetForNewRun(-19f);
        public void ResetForNewRun(float startX)
        {
            transform.position=new Vector3(startX,0,0);body.velocity=Vector2.zero;grounded=false;horizontal=0;invulnerable=0;attackCooldown=0;footstepClock=0;facing=1;flashlightOn=true;
            if(beamRenderer!=null){beamRenderer.enabled=true;beamRenderer.flipX=false;}
            if(inventory!=null){inventory.BeginRun();character?.SetGear(inventory.Gear);}
        }
    }
}
