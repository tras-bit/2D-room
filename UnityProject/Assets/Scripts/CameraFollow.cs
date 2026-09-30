using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        Transform target;Camera cameraComponent;Vector3 velocity;float anchorY=5.6f,anchorZ=-15f;
        void Awake(){cameraComponent=GetComponent<Camera>();}
        void Start(){var p=FindObjectOfType<PlayerController>();if(p!=null)target=p.transform;}
        void LateUpdate()
        {
            if(GameHUD.MenuBackdropActive)
            {
                Vector3 menuPosition=new Vector3(-23.5f,5.9f,-10.5f);
                transform.position=Vector3.SmoothDamp(transform.position,menuPosition,ref velocity,.42f);
                Vector3 menuLook=new Vector3(2.0f,2.5f,4.5f);
                transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(menuLook-transform.position,Vector3.up),Time.deltaTime*1.8f);
                cameraComponent.fieldOfView=Mathf.Lerp(cameraComponent.fieldOfView,47f,Time.deltaTime*1.7f);
                return;
            }
            if(target==null||RunState.Instance!=null&&(RunState.Instance.IsDead||RunState.Instance.HasEscaped))return;
            float x=Mathf.Clamp(target.position.x+.65f,-20f,96f);
            Vector3 desired=new Vector3(x,anchorY,anchorZ);transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,.18f);
            Vector3 look=new Vector3(x,2.3f,4.0f);transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look-transform.position,Vector3.up),Time.deltaTime*3f);
            cameraComponent.fieldOfView=Mathf.Lerp(cameraComponent.fieldOfView,42f,Time.deltaTime*3f);
        }
    }
}
