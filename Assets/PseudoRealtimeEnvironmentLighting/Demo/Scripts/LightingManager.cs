using UnityEngine;
using UniRx;

namespace PseudoRealtimeEnvironmentLighting.Demo
{
    public class LightingManager : MonoBehaviour
    {
        [SerializeField] Light _DirectionalLight;
        [SerializeField] RealtimeEnvironmentLight _RealtimeEnvironmentLight;
        [SerializeField] Material _EmissiveFloorMaterial;
        [SerializeField] Material _EmissiveQuadMaterial;

        private Color _EmissionBaseColor = new Color(1f, 1f, 1f);

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

        public void SetEmissiveFloorIntensity(float value)
        {
            Color color = value * _EmissionBaseColor;
            _EmissiveFloorMaterial.SetColor("_EmissionColor", color);
        }

        public void SetEmissiveQuadIntensity(float value)
        {
            Color color = value * _EmissionBaseColor;
            _EmissiveQuadMaterial.SetColor("_EmissionColor", color);
        }
    }
}
