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

        [Header("碰撞")]
        [SerializeField] private bool updateMeshCollider = true;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Mesh generatedMesh;

        public int VertexCount { get; private set; }
        public int TriangleCount { get; private set; }

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

            GridMeshData meshData = MeshGenerator.GenerateGrid(
                width,
                depth,
                xSegments,
                zSegments);

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
