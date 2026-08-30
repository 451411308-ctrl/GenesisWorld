using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GenesisWorld.Procedural
{
    /// <summary>
    /// 管理程序化地形参数，并将 MeshGenerator 的数据应用到 Unity 组件。
    /// 网格只在明确调用时生成，不会在 Update 中重复构建。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public sealed class TerrainGenerator : MonoBehaviour
    {
        [Header("地形尺寸")]
        [SerializeField, Min(0.01f)] private float width = 20f;
        [SerializeField, Min(0.01f)] private float depth = 20f;

        [Header("网格分辨率")]
        [SerializeField, Min(1)] private int xSegments = 20;
        [SerializeField, Min(1)] private int zSegments = 20;

        [Header("噪声设置")]
        [Tooltip("控制 Perlin Noise 的采样频率。数值越大，地形变化越密集。")]
        [SerializeField, Min(0.0001f)] private float noiseScale = 0.1f;

        [Tooltip("控制地形沿 Y 轴的最大起伏幅度。设为 0 时生成平地。")]
        [SerializeField, Min(0f)] private float heightScale = 5f;

        [Tooltip("在种子生成的偏移基础上，额外移动 Perlin Noise 的采样区域。")]
        [SerializeField] private Vector2 noiseOffset = Vector2.zero;

        [Header("种子设置")]
        [Tooltip("相同种子与相同地形参数会生成完全一致的地形。")]
        [SerializeField] private int seed = 12345;

        [Header("碰撞")]
        [SerializeField] private bool updateMeshCollider = true;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Mesh generatedMesh;

        public int VertexCount { get; private set; }
        public int TriangleCount { get; private set; }
        public int Seed => seed;
        public Vector2 SeedOffset { get; private set; }

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            GenerateTerrain();
        }

        [ContextMenu("Generate Terrain")]
        public void GenerateTerrain()
        {
            CacheComponents();

            SeedOffset = GenerateSeedOffset(seed);
            Vector2 finalNoiseOffset = noiseOffset + SeedOffset;

            GridMeshData meshData = MeshGenerator.GenerateGrid(
                width,
                depth,
                xSegments,
                zSegments,
                noiseScale,
                heightScale,
                finalNoiseOffset);

            Mesh mesh = GetOrCreateMesh(meshData.VertexCount);
            mesh.vertices = meshData.Vertices;
            mesh.triangles = meshData.Triangles;
            mesh.uv = meshData.UV;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.sharedMesh = mesh;
            UpdateCollider(mesh);

            VertexCount = meshData.VertexCount;
            TriangleCount = meshData.TriangleCount;
        }

        [ContextMenu("Randomize Seed")]
        public void RandomizeSeed()
        {
            int newSeed;

            do
            {
                // Guid 仅用于选择一个新种子；地形生成本身仍完全由 seed 决定。
                newSeed = Guid.NewGuid().GetHashCode();
            }
            while (newSeed == seed);

            seed = newSeed;
            GenerateTerrain();
        }

        private static Vector2 GenerateSeedOffset(int worldSeed)
        {
            const double minimumOffset = -10000d;
            const double offsetRange = 20000d;
            var random = new System.Random(worldSeed);

            // 使用局部随机源，避免污染 UnityEngine.Random 的全局状态。
            float offsetX = (float)(minimumOffset + random.NextDouble() * offsetRange);
            float offsetZ = (float)(minimumOffset + random.NextDouble() * offsetRange);

            return new Vector2(offsetX, offsetZ);
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }
        }

        private Mesh GetOrCreateMesh(int vertexCount)
        {
            if (generatedMesh == null)
            {
                generatedMesh = new Mesh
                {
                    name = "Procedural Grid Mesh"
                };
            }
            else
            {
                generatedMesh.Clear();
            }

            generatedMesh.indexFormat = vertexCount > ushort.MaxValue
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;

            return generatedMesh;
        }

        private void UpdateCollider(Mesh mesh)
        {
            if (!updateMeshCollider)
            {
                if (meshCollider.sharedMesh == generatedMesh)
                {
                    meshCollider.sharedMesh = null;
                }

                return;
            }

            // 先清空引用，确保重新生成后 MeshCollider 刷新碰撞数据。
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        private void OnValidate()
        {
            width = Mathf.Max(0.01f, width);
            depth = Mathf.Max(0.01f, depth);
            xSegments = Mathf.Max(1, xSegments);
            zSegments = Mathf.Max(1, zSegments);
            noiseScale = Mathf.Max(0.0001f, noiseScale);
            heightScale = Mathf.Max(0f, heightScale);
        }

        private void OnDestroy()
        {
            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }
        }
    }
}
