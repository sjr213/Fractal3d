using ComputeSharp;
using System;
using System.Numerics;

namespace ImageCalculator
{
    internal static class GpuMethods
    {
        public static Vector3 Transform(Vector3 vIn, Matrix4x4 m)
        {
            Vector4 vector4 = new Vector4(vIn, 1.0f);
            Vector3 vOut = new Vector3();
            vOut.X = vIn.X * m.M11 + vIn.Y * m.M21 + vIn.Z * m.M31 + m.M41;
            vOut.Y = vIn.X * m.M12 + vIn.Y * m.M22 + vIn.Z * m.M32 + m.M42;
            vOut.Z = vIn.X * m.M13 + vIn.Y * m.M23 + vIn.Z * m.M33 + m.M43;
            return vOut;
        }

        public static Float4x4 ToFloat4x4(Matrix4x4 m)
        {
            return new Float4x4(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
        }

        // Expands every parent segment by every branch in a single GPU dispatch
        public static void ExpandLevel(Float4[] parentStarts, Float4[] parentEnds, Float4x4[] branchMats, Float2[] branchParams,
            out Float4[] childStarts, out Float4[] childEnds)
        {
            var device = GraphicsDevice.GetDefault();
            int childCount = parentStarts.Length * branchMats.Length;

            using ReadOnlyBuffer<Float4> gpuParentStarts = device.AllocateReadOnlyBuffer(parentStarts);
            using ReadOnlyBuffer<Float4> gpuParentEnds = device.AllocateReadOnlyBuffer(parentEnds);
            using ReadOnlyBuffer<Float4x4> gpuBranchMats = device.AllocateReadOnlyBuffer(branchMats);
            using ReadOnlyBuffer<Float2> gpuBranchParams = device.AllocateReadOnlyBuffer(branchParams);
            using ReadWriteBuffer<Float4> gpuChildStarts = device.AllocateReadWriteBuffer<Float4>(childCount);
            using ReadWriteBuffer<Float4> gpuChildEnds = device.AllocateReadWriteBuffer<Float4>(childCount);

            device.For(childCount, new ExpandLevelShader(gpuParentStarts, gpuParentEnds, gpuBranchMats, gpuBranchParams,
                gpuChildStarts, gpuChildEnds, branchMats.Length));

            childStarts = gpuChildStarts.ToArray();
            childEnds = gpuChildEnds.ToArray();
        }
    }

    internal static class LsystemGpu
    {
        // Same math as GetNewEnd: (end - start) * relativeLength, transformed by mat (w = 1), plus branchEnd
        public static float3 GetNewEnd(float3 start, float3 end, float4x4 mat, float relativeLength, float3 branchEnd)
        {
            float3 v = (end - start) * relativeLength;
            float4 inter = Hlsl.Mul(new float4(v, 1.0f), mat);
            return inter.XYZ + branchEnd;
        }
    }

    [ThreadGroupSize(64, 1, 1)]
    [GeneratedComputeShaderDescriptor]
    internal readonly partial struct ExpandLevelShader(
        ReadOnlyBuffer<float4> parentStarts,   // xyz = start, w = length
        ReadOnlyBuffer<float4> parentEnds,     // xyz = end
        ReadOnlyBuffer<float4x4> branchMats,
        ReadOnlyBuffer<float2> branchParams,   // x = RelativePosition, y = Attenuation
        ReadWriteBuffer<float4> childStarts,
        ReadWriteBuffer<float4> childEnds,
        int branchCount) : IComputeShader
    {
        public void Execute()
        {
            int id = ThreadIds.X;
            int parent = id / branchCount;
            int b = id % branchCount;

            float3 start = parentStarts[parent].XYZ;
            float length = parentStarts[parent].W;
            float3 end = parentEnds[parent].XYZ;

            float newLength = length * branchParams[b].Y;
            float3 branchStart = (end - start) * branchParams[b].X + start;
            float3 newEnd = LsystemGpu.GetNewEnd(start, end, branchMats[b], newLength, branchStart);

            childStarts[id] = new float4(branchStart, newLength);
            childEnds[id] = new float4(newEnd, 0);
        }
    }
}
