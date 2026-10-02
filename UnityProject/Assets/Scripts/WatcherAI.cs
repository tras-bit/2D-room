using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
    public sealed class WatcherAI : MonoBehaviour
    {
        public float NoticeRadius=9.5f,ChaseSpeed=2.35f;
        Rigidbody2D body;CharacterVisual2D model;Transform target;float stunTimer,attackTimer,patrolClock,originX;bool gone;
        void Awake()
        {
            body=GetComponent<Rigidbody2D>();body.gravityScale=3.1f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            model=GetComponent<CharacterVisual2D>();if(model==null)model=gameObject.AddComponent<CharacterVisual2D>();originX=transform.position.x;
            model.SetHostileAppearance();
        }
        void Start(){var player=FindObjectOfType<PlayerController>();if(player!=null)target=player.transform;}
        void FixedUpdate()
        {
            if(gone||target==null||RunState.Instance==null||!GameHUD.IsPlaying||RunState.Instance.IsDead||RunState.Instance.HasEscaped)return;
            if(stunTimer>0){stunTimer-=Time.fixedDeltaTime;body.velocity=new Vector2(0,body.velocity.y);model.SetMotion(0,true,false,transform.position.x<target.position.x?1:-1,true);return;}
            float dx=target.position.x-transform.position.x,distance=Mathf.Abs(dx),speed;
            if(distance<NoticeRadius)speed=Mathf.Sign(dx)*ChaseSpeed;
            else{patrolClock+=Time.fixedDeltaTime;speed=Mathf.Sin(patrolClock*.7f)*.40f;if(Mathf.Abs(transform.position.x-originX)>2.8f)speed=-Mathf.Sign(transform.position.x-originX)*.45f;}
            body.velocity=new Vector2(speed,body.velocity.y);
            model.SetMotion(Mathf.Abs(speed)/ChaseSpeed,true,false,speed>=0?1:-1,true);
            if(distance<1.05f&&attackTimer<=0)
            {var player=target.GetComponent<PlayerController>();if(player!=null)player.ReceiveHit(15f);attackTimer=1.15f;}
            attackTimer-=Time.fixedDeltaTime;
        }
        public void ResetForNewRun(){gone=false;stunTimer=0;attackTimer=0;patrolClock=0;body.position=new Vector2(originX,0);body.velocity=Vector2.zero;}
        public void Stun(float seconds)
        {if(gone)return;stunTimer=Mathf.Max(stunTimer,seconds);AudioDirector.Instance?.Play("stun",.85f);RunState.Instance?.Notify("Сталкер оглушён — беги!",1.4f);}
    }
}
