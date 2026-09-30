using UnityEngine;

namespace Subsistence
{
    /// <summary>Subtle, authored flicker on the corridor's failing sodium lamps.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class LampFlicker : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        Color baseColor;
        float phase;
        void Awake(){spriteRenderer=GetComponent<SpriteRenderer>();baseColor=spriteRenderer.color;phase=Random.value*6.28f;}
        void Update()
        {
            float slow=.91f+Mathf.Sin(Time.time*2.1f+phase)*.045f;
            float flutter=Random.value>.996f?Random.Range(.38f,.68f):1f;
            spriteRenderer.color=baseColor*(slow*flutter);
        }
    }
}
