using UnityEngine;

namespace Subsistence
{
    /// <summary>Bootstraps the 2.5D survival slice when the current Unity scene is empty.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        static Transform world;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartFieldTest(){if(FindObjectOfType<GameBootstrap>()==null){var root=new GameObject("SUBSISTENCE · Backrooms Field Test");root.AddComponent<GameBootstrap>();}}

        void Awake()
        {
            Application.targetFrameRate=60;RuntimeArtFactory.Initialize();world=new GameObject("World · Backrooms sectors").transform;world.SetParent(transform,false);
            CreateLighting();CreateCamera();RoomBuilder3D.Build(world);CreateWalkableCollider();CreatePlayer();CreateContainers();CreateWorkbenches();CreateWatcher();CreateExit();
            gameObject.AddComponent<RunState>();gameObject.AddComponent<AudioDirector>();gameObject.AddComponent<GameHUD>();
        }
        void CreateLighting()
        {
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.37f,.34f,.26f);RenderSettings.ambientIntensity=.80f;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogColor=new Color(.35f,.32f,.23f);RenderSettings.fogDensity=.0042f;
            var go=new GameObject("Warm overhead bounce light");var light=go.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1f,.84f,.58f);light.intensity=.72f;light.shadows=LightShadows.Soft;light.shadowStrength=.32f;go.transform.rotation=Quaternion.Euler(48f,-28f,0);
        }
        void CreateCamera()
        {
            Camera cam=Camera.main;
            if(cam==null){var go=new GameObject("Main Camera");cam=go.AddComponent<Camera>();go.tag="MainCamera";}
            cam.orthographic=false;cam.fieldOfView=42f;cam.nearClipPlane=.1f;cam.farClipPlane=180f;cam.backgroundColor=new Color(.22f,.21f,.16f);cam.clearFlags=CameraClearFlags.SolidColor;
            cam.transform.position=new Vector3(-17f,5.6f,-15f);cam.transform.LookAt(new Vector3(-17f,2.3f,4f));
            if(cam.GetComponent<CameraFollow>()==null)cam.gameObject.AddComponent<CameraFollow>();
            if(FindObjectOfType<AudioListener>()==null)cam.gameObject.AddComponent<AudioListener>();
        }
        void CreateWalkableCollider()
        {
            var go=new GameObject("Continuous side-view collision plane");go.transform.SetParent(world,false);go.transform.position=new Vector3(36f,-.30f,0f);
            var floor=go.AddComponent<BoxCollider2D>();floor.size=new Vector2(128f,.60f);
        }
        void CreatePlayer()
        {
            var go=new GameObject("Survivor · Player 2.5D");go.transform.SetParent(world,false);go.transform.position=new Vector3(-19f,0,0);
            var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3.25f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(.60f,1.5f);collider.offset=new Vector2(0,.75f);
            go.AddComponent<CharacterModel3D>();go.AddComponent<InventorySystem>();go.AddComponent<PlayerController>();
        }
        void CreateContainers()
        {
            MakeContainer(-8f,1,"FIELD SUPPLY CRATE");MakeContainer(1.5f,1,"FIELD SUPPLY CRATE");
            MakeContainer(19f,2,"OFFICE SECURITY CRATE");MakeContainer(43f,1,"FIELD SUPPLY CRATE");
            MakeContainer(56f,2,"UTILITY LOCKER");MakeContainer(76f,3,"SEALED ARMOURY");MakeContainer(89f,3,"MAINTENANCE CACHE");
        }
        void MakeContainer(float x,int tier,string name)
        {var go=new GameObject(name+" · TIER "+tier);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);go.AddComponent<LootContainer>().Initialize(tier,name);}
        void CreateWorkbenches()
        {MakeBench(14f,1,"FIELD BENCH");MakeBench(53f,2,"ELECTRONICS BENCH");MakeBench(82f,3,"ARMOURY BENCH");}
        void MakeBench(float x,int tier,string name)
        {var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);go.AddComponent<WorkbenchStation>().Initialize(tier,name);}
        void CreateWatcher(){CreateWatcherAt(29f,"WATCHER · OFFICE WING");CreateWatcherAt(83f,"WATCHER · MAINTENANCE");}
        void CreateWatcherAt(float x,string name)
        {
            var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var body=go.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(.66f,1.55f);collider.offset=new Vector2(0,.77f);
            go.AddComponent<CharacterModel3D>();go.AddComponent<WatcherAI>();
        }
        void CreateExit()
        {
            float x=94.5f;
            RuntimeArtFactory.Cube(world,"Final service hatch",new Vector3(x,2.45f,8.5f),new Vector3(3.8f,4.9f,.45f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(world,"Hatch dark interior",new Vector3(x,2.4f,8.20f),new Vector3(2.55f,4.1f,.08f),RuntimeArtFactory.Rubber);
            for(int i=-2;i<=2;i++)RuntimeArtFactory.Cube(world,"Hatch ribs",new Vector3(x+i*.48f,2.4f,8.12f),new Vector3(.055f,3.8f,.04f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(world,"Exit light",new Vector3(x,5.1f,7.9f),new Vector3(.74f,.12f,.10f),RuntimeArtFactory.Fluorescent);
            RuntimeArtFactory.PointLight(world,"Exit status",new Vector3(x,4.7f,6.8f),new Color(.60f,.83f,.51f),.85f,8f);
        }
    }
}
