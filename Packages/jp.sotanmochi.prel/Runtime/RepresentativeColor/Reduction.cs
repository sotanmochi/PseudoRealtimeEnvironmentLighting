using UnityEngine;

namespace Prel.RepresentativeColor
{
    public class Reduction
    {
        public static readonly string KernelName_Reduce = "Reduce";
        public static readonly string KernelName_CopyBuffer = "CopyBuffer";
        
        protected readonly ComputeShader _computeShader;
        
        protected readonly int _kernelId_Reduce;
        protected readonly int _kernelId_CopyBuffer;
        
        protected readonly Vector2Int _threadGroupSize_Reduce;
        protected readonly Vector2Int _threadGroupSize_CopyBuffer;
        
        public Reduction(ComputeShader computeShader)
        {
            _computeShader = computeShader;

            _kernelId_Reduce = _computeShader.FindKernel(KernelName_Reduce);
            _kernelId_CopyBuffer = _computeShader.FindKernel(KernelName_CopyBuffer);

            uint threadGroupsX, threadGroupsY, threadGroupsZ;

            _computeShader.GetKernelThreadGroupSizes(_kernelId_Reduce, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_Reduce = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);

            _computeShader.GetKernelThreadGroupSizes(_kernelId_CopyBuffer, out threadGroupsX, out threadGroupsY, out threadGroupsZ);
            _threadGroupSize_CopyBuffer = new Vector2Int((int)threadGroupsX, (int)threadGroupsY);
        }

        public void Reduce
        (
            ComputeBuffer reductionInputBuffer, 
            ComputeBuffer reductionMaxIndexBuffer, 
            ComputeBuffer reductionMaxBuffer, 
            ComputeBuffer reductionSumBuffer, 
            ComputeBuffer reductionAverageBuffer
        )
        {
            _computeShader.SetBuffer(_kernelId_Reduce, "ReductionInput", reductionInputBuffer);
            _computeShader.SetBuffer(_kernelId_Reduce, "MaxIndexOutput", reductionMaxIndexBuffer);
            _computeShader.SetBuffer(_kernelId_Reduce, "MaxValueOutput", reductionMaxBuffer);
            _computeShader.SetBuffer(_kernelId_Reduce, "SumValueOutput", reductionSumBuffer);
            _computeShader.SetBuffer(_kernelId_Reduce, "AverageValueOutput", reductionAverageBuffer);
            _computeShader.Dispatch(_kernelId_Reduce, 1, 1, 1);
        }

        public void CopyBufferUint2Double(ComputeBuffer srcBuffer, ComputeBuffer dstBuffer)
        {
            _computeShader.SetBuffer(_kernelId_CopyBuffer, "CopySrcBuffer", srcBuffer);
            _computeShader.SetBuffer(_kernelId_CopyBuffer, "CopyDstBuffer", dstBuffer);
            _computeShader.Dispatch(_kernelId_CopyBuffer, 1, 1, 1);
        }
    }
}