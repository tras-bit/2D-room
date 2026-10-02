using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>
    /// Player-side building loop: craft a plan, place supported twig pieces,
    /// then use a hammer to upgrade or dismantle them. Deployables are crafted separately.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BuildingSystem2D : MonoBehaviour
    {
        const float GridWidth=2f,FoundationTop=.46f,WallHeight=2.4f,LevelStep=2.72f,MaxReach=9.4f;
        static readonly BuildPart2D[] Parts={BuildPart2D.Foundation,BuildPart2D.Wall,BuildPart2D.Doorway,BuildPart2D.Floor};

        PlayerController player;
        InventorySystem inventory;
        Camera worldCamera;
        GameObject ghostObject;
        SpriteRenderer ghostRenderer;
        BuildPart2D currentPart=BuildPart2D.Foundation;
        int currentLevel;
        int ghostGridX;
        bool ghostValid;
        string ghostReason="";

        public bool IsBuildPlanSelected=>inventory!=null&&inventory.ActiveItem.id==ItemId.BuildingPlan;
        public bool HasBuildToolSelected=>inventory!=null&&IsBuildItem(inventory.ActiveItem.id);
        public string HudHint
        {
            get
            {
                if(inventory==null)return "";
                ItemId active=inventory.ActiveItem.id;
                if(active==ItemId.BuildingPlan)return "ЧЕРТЁЖ · "+BuildingPiece2D.PartName(currentPart)+" · ЭТАЖ "+(currentPart==BuildPart2D.Foundation?1:currentLevel+1)+"  |  R / КОЛЕСО СЕКЦИЯ · T ЭТАЖ · ЛКМ СТРОИТЬ";
                if(active==ItemId.Hammer)
                {
                    BuildingPiece2D target=PieceAtPointer();
                    if(target!=null)return "МОЛОТОК · "+BuildingPiece2D.GradeName(target.Grade).ToUpperInvariant()+" · "+Mathf.CeilToInt(target.Condition)+"/"+Mathf.CeilToInt(target.MaxCondition)+"  |  ЛКМ ПОЧИНИТЬ / УЛУЧШИТЬ · ПКМ РАЗОБРАТЬ";
                    return "МОЛОТОК  |  ЛКМ УЛУЧШИТЬ / ПОЧИНИТЬ · ПКМ РАЗОБРАТЬ";
                }
                if(IsDeployable(active))return "РАЗМЕЩЕНИЕ · "+ItemCatalog.Get(active).name.ToUpperInvariant()+"  |  ЛКМ УСТАНОВИТЬ";
                return "";
            }
        }

        void Awake()
        {
            player=GetComponent<PlayerController>();inventory=GetComponent<InventorySystem>();
            CreateGhost();
        }

        public void Initialize(PlayerController owner,InventorySystem bag)
        {
            player=owner;inventory=bag;
        }

        void CreateGhost()
        {
            if(ghostObject!=null)return;
            ghostObject=new GameObject("Building placement preview");
            ghostRenderer=ghostObject.AddComponent<SpriteRenderer>();ghostRenderer.sortingOrder=14;ghostRenderer.enabled=false;
        }

        void OnDestroy()
        {
            if(ghostObject!=null)Destroy(ghostObject);
        }

        void Update()
        {
            if(!GameHUD.IsPlaying||RunState.Instance==null||RunState.Instance.IsDead||RunState.Instance.HasEscaped||inventory==null)
            {
                if(ghostRenderer!=null)ghostRenderer.enabled=false;
                return;
            }

            ItemId active=inventory.ActiveItem.id;
            bool plan=active==ItemId.BuildingPlan,hammer=active==ItemId.Hammer,deployable=IsDeployable(active);
            bool aiming=plan||hammer||deployable;
            Cursor.lockState=CursorLockMode.None;Cursor.visible=aiming;
            if(!aiming)
            {
                if(ghostRenderer!=null)ghostRenderer.enabled=false;
                return;
            }

            if(plan)
            {
                if(Input.GetKeyDown(KeyCode.R))CyclePart(1);
                if(currentPart!=BuildPart2D.Foundation&&Input.GetKeyDown(KeyCode.T))currentLevel=(currentLevel+1)%3;
                UpdateBuildingPreview();
                if(Input.GetMouseButtonDown(0)&&!PointerOverHud())TryPlaceBuilding();
            }
            else if(hammer)
            {
                if(ghostRenderer!=null)ghostRenderer.enabled=false;
                if(!PointerOverHud()&&Input.GetMouseButtonDown(0))UpgradeAtPointer();
                if(!PointerOverHud()&&Input.GetMouseButtonDown(1))DemolishAtPointer();
            }
            else
            {
                UpdateDeployablePreview(active);
                if(Input.GetMouseButtonDown(0)&&!PointerOverHud())TryPlaceDeployable(active);
            }
        }

        public void CyclePart(int direction)
        {
            int index=System.Array.IndexOf(Parts,currentPart);index=(index+direction+Parts.Length)%Parts.Length;
            currentPart=Parts[index];
            if(currentPart==BuildPart2D.Foundation)currentLevel=0;
        }

        void UpdateBuildingPreview()
        {
            EnsureCamera();if(worldCamera==null)return;CreateGhost();
            Vector2 pointer=PointerWorld();ghostGridX=Mathf.RoundToInt(pointer.x/GridWidth);
            int level=currentPart==BuildPart2D.Foundation?0:currentLevel;
            Vector3 position=BuildingPosition(currentPart,ghostGridX,level);
            ghostValid=CanPlace(currentPart,ghostGridX,level,out ghostReason);
            Sprite previewSprite=BuildingArt2D.Get(currentPart,BuildingGrade2D.Twig);
            if(ghostRenderer.sprite!=previewSprite){ghostRenderer.sprite=previewSprite;RendererLibrary2D.Configure(ghostRenderer);}
            ghostRenderer.transform.position=position;ghostRenderer.color=ghostValid?new Color(.40f,1f,.50f,.55f):new Color(1f,.28f,.20f,.53f);
            ghostRenderer.enabled=true;
        }

        void TryPlaceBuilding()
        {
            int level=currentPart==BuildPart2D.Foundation?0:currentLevel;
            if(!ghostValid){RunState.Instance?.Notify(ghostReason,1.4f);return;}
            if(!inventory.TrySpend(ItemId.Wood,10))return;
            var go=new GameObject("Player Building · "+BuildingPiece2D.PartName(currentPart));
            if(transform.parent!=null)go.transform.SetParent(transform.parent,false);
            go.transform.position=BuildingPosition(currentPart,ghostGridX,level);
            go.AddComponent<BuildingPiece2D>().Initialize(currentPart,ghostGridX,level,BuildingGrade2D.Twig);
            RunState.Instance?.Notify(BuildingPiece2D.PartName(currentPart)+" поставлена из веток. Молоток улучшает прочность.");
            AudioDirector.Instance?.Play("metal",.35f);
        }

        bool CanPlace(BuildPart2D part,int gridX,int level,out string reason)
        {
            reason="";
            Vector3 position=BuildingPosition(part,gridX,level);
            if(Vector2.Distance(player.transform.position,position)>MaxReach){reason="Подойди ближе к месту строительства.";return false;}
            Vector2 partSize=new Vector2(1.88f,Mathf.Max(.20f,BuildingPiece2D.PartHeight(part)-.04f));
            if(HasSolidOverlap(position,partSize,out _)){reason="Здесь мешает игрок или геометрия уровня.";return false;}
            if(HasDeployableOverlap(position,partSize)){reason="Здесь уже стоит контейнер или верстак.";return false;}
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())
            {
                if(piece.GridX!=gridX||piece.Level!=level)continue;
                if((part==BuildPart2D.Foundation&&piece.Part==BuildPart2D.Foundation)||
                   ((part==BuildPart2D.Wall||part==BuildPart2D.Doorway)&&(piece.Part==BuildPart2D.Wall||piece.Part==BuildPart2D.Doorway))||
                   (part==BuildPart2D.Floor&&piece.Part==BuildPart2D.Floor))
                {reason="В этой ячейке уже есть секция.";return false;}
            }

            if(part==BuildPart2D.Foundation)
            {
                if(level!=0){reason="Фундамент ставится только на уровне земли.";return false;}
                return true;
            }
            if(part==BuildPart2D.Wall||part==BuildPart2D.Doorway)
            {
                bool supported=level==0?HasPiece(gridX,0,BuildPart2D.Foundation):HasPiece(gridX,level-1,BuildPart2D.Floor);
                if(!supported){reason=level==0?"Сначала поставь фундамент под этой стеной.":"Для верхней стены нужно перекрытие снизу.";return false;}
                return true;
            }
            if(!HasPiece(gridX,level,BuildPart2D.Wall)&&!HasPiece(gridX,level,BuildPart2D.Doorway))
            {reason="Перекрытие должно опираться на стену или дверной проём.";return false;}
            return true;
        }

        bool HasPiece(int gridX,int level,BuildPart2D part)
        {
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())if(piece.GridX==gridX&&piece.Level==level&&piece.Part==part)return true;
            return false;
        }

        static Vector3 BuildingPosition(BuildPart2D part,int gridX,int level)
        {
            float x=gridX*GridWidth;
            switch(part)
            {
                case BuildPart2D.Foundation:return new Vector3(x,.23f,0);
                case BuildPart2D.Wall:case BuildPart2D.Doorway:return new Vector3(x,FoundationTop+WallHeight*.5f+level*LevelStep,0);
                default:return new Vector3(x,FoundationTop+WallHeight+BuildingPiece2D.PartHeight(BuildPart2D.Floor)*.5f+level*LevelStep,0);
            }
        }

        void UpdateDeployablePreview(ItemId id)
        {
            EnsureCamera();if(worldCamera==null)return;CreateGhost();
            Vector2 pointer=PointerWorld();int gridX=Mathf.RoundToInt(pointer.x/GridWidth);Vector3 position=new Vector3(gridX*GridWidth,0,0);
            ghostGridX=gridX;ghostValid=CanPlaceDeployableAt(id,position,out ghostReason);
            Sprite previewSprite=id==ItemId.StorageBox?PixelArtFactory.Crate2D(1):id==ItemId.ToolCupboard?PixelArtFactory.Crate2D(3):PixelArtFactory.Workbench2D(id==ItemId.WorkbenchStationII?2:1);
            if(ghostRenderer.sprite!=previewSprite){ghostRenderer.sprite=previewSprite;RendererLibrary2D.Configure(ghostRenderer);}
            ghostRenderer.transform.position=position;ghostRenderer.color=ghostValid?new Color(.42f,1f,.48f,.56f):new Color(1f,.27f,.19f,.54f);
            ghostRenderer.enabled=true;
        }

        bool CanPlaceDeployableAt(ItemId id,Vector3 position,out string reason)
        {
            reason="";
            if(Vector2.Distance(player.transform.position,position)>MaxReach){reason="Подойди ближе к месту размещения.";return false;}
            bool station=id==ItemId.WorkbenchStationI||id==ItemId.WorkbenchStationII;
            float deployWidth=station?2.70f:1.36f,deployHeight=station?1.65f:1.16f;
            Vector2 footprintCenter=new Vector2(position.x,position.y+(station ? 0.82f : 0.56f));
            Vector2 footprintSize=new Vector2(deployWidth,deployHeight);
            if(HasSolidOverlap(footprintCenter,footprintSize,out _,false,true)){reason="Здесь мешает игрок или геометрия уровня.";return false;}
            if(HasDeployableOverlap(footprintCenter,footprintSize)){reason="Здесь уже стоит контейнер или верстак. Выбери свободное место.";return false;}
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())
            {
                // A foundation may support a box or station; walls, doorways and floors may not overlap it.
                if(piece.Part==BuildPart2D.Foundation)continue;
                var pieceBounds=new Bounds(piece.transform.position,new Vector3(1.96f,BuildingPiece2D.PartHeight(piece.Part),.1f));
                if(Overlaps(footprintCenter,footprintSize,pieceBounds)){reason="Здесь мешает секция постройки. Выбери свободную ячейку.";return false;}
            }
            return true;
        }

        void TryPlaceDeployable(ItemId id)
        {
            Vector3 position=new Vector3(ghostGridX*GridWidth,0,0);
            if(!ghostValid){RunState.Instance?.Notify(ghostReason,1.4f);return;}
            if(!inventory.TrySpend(id,1))return;
            var go=new GameObject("Player Placed · "+ItemCatalog.Get(id).name);
            if(transform.parent!=null)go.transform.SetParent(transform.parent,false);
            go.transform.position=position;
            if(id==ItemId.StorageBox)
            {
                go.AddComponent<LootContainer>().InitializePlayerStorage("Домашний ящик");
                RunState.Instance?.Notify("Домашнее хранилище размещено. E — открыть; мировые лут-кейсы остаются отдельными.");
            }
            else if(id==ItemId.ToolCupboard)
            {
                var storage=go.AddComponent<LootContainer>();storage.InitializeToolCupboard("Шкаф строительных прав");
                go.AddComponent<ToolCupboard2D>().Initialize(storage);
                RunState.Instance?.Notify("Шкаф строительных прав размещён. E — открыть, положи upkeep-ресурсы; радиус привилегии 12 м.");
            }
            else
            {
                int tier=id==ItemId.WorkbenchStationII?2:1;
                go.AddComponent<WorkbenchStation>().Initialize(tier,"Верстак "+(tier==1?"I":"II"),true);
                if(Vector2.Distance(player.transform.position,position)<3f)inventory.WorkbenchTier=Mathf.Max(inventory.WorkbenchTier,tier);
                RunState.Instance?.Notify("Верстак "+(tier==1?"I":"II")+" размещён. Подойди к нему, чтобы использовать рецепты.");
            }
            AudioDirector.Instance?.Play("metal",.45f);
        }

        void UpgradeAtPointer()
        {
            BuildingPiece2D piece=PieceAtPointer();
            if(piece==null){RunState.Instance?.Notify("Наведи молоток на свою постройку.",1.1f);return;}
            piece.TryUpgrade(inventory);
        }

        void DemolishAtPointer()
        {
            BuildingPiece2D piece=PieceAtPointer();
            if(piece==null)
            {
                if(DemolishDeployableAtPointer())return;
                RunState.Instance?.Notify("Наведи молоток на свою секцию, ящик или верстак.",1.1f);return;
            }
            if(HasSupportedPieces(piece))
            {RunState.Instance?.Notify("Сначала разбери секции, которые опираются на эту часть.");return;}
            if(!inventory.Add(ItemId.Wood,5)){RunState.Instance?.Notify("Освободи место, чтобы забрать доски.");return;}
            RunState.Instance?.Notify("Секция разобрана · возвращено 5 досок.");Destroy(piece.gameObject);
        }

        bool DemolishDeployableAtPointer()
        {
            EnsureCamera();if(worldCamera==null)return false;Vector2 pointer=PointerWorld();
            foreach(var container in FindObjectsOfType<LootContainer>())
            {
                if((!container.IsPlayerStorage&&!container.IsToolCupboard)||Mathf.Abs(pointer.x-container.transform.position.x)>.78f||Mathf.Abs(pointer.y-(container.transform.position.y+.56f))>.70f)continue;
                string name=container.IsToolCupboard?"шкаф строительных прав":"домашний ящик";
                if(!container.Empty){RunState.Instance?.Notify("Сначала опустоши "+name+".");return true;}
                ItemId item=container.IsToolCupboard?ItemId.ToolCupboard:ItemId.StorageBox;
                if(!inventory.Add(item,1)){RunState.Instance?.Notify("Освободи место для возвращаемого предмета.");return true;}
                Destroy(container.gameObject);RunState.Instance?.Notify(name+" разобран и возвращён в рюкзак.");return true;
            }
            foreach(var station in FindObjectsOfType<WorkbenchStation>())
            {
                if(!station.PlayerPlaced||Mathf.Abs(pointer.x-station.transform.position.x)>1.35f||Mathf.Abs(pointer.y-(station.transform.position.y+.82f))>.95f)continue;
                ItemId item=station.Tier>=2?ItemId.WorkbenchStationII:ItemId.WorkbenchStationI;
                if(!inventory.Add(item,1)){RunState.Instance?.Notify("Освободи место для возвращаемого верстака.");return true;}
                Destroy(station.gameObject);RunState.Instance?.Notify("Верстак разобран и возвращён в рюкзак.");return true;
            }
            return false;
        }

        bool HasSupportedPieces(BuildingPiece2D target)
        {
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())
            {
                if(piece==target||piece.GridX!=target.GridX)continue;
                if(target.Part==BuildPart2D.Foundation&&piece.Level==0&&(piece.Part==BuildPart2D.Wall||piece.Part==BuildPart2D.Doorway))return true;
                if(target.Part==BuildPart2D.Floor&&piece.Level==target.Level+1&&(piece.Part==BuildPart2D.Wall||piece.Part==BuildPart2D.Doorway))return true;
                if((target.Part==BuildPart2D.Wall||target.Part==BuildPart2D.Doorway)&&piece.Part==BuildPart2D.Floor&&piece.Level==target.Level)return true;
            }
            return false;
        }

        bool HasSolidOverlap(Vector2 center,Vector2 size,out Collider2D blocker,bool ignorePlayer=true,bool ignoreBuildingPieces=false)
        {
            foreach(var collider in Physics2D.OverlapBoxAll(center,size,0f))
            {
                if(collider==null||collider.isTrigger)continue;
                if(ignorePlayer&&(collider.transform==transform||collider.transform.IsChildOf(transform)))continue;
                if(ignoreBuildingPieces&&collider.GetComponentInParent<BuildingPiece2D>()!=null)continue;
                blocker=collider;return true;
            }
            blocker=null;return false;
        }

        static bool HasDeployableOverlap(Vector2 center,Vector2 size)
        {
            foreach(var container in FindObjectsOfType<LootContainer>())
            {
                Collider2D collider=container.GetComponent<Collider2D>();
                if(collider!=null&&Overlaps(center,size,collider.bounds))return true;
            }
            foreach(var station in FindObjectsOfType<WorkbenchStation>())
            {
                Collider2D collider=station.GetComponent<Collider2D>();
                if(collider!=null&&Overlaps(center,size,collider.bounds))return true;
            }
            return false;
        }

        static bool Overlaps(Vector2 center,Vector2 size,Bounds other)
        {
            return Mathf.Abs(center.x-other.center.x)<size.x*.5f+other.extents.x&&
                   Mathf.Abs(center.y-other.center.y)<size.y*.5f+other.extents.y;
        }

        BuildingPiece2D PieceAtPointer()
        {
            EnsureCamera();if(worldCamera==null)return null;
            Vector2 pointer=PointerWorld();BuildingPiece2D found=null;float best=float.MaxValue;
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())
            {
                if(!piece.HitTest(pointer))continue;
                float distance=((Vector2)piece.transform.position-pointer).sqrMagnitude;
                if(distance<best){best=distance;found=piece;}
            }
            return found;
        }

        Vector2 PointerWorld()
        {
            Vector3 screen=Input.mousePosition;screen.z=Mathf.Abs(worldCamera.transform.position.z);
            Vector3 point=worldCamera.ScreenToWorldPoint(screen);return new Vector2(point.x,point.y);
        }

        void EnsureCamera(){if(worldCamera==null)worldCamera=Camera.main!=null?Camera.main:FindObjectOfType<Camera>();}
        bool PointerOverHud()=>Input.mousePosition.y<100f||Input.mousePosition.y>Screen.height-100f;
        static bool IsDeployable(ItemId id)=>id==ItemId.StorageBox||id==ItemId.ToolCupboard||id==ItemId.WorkbenchStationI||id==ItemId.WorkbenchStationII;
        static bool IsBuildItem(ItemId id)=>id==ItemId.BuildingPlan||id==ItemId.Hammer||IsDeployable(id);

        /// <summary>Collapse all upper sections that lose their only grid support.</summary>
        public static void CollapseUnsupportedFrom(BuildingPiece2D removed)
        {
            if(removed==null)return;
            var pieces=FindObjectsOfType<BuildingPiece2D>();var collapsing=new HashSet<BuildingPiece2D>{removed};
            bool changed=true;
            while(changed)
            {
                changed=false;
                foreach(var piece in pieces)
                {
                    if(piece==null||collapsing.Contains(piece))continue;
                    if(!HasStructuralSupport(piece,pieces,collapsing)){collapsing.Add(piece);changed=true;}
                }
            }
            int dependents=collapsing.Count-1;
            foreach(var piece in collapsing)if(piece!=null&&piece!=removed)Destroy(piece.gameObject);
            if(dependents>0)RunState.Instance?.Notify("Обрушились неподдержанные секции: "+dependents+".",3f);
        }

        static bool HasStructuralSupport(BuildingPiece2D target,BuildingPiece2D[] pieces,HashSet<BuildingPiece2D> removed)
        {
            if(target.Part==BuildPart2D.Foundation)return true;
            BuildPart2D requiredPart=target.Part==BuildPart2D.Floor?BuildPart2D.Wall:BuildPart2D.Foundation;
            int level=target.Level;
            if(target.Part==BuildPart2D.Wall||target.Part==BuildPart2D.Doorway)
            {
                if(level>0){requiredPart=BuildPart2D.Floor;level--;}
            }
            foreach(var piece in pieces)
            {
                if(piece==null||removed.Contains(piece)||piece.GridX!=target.GridX||piece.Level!=level)continue;
                if(piece.Part==requiredPart||(requiredPart==BuildPart2D.Wall&&piece.Part==BuildPart2D.Doorway))return true;
                if(requiredPart==BuildPart2D.Wall&&piece.Part==BuildPart2D.Wall)return true;
            }
            return false;
        }

        /// <summary>New runs clear only player-built objects; abandoned supply caches are not touched.</summary>
        public static void ClearPlayerPlacedObjects()
        {
            foreach(var piece in FindObjectsOfType<BuildingPiece2D>())Destroy(piece.gameObject);
            foreach(var container in FindObjectsOfType<LootContainer>())if(container.IsPlayerStorage||container.IsToolCupboard)Destroy(container.gameObject);
            foreach(var station in FindObjectsOfType<WorkbenchStation>())if(station.PlayerPlaced)Destroy(station.gameObject);
        }
    }

    /// <summary>Small, cached high-contrast pixel sprites generated for the structural grades.</summary>
    static class BuildingArt2D
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        const int Width=192;
        static readonly Color32 Clear=new Color32(0,0,0,0);

        public static Sprite Get(BuildPart2D part,BuildingGrade2D grade)
        {
            string key=part+"_"+grade;
            if(cache.TryGetValue(key,out var sprite)&&sprite!=null)return sprite;
            int height=part==BuildPart2D.Foundation?44:part==BuildPart2D.Floor?30:230;
            var pixels=new Color32[Width*height];for(int i=0;i<pixels.Length;i++)pixels[i]=Clear;
            Palette(grade,out Color32 dark,out Color32 baseColor,out Color32 light,out Color32 accent,out Color32 mortar);
            if(part==BuildPart2D.Foundation||part==BuildPart2D.Floor)
            {
                Fill(pixels,height,2,2,Width-4,height-4,dark);
                Fill(pixels,height,5,5,Width-10,height-10,baseColor);
                if(grade==BuildingGrade2D.Stone)StonePattern(pixels,height,5,5,Width-10,height-10,light,mortar,dark);
                else if(grade==BuildingGrade2D.Twig)TwigPattern(pixels,height,5,5,Width-10,height-10,light,accent,dark);
                else PlankPattern(pixels,height,5,5,Width-10,height-10,light,accent,dark,grade);
                Fill(pixels,height,7,height-8,Width-14,2,light);
            }
            else
            {
                Fill(pixels,height,2,2,Width-4,height-4,dark);
                Fill(pixels,height,6,6,Width-12,height-12,baseColor);
                if(grade==BuildingGrade2D.Twig)TwigPattern(pixels,height,6,6,Width-12,height-12,light,accent,dark);
                else if(grade==BuildingGrade2D.Stone)StonePattern(pixels,height,6,6,Width-12,height-12,light,mortar,dark);
                else PlankPattern(pixels,height,6,6,Width-12,height-12,light,accent,dark,grade);
                if(part==BuildPart2D.Doorway)
                {
                    Fill(pixels,height,56,0,80,164,Clear);
                    Fill(pixels,height,49,158,94,7,dark);
                    Fill(pixels,height,55,164,82,3,light);
                    Fill(pixels,height,49,height-17,94,7,dark);
                }
            }
            var texture=new Texture2D(Width,height,TextureFormat.RGBA32,false)
            {name="Building_"+key,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels32(pixels);texture.Apply(false,false);
            sprite=Sprite.Create(texture,new Rect(0,0,Width,height),new Vector2(.5f,.5f),96f,0,SpriteMeshType.FullRect,Vector4.zero,false);
            sprite.name="building_"+key;cache[key]=sprite;return sprite;
        }

        static void Palette(BuildingGrade2D grade,out Color32 dark,out Color32 baseColor,out Color32 light,out Color32 accent,out Color32 mortar)
        {
            switch(grade)
            {
                case BuildingGrade2D.Twig:dark=C(35,27,21);baseColor=C(104,73,43);light=C(196,147,84);accent=C(137,94,52);mortar=C(67,49,33);break;
                case BuildingGrade2D.Wood:dark=C(31,25,20);baseColor=C(124,82,47);light=C(206,154,83);accent=C(157,107,58);mortar=C(63,45,31);break;
                case BuildingGrade2D.Stone:dark=C(28,32,31);baseColor=C(98,101,91);light=C(171,172,153);accent=C(125,129,116);mortar=C(49,54,50);break;
                case BuildingGrade2D.Metal:dark=C(18,24,24);baseColor=C(73,85,82);light=C(171,181,170);accent=C(112,129,124);mortar=C(38,47,45);break;
                default:dark=C(12,17,18);baseColor=C(47,56,56);light=C(141,153,148);accent=C(78,96,94);mortar=C(25,34,35);break;
            }
        }

        static void PlankPattern(Color32[] pixels,int height,int x,int y,int width,int h,Color32 light,Color32 accent,Color32 dark,BuildingGrade2D grade)
        {
            int boards=(int)grade>=(int)BuildingGrade2D.Metal?4:8;
            for(int i=1;i<boards;i++)
            {
                int seam=x+width*i/boards;Fill(pixels,height,seam,y,2,h,dark);
                Fill(pixels,height,seam+2,y+3,1,h-6,accent);
                if(i%2==0)Fill(pixels,height,seam+5,y+h/3,6,2,light);
            }
            if((int)grade>=(int)BuildingGrade2D.Metal)
            {
                for(int row=1;row<4;row++){int yy=y+h*row/4;Fill(pixels,height,x,yy,width,2,dark);Fill(pixels,height,x+2,yy+2,width-4,1,accent);}
                for(int xx=x+10;xx<x+width-6;xx+=24){Fill(pixels,height,xx,y+8,3,3,light);Fill(pixels,height,xx,y+h-11,3,3,light);}
            }
            else
            {
                Fill(pixels,height,x+4,y+h/2,width-8,2,dark);
                Fill(pixels,height,x+7,y+h/2+3,width-14,1,accent);
            }
            Fill(pixels,height,x+4,y+h-6,width-8,2,light);
        }

        static void StonePattern(Color32[] pixels,int height,int x,int y,int width,int h,Color32 light,Color32 mortar,Color32 dark)
        {
            int rows=Mathf.Max(2,h/28),rowHeight=Mathf.Max(8,h/rows);
            for(int row=0;row<rows;row++)
            {
                int yy=y+row*rowHeight;Fill(pixels,height,x,yy,width,2,mortar);
                int offset=row%2==0?0:width/5;
                for(int seam=x+offset+width/5;seam<x+width;seam+=width/3)
                {Fill(pixels,height,seam,yy+2,2,Mathf.Max(2,rowHeight-2),mortar);Fill(pixels,height,seam+3,yy+4,1,Mathf.Max(1,rowHeight-7),dark);}
                Fill(pixels,height,x+4,yy+rowHeight-5,width-8,2,light);
            }
            Fill(pixels,height,x,y,width,2,light);
        }

        static void TwigPattern(Color32[] pixels,int height,int x,int y,int width,int h,Color32 light,Color32 accent,Color32 dark)
        {
            int midY=y+h/2;
            Line(pixels,height,x+5,y+5,x+width-6,y+h-6,light);
            Line(pixels,height,x+width-6,y+5,x+5,y+h-6,accent);
            Line(pixels,height,x+5,midY,x+width-6,midY,dark);
            Line(pixels,height,x+width/3,y+4,x+width/2,y+h-5,accent);
            Line(pixels,height,x+width*2/3,y+4,x+width/2,y+h-5,light);
            Fill(pixels,height,x+4,y+h-7,width-8,3,dark);
            Fill(pixels,height,x+7,y+h-11,width-14,2,light);
        }

        static void Line(Color32[] pixels,int height,int x0,int y0,int x1,int y1,Color32 color)
        {
            int dx=Mathf.Abs(x1-x0),sx=x0<x1?1:-1,dy=-Mathf.Abs(y1-y0),sy=y0<y1?1:-1,error=dx+dy;
            while(true)
            {
                Fill(pixels,height,x0,y0,3,3,color);if(x0==x1&&y0==y1)break;
                int twice=2*error;if(twice>=dy){error+=dy;x0+=sx;}if(twice<=dx){error+=dx;y0+=sy;}
            }
        }

        static void Fill(Color32[] pixels,int height,int x,int y,int width,int h,Color32 color)
        {
            int left=Mathf.Max(0,x),right=Mathf.Min(Width,x+width),bottom=Mathf.Max(0,y),top=Mathf.Min(height,y+h);
            for(int yy=bottom;yy<top;yy++)for(int xx=left;xx<right;xx++)pixels[yy*Width+xx]=color;
        }

        static Color32 C(byte r,byte g,byte b)=>new Color32(r,g,b,255);
    }
}
