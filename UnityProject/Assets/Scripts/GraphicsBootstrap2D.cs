using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace Subsistence
{
    public static class GraphicsBootstrap2D
    {
        static VolumeProfile runtimeFallbackProfile;

        public static void Configure(Camera cameraComponent, Transform worldRoot)
        {
            if (cameraComponent == null) return;
            cameraComponent.allowHDR = true;
            #if UNITY_2019_3_OR_NEWER
            UniversalAdditionalCameraData cameraData = cameraComponent.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null) cameraData = cameraComponent.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            EnsureGlobalVolume(worldRoot);
            EnsureGlobalLight(worldRoot);
            #endif
        }

        #if UNITY_2019_3_OR_NEWER
        static void EnsureGlobalLight(Transform parent)
        {
            Transform existing = parent.Find("URP 2D · Global Ambient");
            if (existing != null && existing.GetComponent<Light2D>() != null) return;
            GameObject lightObject = new GameObject("URP 2D · Global Ambient");
            lightObject.transform.SetParent(parent, false);
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = new Color(.78f,.72f,.58f,1f);
            light.intensity = .74f;
        }

        static void EnsureGlobalVolume(Transform parent)
        {
            Transform existing = parent.Find("Cinematic Global Volume");
            Volume volume = existing != null ? existing.GetComponent<Volume>() : null;
            if (volume == null)
            {
                GameObject volumeObject = new GameObject("Cinematic Global Volume");
                volumeObject.transform.SetParent(parent, false);
                volume = volumeObject.AddComponent<Volume>();
            }
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 1f;
            VolumeProfile profile = Resources.Load<VolumeProfile>("Rendering/BackroomsGlobalVolumeProfile");
            if (profile == null)
            {
                if (runtimeFallbackProfile == null) runtimeFallbackProfile = CreateRuntimeProfile();
                profile = runtimeFallbackProfile;
            }
            volume.sharedProfile = profile;
        }

        static VolumeProfile CreateRuntimeProfile()
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Runtime Cinematic 2D Profile";
            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.18f);
            bloom.intensity.Override(.22f);
            bloom.scatter.Override(.55f);
            bloom.dirtIntensity.Override(.12f);
            Texture dirt = Resources.Load<Texture2D>("Rendering/lens_dirt") ?? TextureFactory.LensDirt();
            if (dirt != null) bloom.dirtTexture.Override(dirt);
            ColorAdjustments grade = profile.Add<ColorAdjustments>(true);
            grade.postExposure.Override(-.03f);
            grade.contrast.Override(6f);
            grade.saturation.Override(-4f);
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.color.Override(new Color(.025f,.021f,.018f,1f));
            vignette.intensity.Override(.16f);
            vignette.smoothness.Override(.48f);
            vignette.rounded.Override(true);
            ChromaticAberration chromatic = profile.Add<ChromaticAberration>(true);
            chromatic.intensity.Override(.012f);
            return profile;
        }
        #endif
    }
}
