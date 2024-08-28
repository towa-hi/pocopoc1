using System.Collections.Generic;
using UnityEngine;

namespace Environment.Instancing
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshInstancesBehaviour : InstancesBehaviour
    {
        [Header("Sub Mesh Details")]
        public bool UseSubMesh = false;
        public int SubMeshIndex = 0;

        [Header("Instance Configurations")]
        public float Density = 1f;
        public InstanceConfiguration[] InstanceConfigurations;

        public override Dictionary<InstanceConfiguration, List<InstanceData>> GetInstanceData()
        {
            for (int j = 0; j < InstanceConfigurations.Length; j++)
            {
                var configuration = InstanceConfigurations[j];
                if (configuration.Scale <= 0f || configuration.Material == null || configuration.Mesh == null) 
                {
                    Debug.LogError("The given Instance Configurations should have a material, mesh and a scale larger than 0"); 
                    return null; 
                }
            }
            var mesh = GetComponent<MeshFilter>().mesh;
            if (UseSubMesh)
            {
                if(SubMeshIndex >= mesh.subMeshCount)
                {
                    Debug.LogWarning("The sub mesh index does not exist. Using default; using the whole mesh.");
                }
                else
                {
                    var subMesh = new Mesh
                    {
                        vertices = mesh.vertices,
                        triangles = mesh.GetTriangles(SubMeshIndex)
                    };
                    mesh = subMesh;
                }
            }
            var instanceData = InstanceDataGenerator.RandomMeshInstanceData(mesh, Density, InstanceConfigurations);
            return InstanceDataGenerator.DivideInstanceData(instanceData, InstanceConfigurations);
        }
    }
}