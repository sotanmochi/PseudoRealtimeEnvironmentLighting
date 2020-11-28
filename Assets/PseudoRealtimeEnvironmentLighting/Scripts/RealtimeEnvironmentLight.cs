using UnityEngine;

namespace PseudoRealtimeEnvironmentLighting
{
    public class RealtimeEnvironmentLight : MonoBehaviour
    {
        public Camera CurrentActiveCamera;
        public Transform EnvironmentLightingTarget;

        public bool EnableEnvironmetLighting = true;
        public float IntensityLevel = 1.0f;
        public float MaxIntensity = 0.4f;

        public Color AlbedoColor = Color.white;
        [Range(0.0f, 1.0f)] public float Metallic = 0.0f;
        [Range(0.0f, 1.0f)] public float Smoothness =  0.6f;

        [SerializeField] private Transform _MatCapRenderingCameraParent;
        [SerializeField] private Material _MatCapRenderingSphereMaterial;
        [SerializeField] private ReflectionProbe _ReflectionProbe;

        void Update()
        {
            this.transform.position = EnvironmentLightingTarget.position;
            _MatCapRenderingCameraParent.rotation = CurrentActiveCamera.transform.rotation;
            UpdateIntensity();
            UpdateMaterial();
        }

        void UpdateIntensity()
        {
            if (!EnableEnvironmetLighting)
            {
                _ReflectionProbe.intensity = 0.0f;
            }
            else
            {
                _ReflectionProbe.intensity = IntensityLevel * MaxIntensity;
            }
        }

        void UpdateMaterial()
        {
            _MatCapRenderingSphereMaterial.SetColor("_Color", AlbedoColor);
            _MatCapRenderingSphereMaterial.SetFloat("_Metallic", Metallic);
            _MatCapRenderingSphereMaterial.SetFloat("_Glossiness", Smoothness);
        }

        void OnValidate()
        {
            UpdateIntensity();
            UpdateMaterial();
        }
    }
}
