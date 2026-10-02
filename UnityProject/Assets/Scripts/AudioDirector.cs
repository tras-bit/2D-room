using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Original, locally synthesised sound palette; all clips live in Resources/Audio and require no licensed packs.</summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }
        public float MasterVolume { get; private set; } = .72f;
        public float AmbienceVolume { get; private set; } = .30f;
        public float MusicVolume { get; private set; } = .46f;
        AudioSource ambience;
        AudioSource fluorescentHum;
        AudioSource music;
        AudioSource effects;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            MasterVolume=PlayerPrefs.GetFloat("Subsistence.MasterVolume",.72f);
            AmbienceVolume=PlayerPrefs.GetFloat("Subsistence.AmbienceVolume",.30f);
            if(PlayerPrefs.GetInt("Subsistence.MusicVolumeDoubled",0)==0)
            {
                float previousMusic=PlayerPrefs.GetFloat("Subsistence.MusicVolume",.23f);
                PlayerPrefs.SetFloat("Subsistence.MusicVolume",Mathf.Clamp01(previousMusic*2f));
                PlayerPrefs.SetInt("Subsistence.MusicVolumeDoubled",1);
                PlayerPrefs.Save();
            }
            MusicVolume=PlayerPrefs.GetFloat("Subsistence.MusicVolume",.46f);
            AudioListener.volume=MasterVolume;
            DontDestroyOnLoad(gameObject);
            Load("ambience", "ambience_service_tunnel");
            Load("fluorescent", "fluorescent_level0");
            Load("music", "theme_subsistence");
            Load("ui", "ui_confirm");
            Load("metal", "pickup_metal");
            Load("water", "pickup_water");
            Load("cloth", "pickup_cloth");
            Load("step", "footstep_concrete");
            Load("swipe", "melee_swipe");
            Load("hit", "watcher_hit");
            Load("stun", "watcher_stun");

            ambience = gameObject.AddComponent<AudioSource>();
            ambience.clip = clips.TryGetValue("ambience", out var bed) ? bed : null;
            ambience.loop = true;
            ambience.volume = AmbienceVolume;
            ambience.spatialBlend = 0f;
            ambience.playOnAwake = false;
            ambience.ignoreListenerPause = true;
            if (ambience.clip != null) ambience.Play();

            fluorescentHum=gameObject.AddComponent<AudioSource>();
            fluorescentHum.clip=clips.TryGetValue("fluorescent",out var hum)?hum:null;
            fluorescentHum.loop=true;fluorescentHum.volume=0f;fluorescentHum.spatialBlend=0f;fluorescentHum.playOnAwake=false;fluorescentHum.ignoreListenerPause=true;
            if(fluorescentHum.clip!=null)fluorescentHum.Play();

            music=gameObject.AddComponent<AudioSource>();
            music.clip=clips.TryGetValue("music",out var score)?score:null;
            music.loop=true;music.volume=MusicVolume;music.spatialBlend=0f;music.playOnAwake=false;music.ignoreListenerPause=true;
            if(music.clip!=null)music.Play();

            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.spatialBlend = 0f;
            effects.volume = 1f;
        }

        void Update()
        {
            if(fluorescentHum==null)return;
            var mode=GameHUD.Mode;
            bool inGame=mode==GameHUD.ScreenMode.Playing||mode==GameHUD.ScreenMode.Inventory||mode==GameHUD.ScreenMode.Paused;
            int level=RunState.Instance!=null?RunState.Instance.CurrentLevel:0;
            bool inLevelZero=inGame&&level==0;
            float bedTarget=inGame?(level==0?AmbienceVolume*.14f:AmbienceVolume):AmbienceVolume*.22f;
            ambience.volume=Mathf.MoveTowards(ambience.volume,bedTarget,Time.unscaledDeltaTime*.28f);
            float humTarget=inLevelZero?AmbienceVolume*.44f:0f;
            fluorescentHum.volume=Mathf.MoveTowards(fluorescentHum.volume,humTarget,Time.unscaledDeltaTime*.22f);
        }

        void Load(string key,string resource)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + resource);
            if (clip != null) clips[key] = clip;
        }

        public void Play(string cue,float volume=1f)
        {
            if (effects == null || !clips.TryGetValue(cue,out var clip)) return;
            effects.PlayOneShot(clip,Mathf.Clamp01(volume));
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume=Mathf.Clamp01(value);AudioListener.volume=MasterVolume;
            PlayerPrefs.SetFloat("Subsistence.MasterVolume",MasterVolume);PlayerPrefs.Save();
        }
        public void SetAmbienceVolume(float value)
        {
            AmbienceVolume=Mathf.Clamp01(value);if(ambience!=null)ambience.volume=AmbienceVolume;
            PlayerPrefs.SetFloat("Subsistence.AmbienceVolume",AmbienceVolume);PlayerPrefs.Save();
        }
        public void SetMusicVolume(float value)
        {
            MusicVolume=Mathf.Clamp01(value);if(music!=null)music.volume=MusicVolume;
            PlayerPrefs.SetFloat("Subsistence.MusicVolume",MusicVolume);PlayerPrefs.Save();
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
