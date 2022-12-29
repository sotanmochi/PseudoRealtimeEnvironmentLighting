using UnityEngine;

namespace Prel.RepresentativeColor
{
    public class RepresentativeColorCalculatorComponent : MonoBehaviour
    {
        [SerializeField] ComputeShader _imageAnalyzerShader;
        [SerializeField] ComputeShader _imageProcessorShader;
        [SerializeField] ComputeShader _reductionShader;

        public RepresentativeColorCalculator RepresentativeColorCalculator => _representativeColorCalculator;

        private RepresentativeColorCalculator _representativeColorCalculator;

        void Awake()
        {
            _representativeColorCalculator = new RepresentativeColorCalculator(_imageAnalyzerShader, _imageProcessorShader, _reductionShader);
        }

        void Update()
        {
            _representativeColorCalculator.Update();
        }
    }
}