using UnityEngine;

namespace Subsistence
{
    /// <summary>Legacy component name retained for compatibility; forwards to the orthographic side-view camera.</summary>
    [System.Obsolete("Use CameraFollow2D. This compatibility wrapper never enables perspective rendering.")]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        void Awake()
        {
            var cameraComponent=GetComponent<Camera>();cameraComponent.orthographic=true;
            if(GetComponent<CameraFollow2D>()==null)gameObject.AddComponent<CameraFollow2D>();
        }
    }
}
