using System;
using UnityEngine;

namespace GenesisWorld.Procedural
{
    /// <summary>
    /// 规则网格的不可变数据结果，由 TerrainGenerator 转换为 Unity Mesh。
    /// </summary>
    public sealed class GridMeshData
    {
        public GridMeshData(Vector3[] vertices, int[] triangles, Vector2[] uv)
        {
            Vertices = vertices;
            Triangles = triangles;
            UV = uv;
        }

        public Vector3[] Vertices { get; }
        public int[] Triangles { get; }
        public Vector2[] UV { get; }

        public int VertexCount => Vertices.Length;
        public int TriangleCount => Triangles.Length / 3;
    }

    /// <summary>
    /// 负责生成规则网格的顶点、三角形索引和 UV 数据。
    /// 顶点高度由连续的 Perlin Noise 采样决定，该类不依赖场景对象。
    /// </summary>
    public static class MeshGenerator
    {
        public static GridMeshData GenerateGrid(
            float width,
            float depth,
            int xSegments,
            int zSegments,
            float noiseScale,
            float heightScale,
            Vector2 noiseOffset)
        {
            ValidateParameters(
                width,
                depth,
                xSegments,
                zSegments,
                noiseScale,
                heightScale);

            int verticesPerRow = xSegments + 1;
            int vertexCount = checked(verticesPerRow * (zSegments + 1));
            int triangleIndexCount = checked(xSegments * zSegments * 6);

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            int[] triangles = new int[triangleIndexCount];

            GenerateVerticesAndUV(
                width,
                depth,
                xSegments,
                zSegments,
                noiseScale,
                heightScale,
                noiseOffset,
                vertices,
                uv);
            GenerateTriangles(xSegments, zSegments, verticesPerRow, triangles);

            return new GridMeshData(vertices, triangles, uv);
        }

        private static void GenerateVerticesAndUV(
            float width,
            float depth,
            int xSegments,
            int zSegments,
            float noiseScale,
            float heightScale,
            Vector2 noiseOffset,
            Vector3[] vertices,
            Vector2[] uv)
        {
            float xStep = width / xSegments;
            float zStep = depth / zSegments;
            float xOrigin = -width * 0.5f;
            float zOrigin = -depth * 0.5f;
            int vertexIndex = 0;

            for (int z = 0; z <= zSegments; z++)
            {
                for (int x = 0; x <= xSegments; x++)
                {
                    float xPosition = xOrigin + x * xStep;
                    float zPosition = zOrigin + z * zStep;
                    float sampleX = (xPosition + noiseOffset.x) * noiseScale;
                    float sampleZ = (zPosition + noiseOffset.y) * noiseScale;
                    float noiseValue = Mathf.PerlinNoise(sampleX, sampleZ);

                    // 将 0–1 的噪声中心化，使地形围绕局部 Y=0 上下起伏。
                    float height = (noiseValue - 0.5f) * heightScale;

                    vertices[vertexIndex] = new Vector3(
                        xPosition,
                        height,
                        zPosition);

                    // UV 规范化到 0–1，便于后续材质和 Shader 直接复用。
                    uv[vertexIndex] = new Vector2(
                        (float)x / xSegments,
                        (float)z / zSegments);

                    vertexIndex++;
                }
            }
        }

        private static void GenerateTriangles(
            int xSegments,
            int zSegments,
            int verticesPerRow,
            int[] triangles)
        {
            int triangleIndex = 0;

            for (int z = 0; z < zSegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    int bottomLeft = z * verticesPerRow + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + verticesPerRow;
                    int topRight = topLeft + 1;

                    // 统一使用朝上的顺时针绕序，确保 RecalculateNormals 得到 Vector3.up。
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomRight;

                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                }
            }
        }

        private static void ValidateParameters(
            float width,
            float depth,
            int xSegments,
            int zSegments,
            float noiseScale,
            float heightScale)
        {
            if (width <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }

            if (depth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero.");
            }

            if (xSegments < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(xSegments), "X segments must be at least one.");
            }

            if (zSegments < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(zSegments), "Z segments must be at least one.");
            }

            if (noiseScale <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(noiseScale), "Noise scale must be greater than zero.");
            }

            if (heightScale < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(heightScale), "Height scale cannot be negative.");
            }
        }
    }
}
