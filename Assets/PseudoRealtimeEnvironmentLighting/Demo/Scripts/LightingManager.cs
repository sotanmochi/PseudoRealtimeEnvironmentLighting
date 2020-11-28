using UnityEngine;
using UniRx;

namespace PseudoRealtimeEnvironmentLighting.Demo
{
    public class LightingManager : MonoBehaviour
    {
        [SerializeField] Light _DirectionalLight;
        [SerializeField] RealtimeEnvironmentLight _RealtimeEnvironmentLight;

        public void SetEnableDirectionalLight(bool value)
        {
            _DirectionalLight.gameObject.SetActive(value);
            _DirectionalLight.enabled = value;
        }

        public void SetDirectionalLightIntensity(float intensity)
        {
            _DirectionalLight.intensity = intensity;
        }

        public void SetEnableEnvironmentLight(bool value)
        {
            _RealtimeEnvironmentLight.EnableEnvironmetLighting = value;
        }

        public void SetEnvironmentLightMaxIntensity(float maxIntensity)
        {
            _RealtimeEnvironmentLight.MaxIntensity = maxIntensity;
        }

        public void SetEnvironmentLightMetallic(float value)
        {
            _RealtimeEnvironmentLight.Metallic = value;
        }

        public void SetEnvironmentLightSmoothness(float value)
        {
            _RealtimeEnvironmentLight.Smoothness = value;
        }
    }
}
