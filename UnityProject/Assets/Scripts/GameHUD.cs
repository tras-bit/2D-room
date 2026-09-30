using UnityEngine;

namespace Subsistence
{
    /// <summary>Complete title, controls, audio settings, pause, HUD and end-state flow for the field-test build.</summary>
    public sealed class GameHUD : MonoBehaviour
    {
        public enum ScreenMode { MainMenu, Playing, Paused, Settings, Controls, Defeat, Victory }
        public static ScreenMode Mode { get; private set; } = ScreenMode.MainMenu;
        public static bool IsPlaying => Mode == ScreenMode.Playing;

        GUIStyle tiny, small, body, heading, title, button, mono;
        Texture2D buttonTexture, buttonHoverTexture;
        readonly Color ink = new Color(.92f,.92f,.85f);
        readonly Color lime = new Color(.82f,.94f,.56f);
        readonly Color muted = new Color(.64f,.69f,.60f);
        float uiScale;
        ScreenMode priorMenu = ScreenMode.MainMenu;
        bool fullscreen = true;

        void Awake()
        {
            Mode = ScreenMode.MainMenu;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Screen.fullScreen = fullscreen;
        }

        void Update()
        {
            if (Mode == ScreenMode.Playing && Input.GetKeyDown(KeyCode.Escape)) SetMode(ScreenMode.Paused);
            if (Mode == ScreenMode.Playing && RunState.Instance != null)
            {
                if (RunState.Instance.IsDead) SetMode(ScreenMode.Defeat);
                else if (RunState.Instance.HasEscaped) SetMode(ScreenMode.Victory);
            }
        }

        void EnsureStyles()
        {
            uiScale = Mathf.Clamp(Screen.width / 1180f,.72f,1.35f);
            if (tiny != null && Mathf.Abs(tiny.fontSize-Mathf.RoundToInt(8*uiScale))<1) return;
            buttonTexture = MakeTexture(new Color(.81f,.91f,.59f));
            buttonHoverTexture = MakeTexture(new Color(.91f,.98f,.71f));
            tiny = new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(8*uiScale),richText=true};
            small = new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(10*uiScale),richText=true,wordWrap=true};
            body = new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(12*uiScale),richText=true,wordWrap=true};
            heading = new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(25*uiScale),fontStyle=FontStyle.Bold,richText=true,wordWrap=true};
            title = new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(37*uiScale),fontStyle=FontStyle.Bold,richText=true,wordWrap=true};
            mono = new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(9*uiScale),font=Font.CreateDynamicFontFromOSFont("Consolas",12),richText=true,wordWrap=true};
            button = new GUIStyle(GUI.skin.button);
            button.fontSize=Mathf.RoundToInt(10*uiScale);
            button.font=Font.CreateDynamicFontFromOSFont("Consolas",12);
            button.fontStyle=FontStyle.Bold;
            button.alignment=TextAnchor.MiddleLeft;
            button.padding=new RectOffset(Mathf.RoundToInt(14*uiScale),10,0,0);
            button.normal.background=buttonTexture;button.normal.textColor=new Color(.12f,.17f,.12f);
            button.hover.background=buttonHoverTexture;button.hover.textColor=new Color(.12f,.17f,.12f);
            button.active.background=buttonHoverTexture;button.active.textColor=new Color(.12f,.17f,.12f);
        }

        void OnGUI()
        {
            if (RunState.Instance == null) return;
            EnsureStyles();
            if (Mode == ScreenMode.Playing) DrawGameHUD();
            else if (Mode == ScreenMode.MainMenu) DrawMainMenu();
            else if (Mode == ScreenMode.Settings) DrawSettings();
            else if (Mode == ScreenMode.Controls) DrawControls();
            else if (Mode == ScreenMode.Paused) DrawPause();
            else DrawEndCard(Mode == ScreenMode.Victory);
        }

        void DrawMainMenu()
        {
            DrawPanel(new Rect(0,0,Screen.width,Screen.height),new Color(.025f,.04f,.03f,.67f));
            float w=Mathf.Min(415*uiScale,Screen.width*.83f),h=Mathf.Min(460*uiScale,Screen.height*.88f);
            float x=Screen.width-w-36*uiScale,y=(Screen.height-h)*.5f;
            DrawPanel(new Rect(x,y,w,h),new Color(.065f,.085f,.067f,.95f));
            DrawPanel(new Rect(x,y,3*uiScale,h),new Color(.77f,.87f,.51f));
            Label("●  FIELD TEST 001     /     UNITY 2022.3.62f2",new Rect(x+25*uiScale,y+23*uiScale,w-48*uiScale,18*uiScale),mono,lime);
            Label("SUBSISTENCE",new Rect(x+22*uiScale,y+67*uiScale,w-44*uiScale,62*uiScale),title,ink);
            Label("НЕ ОСТАВАЙСЯ НА СВЕТУ.",new Rect(x+25*uiScale,y+128*uiScale,w-50*uiScale,22*uiScale),mono,lime);
            Label("Ты не помнишь, как сюда попал. Собери припасы, переживи смену и доберись до работающего выхода.",new Rect(x+25*uiScale,y+166*uiScale,w-50*uiScale,58*uiScale),body,muted);
            float bw=w-50*uiScale,bh=39*uiScale,bx=x+25*uiScale;
            if(Button("НАЧАТЬ СМЕНУ   →",new Rect(bx,y+238*uiScale,bw,bh))) StartRun();
            if(Button("УПРАВЛЕНИЕ",new Rect(bx,y+286*uiScale,bw,bh))) SetMode(ScreenMode.Controls);
            if(Button("НАСТРОЙКИ ЗВУКА И ЭКРАНА",new Rect(bx,y+334*uiScale,bw,bh))) {priorMenu=ScreenMode.MainMenu;SetMode(ScreenMode.Settings);}
            if(Button("ВЫХОД",new Rect(bx,y+382*uiScale,bw,bh))) Application.Quit();
            Label("2D SURVIVAL  ·  ОДИН ИГРОК  ·  ПРОТОТИП",new Rect(x+25*uiScale,y+h-34*uiScale,w-50*uiScale,17*uiScale),tiny,muted);
            Label("A/D ИЛИ ← →  ДВИЖЕНИЕ     SPACE  ПРЫЖОК",new Rect(24*uiScale,Screen.height-32*uiScale,Screen.width*.50f,16*uiScale),mono,ink);
        }

        void DrawGameHUD()
        {
            RunState state=RunState.Instance;
            float p=16*uiScale,w=238*uiScale;
            DrawPanel(new Rect(p,p,w,157*uiScale),new Color(.055f,.075f,.06f,.88f));
            Label("SUBSISTENCE  /  FIELD TEST 001",new Rect(p+12*uiScale,p+9*uiScale,w-24*uiScale,16*uiScale),mono,lime);
            Label("CURRENT OBJECTIVE",new Rect(p+12*uiScale,p+31*uiScale,w-24*uiScale,15*uiScale),tiny,muted);
            string goal=state.SuppliesFound<3?$"Найди припасы · {state.SuppliesFound} / 3":state.Scrap<1?"Найди металл для выхода":"Доберись до выхода →";
            Label(goal,new Rect(p+12*uiScale,p+48*uiScale,w-24*uiScale,21*uiScale),body,ink);
            DrawBar(p+12*uiScale,p+80*uiScale,214*uiScale,state.Health/100f,new Color(.76f,.40f,.31f),"ЗДОРОВЬЕ");
            DrawBar(p+12*uiScale,p+102*uiScale,214*uiScale,state.Hunger/100f,new Color(.81f,.66f,.38f),"ГОЛОД");
            DrawBar(p+12*uiScale,p+124*uiScale,214*uiScale,state.Thirst/100f,new Color(.45f,.69f,.63f),"ЖАЖДА");
            Label($"МЕТАЛЛ {state.Scrap:00}     ТКАНЬ {state.Cloth:00}",new Rect(p+12*uiScale,p+143*uiScale,w-24*uiScale,15*uiScale),mono,lime);

            float clock=18*60+42+Mathf.FloorToInt(state.ShiftSeconds*1.08f);
            string time=$"{(clock/60)%24:00}:{clock%60:00}";
            float rw=158*uiScale;
            DrawPanel(new Rect(Screen.width-p-rw,p,rw,48*uiScale),new Color(.055f,.075f,.06f,.88f));
            Label("●  "+time+"  /  СМЕНА 01",new Rect(Screen.width-p-rw+10*uiScale,p+8*uiScale,rw-18*uiScale,15*uiScale),mono,ink);
            Label("КОРИДОР ОБСЛУЖИВАНИЯ",new Rect(Screen.width-p-rw+10*uiScale,p+26*uiScale,rw-18*uiScale,14*uiScale),tiny,muted);
            DrawPanel(new Rect(p,Screen.height-43*uiScale,Screen.width-p*2,27*uiScale),new Color(.055f,.075f,.06f,.90f));
            Label("A/D ДВИЖЕНИЕ    SPACE ПРЫЖОК    E ПОДОБРАТЬ    Q ОТБИТЬСЯ    C ПЕРЕВЯЗКА    F ФОНАРЬ    ESC ПАУЗА",new Rect(p+10*uiScale,Screen.height-37*uiScale,Screen.width-p*2-20*uiScale,15*uiScale),mono,ink);
            DrawInteractHint();
            if(state.NoticeTime>0){float nw=Mathf.Min(440*uiScale,Screen.width*.75f);DrawPanel(new Rect((Screen.width-nw)/2,Screen.height-83*uiScale,nw,28*uiScale),new Color(.06f,.08f,.06f,.92f));Label(state.Notice,new Rect((Screen.width-nw)/2+11*uiScale,Screen.height-77*uiScale,nw-22*uiScale,16*uiScale),small,ink);}
        }

        void DrawInteractHint()
        {
            var player=FindObjectOfType<PlayerController>();if(player==null)return;
            SupplyPickup nearest=null;float best=1.5f;
            foreach(var item in FindObjectsOfType<SupplyPickup>())
            {float d=Vector2.Distance(player.transform.position,item.transform.position);if(d<best){best=d;nearest=item;}}
            string hint=nearest!=null?"[E]  ПОДОБРАТЬ ПРИПАС":Mathf.Abs(player.transform.position.x-RunState.ExitX)<2.8f?"[E]  ПРОЙТИ ЧЕРЕЗ ВЫХОД":"";
            if(string.IsNullOrEmpty(hint))return;
            float ww=230*uiScale;DrawPanel(new Rect((Screen.width-ww)/2,Screen.height-78*uiScale,ww,25*uiScale),new Color(.075f,.10f,.07f,.93f));
            Label(hint,new Rect((Screen.width-ww)/2+9*uiScale,Screen.height-73*uiScale,ww-18*uiScale,15*uiScale),mono,lime);
        }

        void DrawPause()
        {
            Overlay();
            float w=340*uiScale,h=300*uiScale,x=(Screen.width-w)/2,y=(Screen.height-h)/2;
            Card(x,y,w,h,"СМЕНА НА ПАУЗЕ","Снаружи что-то шуршит.");
            if(Button("ПРОДОЛЖИТЬ",new Rect(x+24*uiScale,y+110*uiScale,w-48*uiScale,38*uiScale))) SetMode(ScreenMode.Playing);
            if(Button("НАСТРОЙКИ",new Rect(x+24*uiScale,y+158*uiScale,w-48*uiScale,38*uiScale))){priorMenu=ScreenMode.Paused;SetMode(ScreenMode.Settings);}
            if(Button("ГЛАВНОЕ МЕНЮ",new Rect(x+24*uiScale,y+206*uiScale,w-48*uiScale,38*uiScale))) SetMode(ScreenMode.MainMenu);
        }

        void DrawSettings()
        {
            Overlay();float w=390*uiScale,h=300*uiScale,x=(Screen.width-w)/2,y=(Screen.height-h)/2;
            Card(x,y,w,h,"НАСТРОЙКИ","Экран и звук можно изменить в любой момент.");
            Label("ОБЩАЯ ГРОМКОСТЬ",new Rect(x+25*uiScale,y+110*uiScale,130*uiScale,20*uiScale),mono,ink);
            float volume=AudioDirector.Instance!=null?AudioDirector.Instance.MasterVolume:.72f;
            float changed=GUI.HorizontalSlider(new Rect(x+155*uiScale,y+113*uiScale,w-190*uiScale,18*uiScale),volume,0f,1f);
            if(Mathf.Abs(changed-volume)>.002f)AudioDirector.Instance?.SetMasterVolume(changed);
            Label(Mathf.RoundToInt(changed*100)+"%",new Rect(x+w-60*uiScale,y+109*uiScale,40*uiScale,20*uiScale),mono,lime);
            if(Button(fullscreen?"ПОЛНЫЙ ЭКРАН  ·  ВКЛ":"ПОЛНЫЙ ЭКРАН  ·  ВЫКЛ",new Rect(x+25*uiScale,y+153*uiScale,w-50*uiScale,36*uiScale))){fullscreen=!fullscreen;Screen.fullScreen=fullscreen;AudioDirector.Instance?.Play("ui",.4f);}
            if(Button("НАЗАД",new Rect(x+25*uiScale,y+207*uiScale,w-50*uiScale,36*uiScale)))SetMode(priorMenu);
        }

        void DrawControls()
        {
            Overlay();float w=430*uiScale,h=365*uiScale,x=(Screen.width-w)/2,y=(Screen.height-h)/2;
            Card(x,y,w,h,"УПРАВЛЕНИЕ","Сначала собери припасы. Один кусок металла нужен для выхода.");
            string[] controls={"A / D   или   ← / →       Движение","SPACE / W / ↑               Прыжок","E                                  Подобрать / использовать","Q                                  Оттолкнуть преследователя","C                                  Перевязка · 2 ткани","F                                  Фонарик","ESC                              Пауза"};
            for(int i=0;i<controls.Length;i++)Label(controls[i],new Rect(x+27*uiScale,y+(126+i*25)*uiScale,w-50*uiScale,20*uiScale),i==0?mono:small,i==0?lime:ink);
            if(Button("НАЗАД",new Rect(x+25*uiScale,y+h-55*uiScale,w-50*uiScale,36*uiScale)))SetMode(priorMenu);
        }

        void DrawEndCard(bool won)
        {
            Overlay();float w=400*uiScale,h=250*uiScale,x=(Screen.width-w)/2,y=(Screen.height-h)/2;
            Card(x,y,w,h,won?"ТЫ ВЫБРАЛСЯ.":"ТЕБЯ НАШЛИ.",won?"Первая смена позади. Но сигнал всё ещё зовёт.":"Смена завершена. Попробуй другой маршрут.");
            if(Button("НАЧАТЬ ЗАНОВО",new Rect(x+25*uiScale,y+139*uiScale,w-50*uiScale,38*uiScale)))StartRun();
            if(Button("ГЛАВНОЕ МЕНЮ",new Rect(x+25*uiScale,y+187*uiScale,w-50*uiScale,38*uiScale)))SetMode(ScreenMode.MainMenu);
        }

        void StartRun()
        {
            Time.timeScale=1f;RunState.Instance.ResetForNewRun();
            var player=FindObjectOfType<PlayerController>();if(player!=null)player.ResetForNewRun();
            var watcher=FindObjectOfType<WatcherAI>();if(watcher!=null)watcher.ResetForNewRun();
            foreach(var pickup in FindObjectsOfType<SupplyPickup>(true))pickup.ResetForNewRun();
            SetMode(ScreenMode.Playing);AudioDirector.Instance?.Play("ui",.7f);
        }

        void SetMode(ScreenMode next)
        {
            Mode=next;
            bool stopped=next!=ScreenMode.Playing;
            Time.timeScale=stopped?0f:1f;
            Cursor.visible=stopped;Cursor.lockState=CursorLockMode.None;
            if(next==ScreenMode.Settings && priorMenu==ScreenMode.MainMenu)Time.timeScale=0f;
            if(next==ScreenMode.MainMenu)priorMenu=ScreenMode.MainMenu;
            AudioDirector.Instance?.Play("ui",.45f);
        }

        void Card(float x,float y,float w,float h,string titleText,string subtitle)
        {
            DrawPanel(new Rect(x,y,w,h),new Color(.065f,.085f,.067f,.97f));DrawPanel(new Rect(x,y,3*uiScale,h),lime);
            Label("SUBSISTENCE  /  ПОЛЕВОЙ ПРОТОТИП",new Rect(x+23*uiScale,y+18*uiScale,w-46*uiScale,16*uiScale),mono,lime);
            Label(titleText,new Rect(x+22*uiScale,y+48*uiScale,w-44*uiScale,41*uiScale),heading,ink);
            Label(subtitle,new Rect(x+24*uiScale,y+89*uiScale,w-48*uiScale,35*uiScale),small,muted);
        }
        void Overlay()=>DrawPanel(new Rect(0,0,Screen.width,Screen.height),new Color(.025f,.035f,.028f,.72f));
        bool Button(string caption,Rect rect){return GUI.Button(rect,caption,button);}
        void DrawBar(float x,float y,float width,float ratio,Color color,string label)
        {
            Label(label,new Rect(x,y,width*.34f,14*uiScale),tiny,muted);
            DrawPanel(new Rect(x+width*.36f,y+4*uiScale,width*.47f,5*uiScale),new Color(.23f,.27f,.23f,1));
            DrawPanel(new Rect(x+width*.36f,y+4*uiScale,width*.47f*Mathf.Clamp01(ratio),5*uiScale),color);
            Label(Mathf.RoundToInt(ratio*100).ToString("00"),new Rect(x+width*.85f,y,width*.15f,14*uiScale),mono,ink);
        }
        static Texture2D MakeTexture(Color color){var t=new Texture2D(1,1);t.SetPixel(0,0,color);t.Apply();return t;}
        static void DrawPanel(Rect rect,Color color){Color before=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=before;}
        static void Label(string text,Rect rect,GUIStyle style,Color color){Color before=GUI.color;GUI.color=color;GUI.Label(rect,text,style);GUI.color=before;}
    }
}
