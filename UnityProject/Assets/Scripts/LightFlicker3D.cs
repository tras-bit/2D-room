using UnityEngine;

namespace Subsistence
{
    /// <summary>Legacy script name retained for compatibility; flickers a SpriteRenderer, never a 3D Light.</summary>
    [System.Obsolete("Use PixelLightFlicker. This compatibility wrapper uses only 2D sprites.")]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class LightFlicker3D : MonoBehaviour
    {
        void Awake(){if(GetComponent<PixelLightFlicker>()==null)gameObject.AddComponent<PixelLightFlicker>();}
    }
}
