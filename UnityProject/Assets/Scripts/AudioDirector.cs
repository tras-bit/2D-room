using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Original, locally synthesised sound palette; all clips live in Resources/Audio and require no licensed packs.</summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }
        public float MasterVolume { get; private set; } = .72f;
        AudioSource ambience;
        AudioSource music;
        AudioSource effects;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            MasterVolume=PlayerPrefs.GetFloat("Subsistence.MasterVolume",.72f);
            AudioListener.volume=MasterVolume;
            DontDestroyOnLoad(gameObject);
            Load("ambience", "ambience_service_tunnel");
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
            ambience.volume = .30f;
            ambience.spatialBlend = 0f;
            ambience.playOnAwake = false;
            ambience.ignoreListenerPause = true;
            if (ambience.clip != null) ambience.Play();

            music=gameObject.AddComponent<AudioSource>();
            music.clip=clips.TryGetValue("music",out var score)?score:null;
            music.loop=true;music.volume=.23f;music.spatialBlend=0f;music.playOnAwake=false;music.ignoreListenerPause=true;
            if(music.clip!=null)music.Play();

            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.spatialBlend = 0f;
            effects.volume = 1f;
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
            MasterVolume = Mathf.Clamp01(value);
            AudioListener.volume = MasterVolume;
            PlayerPrefs.SetFloat("Subsistence.MasterVolume",MasterVolume);
            PlayerPrefs.Save();
        }

        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
