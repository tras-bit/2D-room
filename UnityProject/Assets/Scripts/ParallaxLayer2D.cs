using UnityEngine;

namespace Subsistence
{
    /// <summary>Moves a very wide tiled sprite relative to a camera without changing gameplay coordinates.</summary>
    [DisallowMultipleComponent]
    public sealed class ParallaxLayer2D : MonoBehaviour
    {
        [SerializeField, Range(0f,1f)] float horizontalFollow = .2f;
        [SerializeField, Range(0f,1f)] float verticalFollow = .08f;

        Transform followTarget;
        Vector3 layerOrigin;
        Vector3 cameraOrigin;

        public void Initialize(Transform cameraTransform, float horizontal, float vertical)
        {
            followTarget = cameraTransform;
            horizontalFollow = Mathf.Clamp01(horizontal);
            verticalFollow = Mathf.Clamp01(vertical);
            layerOrigin = transform.position;
            cameraOrigin = cameraTransform != null ? cameraTransform.position : Vector3.zero;
        }

        void Awake()
        {
            layerOrigin = transform.position;
        }

        void LateUpdate()
        {
            if (followTarget == null) return;
            Vector3 cameraDelta = followTarget.position - cameraOrigin;
            transform.position = layerOrigin + new Vector3(cameraDelta.x * horizontalFollow, cameraDelta.y * verticalFollow, 0f);
        }
    }
}
