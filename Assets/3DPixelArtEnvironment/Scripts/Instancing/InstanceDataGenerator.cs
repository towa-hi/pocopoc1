using Environment.Utilities;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment.Instancing
{
    public struct InstanceData // Represents data as seen on gpu. Same as struct defined there
    {
        public Matrix4x4 TRS;
        public Vector3 Normal;
    }
    public static class InstanceDataGenerator
    {
        public static InstanceData[] RandomMeshInstanceData(Mesh mesh, float density, InstanceConfiguration[] configurations)
        {
            if (mesh == null || density <= 0 || configurations == null || configurations.Length == 0)
                return null;

            int instanceAmount = Mathf.CeilToInt(MeshUtilities.GetMeshArea(mesh) / density);

            var samples = MeshUtilities.RandomMeshPoints(mesh, instanceAmount);

            var data = new List<InstanceData>();

            for (int i = 0; i < samples.Length; i++)
            {
                var (vertex, normal) = samples[i];

                var instance = new InstanceData
                {
                    TRS = Matrix4x4.TRS(
                        vertex, 
                        Quaternion.identity,
                        Vector3.one
                    ),
                    Normal = normal
                };
                data.Add(instance);
            }
            return data.ToArray();
        }
        public static InstanceData[][] TerrainInstanceData(Terrain terrain, InstanceConfiguration[][] configurations, float[] layerDensities, float positionVariance, float scaleVariance)
        {
            static int LargestColorIndex(Color color) => (new float[] { color.r, color.g, color.b, color.a }).Select((value, index) => (value, index)).Max().index;
            var result = new List<InstanceData>[4] { new List<InstanceData>(), new List<InstanceData>(), new List<InstanceData>(), new List<InstanceData>() };

            var controlTexture = terrain.terrainData.alphamapTextureCount > 0 ? terrain.terrainData.alphamapTextures[0] : null;
            if (controlTexture == null)
            {
                Debug.LogWarning("Control texture not defined. Defaulting to layer 1 everywhere");
            }

            var size = terrain.terrainData.size;
            float width = size.x, height = size.y, length = size.z;

            for (int layer = 0; layer < layerDensities.Length; layer++)
            {
                if(configurations[layer].Length == 0 || (controlTexture == null && layer > 0)) // Dont calculate points for empty layer's configurations
                {
                    continue;
                }

                float density = layerDensities[layer];
                float step = 1f / density;
                int pointWidth = Mathf.RoundToInt(width * density);
                int pointLength = Mathf.RoundToInt(length * density);

                for (int x = 0; x < pointWidth; x++)
                {
                    for (int z = 0; z < pointLength; z++)
                    {
                        var xPosition = (x + 0.5f + UnityEngine.Random.Range(-positionVariance, positionVariance)) * step;
                        var zPosition = (z + 0.5f + UnityEngine.Random.Range(-positionVariance, positionVariance)) * step;
                        var terrainUV = new Vector2(xPosition / width, zPosition / length);

                        var instance = new InstanceData()
                        {
                            TRS = Matrix4x4.TRS(
                                new Vector3()
                                {
                                    x = xPosition,
                                    y = terrain.terrainData.GetInterpolatedHeight(terrainUV.x, terrainUV.y),
                                    z = zPosition,
                                },
                                Quaternion.identity,
                                Vector3.one * UnityEngine.Random.Range(1 - scaleVariance, 1 + scaleVariance)
                            ),
                            Normal = terrain.terrainData.GetInterpolatedNormal(terrainUV.x, terrainUV.y),
                        };

                        var pointLayer = controlTexture == null ? 0 : LargestColorIndex(controlTexture.GetPixelBilinear(terrainUV.x, terrainUV.y)); // Choose layer that is most potent. Could roll with values as weights but this is simple.

                        if (layer == pointLayer)
                        {
                            result[layer].Add(instance);
                        }
                    }
                }
            }
            return result.Select(list => list.ToArray()).ToArray();
        }
        static InstanceData ApplyConfiguration(InstanceConfiguration configuration, InstanceData data)
        {
            data.TRS *= Matrix4x4.TRS(configuration.NormalOffset * data.Normal, Quaternion.identity, new Vector3(configuration.Scale, configuration.Scale, configuration.Scale)); // Matrix4x4.TRS(configuration.Offset, Quaternion.identity, configuration.Scale);
            return data;
        }
        public static Dictionary<InstanceConfiguration, List<InstanceData>> DivideInstanceData(InstanceData[] instanceData, InstanceConfiguration[] instanceConfigurations)
        {
            if (instanceData == null || instanceData.Length == 0 || instanceConfigurations == null || instanceConfigurations.Length == 0)
                return null;

            var result = new Dictionary<InstanceConfiguration, List<InstanceData>>();
            foreach (var configuration in instanceConfigurations)
                result.Add(configuration, new List<InstanceData>());

            var weights = instanceConfigurations.Select(configuration => configuration.Probability).ToArray();
            var weightedSampler = new WeightedDistribution(weights);

            foreach (InstanceData data in instanceData)
            {
                var index = weightedSampler.Sample();

                var sampledConfiguration = instanceConfigurations[index];

                var updatedData = ApplyConfiguration(sampledConfiguration, data);

                result[sampledConfiguration].Add(updatedData);
            }

            return result;
        }
    }
}
