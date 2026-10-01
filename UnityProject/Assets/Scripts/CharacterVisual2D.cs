using System;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Pixel sprite character renderer and frame animator. No meshes, materials or 3D models.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PixelFrameAnimator))]
    public sealed class CharacterVisual2D : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        PixelFrameAnimator animator;
        ItemStack[] gear;
        bool hostile;
        bool facingRight=true;
        bool moving,grounded=true,jumping,threat;
        float attackTimer;

        public Sprite Portrait=>PixelArtFactory.Survivor(0,gear,hostile);
        public Sprite CurrentSprite=>spriteRenderer!=null?spriteRenderer.sprite:Portrait;

        void Awake()
        {
            spriteRenderer=GetComponent<SpriteRenderer>();animator=GetComponent<PixelFrameAnimator>();
            spriteRenderer.sortingOrder=10;
            RebuildClips();
        }

        public void SetGear(ItemStack[] worn)
        {
            gear=worn==null?null:(ItemStack[])worn.Clone();
            if(spriteRenderer==null)spriteRenderer=GetComponent<SpriteRenderer>();
            if(animator==null)animator=GetComponent<PixelFrameAnimator>();
            RebuildClips();
        }

        public void SetHostileAppearance()
        {
            hostile=true;
            if(spriteRenderer==null)spriteRenderer=GetComponent<SpriteRenderer>();
            if(animator==null)animator=GetComponent<PixelFrameAnimator>();
            RebuildClips();
        }

        void RebuildClips()
        {
            if(animator==null)animator=GetComponent<PixelFrameAnimator>();
            if(spriteRenderer==null)spriteRenderer=GetComponent<SpriteRenderer>();
            if(animator==null||spriteRenderer==null)return;
            if(hostile)
            {
                animator.AddClip("idle",new[]{PixelArtFactory.Watcher(0),PixelArtFactory.Watcher(1)},2.3f);
                animator.AddClip("move",new[]{PixelArtFactory.Watcher(1),PixelArtFactory.Watcher(0)},5.2f);
                animator.AddClip("jump",new[]{PixelArtFactory.Watcher(1)},1f);
                animator.Play("idle",true);
            }
            else
            {
                Sprite still=PixelArtFactory.Survivor(0,gear);
                animator.AddClip("idle",new[]{still},1f);
                animator.AddClip("move",new[]{PixelArtFactory.Survivor(1,gear),still,PixelArtFactory.Survivor(3,gear),still},8f);
                animator.AddClip("jump",new[]{PixelArtFactory.Survivor(2,gear)},1f);
                animator.AddClip("attack",new[]{PixelArtFactory.Survivor(0,gear),PixelArtFactory.Survivor(1,gear),still},12f,false);
                animator.Play("idle",true);
            }
        }

        public void SetMotion(float speed,bool isGrounded,bool isJumping,int direction,bool danger=false)
        {
            moving=speed>.12f;grounded=isGrounded;jumping=isJumping;threat=danger;
            if(direction!=0)facingRight=direction>0;
            spriteRenderer.flipX=!facingRight;
            if(attackTimer>0)return;
            string clip=!grounded||jumping?"jump":moving?"move":"idle";
            animator.Play(clip);
            spriteRenderer.color=threat?new Color(1f,.82f,.72f):Color.white;
        }

        public void Attack()
        {
            if(hostile)return;
            attackTimer=.30f;
            animator.Play("attack",true);
        }

        void Update()
        {
            if(attackTimer>0)
            {
                attackTimer-=Time.deltaTime;
                if(attackTimer<=0)animator.Play(!grounded||jumping?"jump":moving?"move":"idle",true);
            }
        }
    }
}
