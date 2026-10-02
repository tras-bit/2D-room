using UnityEngine;

namespace Subsistence
{
    /// <summary>Frame-rate-independent follow with pixel-grid snapping for the 2D side-view game.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] Vector2 followOffset = new Vector2(.75f, 3.45f);
        [SerializeField, Min(.01f)] float gameplaySmoothTime = .14f;
        [SerializeField, Min(.01f)] float menuSmoothTime = .42f;
        [SerializeField, Min(0f)] float maxFollowSpeed = 80f;
        [SerializeField] bool followVerticalMotion;

        [Header("Pixel-art compatibility")]
        [Tooltip("Snap the camera position to the 96 PPU pixel grid for crisp pixel-art edges.")]
        [SerializeField] bool snapToPixelGrid = true;
        [SerializeField, Min(1f)] float pixelsPerUnit = 96f;

        Transform target;
        Rigidbody2D targetBody;
        Camera cameraComponent;
        Vector3 smoothVelocity;

        void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 5.35f;
            cameraComponent.allowHDR = true;
            transform.rotation = Quaternion.identity;
        }

        void Start()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
                targetBody = player.GetComponent<Rigidbody2D>();
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            targetBody = newTarget != null ? newTarget.GetComponent<Rigidbody2D>() : null;
            smoothVelocity = Vector3.zero;
        }

        void LateUpdate()
        {
            bool menu = GameHUD.MenuBackdropActive;
            if (!menu && (target == null || RunState.Instance == null || RunState.Instance.IsDead || RunState.Instance.HasEscaped))
                return;

            float x;
            float y;
            if (menu)
            {
                x = -10f;
                y = followOffset.y;
            }
            else
            {
                Vector3 targetPosition = targetBody != null ? targetBody.position : (Vector2)target.position;
                float halfWidth = cameraComponent.orthographicSize * cameraComponent.aspect;
                float minX = WorldBuilder2D.Level0Start + halfWidth;
                float maxX = WorldBuilder2D.Level1End - halfWidth;
                x = Mathf.Clamp(targetPosition.x + followOffset.x, minX, maxX);
                y = followVerticalMotion ? targetPosition.y + followOffset.y : followOffset.y;
            }

            Vector3 desired = new Vector3(x, y, -10f);
            float smoothTime = menu ? menuSmoothTime : gameplaySmoothTime;
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, smoothTime, maxFollowSpeed, Time.unscaledDeltaTime);

            if (snapToPixelGrid)
            {
                smoothed.x = Mathf.Round(smoothed.x * pixelsPerUnit) / pixelsPerUnit;
                smoothed.y = Mathf.Round(smoothed.y * pixelsPerUnit) / pixelsPerUnit;
            }

            transform.position = smoothed;
        }
    }
}
