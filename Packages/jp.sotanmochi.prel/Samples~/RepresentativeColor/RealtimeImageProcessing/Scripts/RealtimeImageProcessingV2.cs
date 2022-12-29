using UnityEngine;

namespace Prel.RepresentativeColor.Samples
{
    public class RealtimeImageProcessingV2 : MonoBehaviour
    {
        [SerializeField] RepresentativeColorCalculatorComponent _colorCalculatorComponent;
        [SerializeField] Renderer InputImageObject;
        [SerializeField] Renderer GrayscaleImageObject;
        [SerializeField] Renderer VisualizeImageObject;
        [SerializeField] Renderer CutLowOutputImageObject;
        [SerializeField] Renderer AverageColorImageObject;

        void Start()
        {
            var srcTexture = InputImageObject.material.mainTexture;
            
            var colorCalculater = _colorCalculatorComponent.RepresentativeColorCalculator;
            colorCalculater.Initialize(srcTexture, 0);

            GrayscaleImageObject.material.mainTexture = colorCalculater.GrayscaleTexture;
            VisualizeImageObject.material.mainTexture = colorCalculater.VisualizationTex;
            CutLowOutputImageObject.material.mainTexture = colorCalculater.CutLowDstTexture;

            colorCalculater.OnUpdate += OnUpdate;
        }

        void OnDestroy()
        {
            _colorCalculatorComponent.RepresentativeColorCalculator.OnUpdate -= OnUpdate;
        }

        void OnUpdate(Color color)
        {
            AverageColorImageObject.material.color = color;
        }
    }
}
