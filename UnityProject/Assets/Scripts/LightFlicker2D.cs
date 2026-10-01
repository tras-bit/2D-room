using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Subsistence
{
    [DisallowMultipleComponent]
    #if UNITY_2019_3_OR_NEWER
    [RequireComponent(typeof(Light2D))]
    #endif
    public sealed class LightFlicker2D : MonoBehaviour
    {
        #if UNITY_2019_3_OR_NEWER
        Light2D _light;
        float baseIntensity;
        float baseRadius;
        float timer;
        float strength;
        bool industrial;

        public void Initialize(float intensity, bool isIndustrial)
        {
            _light = GetComponent<Light2D>();
            baseIntensity = intensity;
            baseRadius = _light != null ? _light.pointLightOuterRadius : 0f;
            strength = isIndustrial ? .18f : .11f;
            industrial = isIndustrial;
        }

        void Update()
        {
            if (_light == null) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = Random.Range(.03f, industrial ? .28f : .18f);
            float flicker = Mathf.Pow(Random.value, 2.8f);
            float dip = Random.value < .08f ? Random.Range(.32f, .75f) : 1f;
            float finalIntensity = baseIntensity*(1f-flicker*strength)*dip;
            _light.intensity = finalIntensity;
            _light.pointLightOuterRadius = Mathf.Max(baseRadius*.86f, baseRadius*(.88f+finalIntensity/Mathf.Max(.001f,baseIntensity)*.12f));
        }
        #endif
    }
}
