using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Environment.Instancing
{
    [RequireComponent(typeof(Terrain))]
    public class TerrainInstancesBehaviour : InstancesBehaviour
    {
        [Serializable] // Helper class to configure in Unity Editor. The next ~20 lines are some little spaghetti to make the interface nice without editor scripting
        public class TerrainInstancingInput
        {
            public float density = 1f;
            public InstanceConfiguration[] configurations;
        }
        [Header("Terrain Texture Channels")]

        public TerrainInstancingInput FirstLayer;
        public TerrainInstancingInput SecondLayer;
        public TerrainInstancingInput ThirdLayer;
        public TerrainInstancingInput FourthLayer;

        [Header("Variance Parameters")]

        [Range(0f, 0.5f)] public float PositionVariance = 0.5f;
        [Range(0f, 0.9f)] public float ScaleVariance = 0.2f;

        float[] layerDensities;
        InstanceConfiguration[][] configurations;

        public override Dictionary<InstanceConfiguration, List<InstanceData>> GetInstanceData()
        {
            layerDensities = new float[4] { FirstLayer.density, SecondLayer.density, ThirdLayer.density, FourthLayer.density };
            configurations = new InstanceConfiguration[4][] { FirstLayer.configurations, SecondLayer.configurations, ThirdLayer.configurations, FourthLayer.configurations };

            // Workaround for Unity Editors lack of 2D array support
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < configurations[i].Length; j++)
                {
                    var configuration = configurations[i][j];
                    if(configuration.Scale <= 0f || configuration.Material == null || configuration.Mesh == null) { Debug.LogError("The given Instance Configurations should have a material, mesh and a scale larger than 0"); return null; }
                }
            }

            var instanceData = InstanceDataGenerator.TerrainInstanceData(GetComponent<Terrain>(), configurations, layerDensities, PositionVariance, ScaleVariance);

            // Debug.Log($"Generated {instanceData.Sum(a => a.Length)} points for instancing!");

            var result = new Dictionary<InstanceConfiguration, List<InstanceData>>();

            for (int i = 0; i < configurations.Length; i++)
            {
                var dividedInstances = InstanceDataGenerator.DivideInstanceData(instanceData[i], configurations[i]);
                if (dividedInstances != null)
                {
                    result.AddRange(dividedInstances);
                }
            }

            return result;
        }
    }
}
