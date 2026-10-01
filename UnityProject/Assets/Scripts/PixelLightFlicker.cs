using UnityEngine;

namespace Subsistence
{
    /// <summary>Flickers emissive-looking pixel sprite colors without using a 3D Light component.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelLightFlicker : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        Color baseColor;
        float timer;
        void Awake(){spriteRenderer=GetComponent<SpriteRenderer>();baseColor=spriteRenderer.color;}
        void Update()
        {
            timer-=Time.deltaTime;
            if(timer>0)return;
            timer=Random.Range(.08f,.35f);
            float glow=Random.Range(.65f,1.08f);
            spriteRenderer.color=new Color(baseColor.r*glow,baseColor.g*glow,baseColor.b*glow,baseColor.a);
        }
    }
}
