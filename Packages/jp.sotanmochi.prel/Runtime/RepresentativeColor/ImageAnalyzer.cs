using UnityEngine;

namespace Prel.RepresentativeColor
{
    public class ImageAnalyzer
    {
        public static readonly string KernelName_ClearHistogram = "ClearHistogram";
        public static readonly string KernelName_CalculateIntensityHistogram = "CalculateIntensityHistogram";
        public static readonly string KernelName_VisualizeHistogram = "VisualizeHistogram";
        
        protected readonly ComputeShader _computeShader;
        
        protected readonly int _kernelId_ClearHistogram;
        protected readonly int _kernelId_CalculateIntensityHistogram;
        protected readonly int _kernelId_VisualizeHistogram;
        
        protected readonly Vector2Int _threadGroupSize_ClearHistogram;
        protected readonly Vector2Int _threadGroupSize_CalculateIntensityHistogram;
        protected readonly Vector2Int _threadGroupSize_VisualizeHistogram;
        
        public ImageAnalyzer(ComputeShader computeShader)
        {
            _computeShader = computeShader;
            
            _kernelId_ClearHistogram = _computeShader.FindKernel(KernelName_ClearHistogram);
            _kernelId_CalculateIntensityHistogram = _computeShader.FindKernel(KernelName_CalculateIntensityHistogram);
            _kernelId_VisualizeHistogram = _computeShader.FindKernel(KernelName_VisualizeHistogram);
            
            uint threadGroupsX, threadGroupsY, threadGroupsZ;
            
            _computeShader.GetKernelThreadGroupSizes(_kernelId_ClearHistogram, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_ClearHistogram = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);
            
            _computeShader.GetKernelThreadGroupSizes(_kernelId_CalculateIntensityHistogram, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_CalculateIntensityHistogram = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);
            
            _computeShader.GetKernelThreadGroupSizes(_kernelId_VisualizeHistogram, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_VisualizeHistogram = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);
        }

        public void ClearHistogram(ComputeBuffer histogramBuffer)
        {
            _computeShader.SetBuffer(_kernelId_ClearHistogram, "Histogram", histogramBuffer);
            
            var threadGroupsX = Mathf.CeilToInt((float)histogramBuffer.count / _threadGroupSize_ClearHistogram.x);
            _computeShader.Dispatch(_kernelId_ClearHistogram, threadGroupsX, 1, 1);
        }

        public void CalculateIntensityHistogram(Texture srcTex, ComputeBuffer histogramBuffer)
        {
            _computeShader.SetInt("SrcWidth", srcTex.width);
            _computeShader.SetInt("SrcHeight", srcTex.height);
            _computeShader.SetTexture(_kernelId_CalculateIntensityHistogram, "SrcTex", srcTex);
            _computeShader.SetBuffer(_kernelId_CalculateIntensityHistogram, "Histogram", histogramBuffer);
            
            var threadGroupsX = Mathf.CeilToInt((float)srcTex.width / _threadGroupSize_CalculateIntensityHistogram.x);
            var threadGroupsY = Mathf.CeilToInt((float)srcTex.height / _threadGroupSize_CalculateIntensityHistogram.y);
            _computeShader.Dispatch(_kernelId_CalculateIntensityHistogram, threadGroupsX, threadGroupsY, 1);
        }

        public void VisualizeHistogram(ComputeBuffer histogramBuffer, ComputeBuffer maxFrequencyBuffer, ComputeBuffer thresholdBuffer, Texture dstTex)
        {
            _computeShader.SetBuffer(_kernelId_VisualizeHistogram, "Histogram", histogramBuffer);
            _computeShader.SetBuffer(_kernelId_VisualizeHistogram, "MaxFrequency", maxFrequencyBuffer);
            _computeShader.SetBuffer(_kernelId_VisualizeHistogram, "Threshold", thresholdBuffer);
            _computeShader.SetInt("DstWidth", dstTex.width);
            _computeShader.SetInt("DstHeight", dstTex.height);
            _computeShader.SetTexture(_kernelId_VisualizeHistogram, "DstTex", dstTex);
            
            var threadGroupsX = Mathf.CeilToInt((float)dstTex.width / _threadGroupSize_VisualizeHistogram.x);
            var threadGroupsY = Mathf.CeilToInt((float)dstTex.height / _threadGroupSize_VisualizeHistogram.y);
            _computeShader.Dispatch(_kernelId_VisualizeHistogram, threadGroupsX, threadGroupsY, 1);
        }
    }
}