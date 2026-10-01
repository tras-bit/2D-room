using UnityEngine;

namespace Subsistence
{
    /// <summary>Bootstraps a strictly 2D, pixel-art side-view survival scene when the active scene is empty.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        static Transform world;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartFieldTest(){if(FindObjectOfType<GameBootstrap>()==null){var root=new GameObject("SUBSISTENCE · Pixel Side-View");root.AddComponent<GameBootstrap>();}}

        void Awake()
        {
            Application.targetFrameRate=60;
            world=new GameObject("2D Pixel World · Backrooms sectors").transform;world.SetParent(transform,false);
            WorldBuilder2D.Build(world);CreateCamera();CreatePlayer();CreateContainers();CreateWorkbenches();CreateWatchers();CreateExit();
            gameObject.AddComponent<RunState>();gameObject.AddComponent<AudioDirector>();gameObject.AddComponent<GameHUD>();
        }

        void CreateCamera()
        {
            Camera cam=Camera.main;
            if(cam==null){var go=new GameObject("Main Camera · Orthographic");cam=go.AddComponent<Camera>();go.tag="MainCamera";}
            cam.orthographic=true;cam.orthographicSize=5.35f;cam.nearClipPlane=.1f;cam.farClipPlane=30f;
            cam.backgroundColor=new Color(.055f,.066f,.06f);cam.clearFlags=CameraClearFlags.SolidColor;
            cam.transform.position=new Vector3(-10f,3.45f,-10f);cam.transform.rotation=Quaternion.identity;
            if(cam.GetComponent<CameraFollow2D>()==null)cam.gameObject.AddComponent<CameraFollow2D>();
            if(FindObjectOfType<AudioListener>()==null)cam.gameObject.AddComponent<AudioListener>();
        }

        void CreatePlayer()
        {
            var go=new GameObject("Survivor · 2D Pixel Player");go.transform.SetParent(world,false);go.transform.position=new Vector3(-19f,0,0);
            var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3.1f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(.60f,1.48f);collider.offset=new Vector2(0,.74f);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Survivor(0);renderer.sortingOrder=10;
            go.AddComponent<PixelFrameAnimator>();go.AddComponent<CharacterVisual2D>();go.AddComponent<InventorySystem>();go.AddComponent<PlayerController>();
        }

        void CreateContainers()
        {
            MakeContainer(-8f,1,"FIELD SUPPLY CRATE");MakeContainer(1.5f,1,"FIELD SUPPLY CRATE");
            MakeContainer(19f,2,"OFFICE SECURITY CRATE");MakeContainer(43f,1,"FIELD SUPPLY CRATE");
            MakeContainer(56f,2,"UTILITY LOCKER");MakeContainer(76f,3,"SEALED ARMOURY");MakeContainer(89f,3,"MAINTENANCE CACHE");
        }
        void MakeContainer(float x,int tier,string name)
        {
            var go=new GameObject(name+" · TIER "+tier);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Crate2D(tier);renderer.sortingOrder=7;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(1.2f,1.0f);collider.offset=new Vector2(0,.5f);
            go.AddComponent<LootContainer>().Initialize(tier,name);
        }
        void CreateWorkbenches(){MakeBench(14f,1,"FIELD BENCH");MakeBench(53f,2,"ELECTRONICS BENCH");MakeBench(82f,3,"ARMOURY BENCH");}
        void MakeBench(float x,int tier,string name)
        {
            var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Workbench2D(tier);renderer.sortingOrder=6;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(1.9f,.86f);collider.offset=new Vector2(0,.43f);collider.isTrigger=true;
            go.AddComponent<WorkbenchStation>().Initialize(tier,name);
        }
        void CreateWatchers(){CreateWatcherAt(29f,"WATCHER · OFFICE WING");CreateWatcherAt(83f,"WATCHER · MAINTENANCE");}
        void CreateWatcherAt(float x,string name)
        {
            var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3.1f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(.66f,1.50f);collider.offset=new Vector2(0,.75f);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Watcher(0);renderer.sortingOrder=10;
            go.AddComponent<PixelFrameAnimator>();go.AddComponent<CharacterVisual2D>();go.AddComponent<WatcherAI>();
        }
        void CreateExit()
        {
            float x=94.5f;
            PixelBlock("Exit hatch · frame",new Vector2(x,3.15f),new Vector2(3.0f,6.3f),new Color(.12f,.17f,.16f),-1);
            PixelBlock("Exit hatch · dark opening",new Vector2(x,2.5f),new Vector2(2.15f,5f),new Color(.025f,.043f,.042f),0);
            for(int i=-2;i<=2;i++)PixelBlock("Exit hatch rib",new Vector2(x+i*.43f,2.4f),new Vector2(.055f,4.6f),new Color(.36f,.42f,.36f),1);
            PixelBlock("Exit light",new Vector2(x,5.2f),new Vector2(.74f,.12f),new Color(.57f,.83f,.43f),3);
            var stop=new GameObject("Side-view world boundary");stop.transform.SetParent(world,false);stop.transform.position=new Vector3(96.6f,3.5f,0);
            var boundary=stop.AddComponent<BoxCollider2D>();boundary.size=new Vector2(.5f,7f);
        }
        void PixelBlock(string name,Vector2 position,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(position.x,position.y,0);go.transform.localScale=new Vector3(size.x,size.y,1);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=color;renderer.sortingOrder=order;
        }
    }
}
