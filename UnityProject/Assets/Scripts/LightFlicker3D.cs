using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Light))]
    public sealed class LightFlicker3D : MonoBehaviour
    {
        Light source;float phase;float baseIntensity;
        void Awake(){source=GetComponent<Light>();phase=Random.value*6.28f;baseIntensity=source.intensity;}
        void Update()
        {
            float pulse=.92f+Mathf.Sin(Time.time*1.7f+phase)*.065f;
            if(Random.value>.998f)pulse*=Random.Range(.25f,.7f);
            source.intensity=baseIntensity*pulse;
        }
    }
}
