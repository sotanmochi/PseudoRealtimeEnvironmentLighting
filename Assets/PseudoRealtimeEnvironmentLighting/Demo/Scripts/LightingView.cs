﻿using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace PseudoRealtimeEnvironmentLighting.Demo
{
    public class LightingView : MonoBehaviour
    {
        [SerializeField] Toggle _Toggle_EnableEnvironmentLight;
        [SerializeField] Slider _Slider_EnvironmentLightMaxIntensity;
        [SerializeField] Slider _Slider_Metallic;
        [SerializeField] Slider _Slider_Smoothness;
        [SerializeField] Toggle _Toggle_EnableDirectionalLight;
        [SerializeField] Slider _Slider_DirectionalLightIntensity;

        [SerializeField] Text _Text_EnvironmentLightMaxIntensity;
        [SerializeField] Text _Text_Metallic;
        [SerializeField] Text _Text_Smoothness;
        [SerializeField] Text _Text_DirectionalLightIntensity;

        public IReadOnlyReactiveProperty<bool> EnableEnvironmentLight => _EnableEnvironmentLight;
        public IReadOnlyReactiveProperty<float> EnvironmentLightMaxIntensity => _EnvironmentLightMaxIntensity;
        public IReadOnlyReactiveProperty<float> Metallic => _Metallic;
        public IReadOnlyReactiveProperty<float> Smoothness => _Smoothness;
        public IReadOnlyReactiveProperty<bool> EnableDirectionalLight => _EnableDirectionalLight;
        public IReadOnlyReactiveProperty<float> DirectionalLightIntensity => _DirectionalLightIntensity;

        private ReactiveProperty<bool> _EnableEnvironmentLight = new ReactiveProperty<bool>();
        private ReactiveProperty<float> _EnvironmentLightMaxIntensity = new ReactiveProperty<float>();
        private ReactiveProperty<float> _Metallic = new ReactiveProperty<float>();
        private ReactiveProperty<float> _Smoothness = new ReactiveProperty<float>();
        private ReactiveProperty<bool> _EnableDirectionalLight = new ReactiveProperty<bool>();
        private ReactiveProperty<float> _DirectionalLightIntensity = new ReactiveProperty<float>();

        void Awake()
        {
            _Toggle_EnableEnvironmentLight.OnValueChangedAsObservable()
            .Subscribe(value => 
            {
                _EnableEnvironmentLight.Value = value;
            })
            .AddTo(this);

            _Toggle_EnableDirectionalLight.OnValueChangedAsObservable()
            .Subscribe(value => 
            {
                _EnableDirectionalLight.Value = value;
            })
            .AddTo(this);

            _Slider_EnvironmentLightMaxIntensity.OnValueChangedAsObservable()
            .Subscribe(value => 
            {
                _EnvironmentLightMaxIntensity.Value = value;
                _Text_EnvironmentLightMaxIntensity.text = "MaxIntensity : " + value.ToString("F2");
            })
            .AddTo(this);

            _Slider_Metallic.OnValueChangedAsObservable()
            .Subscribe(value => 
            {
                _Metallic.Value = value;
                _Text_Metallic.text = "Metallic : " + value.ToString("F2");
            })
            .AddTo(this);

            _Slider_Smoothness.OnValueChangedAsObservable()
            .Subscribe(value => 
            {
                _Smoothness.Value = value;
                _Text_Smoothness.text = "Smoothness : " + value.ToString("F2");
            })
            .AddTo(this);

            _Slider_DirectionalLightIntensity.OnValueChangedAsObservable()
            .Subscribe(value => 
            {
                _DirectionalLightIntensity.Value = value;
                _Text_DirectionalLightIntensity.text = "Intensity : " + value.ToString("F2");
            })
            .AddTo(this);
        }

        public void SetEnableEnvironmentLight(bool value)
        {
            _Toggle_EnableEnvironmentLight.isOn = value;
        }

        public void SetEnableDirectionalLight(bool value)
        {
            _Toggle_EnableDirectionalLight.isOn = value;
        }

        public void SetEnvironmentLightMaxIntensity(float value)
        {
            _Slider_EnvironmentLightMaxIntensity.value = value;
        }

        public void SetMetallic(float value)
        {
            _Slider_Metallic.value = value;
        }

        public void SetSmoothness(float value)
        {
            _Slider_Smoothness.value = value;
        }

        public void SetDirectionalLightIntensity(float value)
        {
            _Slider_DirectionalLightIntensity.value = value;
        }
    }
}
