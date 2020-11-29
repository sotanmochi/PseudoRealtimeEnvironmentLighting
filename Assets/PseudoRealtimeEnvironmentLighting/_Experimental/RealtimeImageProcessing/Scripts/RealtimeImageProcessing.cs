﻿using UnityEngine;

namespace PseudoRealtimeEnvironmentLighting.Experimental
{
    public class RealtimeImageProcessing : MonoBehaviour
    {
        [SerializeField] ComputeShader ComputeShader;
        [SerializeField] Renderer InputImageObject;
        [SerializeField] Renderer GrayscaleImageObject;
        [SerializeField] Renderer VisualizeImageObject;
        [SerializeField] Renderer CutLowOutputImageObject;

        private Texture _SrcTexture;
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

        void Start()
        {
            _SrcTexture = InputImageObject.material.mainTexture;

            Initialize();

            GrayscaleImageObject.material.mainTexture = _GrayscaleTexture;
            VisualizeImageObject.material.mainTexture = _VisualizationTex;
            CutLowOutputImageObject.material.mainTexture = _CutLowDstTexture;
        }

        void Update()
        {
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
        }

        void Initialize()
        {
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
