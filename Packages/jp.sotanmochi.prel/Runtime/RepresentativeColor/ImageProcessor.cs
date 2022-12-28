using UnityEngine;

namespace Prel.RepresentativeColor
{
    public class ImageProcessor
    {
        public static readonly string KernelName_ResizeBilinear = "ResizeBilinear";
        public static readonly string KernelName_RGBToGrayscale = "RGBToGrayscale";
        
        protected readonly ComputeShader _computeShader;
        
        protected readonly int _kernelId_ResizeBilinear;
        protected readonly int _kernelId_RGBToGrayscale;
        
        protected readonly Vector2Int _threadGroupSize_ResizeBilinear;
        protected readonly Vector2Int _threadGroupSize_RGBToGrayscale;
        
        public ImageProcessor(ComputeShader computeShader)
        {
            _computeShader = computeShader;
            
            _kernelId_ResizeBilinear = _computeShader.FindKernel(KernelName_ResizeBilinear);
            _kernelId_RGBToGrayscale = _computeShader.FindKernel(KernelName_RGBToGrayscale);
            
            uint threadGroupsX, threadGroupsY, threadGroupsZ;
            
            _computeShader.GetKernelThreadGroupSizes(_kernelId_ResizeBilinear, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_ResizeBilinear = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);
            
            _computeShader.GetKernelThreadGroupSizes(_kernelId_RGBToGrayscale, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_RGBToGrayscale = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);
        }
        
        public RenderTexture CreateRenderTexture(int width, int height)
        {
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }
        
        public void ResizeBilinear(Texture srcTex, RenderTexture dstTex)
        {
            _computeShader.SetInt("SrcWidth", srcTex.width);
            _computeShader.SetInt("SrcHeight", srcTex.height);
            _computeShader.SetInt("DstWidth", dstTex.width);
            _computeShader.SetInt("DstHeight", dstTex.height);
            _computeShader.SetTexture(_kernelId_ResizeBilinear, "SrcTex", srcTex);
            _computeShader.SetTexture(_kernelId_ResizeBilinear, "DstTex", dstTex);
            
            var threadGroupsX = Mathf.CeilToInt((float)srcTex.width / _threadGroupSize_ResizeBilinear.x);
            var threadGroupsY = Mathf.CeilToInt((float)srcTex.height / _threadGroupSize_ResizeBilinear.y);
            _computeShader.Dispatch(_kernelId_ResizeBilinear, threadGroupsX, threadGroupsY, 1);
        }
        
        public void RGBToGrayscale(Texture srcTex, RenderTexture dstTex)
        {
            _computeShader.SetInt("SrcWidth", srcTex.width);
            _computeShader.SetInt("SrcHeight", srcTex.height);
            _computeShader.SetInt("DstWidth", dstTex.width);
            _computeShader.SetInt("DstHeight", dstTex.height);
            _computeShader.SetTexture(_kernelId_RGBToGrayscale, "SrcTex", srcTex);
            _computeShader.SetTexture(_kernelId_RGBToGrayscale, "DstTex", dstTex);
            
            var threadGroupsX = Mathf.CeilToInt((float)srcTex.width / _threadGroupSize_RGBToGrayscale.x);
            var threadGroupsY = Mathf.CeilToInt((float)srcTex.height / _threadGroupSize_RGBToGrayscale.y);
            _computeShader.Dispatch(_kernelId_RGBToGrayscale, threadGroupsX, threadGroupsY, 1);
        }
    }
}