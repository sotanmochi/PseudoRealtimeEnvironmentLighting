// Licensed under the MIT License. Copyright (c) 2020 Soichiro Sugimoto.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UniRx;

namespace RepresentativeColorEstimation
{
    public class HighIntensityRepresentativeColorEstimator : MonoBehaviour
    {
        [SerializeField] ComputeShader ComputeShader;

        public IReadOnlyReactiveProperty<Color> AverageColor => _AverageColor;
        public RenderTexture GrayscaleTexture => _GrayscaleTexture;
        public RenderTexture VisualizationTex => _VisualizationTex;
        public RenderTexture CutLowDstTexture => _CutLowDstTexture;

        private bool _Initialized = false;
        private Texture _SrcTexture;
        private ReactiveProperty<Color> _AverageColor;
        private RenderTexture _GrayscaleTexture;
        private RenderTexture _VisualizationTex;
        private RenderTexture _CutLowDstTexture;

        private Vector2Int _GpuThreads = new Vector2Int(16, 16);

        private uint[] _IntensityHistogram;
        private ComputeBuffer _HistogramBuffer;

        private double[] _Separation;
        private ComputeBuffer _SeparationBuffer;

        private double[] _ReductionInput;
        private ComputeBuffer _ReductionInputBuffer;

        private double[] _HistogramMax;
        private double[] _HistogramMaxIndex;
        private double[] _HistogramSum;
        private double[] _HistogramAverage;
        private ComputeBuffer _HistogramMaxBuffer;
        private ComputeBuffer _HistogramMaxIndexBuffer;
        private ComputeBuffer _HistogramSumBuffer;
        private ComputeBuffer _HistogramAverageBuffer;

        private double[] _SeparationMax;
        private ComputeBuffer _SeparationMaxBuffer;

        private double[] _SeparationMaxIndex;
        private double[] _SeparationSum;
        private double[] _SeparationAverage;
        private ComputeBuffer _SeparationMaxIndexBuffer;
        private ComputeBuffer _SeparationSumBuffer;
        private ComputeBuffer _SeparationAverageBuffer;

        private Queue<AsyncGPUReadbackRequest> _GPUReadbackRequests = new Queue<AsyncGPUReadbackRequest>();

        void Update()
        {
            if (!_Initialized)
            {
                Debug.LogError("Color estimator has not initialized!!");
                return;
            }

            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("Compute Shader is not Support!!");
                return;
            }
            if (ComputeShader == null)
            {
                Debug.LogError("Compute Shader has not been assigned!!");
                return;
            }

            Grayscale();
            CalculateHistogram();
            CalculateSeparation();
            CutLowIntensity(_SeparationMaxIndexBuffer);

            VisualizeHistogram(_SeparationMaxIndexBuffer);

            if (_GPUReadbackRequests.Count < 1)
            {
                _GPUReadbackRequests.Enqueue(
                    AsyncGPUReadback.Request(_CutLowDstTexture, 0, request => 
                    {
                        if (request.hasError)
                        {
                            Debug.Log("GPU readback error detected.");
                            _GPUReadbackRequests.Dequeue();
                            return;
                        }

                        var buffer = request.GetData<Color32>().ToArray();

                        float pixelCount = 0;
                        float r = 0.0f;
                        float g = 0.0f;
                        float b = 0.0f;
                        foreach (Color32 color in buffer)
                        {
                            if (!(color.r == 0.0f && color.g == 0.0f && color.b == 0.0f))
                            {
                                pixelCount++;
                                r += color.r;
                                g += color.g;
                                b += color.b;
                            }
                        }
                        Color32 averageColor = new Color32(0, 0, 0, 1);
                        averageColor.r = (byte)(r / pixelCount);
                        averageColor.g = (byte)(g / pixelCount);
                        averageColor.b = (byte)(b / pixelCount);

                        // AverageColorImageObject.material.color = averageColor;
                        _AverageColor.Value = averageColor;

                        _GPUReadbackRequests.Dequeue();
                    })
                );
            }
        }

        public void Initialize(Texture srcTexture)
        {
            _AverageColor = new ReactiveProperty<Color>();

            _SrcTexture = srcTexture;

            int width = _SrcTexture.width;
            int height = _SrcTexture.height;

            _GrayscaleTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            _GrayscaleTexture.enableRandomWrite = true;
            _GrayscaleTexture.Create();

            _VisualizationTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            _VisualizationTex.enableRandomWrite = true;
            _VisualizationTex.Create();

            _CutLowDstTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            _CutLowDstTexture.enableRandomWrite = true;
            _CutLowDstTexture.Create();

            _IntensityHistogram = new uint[256];
            _HistogramBuffer = new ComputeBuffer(256, sizeof(uint));

            _Separation = new double[256];
            _SeparationBuffer = new ComputeBuffer(256, sizeof(double));

            _ReductionInput = new double[256];
            _ReductionInputBuffer = new ComputeBuffer(256, sizeof(double));

            _HistogramMaxIndex = new double[1];
            _HistogramMax = new double[1];
            _HistogramSum = new double[1];
            _HistogramAverage = new double[1];

            _HistogramMaxIndexBuffer = new ComputeBuffer(1, sizeof(double));
            _HistogramMaxBuffer = new ComputeBuffer(1, sizeof(double));
            _HistogramSumBuffer = new ComputeBuffer(1, sizeof(double));
            _HistogramAverageBuffer = new ComputeBuffer(1, sizeof(double));

            _SeparationMaxIndex = new double[1];
            _SeparationMaxIndexBuffer = new ComputeBuffer(1, sizeof(double));

            _SeparationMax = new double[1];
            _SeparationMaxBuffer = new ComputeBuffer(1, sizeof(double));
            _SeparationSum = new double[1];
            _SeparationSumBuffer = new ComputeBuffer(1, sizeof(double));
            _SeparationAverage = new double[1];
            _SeparationAverageBuffer = new ComputeBuffer(1, sizeof(double));

            _Initialized = true;
        }

        void Grayscale()
        {
            string kernelName = "Grayscale";
            int kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetInt("Width", _SrcTexture.width);
            ComputeShader.SetInt("Height", _SrcTexture.height);
            ComputeShader.SetTexture(kernelID, "SrcTex", _SrcTexture);
            ComputeShader.SetTexture(kernelID, "DstTex", _GrayscaleTexture);
            int threadGroupsX = Mathf.CeilToInt((float)_SrcTexture.width / _GpuThreads.x);
            int threadGroupsY = Mathf.CeilToInt((float)_SrcTexture.height / _GpuThreads.y);
            ComputeShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1);
        }

        void CalculateHistogram()
        {
            string kernelName = "InitializeHistogram";
            int kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "HistogramBuffer", _HistogramBuffer);
            ComputeShader.Dispatch(kernelID, 1, 1, 1);

            kernelName = "Histogram";
            kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetInt("Width", _SrcTexture.width);
            ComputeShader.SetInt("Height", _SrcTexture.height);
            ComputeShader.SetTexture(kernelID, "SrcTex", _SrcTexture);
            ComputeShader.SetBuffer(kernelID, "HistogramBuffer", _HistogramBuffer);
            int threadGroupsX = Mathf.CeilToInt((float)_SrcTexture.width / _GpuThreads.x);
            int threadGroupsY = Mathf.CeilToInt((float)_SrcTexture.height / _GpuThreads.y);
            ComputeShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1);

            kernelName = "CopyHistogramBuffer";
            kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "HistogramBuffer", _HistogramBuffer);
            ComputeShader.SetBuffer(kernelID, "ReductionInputBuffer", _ReductionInputBuffer);
            ComputeShader.Dispatch(kernelID, 1, 1, 1);

            kernelName = "Reduction";
            kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "ReductionInputBuffer", _ReductionInputBuffer);
            ComputeShader.SetBuffer(kernelID, "MaxIndexOutput", _HistogramMaxIndexBuffer);
            ComputeShader.SetBuffer(kernelID, "MaxValueOutput", _HistogramMaxBuffer);
            ComputeShader.SetBuffer(kernelID, "SumValueOutput", _HistogramSumBuffer);
            ComputeShader.SetBuffer(kernelID, "AverageValueOutput", _HistogramAverageBuffer);
            ComputeShader.Dispatch(kernelID, 1, 1, 1);
        }

        void VisualizeHistogram(ComputeBuffer thresholdBuffer)
        {
            string kernelName = "VisualizeHistogram";
            int kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "HistogramBuffer", _HistogramBuffer);
            ComputeShader.SetBuffer(kernelID, "HistogramMaxInputBuffer", _HistogramMaxBuffer);
            ComputeShader.SetBuffer(kernelID, "ThresholdBuffer", thresholdBuffer);
            ComputeShader.SetInt("Width", _VisualizationTex.width);
            ComputeShader.SetInt("Height", _VisualizationTex.height);
            ComputeShader.SetTexture(kernelID, "DstTex", _VisualizationTex);
            int threadGroupsX = Mathf.CeilToInt((float)_VisualizationTex.width / _GpuThreads.x);
            int threadGroupsY = Mathf.CeilToInt((float)_VisualizationTex.height / _GpuThreads.y);
            ComputeShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1);
        }

        void CutLowIntensity(ComputeBuffer thresholdBuffer)
        {
            string kernelName = "CutLowIntensity";
            int kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "CutLowIntensityBuffer", thresholdBuffer);
            ComputeShader.SetInt("Width", _SrcTexture.width);
            ComputeShader.SetInt("Height", _SrcTexture.height);
            ComputeShader.SetTexture(kernelID, "SrcTex", _SrcTexture);
            ComputeShader.SetTexture(kernelID, "DstTex", _CutLowDstTexture);
            int threadGroupsX = Mathf.CeilToInt((float)_SrcTexture.width / _GpuThreads.x);
            int threadGroupsY = Mathf.CeilToInt((float)_SrcTexture.height / _GpuThreads.y);
            ComputeShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, 1);
        }

        ////////////////////////////////////////////////
        // Otsu's method to find the optimal threshold
        ////////////////////////////////////////////////
        void CalculateSeparation()
        {
            string kernelName = "CalculateSeparation";
            int kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "HistogramBuffer", _HistogramBuffer);
            ComputeShader.SetBuffer(kernelID, "HistogramAverageBuffer", _HistogramAverageBuffer);
            ComputeShader.SetBuffer(kernelID, "SeparationBuffer", _SeparationBuffer);
            ComputeShader.Dispatch(kernelID, 1, 1, 1);

            kernelName = "Reduction";
            kernelID = ComputeShader.FindKernel(kernelName);
            ComputeShader.SetBuffer(kernelID, "ReductionInputBuffer", _SeparationBuffer);
            ComputeShader.SetBuffer(kernelID, "MaxValueOutput", _SeparationMaxBuffer);
            ComputeShader.SetBuffer(kernelID, "MaxIndexOutput", _SeparationMaxIndexBuffer);
            ComputeShader.SetBuffer(kernelID, "SumValueOutput", _SeparationSumBuffer); // Dummy
            ComputeShader.SetBuffer(kernelID, "AverageValueOutput", _SeparationAverageBuffer); // Dummy
            ComputeShader.Dispatch(kernelID, 1, 1, 1);
        }
    }
}
