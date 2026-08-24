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
    /// 负责生成平坦规则网格的顶点、三角形索引和 UV 数据。
    /// 该类不依赖场景对象，也不包含高度或 Noise 逻辑。
    /// </summary>
    public static class MeshGenerator
    {
        public static GridMeshData GenerateGrid(
            float width,
            float depth,
            int xSegments,
            int zSegments)
        {
            ValidateParameters(width, depth, xSegments, zSegments);

            int verticesPerRow = xSegments + 1;
            int vertexCount = checked(verticesPerRow * (zSegments + 1));
            int triangleIndexCount = checked(xSegments * zSegments * 6);

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            int[] triangles = new int[triangleIndexCount];

            GenerateVerticesAndUV(width, depth, xSegments, zSegments, vertices, uv);
            GenerateTriangles(xSegments, zSegments, verticesPerRow, triangles);

            return new GridMeshData(vertices, triangles, uv);
        }

        private static void GenerateVerticesAndUV(
            float width,
            float depth,
            int xSegments,
            int zSegments,
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
                    vertices[vertexIndex] = new Vector3(
                        xOrigin + x * xStep,
                        0f,
                        zOrigin + z * zStep);

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
            int zSegments)
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
        }
    }
}
