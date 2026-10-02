using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Subsistence
{
    public static class WorldLighting2D
    {
        static readonly List<Vector2> Level0LightPositions = new List<Vector2>
        {
            new Vector2(-14.5f,7.2f),new Vector2(12.5f,7.2f),new Vector2(39f,7.25f),new Vector2(60.5f,7.2f),new Vector2(84f,7.1f)
        };
        static readonly List<Vector2> Level1LightPositions = new List<Vector2>
        {
            new Vector2(127f,7.25f),new Vector2(146.5f,7.25f),new Vector2(166f,7.25f),new Vector2(185.5f,7.25f),new Vector2(205f,7.25f),new Vector2(224f,7.25f)
        };

        public static void Build(Transform parent)
        {
            #if UNITY_2019_3_OR_NEWER
            Transform existing = parent.Find("URP 2D · Scene Light Rig");
            if (existing != null) Object.Destroy(existing.gameObject);
            Transform rig = new GameObject("URP 2D · Scene Light Rig").transform;
            rig.SetParent(parent,false);

            foreach(Vector2 position in Level0LightPositions) AddFluorescentLight(rig,position,new Color(.94f,.86f,.56f),.72f,10f,false);
            foreach(Vector2 position in Level1LightPositions) AddFluorescentLight(rig,position,new Color(.90f,.80f,.49f),.82f,9.5f,true);
            AddFluorescentLight(rig,new Vector2(39f,7.25f),new Color(1f,.72f,.38f),.9f,7.5f,false);
            AddShadowCasters(parent);
            #endif
        }

        #if UNITY_2019_3_OR_NEWER
        static void AddFluorescentLight(Transform parent, Vector2 position, Color color, float intensity, float radius, bool industrial)
        {
            GameObject lightObject = new GameObject("Fluorescent light "+position.ToString("F1"));
            lightObject.transform.SetParent(parent,false);
            lightObject.transform.position = position;
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightInnerRadius = radius*.14f;
            light.pointLightOuterRadius = radius;
            light.pointLightInnerAngle = 145f;
            light.pointLightOuterAngle = 178f;
            light.shadowsEnabled = industrial;
            light.shadowIntensity = industrial ? .64f : .48f;
            light.volumeIntensityEnabled = true;
            light.volumeIntensity = industrial ? .08f : .05f;
            lightObject.AddComponent<LightFlicker2D>().Initialize(intensity,industrial);
        }

        static void AddShadowCasters(Transform parent)
        {
            BoxCollider2D[] colliders = parent.GetComponentsInChildren<BoxCollider2D>(true);
            for(int i=0;i<colliders.Length;i++)
            {
                BoxCollider2D collider = colliders[i];
                if(!ShouldCastShadow(collider)) continue;
                if(collider.GetComponent<ShadowCaster2D>() != null) continue;
                ShadowCaster2D caster = collider.gameObject.AddComponent<ShadowCaster2D>();
                caster.selfShadows = false;
                caster.castsShadows = true;
                caster.useRendererSilhouette = true;
            }
        }

        static bool ShouldCastShadow(BoxCollider2D collider)
        {
            if (collider == null || collider.isTrigger) return false;
            string name = collider.gameObject.name;
            return name.Contains("floor") || name.Contains("boundary") || name.Contains("column") || name.Contains("crate") || name.Contains("carton") || name.Contains("bench") || name.Contains("cabinet") || name.Contains("elevator");
        }
        #endif
    }
}
