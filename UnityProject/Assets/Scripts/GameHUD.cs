using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Offline title/sector flow, 2.5D survival HUD, Rust-inspired inventory, loot, gear and crafting UI.</summary>
    public sealed class GameHUD : MonoBehaviour
    {
        public enum ScreenMode { MainMenu, ServerSelect, Playing, Inventory, Paused, Settings, Controls, Support, Defeat, Victory }
        public static ScreenMode Mode{get;private set;}=ScreenMode.MainMenu;
        public static bool IsPlaying=>Mode==ScreenMode.Playing;
        public static bool MenuBackdropActive{get;private set;}=true;
        GUIStyle tiny,small,body,heading,title,mono,lightButton,darkButton,slotStyle;
        Texture2D lightTex,lightHover,darkTex,darkHover,slotTex,slotHover,selectedTex;
        readonly Color ink=new Color(.91f,.91f,.84f),lime=new Color(.82f,.90f,.58f),muted=new Color(.62f,.67f,.59f),gold=new Color(.78f,.66f,.40f),copper=new Color(.77f,.36f,.18f);
        float uiScale,uiScaleSetting=.94f,brightnessSetting=.5f;ScreenMode returnMode=ScreenMode.MainMenu;bool fullscreen=true,craftTab,showInteractionHints=true,showNotices=true;int selectedSector,settingsTab,qualityIndex,vSyncSetting=1,resolutionIndex;
        CharacterPreview preview;InventorySystem bag;ItemStack held;int selectedKind=-1,selectedIndex=-1;
        Texture2D menuBackdrop;
        Resolution[] resolutions;
        string[] sectorNames={"ВЛОЖЕННЫЙ СЕКТОР #01","ОФИСНЫЙ ЛАБИРИНТ","ЗАТОПЛЕННЫЙ КОРПУС","ТЕХНИЧЕСКИЙ СЕКТОР"};
        string[] sectorInfo={"Жёлтые комнаты · низкий риск · базовые припасы","Пустые рабочие места · ресурсы и первые следы","Влажные коридоры · электроника · высокий риск","Оружейный тайник · броня · критический риск"};
        float[] sectorStarts={-19f,9f,39f,69f};

        void Awake()
        {
            menuBackdrop=Resources.Load<Texture2D>("Art/menu_backrooms");
            fullscreen=PlayerPrefs.GetInt("Subsistence.Fullscreen",1)!=0;
            brightnessSetting=PlayerPrefs.GetFloat("Subsistence.Brightness",.5f);
            uiScaleSetting=PlayerPrefs.GetFloat("Subsistence.UIScale",.94f);
            showInteractionHints=PlayerPrefs.GetInt("Subsistence.InteractionHints",1)!=0;
            showNotices=PlayerPrefs.GetInt("Subsistence.Notices",1)!=0;
            vSyncSetting=Mathf.Clamp(PlayerPrefs.GetInt("Subsistence.VSync",1),0,1);
            qualityIndex=Mathf.Clamp(PlayerPrefs.GetInt("Subsistence.Quality",QualitySettings.GetQualityLevel()),0,Mathf.Max(0,QualitySettings.names.Length-1));
            Resolution[] supported=Screen.resolutions;var unique=new List<Resolution>();
            if(supported!=null)foreach(var candidate in supported){bool duplicate=false;foreach(var existing in unique)if(existing.width==candidate.width&&existing.height==candidate.height){duplicate=true;break;}if(!duplicate)unique.Add(candidate);}
            if(unique.Count==0)unique.Add(Screen.currentResolution);resolutions=unique.ToArray();resolutionIndex=0;
            for(int i=0;i<resolutions.Length;i++)if(resolutions[i].width==Screen.width&&resolutions[i].height==Screen.height){resolutionIndex=i;break;}
            int savedW=PlayerPrefs.GetInt("Subsistence.ResWidth",0),savedH=PlayerPrefs.GetInt("Subsistence.ResHeight",0);
            for(int i=0;i<resolutions.Length;i++)if(resolutions[i].width==savedW&&resolutions[i].height==savedH){resolutionIndex=i;break;}
            Mode=ScreenMode.MainMenu;MenuBackdropActive=true;Time.timeScale=0;Cursor.visible=true;Cursor.lockState=CursorLockMode.None;Screen.fullScreen=fullscreen;
            ApplyVisualSettings();ApplyResolution();
        }
        void Start(){preview=gameObject.AddComponent<CharacterPreview>();}
        void Update()
        {
            if(Mode==ScreenMode.Playing)
            {
                if(Input.GetKeyDown(KeyCode.Escape))SetMode(ScreenMode.Paused);
                else if(Input.GetKeyDown(KeyCode.Tab)||Input.GetKeyDown(KeyCode.I))OpenInventory();
                var player=FindObjectOfType<PlayerController>();if(player!=null&&player.Inventory!=null&&player.Inventory.IsLootOpen)OpenInventory();
                if(RunState.Instance!=null){if(RunState.Instance.IsDead)SetMode(ScreenMode.Defeat);else if(RunState.Instance.HasEscaped)SetMode(ScreenMode.Victory);}
            }
            else if(Mode==ScreenMode.Inventory&&(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Tab)||Input.GetKeyDown(KeyCode.I)))CloseInventory();
            else if(Mode==ScreenMode.Paused&&Input.GetKeyDown(KeyCode.Escape))SetMode(ScreenMode.Playing);
            else if((Mode==ScreenMode.ServerSelect||Mode==ScreenMode.Support)&&Input.GetKeyDown(KeyCode.Escape))SetMode(ScreenMode.MainMenu);
            else if(Mode==ScreenMode.Controls&&Input.GetKeyDown(KeyCode.Escape))SetMode(returnMode);
            else if(Mode==ScreenMode.Settings&&Input.GetKeyDown(KeyCode.Escape))SetMode(returnMode);
        }
        void EnsureStyles()
        {
            uiScale=Mathf.Clamp(Mathf.Clamp(Screen.width/1280f,.62f,1.22f)*uiScaleSetting,.55f,1.22f);
            if(tiny!=null&&Mathf.Abs(tiny.fontSize-Mathf.RoundToInt(10*uiScale))<1)return;
            lightTex=Tex(new Color(.75f,.83f,.52f));lightHover=Tex(new Color(.87f,.93f,.62f));darkTex=Tex(new Color(.105f,.13f,.105f));darkHover=Tex(new Color(.16f,.19f,.14f));slotTex=Tex(new Color(.11f,.135f,.112f));slotHover=Tex(new Color(.19f,.22f,.16f));selectedTex=Tex(new Color(.25f,.29f,.19f));
            tiny=Style(9*uiScale,false);small=Style(11*uiScale,false);body=Style(13*uiScale,false);heading=Style(24*uiScale,true);title=Style(42*uiScale,true);mono=Style(10*uiScale,true);mono.font=Font.CreateDynamicFontFromOSFont("Consolas",Mathf.RoundToInt(12*uiScale));
            lightButton=MakeButton(lightTex,lightHover,new Color(.11f,.15f,.11f),Mathf.RoundToInt(11*uiScale));darkButton=MakeButton(darkTex,darkHover,ink,Mathf.RoundToInt(10*uiScale));
            slotStyle=new GUIStyle(GUI.skin.button){padding=new RectOffset(1,1,1,1),border=new RectOffset(1,1,1,1),fontSize=1};slotStyle.normal.background=slotTex;slotStyle.hover.background=slotHover;slotStyle.active.background=selectedTex;
        }
        GUIStyle Style(float size,bool bold){return new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(size),fontStyle=bold?FontStyle.Bold:FontStyle.Normal,richText=true,wordWrap=true,alignment=TextAnchor.UpperLeft};}
        GUIStyle MakeButton(Texture2D normal,Texture2D hover,Color text,int size)
        {var s=new GUIStyle(GUI.skin.button){fontSize=size,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(Mathf.RoundToInt(14*uiScale),8,0,0)};s.normal.background=normal;s.hover.background=hover;s.active.background=hover;s.normal.textColor=text;s.hover.textColor=text;s.active.textColor=text;return s;}
        void OnGUI()
        {
            if(RunState.Instance==null)return;EnsureStyles();
            switch(Mode)
            {
                case ScreenMode.Playing:DrawGameHUD();break;
                case ScreenMode.MainMenu:DrawMainMenu();break;
                case ScreenMode.ServerSelect:DrawSectorSelect();break;
                case ScreenMode.Inventory:DrawInventory();break;
                case ScreenMode.Paused:DrawPause();break;
                case ScreenMode.Settings:DrawSettings();break;
                case ScreenMode.Controls:DrawControls();break;
                case ScreenMode.Support:DrawSupport();break;
                case ScreenMode.Defeat:DrawEndCard(false);break;
                case ScreenMode.Victory:DrawEndCard(true);break;
            }
        }

        void DrawMainMenu()
        {
            DrawBackdrop();
            float leftW=Screen.width*.465f;
            Panel(R(0,0,leftW,Screen.height),new Color(.012f,.014f,.011f,.91f));
            for(int i=0;i<12;i++)DrawRect(R(leftW+i*Screen.width*.006f,0,Screen.width*.0061f,Screen.height),new Color(.012f,.014f,.011f,.91f-i*.068f));
            float x=66*uiScale,y=52*uiScale,w=Mathf.Min(leftW-112*uiScale,520*uiScale);
            DrawRect(R(x,y,356*uiScale,2*uiScale),copper);
            DrawGameLogo(R(x+5*uiScale,y+20*uiScale,43*uiScale,43*uiScale));
            Label("SUBSISTENCE",R(x+62*uiScale,y+14*uiScale,w-70*uiScale,48*uiScale),title,ink);
            Label("В Ы Ж И В А Н И Е     /     Л И М И Н А Л Ь Н А Я   З О Н А",R(x+64*uiScale,y+62*uiScale,w-70*uiScale,20*uiScale),tiny,muted);
            Label("С Е К Т О Р   0 9     •     С И Г Н А Л   Н Е С Т А Б И Л Е Н",R(x+5*uiScale,y+120*uiScale,w-10*uiScale,20*uiScale),mono,ink);
            DrawRule(x+5*uiScale,y+151*uiScale,w-18*uiScale);
            float bx=x+5*uiScale,bw=w-20*uiScale;
            DrawMenuAction("НАЧАТЬ ИГРУ","БЫСТРЫЙ ВХОД В ЛОКАЛЬНЫЙ МИР",R(bx,y+178*uiScale,bw,74*uiScale),()=>SetMode(ScreenMode.ServerSelect));
            DrawMenuOption("01","ВЫБОР СЕРВЕРА","НАЙТИ СВОЙ СЕКТОР",R(bx,y+273*uiScale,bw,57*uiScale),()=>SetMode(ScreenMode.ServerSelect));
            DrawMenuOption("02","НАСТРОЙКИ","ИЗОБРАЖЕНИЕ И УПРАВЛЕНИЕ",R(bx,y+339*uiScale,bw,57*uiScale),()=>{returnMode=ScreenMode.MainMenu;SetMode(ScreenMode.Settings);});
            DrawMenuOption("03","ПОДДЕРЖКА","ПОМОЩЬ И СВЯЗЬ",R(bx,y+405*uiScale,bw,57*uiScale),()=>SetMode(ScreenMode.Support));
            DrawMenuOption("04","ВЫЙТИ","ЗАВЕРШИТЬ СЕАНС",R(bx,y+471*uiScale,bw,57*uiScale),()=>Application.Quit());
            float footerY=Screen.height-143*uiScale;
            Label("[ W ]   [ A ]   [ S ]   [ D ]",R(x+5*uiScale,footerY,w-10*uiScale,18*uiScale),mono,ink);
            Label("ПОСЛЕДНИЙ ЗАПУСК: ЛОКАЛЬНЫЙ  /  СОХРАНЕНИЯ НЕ СОЗДАНЫ",R(x+5*uiScale,footerY+25*uiScale,w-10*uiScale,16*uiScale),tiny,muted);
            DrawRule(x+5*uiScale,footerY+55*uiScale,w-18*uiScale);
            Label("LOCAL FIELD TEST  ·  ОДИНОЧНЫЙ ПРОТОТИП",R(x+5*uiScale,footerY+68*uiScale,w-10*uiScale,16*uiScale),mono,muted);
            DrawMenuStatus();DrawSignalCard();
        }
        void DrawGameLogo(Rect rect)
        {
            DrawRect(rect,new Color(.025f,.028f,.023f,.82f));Outline(rect,copper);
            Matrix4x4 previous=GUI.matrix;
            GUIUtility.RotateAroundPivot(22f,rect.center);
            float cx=rect.center.x,cy=rect.center.y;
            DrawRect(R(cx-10*uiScale,cy-12*uiScale,4*uiScale,24*uiScale),copper);
            DrawRect(R(cx-2*uiScale,cy-12*uiScale,4*uiScale,24*uiScale),new Color(.82f,.70f,.44f));
            DrawRect(R(cx+6*uiScale,cy-12*uiScale,4*uiScale,24*uiScale),copper);
            GUI.matrix=previous;
            DrawRect(R(rect.x+8*uiScale,rect.y+rect.height-7*uiScale,rect.width-16*uiScale,2*uiScale),new Color(.77f,.36f,.18f,.56f));
        }

        void DrawMenuAction(string titleText,string subtitle,Rect rect,System.Action action)
        {
            Panel(rect,new Color(.018f,.019f,.016f,.92f));
            bool clicked=GUI.Button(rect,GUIContent.none,slotStyle);Outline(rect,copper);DrawRect(R(rect.x,rect.y,5*uiScale,rect.height),copper);
            Label(titleText,R(rect.x+24*uiScale,rect.y+12*uiScale,rect.width-40*uiScale,26*uiScale),heading,ink);
            Label(subtitle,R(rect.x+24*uiScale,rect.y+43*uiScale,rect.width-40*uiScale,16*uiScale),tiny,muted);
            if(clicked)action?.Invoke();
        }
        void DrawMenuOption(string number,string titleText,string subtitle,Rect rect,System.Action action)
        {
            Panel(rect,new Color(.028f,.029f,.024f,.88f));bool clicked=GUI.Button(rect,GUIContent.none,slotStyle);DrawRect(R(rect.x,rect.y,4*uiScale,rect.height),copper);
            Label(number,R(rect.x+17*uiScale,rect.y+9*uiScale,29*uiScale,19*uiScale),mono,copper);
            Label(titleText,R(rect.x+49*uiScale,rect.y+7*uiScale,rect.width-82*uiScale,22*uiScale),mono,ink);
            Label(subtitle,R(rect.x+49*uiScale,rect.y+31*uiScale,rect.width-82*uiScale,15*uiScale),tiny,muted);
            Label("›",R(rect.x+rect.width-31*uiScale,rect.y+10*uiScale,19*uiScale,24*uiScale),heading,muted);
            if(clicked)action?.Invoke();
        }
        void DrawMenuStatus()
        {
            Rect r=R(Screen.width-302*uiScale,48*uiScale,265*uiScale,54*uiScale);Panel(r,new Color(.035f,.035f,.029f,.75f));
            DrawRect(R(r.x+15*uiScale,r.y+18*uiScale,10*uiScale,10*uiScale),lime);
            Label("С И С Т Е М А   А К Т И В Н А",R(r.x+37*uiScale,r.y+9*uiScale,r.width-48*uiScale,17*uiScale),mono,ink);
            Label("Л О К А Л Ь Н Ы Й   М И Р",R(r.x+37*uiScale,r.y+29*uiScale,r.width-48*uiScale,14*uiScale),tiny,muted);
        }
        void DrawSignalCard()
        {
            float w=Mathf.Min(430*uiScale,Screen.width*.31f);Rect r=R(Screen.width-w-54*uiScale,Screen.height-196*uiScale,w,133*uiScale);
            Panel(r,new Color(.027f,.028f,.022f,.91f));DrawRect(R(r.x,r.y,4*uiScale,r.height),copper);DrawRect(R(r.x,r.y, r.width,4*uiScale),copper);
            Label("П Е Р Е Х В А Ч Е Н Н Ы Й   С И Г Н А Л",R(r.x+22*uiScale,r.y+16*uiScale,r.width-42*uiScale,14*uiScale),tiny,copper);
            Label("« НЕ ИДИ НА СВЕТ »",R(r.x+22*uiScale,r.y+41*uiScale,r.width-42*uiScale,27*uiScale),heading,ink);
            Label("В коридорах снова слышен гул генератора.",R(r.x+22*uiScale,r.y+76*uiScale,r.width-42*uiScale,18*uiScale),small,muted);
            Label("ИСТОЧНИК: УРОВЕНЬ 0  /  ЗАПИСЬ 02:47",R(r.x+22*uiScale,r.y+105*uiScale,r.width-42*uiScale,13*uiScale),tiny,muted);
        }
        void DrawSectorSelect()
        {
            DrawBackdrop();float panelW=Screen.width*.95f,panelH=Screen.height*.93f;Rect r=CenteredPanel(panelW,panelH);Panel(r,new Color(.025f,.029f,.024f,.90f));
            Label("S U B S I S T E N C E   /   С Е Т Ь   У Б Е Ж И Щ",R(r.x+28*uiScale,r.y+20*uiScale,r.width-56*uiScale,17*uiScale),tiny,muted);
            Label("ВЫБОР СЕРВЕРА",R(r.x+27*uiScale,r.y+48*uiScale,r.width*.65f,44*uiScale),title,ink);
            Label("НАЙДИ СЕКТОР, ГДЕ МОЖНО ПЕРЕЖИТЬ ЭТУ НОЧЬ",R(r.x+30*uiScale,r.y+99*uiScale,r.width*.65f,18*uiScale),mono,muted);
            Rect net=R(r.x+r.width-244*uiScale,r.y+30*uiScale,212*uiScale,49*uiScale);Panel(net,new Color(.05f,.055f,.045f,.85f));
            Label("●  СЕТЬ НЕДОСТУПНА",R(net.x+12*uiScale,net.y+6*uiScale,net.width-24*uiScale,17*uiScale),mono,gold);
            Label("ЛОКАЛЬНЫЙ РЕЖИМ  ·  0 ОНЛАЙН-СЕРВЕРОВ",R(net.x+12*uiScale,net.y+27*uiScale,net.width-24*uiScale,13*uiScale),tiny,muted);
            DrawRule(r.x+28*uiScale,r.y+127*uiScale,r.width-56*uiScale);
            float leftX=r.x+25*uiScale,leftW=r.width*.225f,listX=leftX+leftW+19*uiScale,listW=r.x+r.width-28*uiScale-listX;
            Rect filter=R(leftX,r.y+145*uiScale,leftW,Mathf.Min(392*uiScale,r.height*.51f));Panel(filter,new Color(.044f,.049f,.041f,.88f));
            Label("Ф И Л Ь Т Р Ы",R(filter.x+14*uiScale,filter.y+13*uiScale,filter.width-28*uiScale,22*uiScale),mono,ink);
            DrawRule(filter.x+13*uiScale,filter.y+44*uiScale,filter.width-26*uiScale);
            Label("ТИП ПОДКЛЮЧЕНИЯ",R(filter.x+14*uiScale,filter.y+59*uiScale,filter.width-28*uiScale,15*uiScale),tiny,muted);
            Panel(R(filter.x+13*uiScale,filter.y+82*uiScale,filter.width-26*uiScale,39*uiScale),new Color(.14f,.16f,.12f));
            Label("ЛОКАЛЬНАЯ ЭКСПЕДИЦИЯ",R(filter.x+24*uiScale,filter.y+93*uiScale,filter.width-48*uiScale,17*uiScale),mono,lime);
            Label("СЕКТОР",R(filter.x+14*uiScale,filter.y+139*uiScale,filter.width-28*uiScale,15*uiScale),tiny,muted);
            for(int i=0;i<4;i++)
            {
                float yy=filter.y+(166+i*38)*uiScale;Rect fr=R(filter.x+13*uiScale,yy,filter.width-26*uiScale,31*uiScale);Panel(fr,i==selectedSector?new Color(.24f,.11f,.065f):new Color(.065f,.072f,.06f));
                bool sectorClicked=GUI.Button(fr,GUIContent.none,GUIStyle.none);if(i==selectedSector)DrawRect(R(fr.x,fr.y,3*uiScale,fr.height),copper);
                Label((i==selectedSector?"●  ":"○  ")+"0"+(i+1)+"  "+ShortSectorName(i),R(fr.x+9*uiScale,fr.y+8*uiScale,fr.width-18*uiScale,15*uiScale),tiny,i==selectedSector?ink:muted);if(sectorClicked)selectedSector=i;
            }
            Label("РЕЖИМ",R(filter.x+14*uiScale,filter.y+330*uiScale,filter.width-28*uiScale,14*uiScale),tiny,muted);
            Label("ОДИНОЧНАЯ ИГРА\nБЕЗ МАТЧМЕЙКИНГА",R(filter.x+14*uiScale,filter.y+350*uiScale,filter.width-28*uiScale,36*uiScale),small,ink);
            float tableY=r.y+145*uiScale,headH=30*uiScale,rowH=53*uiScale;
            DrawRect(R(listX,tableY,listW,headH),new Color(.043f,.048f,.04f,.93f));
            Label("ЛОКАЛЬНЫЙ СЕКТОР",R(listX+14*uiScale,tableY+8*uiScale,listW*.48f,14*uiScale),tiny,muted);
            Label("РЕЖИМ",R(listX+listW*.53f,tableY+8*uiScale,listW*.13f,14*uiScale),tiny,muted);
            Label("РИСК",R(listX+listW*.68f,tableY+8*uiScale,listW*.12f,14*uiScale),tiny,muted);
            Label("ДОБЫЧА",R(listX+listW*.82f,tableY+8*uiScale,listW*.14f,14*uiScale),tiny,muted);
            for(int i=0;i<4;i++)
            {
                float yy=tableY+headH+i*(rowH+2*uiScale);Rect row=R(listX,yy,listW,rowH);Panel(row,i==selectedSector?new Color(.24f,.105f,.055f,.90f):new Color(.035f,.040f,.034f,.82f));
                bool clicked=GUI.Button(row,GUIContent.none,GUIStyle.none);if(clicked)selectedSector=i;
                if(i==selectedSector)DrawRect(R(row.x,row.y,4*uiScale,row.height),copper);
                DrawRect(R(row.x+14*uiScale,row.y+21*uiScale,8*uiScale,8*uiScale),i<2?lime:gold);
                Label(sectorNames[i],R(row.x+32*uiScale,row.y+10*uiScale,listW*.47f,19*uiScale),mono,ink);
                Label("SOLO",R(row.x+listW*.53f,row.y+17*uiScale,listW*.13f,17*uiScale),tiny,muted);
                Label(i==0?"НИЗКИЙ":i==1?"СРЕДНИЙ":i==2?"ВЫСОКИЙ":"КРИТИЧЕСКИЙ",R(row.x+listW*.68f,row.y+17*uiScale,listW*.14f,17*uiScale),tiny,i<2?lime:gold);
                Label(i==0?"I":i==1?"I—II":i==2?"II":"III",R(row.x+listW*.84f,row.y+17*uiScale,listW*.11f,17*uiScale),tiny,ink);
            }
            float detailY=tableY+headH+4*(rowH+2*uiScale)+19*uiScale;Rect detail=R(listX,detailY,listW,r.y+r.height-72*uiScale-detailY);Panel(detail,new Color(.037f,.043f,.036f,.88f));DrawRect(R(detail.x,detail.y,4*uiScale,detail.height),copper);
            Label("ВЫБРАННЫЙ СЕКТОР",R(detail.x+19*uiScale,detail.y+12*uiScale,listW*.48f,14*uiScale),tiny,copper);
            Label(sectorNames[selectedSector],R(detail.x+19*uiScale,detail.y+35*uiScale,listW*.56f,24*uiScale),heading,ink);
            Label(sectorInfo[selectedSector],R(detail.x+19*uiScale,detail.y+65*uiScale,listW*.61f,33*uiScale),small,muted);
            Label("СТАТУС\nЛОКАЛЬНО\n\nПИНГ\nНЕ ПРИМЕНИМ\n\nСЕССИЯ\nОДИНОЧНАЯ",R(detail.x+listW*.68f,detail.y+14*uiScale,listW*.27f,detail.height-26*uiScale),tiny,ink);
            if(Button("НАЧАТЬ ЛОКАЛЬНУЮ ЭКСПЕДИЦИЮ   →",R(detail.x+listW*.62f,detail.y+detail.height-51*uiScale,listW*.36f,39*uiScale),lightButton))StartRun();
            Label("СЕТЕВЫЕ СЕРВЕРЫ НЕ ПОДКЛЮЧЕНЫ — СПИСОК РЕАЛЬНЫХ ОНЛАЙН-МИРОВ НЕ ПОКАЗЫВАЕТСЯ",R(leftX,r.y+r.height-42*uiScale,r.width*.60f,15*uiScale),tiny,muted);
            if(Button("ESC  /  НАЗАД",R(r.x+r.width-160*uiScale,r.y+r.height-48*uiScale,130*uiScale,30*uiScale),darkButton))SetMode(ScreenMode.MainMenu);
        }
        string ShortSectorName(int i){return i==0?"ВЛОЖЕННЫЙ":i==1?"ОФИС":i==2?"ЗАТОПЛЕННЫЙ":"ТЕХНИЧЕСКИЙ";}
        void DrawBackdrop()
        {
            Rect screen=new Rect(0,0,Screen.width,Screen.height);
            if(MenuBackdropActive&&menuBackdrop!=null)
            {
                GUI.DrawTexture(screen,menuBackdrop,ScaleMode.ScaleAndCrop,true);
                DrawRect(screen,new Color(.018f,.020f,.016f,.25f));
                DrawRect(new Rect(0,Screen.height*.78f,Screen.width,Screen.height*.22f),new Color(.008f,.010f,.008f,.48f));
            }
            else DrawRect(screen,new Color(.012f,.016f,.012f,.38f));
            DrawRect(new Rect(0,0,Screen.width,Screen.height*.035f),new Color(.008f,.010f,.008f,.22f));
        }
        void DrawGameHUD()
        {
            RunState s=RunState.Instance;var p=FindObjectOfType<PlayerController>();
            float margin=14*uiScale,w=254*uiScale;
            Panel(R(margin,margin,w,163*uiScale),new Color(.035f,.048f,.04f,.88f));
            Label("SUBSISTENCE   /   SECTOR 0"+(selectedSector+1),R(margin+12*uiScale,margin+9*uiScale,w-24*uiScale,15*uiScale),mono,lime);
            Label(sectorNames[selectedSector],R(margin+12*uiScale,margin+29*uiScale,w-24*uiScale,17*uiScale),tiny,ink);
            string goal=s.SuppliesFound<3?$"Осмотреть ящики  ·  {s.SuppliesFound} / 3":p!=null&&p.Inventory.Count(ItemId.Keycard)==0?"Найти карту доступа · ящик III":"Вернуться к выходу  →";
            Label(goal,R(margin+12*uiScale,margin+52*uiScale,w-24*uiScale,21*uiScale),small,ink);
            Bar(margin+12*uiScale,margin+82*uiScale,228*uiScale,s.Health/100f,new Color(.72f,.35f,.29f),"ЗДОРОВЬЕ");
            Bar(margin+12*uiScale,margin+105*uiScale,228*uiScale,s.Hunger/100f,new Color(.78f,.61f,.34f),"ГОЛОД");
            Bar(margin+12*uiScale,margin+128*uiScale,228*uiScale,s.Thirst/100f,new Color(.39f,.64f,.65f),"ЖАЖДА");
            float clock=18*60+42+Mathf.FloorToInt(s.ShiftSeconds*1.08f);string time=$"{(clock/60)%24:00}:{clock%60:00}";
            float rw=174*uiScale;Panel(R(Screen.width-margin-rw,margin,rw,47*uiScale),new Color(.035f,.048f,.04f,.88f));
            Label("●  "+time+"   /   SHIFT 01",R(Screen.width-margin-rw+10*uiScale,margin+7*uiScale,rw-20*uiScale,16*uiScale),mono,ink);
            Label("ОДИНОЧНАЯ ЭКСПЕДИЦИЯ",R(Screen.width-margin-rw+10*uiScale,margin+26*uiScale,rw-20*uiScale,13*uiScale),tiny,muted);
            DrawHotbar();DrawInteractHint(p);
            string hint="A/D ДВИЖЕНИЕ     SPACE ПРЫЖОК     E ВЗАИМОДЕЙСТВИЕ     Q АТАКА     TAB ИНВЕНТАРЬ     ESC ПАУЗА";
            Panel(R(margin,Screen.height-34*uiScale,Screen.width-margin*2,24*uiScale),new Color(.035f,.048f,.04f,.87f));Label(hint,R(margin+9*uiScale,Screen.height-29*uiScale,Screen.width-margin*2-18*uiScale,15*uiScale),tiny,ink);
            if(showNotices&&s.NoticeTime>0){float nw=Mathf.Min(480*uiScale,Screen.width*.72f);Panel(R((Screen.width-nw)/2,Screen.height-180*uiScale,nw,27*uiScale),new Color(.045f,.06f,.046f,.95f));Label(s.Notice,R((Screen.width-nw)/2+10*uiScale,Screen.height-174*uiScale,nw-20*uiScale,16*uiScale),small,ink);}
        }
        void DrawHotbar()
        {
            var player=FindObjectOfType<PlayerController>();if(player==null||player.Inventory==null)return;var b=player.Inventory;
            float cell=52*uiScale,gap=5*uiScale,total=6*cell+5*gap,start=(Screen.width-total)/2,y=Screen.height-112*uiScale;
            for(int i=0;i<6;i++)
            {
                Rect r=R(start+i*(cell+gap),y,cell,cell);if(GUI.Button(r,GUIContent.none,slotStyle))b.SelectBelt(i);
                DrawSlotContents(r,b.Belt[i],i==b.SelectedBeltSlot,i+1);
            }
            ItemStack active=b.ActiveItem;
            if(!active.Empty)Label(ItemCatalog.Get(active.id).name.ToUpperInvariant(),R(start,y-18*uiScale,total,14*uiScale),tiny,ink);
        }
        void DrawInteractHint(PlayerController player)
        {
            if(!showInteractionHints||player==null)return;string hint=NearestHint(player);if(string.IsNullOrEmpty(hint))return;
            float ww=Mathf.Min(350*uiScale,Screen.width*.72f);Panel(R((Screen.width-ww)/2,Screen.height-147*uiScale,ww,27*uiScale),new Color(.06f,.08f,.06f,.94f));Label(hint,R((Screen.width-ww)/2+10*uiScale,Screen.height-141*uiScale,ww-20*uiScale,16*uiScale),mono,lime);
        }
        string NearestHint(PlayerController player)
        {
            float best=1.8f;string hint="";
            foreach(var crate in FindObjectsOfType<LootContainer>()){float d=Vector2.Distance(player.transform.position,crate.transform.position);if(d<best){best=d;hint=crate.GetHint();}}
            foreach(var item in FindObjectsOfType<WorldItem>()){float d=Vector2.Distance(player.transform.position,item.transform.position);if(d<best){best=d;hint=item.Hint;}}
            foreach(var item in FindObjectsOfType<SupplyPickup>()){if(item.Collected)continue;float d=Vector2.Distance(player.transform.position,item.transform.position);if(d<best){best=d;hint="E  ·  ПОДОБРАТЬ ПРИПАС";}}
            if(Mathf.Abs(player.transform.position.x-RunState.ExitX)<2.5f)return "E  ·  ПРОВЕРИТЬ ВЫХОД";
            return hint;
        }
        void Bar(float x,float y,float width,float value,Color color,string caption)
        {Label(caption,R(x,y,width*.33f,13*uiScale),tiny,muted);DrawRect(R(x+width*.35f,y+4*uiScale,width*.49f,5*uiScale),new Color(.19f,.22f,.19f));DrawRect(R(x+width*.35f,y+4*uiScale,width*.49f*Mathf.Clamp01(value),5*uiScale),color);Label(Mathf.RoundToInt(value*100).ToString("00"),R(x+width*.86f,y-1*uiScale,width*.14f,14*uiScale),mono,ink);}

        void OpenInventory()
        {
            var p=FindObjectOfType<PlayerController>();if(p==null||p.Inventory==null)return;bag=p.Inventory;bag.InventoryOpen=true;if(bag.IsLootOpen)craftTab=false;if(preview!=null)preview.SetGear(bag.Gear);SetMode(ScreenMode.Inventory);
        }
        void CloseInventory()
        {if(bag!=null){bag.CloseContainer();bag.InventoryOpen=false;}held.Clear();selectedKind=-1;selectedIndex=-1;SetMode(ScreenMode.Playing);}
        void DrawInventory()
        {
            if(bag==null){var p=FindObjectOfType<PlayerController>();if(p!=null)bag=p.Inventory;}if(bag==null)return;
            DrawRect(new Rect(0,0,Screen.width,Screen.height),new Color(.015f,.022f,.018f,.78f));
            float w=Mathf.Min(1510*uiScale,Screen.width-30*uiScale),h=Mathf.Min(875*uiScale,Screen.height-26*uiScale);Rect panel=CenteredPanel(w,h);Panel(panel,new Color(.055f,.068f,.057f,.985f));Outline(panel,new Color(.26f,.30f,.23f));
            DrawRect(R(panel.x,panel.y,panel.width,3*uiScale),new Color(.73f,.79f,.49f));
            Label("ЭКИПИРОВКА  /  РЮКЗАК",R(panel.x+22*uiScale,panel.y+14*uiScale,panel.width*.43f,26*uiScale),heading,ink);
            Label("SURVIVAL LOADOUT     ·     TAB / ESC — НАЗАД",R(panel.x+22*uiScale,panel.y+48*uiScale,panel.width*.45f,15*uiScale),mono,muted);
            if(Button("×  ЗАКРЫТЬ",R(panel.x+panel.width-150*uiScale,panel.y+18*uiScale,128*uiScale,33*uiScale),darkButton))CloseInventory();
            DrawRule(panel.x+20*uiScale,panel.y+76*uiScale,panel.width-40*uiScale);
            float contentY=panel.y+91*uiScale,contentH=panel.height-167*uiScale,leftW=panel.width*.245f,centerW=panel.width*.425f,rightW=panel.width-leftW-centerW-58*uiScale;
            float leftX=panel.x+18*uiScale,centerX=leftX+leftW+12*uiScale,rightX=centerX+centerW+12*uiScale;
            DrawCharacterPanel(leftX,contentY,leftW,contentH);
            DrawBackpackPanel(centerX,contentY,centerW,contentH);
            DrawRightPanel(rightX,contentY,rightW,contentH);
            DrawRule(panel.x+20*uiScale,panel.y+panel.height-61*uiScale,panel.width-40*uiScale);
            string interaction=bag.IsLootOpen?"ЛКМ — переместить     SHIFT + ЛКМ — быстрый перенос     ПКМ — разделить стопку":"ЛКМ — переместить     ПКМ — разделить стопку     Перетаскивай предметы между ячейками";
            Label(interaction,R(panel.x+22*uiScale,panel.y+panel.height-48*uiScale,panel.width-44*uiScale,18*uiScale),tiny,muted);
            DrawCursorStack();
            if(showNotices&&bag.ShowNotice)Toast(bag.LastCraftStatus);
        }
        void DrawCharacterPanel(float x,float y,float w,float h)
        {
            Panel(R(x,y,w,h),new Color(.075f,.089f,.074f));Label("ПЕРСОНАЖ",R(x+10*uiScale,y+7*uiScale,w-20*uiScale,20*uiScale),mono,lime);
            float portraitW=Mathf.Max(100*uiScale,w-128*uiScale),portraitH=Mathf.Min(330*uiScale,h*.52f),portraitX=x+(w-portraitW)/2,portraitY=y+35*uiScale;
            if(preview!=null&&preview.Texture!=null)GUI.DrawTexture(R(portraitX,portraitY,portraitW,portraitH),preview.Texture,ScaleMode.ScaleToFit,true);
            int[] gearSlots={(int)GearSlot.Head,(int)GearSlot.Chest,(int)GearSlot.Legs,(int)GearSlot.Feet,(int)GearSlot.Back};string[] labels={"ГОЛОВА","ТОРС","НОГИ","ОБУВЬ","СПИНА"};
            float slot=49*uiScale;float lx=x+8*uiScale,rx=x+w-slot-8*uiScale;
            float[] ys={portraitY+4*uiScale,portraitY+70*uiScale,portraitY+136*uiScale,portraitY+202*uiScale,portraitY+88*uiScale};float[] xs={lx,lx,lx,lx,rx};
            for(int i=0;i<gearSlots.Length;i++)DrawGearSlot(gearSlots[i],labels[i],R(xs[i],ys[i],slot,slot),i);
            float sy=portraitY+portraitH+8*uiScale;
            Label("ЗАЩИТА  /  ЭКИПИРОВКА",R(x+11*uiScale,sy,w-22*uiScale,15*uiScale),tiny,muted);
            ArmorLine(x+11*uiScale,sy+20*uiScale,w-22*uiScale,"ПУЛИ",bag.ArmorBullet);ArmorLine(x+11*uiScale,sy+39*uiScale,w-22*uiScale,"БЛИЖНИЙ БОЙ",bag.ArmorMelee);ArmorLine(x+11*uiScale,sy+58*uiScale,w-22*uiScale,"ХОЛОД",bag.ColdProtection);ArmorLine(x+11*uiScale,sy+77*uiScale,w-22*uiScale,"РАДИАЦИЯ",bag.RadiationProtection);
            Label("VER. 0.2  ·  ОДИНОЧНАЯ СМЕНА",R(x+10*uiScale,y+h-19*uiScale,w-20*uiScale,13*uiScale),tiny,muted);
        }
        void DrawGearSlot(int slotIndex,string label,Rect rect,int displayIndex)
        {
            ItemStack stack=bag.Gear[slotIndex];bool pressed=GUI.Button(rect,GUIContent.none,slotStyle);DrawSlotContents(rect,stack,false,0);
            if(pressed)HandleGearClick(slotIndex,Event.current.button);
            Label(label,R(rect.x-4*uiScale,rect.y+rect.height+2*uiScale,rect.width+8*uiScale,12*uiScale),tiny,muted);
        }
        void ArmorLine(float x,float y,float width,string label,float value)
        {Label(label,R(x,y,width*.46f,14*uiScale),tiny,muted);DrawRect(R(x+width*.48f,y+4*uiScale,width*.38f,5*uiScale),new Color(.18f,.21f,.18f));DrawRect(R(x+width*.48f,y+4*uiScale,width*.38f*Mathf.Clamp01(value/100f),5*uiScale),value>0?gold:new Color(.28f,.30f,.25f));Label(Mathf.RoundToInt(value)+"%",R(x+width*.88f,y,width*.12f,14*uiScale),tiny,ink);}
        void DrawBackpackPanel(float x,float y,float w,float h)
        {
            Panel(R(x,y,w,h),new Color(.075f,.089f,.074f));Label("РЮКЗАК  /  24 СЛОТА",R(x+10*uiScale,y+7*uiScale,w-20*uiScale,19*uiScale),mono,lime);
            float gap=5*uiScale,cell=Mathf.Min(68*uiScale,(w-22*uiScale-5*gap)/6),gridW=6*cell+5*gap,startX=x+(w-gridW)/2,gridY=y+35*uiScale;
            for(int i=0;i<24;i++){int col=i%6,row=i/6;DrawItemSlot(bag.Backpack,i,R(startX+col*(cell+gap),gridY+row*(cell+gap),cell,cell),0);}
            float beltY=gridY+4*(cell+gap)+8*uiScale;
            Label("ПОЯС БЫСТРОГО ДОСТУПА",R(x+10*uiScale,beltY,w-20*uiScale,15*uiScale),tiny,muted);
            for(int i=0;i<6;i++)DrawItemSlot(bag.Belt,i,R(startX+i*(cell+gap),beltY+17*uiScale,cell,cell),1);
            float detailY=beltY+cell+29*uiScale;DrawItemDetails(x+10*uiScale,detailY,w-20*uiScale,h-(detailY-y)-10*uiScale);
        }
        void DrawRightPanel(float x,float y,float w,float h)
        {
            Panel(R(x,y,w,h),new Color(.075f,.089f,.074f));
            float tabsY=y+7*uiScale,tabW=(w-26*uiScale)/2;
            bool hasCrate=bag.OpenContainer!=null;
            if(Button("КОНТЕЙНЕР",R(x+9*uiScale,tabsY,tabW,29*uiScale),!craftTab&&hasCrate?lightButton:darkButton)&&hasCrate)craftTab=false;
            if(Button("КРАФТ",R(x+17*uiScale+tabW,tabsY,tabW,29*uiScale),craftTab?lightButton:darkButton))craftTab=true;
            if(!craftTab&&hasCrate)DrawContainerContents(x,y,w,h);
            else if(!craftTab&&!hasCrate){Label("ПРЕДМЕТЫ",R(x+12*uiScale,y+50*uiScale,w-24*uiScale,20*uiScale),mono,lime);Label("Подойди к ящику в мире и нажми E, чтобы осмотреть содержимое.",R(x+13*uiScale,y+80*uiScale,w-26*uiScale,65*uiScale),small,muted);Label("УРОВНИ ДОБЫЧИ",R(x+13*uiScale,y+166*uiScale,w-26*uiScale,18*uiScale),tiny,gold);Label("I   Ресурсы и базовые запасы\nII  Оружие, патроны, электроника\nIII Броня, карабин, карта доступа",R(x+13*uiScale,y+190*uiScale,w-26*uiScale,96*uiScale),small,ink);}
            else DrawCraftPanel(x,y,w,h);
        }
        void DrawContainerContents(float x,float y,float w,float h)
        {
            var crate=bag.OpenContainer;Label("ЯЩИК  /  TIER "+crate.Tier,R(x+12*uiScale,y+47*uiScale,w-24*uiScale,20*uiScale),mono,lime);
            Label(crate.DisplayName.ToUpperInvariant(),R(x+12*uiScale,y+69*uiScale,w-24*uiScale,18*uiScale),tiny,muted);
            int columns=4;float gap=5*uiScale,cell=Mathf.Min(66*uiScale,(w-28*uiScale-3*gap)/columns),gridW=columns*cell+3*gap,start=x+(w-gridW)/2,top=y+99*uiScale;
            for(int i=0;i<crate.Items.Length;i++)DrawItemSlot(crate.Items,i,R(start+(i%columns)*(cell+gap),top+(i/columns)*(cell+gap),cell,cell),2);
            float remaining=top+3*(cell+gap)+13*uiScale;
            Label(crate.Empty?"КОНТЕЙНЕР ПУСТ":"SHIFT + КЛИК — ПЕРЕНЕСТИ",R(x+12*uiScale,remaining,w-24*uiScale,18*uiScale),tiny,crate.Empty?muted:gold);
            if(Button("ЗАБРАТЬ ВСЁ",R(x+12*uiScale,h+y-56*uiScale,w-24*uiScale,34*uiScale),darkButton))for(int i=0;i<crate.Items.Length;i++)bag.QuickMoveContainer(i);
        }
        void DrawCraftPanel(float x,float y,float w,float h)
        {
            Label("ПОЛЕВОЙ КРАФТ",R(x+12*uiScale,y+48*uiScale,w-24*uiScale,20*uiScale),mono,lime);
            string bench=bag.WorkbenchTier>0?"ВЕРСТАК УРОВНЯ "+bag.WorkbenchTier:"НЕТ ДОСТУПА К ВЕРСТАКУ";Label(bench,R(x+12*uiScale,y+71*uiScale,w-24*uiScale,18*uiScale),tiny,bag.WorkbenchTier>0?gold:muted);
            float by=y+104*uiScale,bh=61*uiScale;
            CraftButton(x,by,w,bh,"БИНТ","2 ткани  ·  без верстака",0);CraftButton(x,by+bh+8*uiScale,w,bh,"СТАЛЬНАЯ ТРУБА","12 металла + 2 ткани  ·  верстак I",1);CraftButton(x,by+(bh+8*uiScale)*2,w,bh,"ПАТРОНЫ 9 ММ ×8","5 фрагментов металла  ·  верстак I",2);
            Label("РЕДКОЕ СНАРЯЖЕНИЕ ОТКРЫВАЕТСЯ ЧЕРТЕЖАМИ В ЯЩИКАХ.",R(x+12*uiScale,by+(bh+8*uiScale)*3+9*uiScale,w-24*uiScale,46*uiScale),tiny,muted);
        }
        void CraftButton(float x,float y,float w,float h,string name,string req,int type)
        {
            Rect r=R(x+10*uiScale,y,w-20*uiScale,h);Panel(r,new Color(.105f,.13f,.105f));Outline(r,new Color(.22f,.27f,.20f));
            if(GUI.Button(r,GUIContent.none,slotStyle)){if(type==0)bag.CraftBandage();else if(type==1)bag.CraftPipe();else bag.CraftAmmo();}
            Label(name,R(r.x+9*uiScale,r.y+7*uiScale,r.width-18*uiScale,19*uiScale),mono,ink);Label(req,R(r.x+9*uiScale,r.y+30*uiScale,r.width-18*uiScale,15*uiScale),tiny,muted);
        }
        void DrawItemDetails(float x,float y,float w,float h)
        {
            float available=Mathf.Max(74*uiScale,h);Panel(R(x,y,w,available),new Color(.055f,.07f,.057f));
            ItemStack item=SelectedStack();
            if(item.Empty&&held.Empty){Label("ВЫБЕРИ ПРЕДМЕТ",R(x+9*uiScale,y+7*uiScale,w-18*uiScale,17*uiScale),mono,muted);Label("Одежду можно экипировать. Пищу, воду и медицину — использовать.",R(x+9*uiScale,y+27*uiScale,w-18*uiScale,31*uiScale),tiny,muted);return;}
            bool fromCursor=!held.Empty;ItemStack shown=fromCursor?held:item;ItemDefinition def=ItemCatalog.Get(shown.id);
            Label(def.name.ToUpperInvariant()+(shown.count>1?"  ×"+shown.count:""),R(x+9*uiScale,y+6*uiScale,w-18*uiScale,19*uiScale),mono,ink);
            Label(def.description,R(x+9*uiScale,y+26*uiScale,w-18*uiScale,34*uiScale),tiny,muted);
            float by=y+available-34*uiScale,bw=(w-24*uiScale)/3;
            if(Usable(shown.id)&&Button("ИСПОЛЬЗОВАТЬ",R(x+8*uiScale,by,bw,27*uiScale),darkButton))UseShown(fromCursor);
            if(def.gearSlot.HasValue&&Button("НАДЕТЬ",R(x+12*uiScale+bw,by,bw,27*uiScale),darkButton))EquipShown(fromCursor);
            if(Button("ВЫБРОСИТЬ",R(x+16*uiScale+bw*2,by,bw,27*uiScale),darkButton))DropShown(fromCursor);
        }
        void DrawItemSlot(ItemStack[] slots,int index,Rect rect,int kind)
        {
            bool selected=selectedKind==kind&&selectedIndex==index;bool pressed=GUI.Button(rect,GUIContent.none,slotStyle);DrawSlotContents(rect,slots[index],selected,0);
            if(pressed)HandleArrayClick(slots,index,kind,Event.current.button,Event.current.shift);
        }
        void DrawSlotContents(Rect rect,ItemStack stack,bool selected,int number)
        {
            DrawRect(rect,selected?new Color(.19f,.23f,.15f):new Color(.09f,.115f,.094f));Outline(rect,selected?new Color(.83f,.75f,.42f):new Color(.27f,.30f,.24f));
            if(!stack.Empty)
            {
                Texture2D icon=ItemIconFactory.Get(stack.id);float pad=rect.width*.12f;GUI.DrawTexture(R(rect.x+pad,rect.y+pad,rect.width-pad*2,rect.height-pad*2),icon,ScaleMode.ScaleToFit,true);
                if(stack.count>1){DrawRect(R(rect.x+rect.width*.55f,rect.y+rect.height*.69f,rect.width*.40f,rect.height*.23f),new Color(.025f,.035f,.028f,.94f));Label(stack.count.ToString(),R(rect.x+rect.width*.55f,rect.y+rect.height*.68f,rect.width*.39f,rect.height*.24f),tiny,ink);}
            }
            if(number>0)Label(number.ToString(),R(rect.x+3*uiScale,rect.y+2*uiScale,rect.width*.25f,rect.height*.22f),tiny,muted);
        }
        void HandleArrayClick(ItemStack[] slots,int index,int kind,int button,bool shift)
        {
            selectedKind=kind;selectedIndex=index;
            if(button==1)
            {
                if(held.Empty)
                {if(slots[index].count>1){int take=(slots[index].count+1)/2;held=new ItemStack(slots[index].id,take);slots[index].count-=take;if(slots[index].count<=0)slots[index].Clear();}else if(!slots[index].Empty){held=slots[index];slots[index].Clear();}}
                else if(slots[index].Empty){slots[index]=new ItemStack(held.id,1);held.count--;if(held.count<=0)held.Clear();}
                else if(slots[index].id==held.id&&slots[index].count<ItemCatalog.Get(held.id).maxStack){slots[index].count++;held.count--;if(held.count<=0)held.Clear();}
                return;
            }
            if(shift&&held.Empty)
            {
                if(kind==0)bag.QuickMoveBackpack(index);else if(kind==1)bag.QuickMoveBelt(index);else if(kind==2)bag.QuickMoveContainer(index);else if(kind==3){bag.Unequip(index);RefreshGear();}return;
            }
            if(held.Empty){if(!slots[index].Empty){held=slots[index];slots[index].Clear();}}
            else PlaceHeld(slots,index);
            if(kind==3)RefreshGear();
        }
        void HandleGearClick(int index,int button)
        {
            selectedKind=3;selectedIndex=index;var slot=bag.Gear[index];
            if(button==1){if(held.Empty&&!slot.Empty){held=new ItemStack(slot.id,1);slot.count--;if(slot.count<=0)slot.Clear();}else if(!held.Empty&&slot.Empty&&ItemCatalog.Get(held.id).gearSlot==(GearSlot)index){slot=new ItemStack(held.id,1);held.count--;if(held.count<=0)held.Clear();}bag.Gear[index]=slot;RefreshGear();return;}
            if(held.Empty){if(!slot.Empty){held=slot;bag.Gear[index].Clear();}}
            else if(ItemCatalog.Get(held.id).gearSlot==(GearSlot)index)
            {bag.Gear[index]=held;held=slot;}
            RefreshGear();
        }
        void PlaceHeld(ItemStack[] slots,int index)
        {
            ItemStack target=slots[index];
            if(target.Empty){slots[index]=held;held.Clear();return;}
            if(target.id==held.id&&target.count<ItemCatalog.Get(held.id).maxStack){int moved=Mathf.Min(held.count,ItemCatalog.Get(held.id).maxStack-target.count);target.count+=moved;held.count-=moved;slots[index]=target;if(held.count<=0)held.Clear();return;}
            slots[index]=held;held=target;
        }
        void RefreshGear(){bag.RefreshGear();preview?.SetGear(bag.Gear);}
        ItemStack SelectedStack()
        {
            if(selectedKind==0&&selectedIndex>=0&&selectedIndex<bag.Backpack.Length)return bag.Backpack[selectedIndex];
            if(selectedKind==1&&selectedIndex>=0&&selectedIndex<bag.Belt.Length)return bag.Belt[selectedIndex];
            if(selectedKind==2&&bag.OpenContainer!=null&&selectedIndex>=0&&selectedIndex<bag.OpenContainer.Items.Length)return bag.OpenContainer.Items[selectedIndex];
            if(selectedKind==3&&selectedIndex>=0&&selectedIndex<bag.Gear.Length)return bag.Gear[selectedIndex];return new ItemStack();
        }
        static bool Usable(ItemId id)=>id==ItemId.Water||id==ItemId.CannedFood||id==ItemId.Bandage||id==ItemId.Medkit;
        void UseShown(bool fromCursor)
        {
            if(fromCursor){ItemId id=held.id;if(id==ItemId.Water)RunState.Instance.Drink(38);else if(id==ItemId.CannedFood)RunState.Instance.Eat(32);else if(id==ItemId.Bandage)RunState.Instance.Heal(24);else if(id==ItemId.Medkit)RunState.Instance.Heal(62);held.count--;if(held.count<=0)held.Clear();}
            else if(selectedKind==0)bag.UseBackpack(selectedIndex);else if(selectedKind==1)bag.UseBelt(selectedIndex);
        }
        void EquipShown(bool fromCursor)
        {
            if(fromCursor&&ItemCatalog.Get(held.id).gearSlot.HasValue)
            {int i=(int)ItemCatalog.Get(held.id).gearSlot.Value;ItemStack old=bag.Gear[i];bag.Gear[i]=new ItemStack(held.id,1);held.count--;if(held.count<=0)held=old;else if(!old.Empty)WorldItem.Spawn(old.id,old.count,FindObjectOfType<PlayerController>().transform.position+Vector3.right);RefreshGear();}
            else if(selectedKind==0){bag.EquipFromBackpack(selectedIndex);RefreshGear();}
        }
        void DropShown(bool fromCursor)
        {
            var player=FindObjectOfType<PlayerController>();if(player==null)return;
            if(fromCursor){WorldItem.Spawn(held.id,held.count,player.transform.position+Vector3.right*1.1f);held.Clear();}
            else if(selectedKind==0)bag.DropBackpack(selectedIndex);else if(selectedKind==1){var s=bag.Belt[selectedIndex];if(!s.Empty){bag.Belt[selectedIndex].Clear();WorldItem.Spawn(s.id,s.count,player.transform.position+Vector3.right);}}
        }
        void DrawCursorStack()
        {if(held.Empty)return;Rect r=R(Event.current.mousePosition.x+12,Event.current.mousePosition.y+8,42*uiScale,42*uiScale);DrawSlotContents(r,held,true,0);}

        void DrawPause()
        {
            DrawBackdrop();Rect r=CenteredPanel(440*uiScale,350*uiScale);Panel(r,new Color(.045f,.06f,.05f,.97f));Accent(r.x,r.y,r.height);
            Label("СМЕНА ПРИОСТАНОВЛЕНА",R(r.x+27*uiScale,r.y+28*uiScale,r.width-54*uiScale,43*uiScale),heading,ink);Label("Тишина здесь никогда не длится долго.",R(r.x+29*uiScale,r.y+81*uiScale,r.width-58*uiScale,34*uiScale),small,muted);
            float bx=r.x+28*uiScale,bw=r.width-56*uiScale,bh=40*uiScale;if(Button("ПРОДОЛЖИТЬ",R(bx,r.y+143*uiScale,bw,bh),lightButton))SetMode(ScreenMode.Playing);if(Button("ИНВЕНТАРЬ",R(bx,r.y+193*uiScale,bw,bh),darkButton))OpenInventory();if(Button("НАСТРОЙКИ",R(bx,r.y+243*uiScale,bw,bh),darkButton)){returnMode=ScreenMode.Paused;SetMode(ScreenMode.Settings);}if(Button("ГЛАВНОЕ МЕНЮ",R(bx,r.y+293*uiScale,bw,bh),darkButton))SetMode(ScreenMode.MainMenu);
        }
        void DrawSettings()
        {
            DrawBackdrop();float pw=Mathf.Min(Screen.width*.94f,1420*uiScale),ph=Mathf.Min(Screen.height*.92f,840*uiScale);Rect r=CenteredPanel(pw,ph);Panel(r,new Color(.025f,.030f,.025f,.91f));
            Label("S U B S I S T E N C E  /  С И С Т Е М А",R(r.x+26*uiScale,r.y+18*uiScale,r.width*.62f,16*uiScale),tiny,muted);
            Label("НАСТРОЙКИ",R(r.x+25*uiScale,r.y+46*uiScale,r.width*.7f,45*uiScale),title,ink);
            Label("ПОДГОТОВЬСЯ К ВЫХОДУ В КОРИДОР",R(r.x+30*uiScale,r.y+96*uiScale,r.width*.6f,16*uiScale),mono,muted);
            if(Button("ESC  НАЗАД",R(r.x+r.width-165*uiScale,r.y+26*uiScale,132*uiScale,30*uiScale),darkButton))SetMode(returnMode);
            DrawRule(r.x+25*uiScale,r.y+125*uiScale,r.width-50*uiScale);
            float navX=r.x+24*uiScale,navY=r.y+148*uiScale,navW=Mathf.Min(246*uiScale,r.width*.22f),navH=48*uiScale,contentX=navX+navW+24*uiScale,contentW=r.x+r.width-26*uiScale-contentX,contentY=navY,contentH=r.height-218*uiScale;
            string[] tabs={"01   ГРАФИКА","02   ЗВУК","03   УПРАВЛЕНИЕ","04   ИНТЕРФЕЙС"};
            for(int i=0;i<tabs.Length;i++)
            {
                Rect tr=R(navX,navY+i*(navH+8*uiScale),navW,navH);Panel(tr,i==settingsTab?new Color(.32f,.15f,.08f,.94f):new Color(.042f,.048f,.04f,.9f));
                bool clicked=GUI.Button(tr,GUIContent.none,GUIStyle.none);if(i==settingsTab)DrawRect(R(tr.x,tr.y,4*uiScale,tr.height),copper);
                Label(tabs[i],R(tr.x+16*uiScale,tr.y+14*uiScale,tr.width-30*uiScale,18*uiScale),mono,i==settingsTab?ink:muted);if(clicked)settingsTab=i;
            }
            Rect content=R(contentX,contentY,contentW,contentH);Panel(content,new Color(.037f,.043f,.036f,.87f));
            if(settingsTab==0)DrawGraphicsSettings(content);
            else if(settingsTab==1)DrawSoundSettings(content);
            else if(settingsTab==2)DrawControlRows(content);
            else DrawInterfaceSettings(content);
            DrawRule(r.x+25*uiScale,r.y+r.height-62*uiScale,r.width-50*uiScale);
            if(Button("СБРОСИТЬ",R(r.x+25*uiScale,r.y+r.height-49*uiScale,150*uiScale,34*uiScale),darkButton))ResetSettings();
            Label("НАСТРОЙКИ СОХРАНЯЮТСЯ НА ЭТОМ УСТРОЙСТВЕ",R(r.x+190*uiScale,r.y+r.height-42*uiScale,r.width*.43f,17*uiScale),tiny,muted);
            if(Button("ПРИМЕНИТЬ",R(r.x+r.width-214*uiScale,r.y+r.height-49*uiScale,188*uiScale,34*uiScale),lightButton)){SaveSettings();SetMode(returnMode);}
        }
        void DrawGraphicsSettings(Rect r)
        {
            Label("ИЗОБРАЖЕНИЕ",R(r.x+24*uiScale,r.y+17*uiScale,r.width-48*uiScale,25*uiScale),heading,ink);
            Label("Качество картинки и видимость окружения",R(r.x+26*uiScale,r.y+47*uiScale,r.width-52*uiScale,18*uiScale),small,muted);
            float y=r.y+91*uiScale,rowH=55*uiScale;SettingLabel(r,"РЕЖИМ ОТОБРАЖЕНИЯ",y);
            if(Button(fullscreen?"ПОЛНЫЙ ЭКРАН":"ОКОННЫЙ РЕЖИМ",R(r.x+r.width*.52f,y-5*uiScale,r.width*.43f,37*uiScale),darkButton)){fullscreen=!fullscreen;ApplyResolution();}
            y+=rowH;SettingLabel(r,"РАЗРЕШЕНИЕ",y);
            string resolution=resolutions.Length>0?resolutions[Mathf.Clamp(resolutionIndex,0,resolutions.Length-1)].width+" × "+resolutions[Mathf.Clamp(resolutionIndex,0,resolutions.Length-1)].height:"ПО УМОЛЧАНИЮ";
            if(Button("‹    "+resolution+"    ›",R(r.x+r.width*.52f,y-5*uiScale,r.width*.43f,37*uiScale),darkButton)){resolutionIndex=(resolutionIndex+1)%resolutions.Length;ApplyResolution();}
            y+=rowH;SettingLabel(r,"КАЧЕСТВО ГРАФИКИ",y);
            string quality=QualitySettings.names.Length>0?QualitySettings.names[Mathf.Clamp(qualityIndex,0,QualitySettings.names.Length-1)].ToUpperInvariant():"ПО УМОЛЧАНИЮ";
            if(Button("‹    "+quality+"    ›",R(r.x+r.width*.52f,y-5*uiScale,r.width*.43f,37*uiScale),darkButton)){qualityIndex=(qualityIndex+1)%Mathf.Max(1,QualitySettings.names.Length);ApplyVisualSettings();}
            y+=rowH;SettingLabel(r,"ВЕРТИКАЛЬНАЯ СИНХРОНИЗАЦИЯ",y);
            if(Button(vSyncSetting==1?"ВКЛЮЧЕНА":"ВЫКЛЮЧЕНА",R(r.x+r.width*.52f,y-5*uiScale,r.width*.43f,37*uiScale),darkButton)){vSyncSetting=vSyncSetting==0?1:0;ApplyVisualSettings();}
            y+=rowH+3*uiScale;DrawSliderSetting(r,"ЯРКОСТЬ",ref brightnessSetting,y,.15f,1f);
            Label("Совет: понизь яркость, чтобы сохранить контраст тёмных коридоров.",R(r.x+25*uiScale,y+32*uiScale,r.width-50*uiScale,28*uiScale),tiny,muted);
        }
        void DrawSoundSettings(Rect r)
        {
            Label("ЗВУК",R(r.x+24*uiScale,r.y+17*uiScale,r.width-48*uiScale,25*uiScale),heading,ink);
            Label("Гул коридора, радио и сигналы окружения.",R(r.x+26*uiScale,r.y+47*uiScale,r.width-52*uiScale,18*uiScale),small,muted);
            float y=r.y+108*uiScale;float volume=AudioDirector.Instance!=null?AudioDirector.Instance.MasterVolume:.72f,oldVolume=volume;
            DrawSliderSetting(r,"ОБЩАЯ ГРОМКОСТЬ",ref volume,y,0,1);if(Mathf.Abs(volume-oldVolume)>.001f)AudioDirector.Instance?.SetMasterVolume(volume);
            y+=81*uiScale;float ambience=AudioDirector.Instance!=null?AudioDirector.Instance.AmbienceVolume:.30f,oldAmbience=ambience;
            DrawSliderSetting(r,"АТМОСФЕРА",ref ambience,y,0,1);if(Mathf.Abs(ambience-oldAmbience)>.001f)AudioDirector.Instance?.SetAmbienceVolume(ambience);
            y+=81*uiScale;float music=AudioDirector.Instance!=null?AudioDirector.Instance.MusicVolume:.23f,oldMusic=music;
            DrawSliderSetting(r,"МУЗЫКА",ref music,y,0,1);if(Mathf.Abs(music-oldMusic)>.001f)AudioDirector.Instance?.SetMusicVolume(music);
            Label("Отдельные уровни сохраняются локально.",R(r.x+25*uiScale,y+42*uiScale,r.width-50*uiScale,18*uiScale),tiny,muted);
        }
        void DrawControlRows(Rect r)
        {
            Label("УПРАВЛЕНИЕ",R(r.x+24*uiScale,r.y+17*uiScale,r.width-48*uiScale,25*uiScale),heading,ink);
            string[] keys={"W / A / S / D   ИЛИ   ← →","SPACE / W / ↑","E","Q","F","1 — 6","TAB / I","SHIFT + ЛКМ","ПКМ","ESC"};
            string[] actions={"Движение по сектору","Прыжок","Открыть ящик · подобрать · проверить выход","Атака / отбить сталкера","Фонарь","Выбрать слот пояса","Инвентарь и экипировка","Быстрый перенос стопки","Разделить стопку / положить одну","Пауза или закрыть окно"};
            float top=r.y+61*uiScale,row=29*uiScale;
            for(int i=0;i<keys.Length;i++){float yy=top+i*row;Label(keys[i],R(r.x+26*uiScale,yy,r.width*.37f,20*uiScale),mono,gold);Label(actions[i],R(r.x+r.width*.40f,yy,r.width*.56f,20*uiScale),small,ink);}
        }
        void DrawInterfaceSettings(Rect r)
        {
            Label("ИНТЕРФЕЙС",R(r.x+24*uiScale,r.y+17*uiScale,r.width-48*uiScale,25*uiScale),heading,ink);
            DrawSliderSetting(r,"МАСШТАБ ИНТЕРФЕЙСА",ref uiScaleSetting,r.y+102*uiScale,.75f,1.20f);
            float y=r.y+177*uiScale;Panel(R(r.x+22*uiScale,y,r.width-44*uiScale,56*uiScale),new Color(.06f,.07f,.058f));
            Label("ПОДСКАЗКИ ВЗАИМОДЕЙСТВИЯ",R(r.x+35*uiScale,y+8*uiScale,r.width*.58f,18*uiScale),mono,ink);
            if(Button(showInteractionHints?"ВКЛ":"ВЫКЛ",R(r.x+r.width-142*uiScale,y+9*uiScale,100*uiScale,36*uiScale),darkButton)){showInteractionHints=!showInteractionHints;PlayerPrefs.SetInt("Subsistence.InteractionHints",showInteractionHints?1:0);}
            y+=68*uiScale;Panel(R(r.x+22*uiScale,y,r.width-44*uiScale,56*uiScale),new Color(.06f,.07f,.058f));
            Label("СООБЩЕНИЯ ИГРЫ",R(r.x+35*uiScale,y+8*uiScale,r.width*.58f,18*uiScale),mono,ink);
            if(Button(showNotices?"ВКЛ":"ВЫКЛ",R(r.x+r.width-142*uiScale,y+9*uiScale,100*uiScale,36*uiScale),darkButton)){showNotices=!showNotices;PlayerPrefs.SetInt("Subsistence.Notices",showNotices?1:0);}
            Label("Предпросмотр интерфейса",R(r.x+27*uiScale,y+82*uiScale,r.width-54*uiScale,18*uiScale),tiny,muted);
        }
        void SettingLabel(Rect r,string text,float y){Label(text,R(r.x+26*uiScale,y,r.width*.45f,20*uiScale),mono,ink);}
        void DrawSliderSetting(Rect r,string titleText,ref float value,float y,float min,float max)
        {
            SettingLabel(r,titleText,y);float old=value;
            value=GUI.HorizontalSlider(R(r.x+r.width*.52f,y+2*uiScale,r.width*.34f,20*uiScale),value,min,max);
            Label(Mathf.RoundToInt(value*100)+"%",R(r.x+r.width*.88f,y,r.width*.09f,20*uiScale),mono,lime);
            if(Mathf.Abs(value-old)>.0005f){if(titleText=="ЯРКОСТЬ")ApplyVisualSettings();if(titleText=="МАСШТАБ ИНТЕРФЕЙСА")PlayerPrefs.SetFloat("Subsistence.UIScale",uiScaleSetting);}
        }
        void ApplyResolution()
        {
            if(resolutions==null||resolutions.Length==0)return;resolutionIndex=Mathf.Clamp(resolutionIndex,0,resolutions.Length-1);Resolution res=resolutions[resolutionIndex];Screen.SetResolution(res.width,res.height,fullscreen);
        }
        void ApplyVisualSettings()
        {
            if(QualitySettings.names.Length>0){qualityIndex=Mathf.Clamp(qualityIndex,0,QualitySettings.names.Length-1);QualitySettings.SetQualityLevel(qualityIndex,true);}
            QualitySettings.vSyncCount=vSyncSetting;RenderSettings.ambientIntensity=Mathf.Lerp(.42f,1.20f,brightnessSetting);
        }
        void SaveSettings()
        {
            PlayerPrefs.SetInt("Subsistence.Fullscreen",fullscreen?1:0);PlayerPrefs.SetFloat("Subsistence.Brightness",brightnessSetting);PlayerPrefs.SetFloat("Subsistence.UIScale",uiScaleSetting);
            PlayerPrefs.SetInt("Subsistence.Quality",qualityIndex);PlayerPrefs.SetInt("Subsistence.VSync",vSyncSetting);PlayerPrefs.SetInt("Subsistence.InteractionHints",showInteractionHints?1:0);PlayerPrefs.SetInt("Subsistence.Notices",showNotices?1:0);
            if(resolutions!=null&&resolutions.Length>0){Resolution res=resolutions[Mathf.Clamp(resolutionIndex,0,resolutions.Length-1)];PlayerPrefs.SetInt("Subsistence.ResWidth",res.width);PlayerPrefs.SetInt("Subsistence.ResHeight",res.height);}
            PlayerPrefs.Save();
        }
        void ResetSettings()
        {
            fullscreen=true;brightnessSetting=.5f;uiScaleSetting=.94f;showInteractionHints=true;showNotices=true;qualityIndex=Mathf.Max(0,QualitySettings.names.Length-1);vSyncSetting=1;resolutionIndex=Mathf.Max(0,resolutions.Length-1);
            ApplyResolution();ApplyVisualSettings();AudioDirector.Instance?.SetMasterVolume(.72f);AudioDirector.Instance?.SetAmbienceVolume(.30f);AudioDirector.Instance?.SetMusicVolume(.23f);SaveSettings();
        }

        void DrawControls()
        {
            DrawBackdrop();Rect r=CenteredPanel(Mathf.Min(900*uiScale,Screen.width*.90f),Mathf.Min(650*uiScale,Screen.height*.90f));Panel(r,new Color(.025f,.030f,.025f,.91f));
            Label("S U B S I S T E N C E  /  С И С Т Е М А",R(r.x+25*uiScale,r.y+20*uiScale,r.width-50*uiScale,16*uiScale),tiny,muted);
            Label("УПРАВЛЕНИЕ",R(r.x+24*uiScale,r.y+50*uiScale,r.width-48*uiScale,44*uiScale),title,ink);
            DrawControlRows(R(r.x+24*uiScale,r.y+115*uiScale,r.width-48*uiScale,r.height-195*uiScale));
            if(Button("←  НАЗАД",R(r.x+26*uiScale,r.y+r.height-57*uiScale,170*uiScale,36*uiScale),darkButton))SetMode(returnMode);
        }
        void DrawSupport()
        {
            DrawBackdrop();Rect r=CenteredPanel(Mathf.Min(850*uiScale,Screen.width*.90f),Mathf.Min(590*uiScale,Screen.height*.90f));Panel(r,new Color(.025f,.030f,.025f,.91f));
            Label("S U B S I S T E N C E  /  FIELD SUPPORT",R(r.x+26*uiScale,r.y+21*uiScale,r.width-52*uiScale,16*uiScale),tiny,muted);
            Label("ПОДДЕРЖКА",R(r.x+25*uiScale,r.y+53*uiScale,r.width-50*uiScale,48*uiScale),title,ink);
            Label("ЭТА СБОРКА РАБОТАЕТ ЛОКАЛЬНО",R(r.x+29*uiScale,r.y+113*uiScale,r.width-58*uiScale,19*uiScale),mono,copper);
            Label("Сетевой вход и игровые серверы пока не подключены. Все доступные экспедиции запускаются на этом устройстве; список не имитирует онлайн-игроков или пинг.",R(r.x+29*uiScale,r.y+146*uiScale,r.width-58*uiScale,60*uiScale),body,muted);
            Rect note=R(r.x+27*uiScale,r.y+231*uiScale,r.width-54*uiScale,112*uiScale);Panel(note,new Color(.05f,.06f,.048f));
            Label("ЕСЛИ НУЖНА ПОМОЩЬ",R(note.x+16*uiScale,note.y+14*uiScale,note.width-32*uiScale,19*uiScale),mono,ink);
            Label("Открой вкладку управления для клавиш. Для ошибки запиши шаги, которые к ней привели, и приложи лог Unity Editor после проверки проекта.",R(note.x+16*uiScale,note.y+42*uiScale,note.width-32*uiScale,54*uiScale),small,muted);
            if(Button("УПРАВЛЕНИЕ  →",R(r.x+28*uiScale,r.y+374*uiScale,250*uiScale,40*uiScale),darkButton)){returnMode=ScreenMode.Support;SetMode(ScreenMode.Controls);}
            Label("ВЕРСИЯ ПРОТОТИПА  /  UNITY 2022.3.62f2",R(r.x+29*uiScale,r.y+r.height-51*uiScale,r.width*.58f,16*uiScale),tiny,muted);
            if(Button("НАЗАД",R(r.x+r.width-170*uiScale,r.y+r.height-59*uiScale,140*uiScale,38*uiScale),lightButton))SetMode(ScreenMode.MainMenu);
        }
        void DrawEndCard(bool won)
        {
            DrawBackdrop();Rect r=CenteredPanel(500*uiScale,330*uiScale);Panel(r,new Color(.045f,.06f,.05f,.91f));Accent(r.x,r.y,r.height);
            Label(won?"СЕКТОР ПОКИНУТ":"СМЕНА ЗАВЕРШЕНА",R(r.x+28*uiScale,r.y+32*uiScale,r.width-56*uiScale,50*uiScale),heading,ink);
            Label(won?"Ты выбрался из этого крыла. В глубине снова загорается свет.":"Сталкер оказался быстрее. Попробуй другой сектор.",R(r.x+30*uiScale,r.y+99*uiScale,r.width-60*uiScale,55*uiScale),body,muted);
            if(Button("НАЧАТЬ СНОВА",R(r.x+30*uiScale,r.y+188*uiScale,r.width-60*uiScale,40*uiScale),lightButton))StartRun();if(Button("ГЛАВНОЕ МЕНЮ",R(r.x+30*uiScale,r.y+240*uiScale,r.width-60*uiScale,40*uiScale),darkButton))SetMode(ScreenMode.MainMenu);
        }
        void StartRun()
        {
            RunState.Instance.ResetForNewRun();var player=FindObjectOfType<PlayerController>();if(player!=null)player.ResetForNewRun(sectorStarts[selectedSector]);
            foreach(var watcher in FindObjectsOfType<WatcherAI>())watcher.ResetForNewRun();foreach(var crate in FindObjectsOfType<LootContainer>())crate.ResetForNewRun();
            bag=player!=null?player.Inventory:null;if(preview!=null&&bag!=null)preview.SetGear(bag.Gear);held.Clear();selectedKind=-1;SetMode(ScreenMode.Playing);AudioDirector.Instance?.Play("ui",.7f);
        }
        void SetMode(ScreenMode next)
        {
            Mode=next;bool stopped=next!=ScreenMode.Playing;Time.timeScale=stopped?0:1;Cursor.visible=stopped;Cursor.lockState=CursorLockMode.None;
            if(next==ScreenMode.MainMenu)returnMode=ScreenMode.MainMenu;
            MenuBackdropActive=next==ScreenMode.MainMenu||next==ScreenMode.ServerSelect||next==ScreenMode.Support||((next==ScreenMode.Settings||next==ScreenMode.Controls)&&(returnMode==ScreenMode.MainMenu||returnMode==ScreenMode.Support));
            AudioDirector.Instance?.Play("ui",.4f);
        }
        Rect CenteredPanel(float w,float h)=>R((Screen.width-w)/2,(Screen.height-h)/2,w,h);
        static Rect R(float x,float y,float w,float h)=>new Rect(x,y,w,h);
        void Panel(Rect r,Color c){DrawRect(r,c);Outline(r,new Color(.25f,.29f,.23f));}
        void Accent(float x,float y,float h){DrawRect(R(x,y,3*uiScale,h),new Color(.75f,.82f,.51f));}
        void Outline(Rect r,Color c){float t=Mathf.Max(1,uiScale);DrawRect(R(r.x,r.y,r.width,t),c);DrawRect(R(r.x,r.y+r.height-t,r.width,t),c);DrawRect(R(r.x,r.y,t,r.height),c);DrawRect(R(r.x+r.width-t,r.y,t,r.height),c);}
        void DrawRule(float x,float y,float width){DrawRect(R(x,y,width,1*uiScale),new Color(.29f,.33f,.26f));}
        bool Button(string text,Rect rect,GUIStyle style)=>GUI.Button(rect,text,style);
        void Label(string text,Rect rect,GUIStyle style,Color color){Color old=GUI.color;GUI.color=color;GUI.Label(rect,text,style);GUI.color=old;}
        static void DrawRect(Rect rect,Color color){Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        static Texture2D Tex(Color color){var t=new Texture2D(1,1,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};t.SetPixel(0,0,color);t.Apply();return t;}
        void Toast(string message){float w=Mathf.Min(430*uiScale,Screen.width*.7f);Rect r=R((Screen.width-w)/2,Screen.height-39*uiScale,w,28*uiScale);Panel(r,new Color(.045f,.06f,.045f,.97f));Label(message,R(r.x+9*uiScale,r.y+6*uiScale,r.width-18*uiScale,16*uiScale),small,ink);}
    }
}
