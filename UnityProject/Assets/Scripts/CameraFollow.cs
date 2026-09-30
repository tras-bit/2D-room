using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        Transform target;
        Camera cameraComponent;
        Vector3 velocity;

        void Awake() { cameraComponent = GetComponent<Camera>(); }
        void Start()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) target = player.transform;
        }
        void LateUpdate()
        {
            if (target == null || RunState.Instance != null && (RunState.Instance.IsDead || RunState.Instance.HasEscaped)) return;
            float halfWidth = cameraComponent.orthographicSize * cameraComponent.aspect;
            float x = Mathf.Clamp(target.position.x + 2.1f, -25f + halfWidth, 74f + halfWidth - .5f);
            Vector3 desired = new Vector3(x, 2.45f, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, .22f);
        }
    }
}
