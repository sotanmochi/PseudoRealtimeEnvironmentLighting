using UnityEngine;
using UniRx;

namespace PseudoRealtimeEnvironmentLighting.Experimental
{
    public class RealtimeImageProcessing : MonoBehaviour
    {
        [SerializeField] HighIntensityRepresentativeColorEstimator _ColorEstimator;
        [SerializeField] Renderer InputImageObject;
        [SerializeField] Renderer GrayscaleImageObject;
        [SerializeField] Renderer VisualizeImageObject;
        [SerializeField] Renderer CutLowOutputImageObject;
        [SerializeField] Renderer AverageColorImageObject;

        void Start()
        {
            Texture srcTexture = InputImageObject.material.mainTexture;
            _ColorEstimator.Initialize(srcTexture);

            GrayscaleImageObject.material.mainTexture = _ColorEstimator.GrayscaleTexture;
            VisualizeImageObject.material.mainTexture = _ColorEstimator.VisualizationTex;
            CutLowOutputImageObject.material.mainTexture = _ColorEstimator.CutLowDstTexture;

            _ColorEstimator.AverageColor
            .Subscribe(color => 
            {
                AverageColorImageObject.material.color = color;
            });
        }
    }
}
