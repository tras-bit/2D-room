using UnityEngine;

namespace Subsistence
{
    /// <summary>Soft drifting dust flecks for Level 0's dim, stagnant fluorescent air.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AmbientDust2D : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        Vector3 home;
        Color baseColor;
        float phase;
        float drift;

        public void Initialize(int seed)
        {
            spriteRenderer=GetComponent<SpriteRenderer>();
            home=transform.position;
            baseColor=spriteRenderer.color;
            phase=seed*1.713f;
            drift=.055f+(seed%4)*.012f;
        }

        void Awake()
        {
            spriteRenderer=GetComponent<SpriteRenderer>();
            if(spriteRenderer!=null)baseColor=spriteRenderer.color;
            home=transform.position;
        }

        void Update()
        {
            float t=Time.time*.42f+phase;
            transform.position=home+new Vector3(Mathf.Sin(t*.71f)*drift,Mathf.Sin(t)*drift*1.7f,0);
            if(spriteRenderer!=null)
            {
                float pulse=.76f+.24f*(.5f+.5f*Mathf.Sin(t*1.8f));
                spriteRenderer.color=new Color(baseColor.r,baseColor.g,baseColor.b,baseColor.a*pulse);
            }
        }
    }
}
