using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Small authored-in-code sprite clip player for pixel-art idle, move, jump, attack and stagger cycles.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelFrameAnimator : MonoBehaviour
    {
        sealed class Clip
        {
            public Sprite[] frames;
            public float framesPerSecond;
            public bool loop;
        }

        readonly Dictionary<string, Clip> clips = new Dictionary<string, Clip>();
        SpriteRenderer spriteRenderer;
        string current;
        float clock;
        int frameIndex;

        void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }

        public void AddClip(string name, Sprite[] frames, float framesPerSecond, bool loop = true)
        {
            if (frames == null || frames.Length == 0) return;
            clips[name] = new Clip { frames = frames, framesPerSecond = Mathf.Max(.1f, framesPerSecond), loop = loop };
            if (string.IsNullOrEmpty(current)) Play(name, true);
        }

        public void Play(string name, bool restart = false)
        {
            if (!clips.ContainsKey(name)) return;
            if (!restart && current == name) return;
            current = name;
            frameIndex = 0;
            clock = 0;
            spriteRenderer.sprite = clips[current].frames[0];
        }

        void Update()
        {
            if (string.IsNullOrEmpty(current) || !clips.TryGetValue(current, out var clip) || clip.frames.Length < 2) return;
            clock += Time.deltaTime;
            float frameDuration = 1f / clip.framesPerSecond;
            while (clock >= frameDuration)
            {
                clock -= frameDuration;
                if (frameIndex < clip.frames.Length - 1) frameIndex++;
                else if (clip.loop) frameIndex = 0;
            }
            spriteRenderer.sprite = clip.frames[frameIndex];
        }
    }
}
