using UnityEngine;
using System.Linq;

namespace Environment.Utilities
{
    public static class MeshUtilities
    {
        public static (Vector3 vertices, Vector3 normals)[] RandomMeshPoints(Mesh mesh, int sampleAmount)
        {
            var triangleAreas = TriangleAreas(mesh);

            var meshArea = triangleAreas.Sum();

            var sampleIndices = UniformSampleMesh(triangleAreas, meshArea, sampleAmount);

            var uniformSamples = SampleTriangles(mesh, sampleIndices);

            return Enumerable.Range(0, uniformSamples.Item1.Length).Select(i => (uniformSamples.Item1[i], uniformSamples.Item2[i])).ToArray();
        }
        public static float GetMeshArea(Mesh mesh)
        {
            return TriangleAreas(mesh).Sum();
        }
        static float[] TriangleAreas(Mesh mesh)
        {
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;

            var areas = new float[triangles.Length / 3];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 corner = vertices[triangles[i]];
                Vector3 a = vertices[triangles[i + 1]] - corner;
                Vector3 b = vertices[triangles[i + 2]] - corner;

                var area = Vector3.Cross(a, b).magnitude * 0.5f;
                areas[i / 3] = area;
            }

            return areas;
        }
        static Vector3[] TriangleNormals(Mesh mesh)
        {
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;

            var normals = new Vector3[triangles.Length / 3];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 corner = vertices[triangles[i]];
                Vector3 a = vertices[triangles[i + 1]] - corner;
                Vector3 b = vertices[triangles[i + 2]] - corner;

                var normal = Vector3.Cross(a, b).normalized;
                normals[i / 3] = normal;
            }

            return normals;
        }

        // Returns triangle index array
        static int[] UniformSampleMesh(float[] triangleAreas, float meshArea, int sampleAmount)
        {
            var weights = new float[triangleAreas.Length];
            var sum = 0f;
            for (int i = 0; i < triangleAreas.Length; i++)
            {
                sum += triangleAreas[i] / meshArea;
                weights[i] = sum;
            }
            var triangleIndices = new int[sampleAmount];
            for (int i = 0; i < sampleAmount; i++)
            {
                var random = Random.value;
                for (int j = 0; j < weights.Length; j++)
                {
                    if (random <= weights[j])
                    {
                        triangleIndices[i] = j;
                        break;
                    }
                }
            }

            return triangleIndices;
        }
        static (Vector3[], Vector3[]) SampleTriangles(Mesh mesh, int[] indices)
        {
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;
            var normals = TriangleNormals(mesh);

            var sampleVertices = new Vector3[indices.Length];
            var sampleNormals = new Vector3[indices.Length];

            for (int sample = 0; sample < indices.Length; sample++)
            {
                int i = indices[sample] * 3;
                // triangle to origo
                Vector3 corner = vertices[triangles[i]];
                Vector3 a = vertices[triangles[i + 1]] - corner;
                Vector3 b = vertices[triangles[i + 2]] - corner;

                // reflection step
                float randomU = Random.value;
                float randomV = Random.value;
                if (randomU + randomV > 1.0f)
                {
                    randomU = 1 - randomU;
                    randomV = 1 - randomV;
                }

                var samplePoint = corner + randomU * a + randomV * b;
                sampleVertices[sample] = samplePoint;
                sampleNormals[sample] = normals[indices[sample]];
            }
            return (sampleVertices, sampleNormals);
        }
    }
}