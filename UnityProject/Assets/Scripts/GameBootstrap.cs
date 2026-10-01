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
            world=new GameObject("2D Pixel World · Backrooms levels").transform;world.SetParent(transform,false);
            WorldBuilder2D.Build(world);CreateCamera();CreatePlayer();CreateContainers();CreateWorkbenches();CreateWatchers();CreateTrader();CreateElevators();
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
            // Sparse, non-blocking supply finds preserve Level 0's empty, resource-starved entry-level feeling.
            MakeContainer(-9f,1,"LEVEL 0 · ПОВРЕЖДЁННАЯ КОРОБКА");
            MakeContainer(18f,1,"LEVEL 0 · ПОТЕРЯННАЯ ПОСЫЛКА");
            MakeContainer(62f,2,"LEVEL 0 · ЗАКЛЕЕННАЯ КОРОБКА С ПРИПАСАМИ");
        }
        void MakeContainer(float x,int tier,string name)
        {
            var go=new GameObject(name+" · TIER "+tier);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Crate2D(tier);renderer.sortingOrder=7;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(1.2f,1.0f);collider.offset=new Vector2(0,.5f);
            go.AddComponent<LootContainer>().Initialize(tier,name);
        }
        void CreateWorkbenches(){MakeBench(137f,1,"LEVEL 1 · FOUND WORKTABLE");MakeBench(191f,2,"LEVEL 1 · SERVICE BENCH");}
        void MakeBench(float x,int tier,string name)
        {
            var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Workbench2D(tier);renderer.sortingOrder=6;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(1.9f,.86f);collider.offset=new Vector2(0,.43f);collider.isTrigger=true;
            go.AddComponent<WorkbenchStation>().Initialize(tier,name);
        }
        void CreateWatchers(){CreateWatcherAt(153f,"LEVEL 1 · WATCHER IN THE DARK");CreateWatcherAt(198f,"LEVEL 1 · WATCHER NEAR THE SERVICE BAY");}
        void CreateWatcherAt(float x,string name)
        {
            var go=new GameObject(name);go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            var body=go.AddComponent<Rigidbody2D>();body.gravityScale=3.1f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(.66f,1.50f);collider.offset=new Vector2(0,.75f);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Watcher(0);renderer.sortingOrder=10;
            go.AddComponent<PixelFrameAnimator>();go.AddComponent<CharacterVisual2D>();go.AddComponent<WatcherAI>();
        }
        void CreateTrader()
        {
            var go=new GameObject("Level 0 · Manila Room attendant");go.transform.SetParent(world,false);go.transform.position=new Vector3(39f,0,0);
            go.AddComponent<TraderNPC>().Initialize();
        }
        void CreateElevators()
        {
            // An anomalous twin-lift transition sits beyond the yellow maze and arrives at Level 1.
            MakeElevator(1,87f,116f);MakeElevator(2,88.5f,116.5f);
        }
        void MakeElevator(int number,float x,float arrivalX)
        {
            var go=new GameObject("Level 0 · elevator "+number+" to Level 1");go.transform.SetParent(world,false);go.transform.position=new Vector3(x,0,0);
            go.AddComponent<LevelElevator>().Initialize(number,1,arrivalX);
        }
    }
}
