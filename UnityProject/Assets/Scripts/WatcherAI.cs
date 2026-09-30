using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class WatcherAI : MonoBehaviour
    {
        public float NoticeRadius = 8f;
        public float ChaseSpeed = 2.75f;
        Rigidbody2D body;
        SpriteRenderer spriteRenderer;
        PixelFrameAnimator spriteAnimator;
        Transform target;
        float stunTimer;
        float attackTimer;
        float patrolClock;
        float originX;
        bool isGone;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteAnimator = GetComponent<PixelFrameAnimator>();
            originX = transform.position.x;
        }

        void Start()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) target = player.transform;
        }

        void FixedUpdate()
        {
            if (isGone || target == null || RunState.Instance == null || !GameHUD.IsPlaying || RunState.Instance.IsDead || RunState.Instance.HasEscaped) return;
            if (stunTimer > 0)
            {
                stunTimer -= Time.fixedDeltaTime;
                body.MovePosition(body.position);
                spriteRenderer.color = new Color(.68f,.78f,.59f);
                spriteAnimator?.Play("stunned");
                return;
            }
            spriteRenderer.color = Color.white;
            spriteAnimator?.Play("patrol");
            float dx = target.position.x - transform.position.x;
            float distance = Mathf.Abs(dx);
            float speed;
            if (distance < NoticeRadius) speed = Mathf.Sign(dx) * ChaseSpeed;
            else
            {
                patrolClock += Time.fixedDeltaTime;
                speed = Mathf.Sin(patrolClock * .85f) * .48f;
                if (Mathf.Abs(transform.position.x-originX)>2.2f) speed=-Mathf.Sign(transform.position.x-originX)*.45f;
            }
            body.MovePosition(body.position + Vector2.right * speed * Time.fixedDeltaTime);
            if (speed != 0) spriteRenderer.flipX = speed < 0;
            if (distance < 1.22f && attackTimer <= 0f)
            {
                var player = target.GetComponent<PlayerController>();
                if (player != null) player.ReceiveHit(13f);
                attackTimer = 1.05f;
            }
            attackTimer -= Time.fixedDeltaTime;
        }

        public void ResetForNewRun()
        {
            isGone=false;stunTimer=0;attackTimer=0;patrolClock=0;
            body.position=new Vector2(originX,0f);spriteRenderer.color=Color.white;
            spriteAnimator?.Play("patrol",true);
        }

        public void Stun(float seconds)
        {
            if (isGone) return;
            stunTimer = Mathf.Max(stunTimer, seconds);
            spriteAnimator?.Play("stunned",true);
            AudioDirector.Instance?.Play("stun",.85f);
            RunState.Instance?.Notify("Ударил его — беги, пока оно оглушено.");
        }
    }
}
