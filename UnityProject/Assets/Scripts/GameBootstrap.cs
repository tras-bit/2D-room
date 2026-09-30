using UnityEngine;

namespace Subsistence
{
    /// <summary>Builds the first playable field test on a fresh empty Unity scene.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        static Sprite block;
        static Transform world;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartFieldTest()
        {
            if (FindObjectOfType<GameBootstrap>() != null) return;
            var root = new GameObject("SUBSISTENCE · Field Test 001");
            root.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            Application.targetFrameRate = 60;
            block = PixelArtFactory.Block(Color.white);
            world = new GameObject("World").transform;
            world.SetParent(transform);
            CreateCamera();
            CreateEnvironment();
            CreatePlayer();
            CreatePickups();
            CreateWatcher();
            CreateExit();
            gameObject.AddComponent<RunState>();
            gameObject.AddComponent<AudioDirector>();
            gameObject.AddComponent<GameHUD>();
        }

        void CreateCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }
            if (FindObjectOfType<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 4.45f;
            cam.backgroundColor = new Color(.34f, .39f, .33f);
            cam.transform.position = new Vector3(-25f, 2.6f, -10f);
            cam.gameObject.AddComponent<CameraFollow>();
        }

        void CreateEnvironment()
        {
            // Distorted service concourse and a distant, fog-broken industrial skyline.
            BlockAt("Distant wall", 0, 3.2f, 125, 6f, new Color(.42f,.48f,.41f), -30);
            for (int i = 0; i < 20; i++)
            {
                float x = -42f + i * 7.5f;
                float h = 1.8f + (i % 4) * .47f;
                BlockAt("Far building", x, 1.3f + h * .5f, 5.2f, h, i % 3 == 0 ? new Color(.37f,.44f,.38f) : new Color(.43f,.48f,.39f), -25);
                for (int w = 0; w < 3; w++) BlockAt("Distant window", x - 1.55f + w * 1.45f, 1.2f + h * .55f, .38f, .48f, new Color(.30f,.38f,.34f), -24);
            }
            // Repeating overhead ribs vanish toward the mist; deliberately not the reference's open village backdrop.
            for (int i = 0; i < 24; i++)
            {
                float x = -52 + i * 6.2f;
                BlockAt("Ceiling rib", x, 7.1f, .14f, 5.6f, new Color(.31f,.37f,.33f), -12);
                BlockAt("Ceiling crossbar", x + 1.55f, 9.85f, 3.3f, .12f, new Color(.43f,.46f,.36f), -12);
            }
            BlockAt("Hanging sodium lamp", -11f, 8.45f, 1.45f, .11f, new Color(.77f,.70f,.45f), -10);
            BlockAt("Hanging sodium lamp", 17f, 8.45f, 1.45f, .11f, new Color(.77f,.70f,.45f), -10);

            // Service buildings, barriers, old pipework, crates and overgrowth.
            PlaceSprite("Maintenance shelter", -21f, 0f, PixelArtFactory.Shelter(), -2);
            PlaceSprite("Sealed bulkhead", 6f, 0f, PixelArtFactory.Bulkhead(), -2);
            PlaceSprite("Maintenance shelter", 39f, 0f, PixelArtFactory.Shelter(), -2);
            PlaceSprite("Weathered crate", -14.5f, 0f, PixelArtFactory.Crate(), 1, .72f);
            PlaceSprite("Weathered crate", 4.5f, 0f, PixelArtFactory.Crate(), 1, .72f);
            PlaceSprite("Weathered crate", 26f, 0f, PixelArtFactory.Crate(), 1, .72f);
            PlaceSprite("Weathered crate", 54f, 0f, PixelArtFactory.Crate(), 1, .72f);

            for (int i = 0; i < 16; i++)
            {
                float x = -32 + i * 7.1f;
                float height = .25f + (i % 4) * .12f;
                BlockAt("Broken fence post", x, .93f, .09f, 1.85f, new Color(.31f,.37f,.31f), -1);
                BlockAt("Fence cap", x, 1.88f, .22f, .07f, new Color(.56f,.55f,.40f), -1);
                if (i < 15) BlockAt("Fence wire", x + 3.55f, 1.30f, 7.1f, .045f, new Color(.43f,.48f,.39f), -1);
                BlockAt("Platform weeds", x + .7f, height * .5f, .09f, height, i % 2 == 0 ? new Color(.40f,.48f,.32f) : new Color(.57f,.54f,.34f), 0);
            }
            for(int i=0;i<8;i++)
            {
                float x=-29+i*13.5f;
                BlockAt("Drain pipe",x,1.03f,8.4f,.16f,new Color(.39f,.43f,.35f),-3);
                BlockAt("Pipe bracket",x+.3f,.80f,.11f,.55f,new Color(.48f,.49f,.37f),-2);
            }

            var ground = new GameObject("Walkable · cracked service floor");
            ground.transform.SetParent(world);
            ground.transform.position = new Vector3(15f, -.34f, 0f);
            var groundRenderer = ground.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = block;
            groundRenderer.color = new Color(.17f,.20f,.17f);
            groundRenderer.sortingOrder = 2;
            ground.transform.localScale = new Vector3(125f,.7f,1f);
            var collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f,1f);

            for (int i = 0; i < 50; i++)
            {
                float x = -43f + i * 2.8f;
                BlockAt("Cracked floor mark",x,-.14f,.35f+(i%4)*.18f,.025f,i%3==0?new Color(.36f,.39f,.31f):new Color(.23f,.28f,.23f),3);
                if (i % 2 == 0) BlockAt("Dead grass",x+.35f,.02f,.04f,.26f,new Color(.45f,.48f,.32f),3);
            }
            BlockAt("Exit arch left", 72f, 2.05f, .28f, 4.1f, new Color(.35f,.41f,.35f), 4);
            BlockAt("Exit arch right", 76f, 2.05f, .28f, 4.1f, new Color(.35f,.41f,.35f), 4);
            BlockAt("Exit arch lintel", 74f, 4.1f, 4.3f, .26f, new Color(.50f,.53f,.40f), 4);
            BlockAt("Exit glow", 74f, 1.9f, 2.2f, 3.2f, new Color(.25f,.38f,.31f,.8f), 3);
            BlockAt("Exit indicator", 75.25f, 2.45f, .12f, .45f, new Color(.75f,.81f,.47f), 5);
        }

        void CreatePlayer()
        {
            var go = new GameObject("Survivor · Player");
            go.transform.SetParent(world);
            go.transform.position = new Vector3(-25f, 0f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtFactory.Survivor(0);
            sr.sortingOrder = 20;
            var animation = go.AddComponent<PixelFrameAnimator>();
            animation.AddClip("idle", new[] { PixelArtFactory.Survivor(0), PixelArtFactory.Survivor(2) }, 2.2f);
            animation.AddClip("walk", new[] { PixelArtFactory.Survivor(0), PixelArtFactory.Survivor(1), PixelArtFactory.Survivor(2), PixelArtFactory.Survivor(3) }, 9f);
            animation.AddClip("jump", new[] { PixelArtFactory.Survivor(1), PixelArtFactory.Survivor(2) }, 3f, false);
            animation.AddClip("attack", new[] { PixelArtFactory.Survivor(1), PixelArtFactory.Survivor(3), PixelArtFactory.Survivor(0) }, 13f, false);
            animation.AddClip("hurt", new[] { PixelArtFactory.Survivor(3), PixelArtFactory.Survivor(1) }, 8f, false);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 3.2f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(.62f, 1.35f);
            collider.offset = new Vector2(0f,.67f);
            go.AddComponent<PlayerController>();
            var glow = new GameObject("Flashlight glow");
            glow.transform.SetParent(go.transform);
            glow.transform.localPosition = new Vector3(.32f,.66f,.1f);
            var lightSprite = glow.AddComponent<SpriteRenderer>();
            lightSprite.sprite = PixelArtFactory.FlashlightBeam();
            lightSprite.color = new Color(1f,1f,1f,.90f);
            lightSprite.sortingOrder = 19;
            go.GetComponent<PlayerController>().SetFlashlightVisual(lightSprite, glow.transform);
        }

        void CreatePickups()
        {
            SpawnPickup(-19f, SupplyPickup.Kind.Scrap);
            SpawnPickup(-11f, SupplyPickup.Kind.Water);
            SpawnPickup(-2f, SupplyPickup.Kind.Cloth);
            SpawnPickup(11f, SupplyPickup.Kind.Food);
            SpawnPickup(20f, SupplyPickup.Kind.Scrap);
            SpawnPickup(31f, SupplyPickup.Kind.Cloth);
            SpawnPickup(44f, SupplyPickup.Kind.Water);
            SpawnPickup(55f, SupplyPickup.Kind.Scrap);
        }

        void SpawnPickup(float x, SupplyPickup.Kind kind)
        {
            var go = new GameObject("Supply · " + kind);
            go.transform.SetParent(world);
            go.transform.position = new Vector3(x,.08f,0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtFactory.Supply(kind);
            sr.sortingOrder = 12;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(.8f,.8f);
            go.AddComponent<SupplyPickup>().Initialize(kind);
        }

        void CreateWatcher()
        {
            var go = new GameObject("Watcher · hostile");
            go.transform.SetParent(world);
            go.transform.position = new Vector3(15f,0f,0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtFactory.Watcher(0);
            sr.sortingOrder = 18;
            var animation = go.AddComponent<PixelFrameAnimator>();
            animation.AddClip("patrol", new[] { PixelArtFactory.Watcher(0), PixelArtFactory.Watcher(1) }, 3f);
            animation.AddClip("stunned", new[] { PixelArtFactory.Watcher(1), PixelArtFactory.Watcher(0) }, 6f);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(.82f,1.38f);
            collider.offset = new Vector2(0f,.69f);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            go.AddComponent<WatcherAI>();
        }

        void CreateExit()
        {
            BlockAt("Exit door shadow",74f,1.72f,1.85f,3.4f,new Color(.11f,.17f,.14f),5);
            for(int i=0;i<5;i++)BlockAt("Door ribs",73.35f+i*.33f,1.7f,.045f,3.1f,new Color(.34f,.44f,.34f),6);
            BlockAt("Exit lamp",74f,3.62f,.48f,.08f,new Color(.79f,.82f,.50f),7);
        }

        void PlaceSprite(string name,float x,float y,Sprite sprite,int order,float scale=1f)
        {
            var go=new GameObject(name);go.transform.SetParent(world);go.transform.position=new Vector3(x,y,0);go.transform.localScale=Vector3.one*scale;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=order;
        }

        void BlockAt(string name,float x,float y,float width,float height,Color color,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(world);go.transform.position=new Vector3(x,y,0);go.transform.localScale=new Vector3(width,height,1);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=block;sr.color=color;sr.sortingOrder=order;
            string key=name.ToLowerInvariant();
            if(key.Contains("lamp")||key.Contains("indicator"))go.AddComponent<LampFlicker>();
        }
    }
}
