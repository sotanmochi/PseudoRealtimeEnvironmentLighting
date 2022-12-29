// Licensed under the MIT License. Copyright (c) 2020 Soichiro Sugimoto.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Prel.RepresentativeColor
{
    public class RepresentativeColorCalculator
    {
        public event Action<Color> OnUpdate;

        public RenderTexture ResizedSrcTexture => _resizedSrcTexture;
        public RenderTexture GrayscaleTexture => _grayscaleTexture;
        public RenderTexture VisualizationTex => _visualizationTex;
        public RenderTexture CutLowDstTexture => _cutLowDstTexture;

        private readonly ImageAnalyzer _imageAnalyzer;
        private readonly ImageProcessor _imageProcessor;
        private readonly Reduction _reduction;

        private bool _initialized;

        private Texture _srcTexture;
        private RenderTexture _resizedSrcTexture;
        private RenderTexture _grayscaleTexture;
        private RenderTexture _visualizationTex;
        private RenderTexture _cutLowDstTexture;

        private uint[] _intensityHistogram;
        private ComputeBuffer _histogramBuffer;

        private double[] _separation;
        private ComputeBuffer _separationBuffer;

        private double[] _reductionInput;
        private ComputeBuffer _reductionInputBuffer;

        private double[] _histogramMax;
        private double[] _histogramMaxIndex;
        private double[] _histogramSum;
        private double[] _histogramAverage;
        private ComputeBuffer _histogramMaxBuffer;
        private ComputeBuffer _histogramMaxIndexBuffer;
        private ComputeBuffer _histogramSumBuffer;
        private ComputeBuffer _histogramAverageBuffer;

        private double[] _separationMax;
        private ComputeBuffer _separationMaxBuffer;

        private double[] _separationMaxIndex;
        private double[] _separationSum;
        private double[] _separationAverage;
        private ComputeBuffer _separationMaxIndexBuffer;
        private ComputeBuffer _separationSumBuffer;
        private ComputeBuffer _separationAverageBuffer;

        private Queue<AsyncGPUReadbackRequest> _gpuReadbackRequests = new Queue<AsyncGPUReadbackRequest>();

        public RepresentativeColorCalculator
        (
            ComputeShader imageAnalyzerShader,
            ComputeShader imageProcessorShader,
            ComputeShader reductionShader
        )
        {
            _imageAnalyzer = new ImageAnalyzer(imageAnalyzerShader);
            _imageProcessor = new ImageProcessor(imageProcessorShader);
            _reduction = new Reduction(reductionShader);
        }

        public void Initialize(Texture srcTex, uint downSampleLevel = 2)
        {
            var divisor = (int)Math.Pow(2, downSampleLevel);

            var width = srcTex.width / divisor;
            var height = srcTex.height / divisor;

            _srcTexture = srcTex;

            _resizedSrcTexture = _imageProcessor.CreateRenderTexture(width, height);
            _grayscaleTexture = _imageProcessor.CreateRenderTexture(width, height);
            _visualizationTex = _imageProcessor.CreateRenderTexture(width, height);
            _cutLowDstTexture = _imageProcessor.CreateRenderTexture(width, height);

            _intensityHistogram = new uint[256];
            _histogramBuffer = new ComputeBuffer(256, sizeof(uint));

            _separation = new double[256];
            _separationBuffer = new ComputeBuffer(256, sizeof(double));

            _reductionInput = new double[256];
            _reductionInputBuffer = new ComputeBuffer(256, sizeof(double));

            _histogramMaxIndex = new double[1];
            _histogramMax = new double[1];
            _histogramSum = new double[1];
            _histogramAverage = new double[1];

            _histogramMaxIndexBuffer = new ComputeBuffer(1, sizeof(double));
            _histogramMaxBuffer = new ComputeBuffer(1, sizeof(double));
            _histogramSumBuffer = new ComputeBuffer(1, sizeof(double));
            _histogramAverageBuffer = new ComputeBuffer(1, sizeof(double));

            _separationMaxIndex = new double[1];
            _separationMaxIndexBuffer = new ComputeBuffer(1, sizeof(double));

            _separationMax = new double[1];
            _separationMaxBuffer = new ComputeBuffer(1, sizeof(double));
            _separationSum = new double[1];
            _separationSumBuffer = new ComputeBuffer(1, sizeof(double));
            _separationAverage = new double[1];
            _separationAverageBuffer = new ComputeBuffer(1, sizeof(double));

            _initialized = true;
        }

        public void Update()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("Compute Shader is not Support!!");
                return;
            }

            if (!_initialized)
            {
                Debug.LogError("Color estimator has not initialized!!");
                return;
            }

            _imageProcessor.ResizeBilinear(_srcTexture, _resizedSrcTexture);
            _imageProcessor.RGBToGrayscale(_resizedSrcTexture, _grayscaleTexture);

            _imageAnalyzer.ClearHistogram(_histogramBuffer);
            _imageAnalyzer.CalculateIntensityHistogram(_resizedSrcTexture, _histogramBuffer);
            _reduction.CopyBufferUint2Double(_histogramBuffer, _reductionInputBuffer);
            _reduction.Reduce(_reductionInputBuffer, _histogramMaxIndexBuffer, _histogramMaxBuffer, _histogramSumBuffer, _histogramAverageBuffer);

            _imageAnalyzer.CalculateSeparation(_histogramBuffer, _histogramAverageBuffer, _separationBuffer);
            _reduction.Reduce(_separationBuffer, _separationMaxIndexBuffer, _separationMaxBuffer, _separationSumBuffer, _separationAverageBuffer);

            _imageProcessor.CutLowIntensity(_resizedSrcTexture, _cutLowDstTexture, _separationMaxIndexBuffer);
            _imageAnalyzer.VisualizeHistogram(_histogramBuffer, _histogramMaxBuffer, _separationMaxIndexBuffer, _visualizationTex);            

            if (_gpuReadbackRequests.Count < 1)
            {
                _gpuReadbackRequests.Enqueue(
                    AsyncGPUReadback.Request(_cutLowDstTexture, 0, request => 
                    {
                        if (request.hasError)
                        {
                            Debug.Log("GPU readback error detected.");
                            _gpuReadbackRequests.Dequeue();
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
                    
                        OnUpdate?.Invoke(averageColor);
                    
                        _gpuReadbackRequests.Dequeue();
                    })
                );
            }
        }
    }
}