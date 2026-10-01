using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelLightFlicker : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        Color baseColor;
        float timer;
        float intensityScale=1f;
        void Awake(){spriteRenderer=GetComponent<SpriteRenderer>();baseColor=spriteRenderer.color;}
        public void SetIntensityScale(float scale){intensityScale=Mathf.Clamp(scale,.1f,1.2f);}
        void Update()
        {
            timer-=Time.deltaTime;if(timer>0)return;
            timer=Random.Range(.08f,.35f);
            float glow=Random.Range(.65f,1.08f)*intensityScale;
            spriteRenderer.color=new Color(baseColor.r*glow,baseColor.g*glow,baseColor.b*glow,baseColor.a);
        }
    }
}
