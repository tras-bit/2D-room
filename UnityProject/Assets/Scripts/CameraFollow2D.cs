using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        Transform target;Vector3 velocity;Camera cameraComponent;
        void Awake(){cameraComponent=GetComponent<Camera>();cameraComponent.orthographic=true;cameraComponent.orthographicSize=5.35f;transform.rotation=Quaternion.identity;}
        void Start(){var player=FindObjectOfType<PlayerController>();if(player!=null)target=player.transform;}
        void LateUpdate()
        {
            float x;
            if(GameHUD.MenuBackdropActive)x=-10f;
            else if(target!=null&&RunState.Instance!=null&&!RunState.Instance.IsDead&&!RunState.Instance.HasEscaped)
            {
                float halfWidth=cameraComponent.orthographicSize*cameraComponent.aspect;
                float minX=WorldBuilder2D.Level0Start+halfWidth,maxX=WorldBuilder2D.Level1End-halfWidth;
                x=Mathf.Clamp(target.position.x+.75f,minX,maxX);
            }
            else return;
            Vector3 wanted=new Vector3(x,3.45f,-10f);
            Vector3 smoothed=Vector3.SmoothDamp(transform.position,wanted,ref velocity,GameHUD.MenuBackdropActive ? .4f : .16f);
            const float pixelsPerUnit=32f;smoothed.x=Mathf.Round(smoothed.x*pixelsPerUnit)/pixelsPerUnit;smoothed.y=Mathf.Round(smoothed.y*pixelsPerUnit)/pixelsPerUnit;
            transform.position=smoothed;
        }
    }
}
