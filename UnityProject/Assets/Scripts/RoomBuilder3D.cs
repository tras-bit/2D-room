using UnityEngine;

namespace Subsistence
{
    /// <summary>Legacy API name retained for compatibility; delegates to the fully 2D pixel-art world builder.</summary>
    [System.Obsolete("Use WorldBuilder2D. This wrapper builds no 3D geometry.")]
    public static class RoomBuilder3D
    {
        public static void Build(Transform parent)=>WorldBuilder2D.Build(parent);
    }
}
