using UnityEngine;
using UniRx;

namespace PseudoRealtimeEnvironmentLighting.Demo
{
    public class LightingPresenter : MonoBehaviour
    {
        [SerializeField] LightingView _LightingView;
        [SerializeField] LightingManager _LightingManager;

        void Awake()
        {
            _LightingView.EnableEnvironmentLight
            .Subscribe(value => 
            {
                _LightingManager.SetEnableEnvironmentLight(value);
            })
            .AddTo(this);

            _LightingView.EnableDirectionalLight
            .Subscribe(value => 
            {
                _LightingManager.SetEnableDirectionalLight(value);
            })
            .AddTo(this);

            _LightingView.EnvironmentLightMaxIntensity
            .Subscribe(value => 
            {
                _LightingManager.SetEnvironmentLightMaxIntensity(value);
            })
            .AddTo(this);

            _LightingView.Metallic
            .Subscribe(value => 
            {
                _LightingManager.SetEnvironmentLightMetallic(value);
            })
            .AddTo(this);

            _LightingView.Smoothness
            .Subscribe(value => 
            {
                _LightingManager.SetEnvironmentLightSmoothness(value);
            })
            .AddTo(this);

            _LightingView.DirectionalLightIntensity
            .Subscribe(value => 
            {
                _LightingManager.SetDirectionalLightIntensity(value);
            })
            .AddTo(this);

            _LightingView.EmissiveFloorIntensity
            .Subscribe(value => 
            {
                _LightingManager.SetEmissiveFloorIntensity(value);
            })
            .AddTo(this);

            _LightingView.EmissiveQuadIntensity
            .Subscribe(value => 
            {
                _LightingManager.SetEmissiveQuadIntensity(value);
            })
            .AddTo(this);
        }
    }
}
