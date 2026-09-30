using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float MoveSpeed = 5.4f;
        public float JumpVelocity = 8.8f;
        Rigidbody2D body;
        SpriteRenderer spriteRenderer;
        SpriteRenderer flashlightSprite;
        Transform flashlightTransform;
        PixelFrameAnimator animation;
        bool grounded;
        float attackVisual;
        bool flashlightOn = true;
        float invulnerable;
        float attackCooldown;
        float footstepClock;
        float horizontal;

        public bool FlashlightOn => flashlightOn;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animation = GetComponent<PixelFrameAnimator>();
        }

        public void SetFlashlightVisual(SpriteRenderer visual, Transform visualTransform)
        {
            flashlightSprite = visual;
            flashlightTransform = visualTransform;
        }

        void Update()
        {
            if (RunState.Instance == null || !GameHUD.IsPlaying || RunState.Instance.IsDead || RunState.Instance.HasEscaped) return;
            horizontal = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (grounded)
                {
                    body.velocity = new Vector2(body.velocity.x, JumpVelocity);
                    grounded = false;
                    animation?.Play("jump", true);
                }
            }
            if (Input.GetKeyDown(KeyCode.E)) Interact();
            if (Input.GetKeyDown(KeyCode.Q) && attackCooldown <= 0f) Attack();
            if (Input.GetKeyDown(KeyCode.C)) RunState.Instance.CraftBandage();
            if (Input.GetKeyDown(KeyCode.F)) ToggleFlashlight();
            if (invulnerable > 0) invulnerable -= Time.deltaTime;
            if (attackCooldown > 0) attackCooldown -= Time.deltaTime;
            if (horizontal != 0)
            {
                spriteRenderer.flipX = horizontal < 0;
                footstepClock += Time.deltaTime;
                if (footstepClock > .32f) { footstepClock = 0; AudioDirector.Instance?.Play("step", .34f); }
            }
            else footstepClock = 0;
            attackVisual = Mathf.Max(0, attackVisual - Time.deltaTime);
            if (animation != null && attackVisual <= 0)
            {
                if (!grounded) animation.Play("jump");
                else if (Mathf.Abs(horizontal) > .05f) animation.Play("walk");
                else animation.Play("idle");
            }
        }

        void FixedUpdate()
        {
            if (RunState.Instance == null || !GameHUD.IsPlaying || RunState.Instance.IsDead || RunState.Instance.HasEscaped) { body.velocity = new Vector2(0, body.velocity.y); return; }
            body.velocity = new Vector2(horizontal * MoveSpeed, body.velocity.y);
            if (transform.position.x < -34f) body.position = new Vector2(-34f, body.position.y);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y > .55f) grounded = true;
        }

        void OnCollisionExit2D(Collision2D collision) { grounded = false; }

        void Interact()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position + Vector3.up * .55f, 1.5f);
            SupplyPickup nearest = null;
            float distance = float.MaxValue;
            foreach (var hit in hits)
            {
                var pickup = hit.GetComponent<SupplyPickup>();
                if (pickup == null || pickup.Collected) continue;
                float candidate = Vector2.Distance(transform.position, pickup.transform.position);
                if (candidate < distance) { distance = candidate; nearest = pickup; }
            }
            if (nearest != null) { nearest.Collect(); return; }
            if (Mathf.Abs(transform.position.x - RunState.ExitX) < 2.4f) RunState.Instance.CheckExit(transform.position.x);
            else RunState.Instance.Notify("Здесь больше ничего нет.", 1.1f);
        }

        void Attack()
        {
            attackCooldown = .38f;
            attackVisual = .28f;
            animation?.Play("attack", true);
            AudioDirector.Instance?.Play("swipe", .72f);
            var watcher = FindObjectOfType<WatcherAI>();
            if (watcher != null && Mathf.Abs(watcher.transform.position.x - transform.position.x) < 2.1f)
                watcher.Stun(1.1f);
            else RunState.Instance.Notify("Ты бьёшь в пустоту.", .8f);
        }

        public void ReceiveHit(float damage)
        {
            if (invulnerable > 0 || RunState.Instance == null || !GameHUD.IsPlaying) return;
            invulnerable = .72f;
            animation?.Play("hurt", true);
            AudioDirector.Instance?.Play("hit", .8f);
            RunState.Instance.TakeDamage(damage);
            spriteRenderer.color = new Color(1f,.62f,.48f);
            RunState.Instance.Notify("Оно тебя зацепило — оттолкни его клавишей Q.", 1.3f);
        }

        void LateUpdate()
        {
            if (spriteRenderer != null && invulnerable <= 0) spriteRenderer.color = Color.white;
            if (flashlightTransform != null)
            {
                bool faceLeft=spriteRenderer!=null&&spriteRenderer.flipX;
                flashlightTransform.localPosition=new Vector3(faceLeft?-.32f:.32f,.66f,.1f);
                if(flashlightSprite!=null)flashlightSprite.flipX=faceLeft;
            }
        }

        public void ResetForNewRun()
        {
            transform.position = new Vector3(-25f, 0f, 0f);
            body.velocity = Vector2.zero;
            grounded = false;horizontal = 0;invulnerable = 0;attackCooldown = 0;attackVisual = 0;
            flashlightOn = true;
            if (flashlightSprite != null) flashlightSprite.enabled = true;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            animation?.Play("idle", true);
        }

        void ToggleFlashlight()
        {
            flashlightOn = !flashlightOn;
            if (flashlightSprite != null) flashlightSprite.enabled = flashlightOn;
            AudioDirector.Instance?.Play("ui", .45f);
            RunState.Instance.Notify(flashlightOn ? "Фонарь включён. Держись настороже." : "Фонарь выключен. Остались только аварийные огни.", 1.4f);
        }
    }
}
