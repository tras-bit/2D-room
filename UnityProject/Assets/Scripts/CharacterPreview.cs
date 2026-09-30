using UnityEngine;

namespace Subsistence
{
    /// <summary>Isolated real-time render-texture portrait of the equipped 3D survivor for the inventory screen.</summary>
    public sealed class CharacterPreview : MonoBehaviour
    {
        const int PreviewLayer=31;RenderTexture target;CharacterModel3D model;Camera previewCamera;
        public Texture Texture=>target;
        void Awake()
        {
            target=new RenderTexture(384,512,16,RenderTextureFormat.ARGB32){name="Inventory character preview",filterMode=FilterMode.Bilinear};target.Create();
            var rig=new GameObject("Inventory preview survivor");rig.transform.position=new Vector3(0,-500,0);rig.AddComponent<CharacterModel3D>();model=rig.GetComponent<CharacterModel3D>();SetLayer(rig.transform,PreviewLayer);
            var cameraGo=new GameObject("Inventory portrait camera");cameraGo.transform.position=new Vector3(0,-498.85f,-5.8f);cameraGo.transform.rotation=Quaternion.LookRotation(new Vector3(0,-498.95f,0)-cameraGo.transform.position,Vector3.up);
            previewCamera=cameraGo.AddComponent<Camera>();previewCamera.targetTexture=target;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=new Color(.075f,.09f,.075f);previewCamera.fieldOfView=29;previewCamera.nearClipPlane=.1f;previewCamera.farClipPlane=20;previewCamera.cullingMask=1<<PreviewLayer;previewCamera.depth=10;previewCamera.allowHDR=false;
            var main=Camera.main;if(main!=null)main.cullingMask&=~(1<<PreviewLayer);
        }
        public void SetGear(ItemStack[] gear){if(model!=null)model.SetGear(gear);}
        static void SetLayer(Transform t,int layer){t.gameObject.layer=layer;for(int i=0;i<t.childCount;i++)SetLayer(t.GetChild(i),layer);}
        void OnDestroy(){if(target!=null){target.Release();Destroy(target);}}
    }
}
