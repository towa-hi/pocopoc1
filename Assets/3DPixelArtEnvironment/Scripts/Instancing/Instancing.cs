using System.Runtime.InteropServices;
using UnityEngine;

namespace Environment.Instancing
{
    public struct InstancingInfo
    {
        //public RenderParams renderParams;
        public MaterialPropertyBlock materialPropertyBlock;
        public ComputeBuffer argsBuffer;
        public ComputeBuffer dataBuffer;
        public const int commandCount = 1;
    }

    public static class Instancing<T>
    {
        public static InstancingInfo DataToInstancingInfo(Mesh mesh, T[] data, string shaderDataParameter)
        {
            InstancingInfo info = new InstancingInfo();
            info.dataBuffer = GetComputeBuffer(data);
            info.argsBuffer = GetArgsBuffer(mesh, (uint)data.Length);
            info.materialPropertyBlock = GetMaterialProperties(info.dataBuffer, shaderDataParameter);

            return info;
        }
        public static void ReleaseGPUMemory(InstancingInfo[] instancingInfo)
        {
            static void ReleaseInstancingInfo(InstancingInfo info)
            {
                info.argsBuffer?.Release();
                info.argsBuffer = null;

                info.dataBuffer?.Release();
                info.dataBuffer = null;
            }
            foreach (InstancingInfo entry in instancingInfo)
            {
                ReleaseInstancingInfo(entry);
            }
        }
        static ComputeBuffer GetComputeBuffer(T[] data)
        {
            var dataBuffer = new ComputeBuffer(data.Length, Marshal.SizeOf<T>());
            dataBuffer.SetData(data);
            return dataBuffer;
        }
        static ComputeBuffer GetArgsBuffer(Mesh mesh, uint instanceCount)
        {
            uint[] args = new uint[5];
            args[0] = mesh.GetIndexCount(0);
            args[1] = instanceCount;

            ComputeBuffer argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);

            return argsBuffer;
        }
        static MaterialPropertyBlock GetMaterialProperties(ComputeBuffer gpuData, string shaderDataParameter)
        {
            var props = new MaterialPropertyBlock();
            props.SetBuffer(shaderDataParameter, gpuData);
            return props;
        }
    }
}