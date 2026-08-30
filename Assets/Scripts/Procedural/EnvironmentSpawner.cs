using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenesisWorld.Procedural
{
    /// <summary>
    /// 使用 TerrainGenerator 的 World Seed，在地形表面确定性生成树木和岩石。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnvironmentSpawner : MonoBehaviour
    {
        private const string GeneratedRootName = "Generated Environment";
        private const int EnvironmentSeedSalt = unchecked((int)0x6E624EB7);

        [Header("地形引用")]
        [SerializeField] private TerrainGenerator terrainGenerator;
        [SerializeField] private bool generateOnStart = true;

        [Header("环境 Prefab")]
        [SerializeField] private GameObject[] treePrefabs = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] rockPrefabs = Array.Empty<GameObject>();

        [Header("生成数量")]
        [SerializeField, Min(0)] private int treeCount = 30;
        [SerializeField, Min(0)] private int rockCount = 15;

        [Header("生成规则")]
        [SerializeField, Min(0f)] private float spawnMargin = 1f;
        [SerializeField, Min(0f)] private float minimumSpacing = 1.2f;
        [SerializeField, Range(0f, 90f)] private float maxTreeSlope = 30f;
        [SerializeField, Range(0f, 90f)] private float maxRockSlope = 45f;
        [SerializeField, Min(0f)] private float centerClearRadius = 2f;
        [SerializeField, Min(1)] private int maxAttemptsPerObject = 20;

        [Header("随机缩放")]
        [SerializeField] private Vector2 treeScaleRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private Vector2 rockScaleRange = new Vector2(0.7f, 1.3f);

        private Transform generatedRoot;
        private bool hasGenerated;

        public int SpawnedTreeCount { get; private set; }
        public int SpawnedRockCount { get; private set; }
        public int LayoutSignature { get; private set; }

        private void Awake()
        {
            CacheTerrainGenerator();
        }

        private void OnEnable()
        {
            CacheTerrainGenerator();

            if (terrainGenerator != null)
            {
                terrainGenerator.TerrainGenerated += HandleTerrainGenerated;
            }
        }

        private void Start()
        {
            if (!generateOnStart || terrainGenerator == null)
            {
                return;
            }

            if (!terrainGenerator.IsGenerated)
            {
                terrainGenerator.GenerateTerrain();
            }
            else if (!hasGenerated)
            {
                GenerateEnvironment();
            }
        }

        private void OnDisable()
        {
            if (terrainGenerator != null)
            {
                terrainGenerator.TerrainGenerated -= HandleTerrainGenerated;
            }
        }

        [ContextMenu("Generate Environment")]
        public void GenerateEnvironment()
        {
            CacheTerrainGenerator();

            if (!CanGenerate())
            {
                return;
            }

            ClearEnvironment();
            CreateGeneratedHierarchy(out Transform treesRoot, out Transform rocksRoot);

            int environmentSeed = DeriveEnvironmentSeed(terrainGenerator.Seed);
            var random = new System.Random(environmentSeed);
            var spawnedPositions = new List<Vector3>(treeCount + rockCount);
            int signature = 17;

            SpawnedTreeCount = SpawnCategory(
                random,
                treePrefabs,
                treeCount,
                "Tree",
                treeScaleRange,
                maxTreeSlope,
                treesRoot,
                spawnedPositions,
                ref signature);

            SpawnedRockCount = SpawnCategory(
                random,
                rockPrefabs,
                rockCount,
                "Rock",
                rockScaleRange,
                maxRockSlope,
                rocksRoot,
                spawnedPositions,
                ref signature);

            LayoutSignature = signature;
            hasGenerated = true;

            WarnIfCountIsIncomplete("trees", treeCount, SpawnedTreeCount);
            WarnIfCountIsIncomplete("rocks", rockCount, SpawnedRockCount);
            LogGenerationSummary(treesRoot, rocksRoot);
        }

        [ContextMenu("Clear Environment")]
        public void ClearEnvironment()
        {
            Transform rootToRemove = generatedRoot != null
                ? generatedRoot
                : transform.Find(GeneratedRootName);

            if (rootToRemove != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(rootToRemove.gameObject);
                }
                else
                {
                    DestroyImmediate(rootToRemove.gameObject);
                }
            }

            generatedRoot = null;
            hasGenerated = false;
            SpawnedTreeCount = 0;
            SpawnedRockCount = 0;
            LayoutSignature = 0;
        }

        private int SpawnCategory(
            System.Random random,
            GameObject[] prefabs,
            int requestedCount,
            string objectPrefix,
            Vector2 scaleRange,
            float maximumSlope,
            Transform categoryRoot,
            List<Vector3> spawnedPositions,
            ref int signature)
        {
            if (requestedCount == 0 || prefabs == null || prefabs.Length == 0)
            {
                return 0;
            }

            int spawnedCount = 0;

            for (int objectIndex = 0; objectIndex < requestedCount; objectIndex++)
            {
                for (int attempt = 0; attempt < maxAttemptsPerObject; attempt++)
                {
                    if (!TryFindSpawnPoint(random, maximumSlope, spawnedPositions, out Vector3 position))
                    {
                        continue;
                    }

                    int prefabIndex = random.Next(prefabs.Length);
                    GameObject prefab = prefabs[prefabIndex];

                    if (prefab == null)
                    {
                        continue;
                    }

                    float yaw = NextFloat(random, 0f, 360f);
                    float uniformScale = NextFloat(random, scaleRange.x, scaleRange.y);
                    GameObject instance = Instantiate(prefab, position, Quaternion.Euler(0f, yaw, 0f), categoryRoot);
                    instance.name = $"{objectPrefix}_{spawnedCount:000}";
                    instance.transform.localScale = Vector3.one * uniformScale;

                    spawnedPositions.Add(position);
                    AddToSignature(ref signature, objectPrefix == "Tree" ? 1 : 2);
                    AddToSignature(ref signature, prefabIndex);
                    AddToSignature(ref signature, position.x);
                    AddToSignature(ref signature, position.y);
                    AddToSignature(ref signature, position.z);
                    AddToSignature(ref signature, yaw);
                    AddToSignature(ref signature, uniformScale);

                    spawnedCount++;
                    break;
                }
            }

            return spawnedCount;
        }

        private bool TryFindSpawnPoint(
            System.Random random,
            float maximumSlope,
            List<Vector3> spawnedPositions,
            out Vector3 position)
        {
            position = default;

            float halfWidth = terrainGenerator.Width * 0.5f;
            float halfDepth = terrainGenerator.Depth * 0.5f;
            float maxMargin = Mathf.Min(halfWidth, halfDepth);
            float margin = Mathf.Min(spawnMargin, maxMargin);
            float localX = NextFloat(random, -halfWidth + margin, halfWidth - margin);
            float localZ = NextFloat(random, -halfDepth + margin, halfDepth - margin);

            if (new Vector2(localX, localZ).sqrMagnitude < centerClearRadius * centerClearRadius)
            {
                return false;
            }

            Vector3 candidateWorldPosition = terrainGenerator.transform.TransformPoint(localX, 0f, localZ);
            float rayHeight = Mathf.Max(10f, terrainGenerator.HeightScale + 5f);
            Vector3 rayOrigin = candidateWorldPosition + Vector3.up * rayHeight;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayHeight * 2f);

            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                RaycastHit hit = hits[hitIndex];

                // 只接受目标地形碰撞体，避免 Player 或已生成物体干扰后续射线。
                if (hit.collider != terrainGenerator.TerrainCollider)
                {
                    continue;
                }

                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                if (slopeAngle > maximumSlope || !HasMinimumSpacing(hit.point, spawnedPositions))
                {
                    return false;
                }

                position = hit.point;
                return true;
            }

            return false;
        }

        private bool HasMinimumSpacing(Vector3 candidate, List<Vector3> spawnedPositions)
        {
            float spacingSquared = minimumSpacing * minimumSpacing;

            for (int index = 0; index < spawnedPositions.Count; index++)
            {
                Vector3 difference = candidate - spawnedPositions[index];
                difference.y = 0f;

                if (difference.sqrMagnitude < spacingSquared)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanGenerate()
        {
            if (terrainGenerator == null)
            {
                Debug.LogWarning("EnvironmentSpawner requires a TerrainGenerator reference.", this);
                return false;
            }

            if (!terrainGenerator.IsGenerated || terrainGenerator.TerrainCollider == null ||
                terrainGenerator.TerrainCollider.sharedMesh == null)
            {
                Debug.LogWarning("Generate the terrain and its MeshCollider before generating the environment.", this);
                return false;
            }

            if ((treeCount > 0 && !HasUsablePrefab(treePrefabs)) ||
                (rockCount > 0 && !HasUsablePrefab(rockPrefabs)))
            {
                Debug.LogWarning("EnvironmentSpawner requires at least one assigned prefab for each enabled category.", this);
                return false;
            }

            return true;
        }

        private static bool HasUsablePrefab(GameObject[] prefabs)
        {
            if (prefabs == null)
            {
                return false;
            }

            for (int index = 0; index < prefabs.Length; index++)
            {
                if (prefabs[index] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateGeneratedHierarchy(out Transform treesRoot, out Transform rocksRoot)
        {
            generatedRoot = new GameObject(GeneratedRootName).transform;
            generatedRoot.SetParent(transform, false);

            treesRoot = new GameObject("Trees").transform;
            treesRoot.SetParent(generatedRoot, false);

            rocksRoot = new GameObject("Rocks").transform;
            rocksRoot.SetParent(generatedRoot, false);
        }

        private void HandleTerrainGenerated()
        {
            if (generateOnStart || hasGenerated)
            {
                GenerateEnvironment();
            }
        }

        private void CacheTerrainGenerator()
        {
            if (terrainGenerator == null)
            {
                terrainGenerator = GetComponent<TerrainGenerator>();
            }
        }

        private static int DeriveEnvironmentSeed(int worldSeed)
        {
            unchecked
            {
                return worldSeed * 397 ^ EnvironmentSeedSalt;
            }
        }

        private static float NextFloat(System.Random random, float minimum, float maximum)
        {
            return minimum + (float)random.NextDouble() * (maximum - minimum);
        }

        private static void AddToSignature(ref int signature, int value)
        {
            unchecked
            {
                signature = signature * 31 + value;
            }
        }

        private static void AddToSignature(ref int signature, float value)
        {
            AddToSignature(ref signature, value.GetHashCode());
        }

        private void WarnIfCountIsIncomplete(string category, int requested, int spawned)
        {
            if (spawned < requested)
            {
                Debug.LogWarning($"Requested {requested} {category}, spawned {spawned}.", this);
            }
        }

        private void LogGenerationSummary(Transform treesRoot, Transform rocksRoot)
        {
            string treeSample = DescribeFirstInstance(treesRoot);
            string rockSample = DescribeFirstInstance(rocksRoot);

            Debug.Log(
                $"Environment generated | Seed {terrainGenerator.Seed} | " +
                $"Trees {SpawnedTreeCount}/{treeCount} | Rocks {SpawnedRockCount}/{rockCount} | " +
                $"Signature {LayoutSignature} | Tree_000 {treeSample} | Rock_000 {rockSample}",
                this);
        }

        private static string DescribeFirstInstance(Transform categoryRoot)
        {
            if (categoryRoot.childCount == 0)
            {
                return "not spawned";
            }

            Transform instance = categoryRoot.GetChild(0);
            Vector3 position = instance.position;
            return $"P({position.x:F3},{position.y:F3},{position.z:F3}) " +
                $"Yaw({instance.eulerAngles.y:F3}) Scale({instance.localScale.x:F3})";
        }

        private void OnValidate()
        {
            treeCount = Mathf.Max(0, treeCount);
            rockCount = Mathf.Max(0, rockCount);
            spawnMargin = Mathf.Max(0f, spawnMargin);

            TerrainGenerator generator = terrainGenerator != null
                ? terrainGenerator
                : GetComponent<TerrainGenerator>();

            if (generator != null)
            {
                spawnMargin = Mathf.Min(spawnMargin, generator.Width * 0.5f, generator.Depth * 0.5f);
            }

            minimumSpacing = Mathf.Max(0f, minimumSpacing);
            maxTreeSlope = Mathf.Clamp(maxTreeSlope, 0f, 90f);
            maxRockSlope = Mathf.Clamp(maxRockSlope, 0f, 90f);
            centerClearRadius = Mathf.Max(0f, centerClearRadius);
            maxAttemptsPerObject = Mathf.Max(1, maxAttemptsPerObject);
            treeScaleRange = ValidateScaleRange(treeScaleRange);
            rockScaleRange = ValidateScaleRange(rockScaleRange);
        }

        private static Vector2 ValidateScaleRange(Vector2 range)
        {
            range.x = Mathf.Max(0.01f, range.x);
            range.y = Mathf.Max(range.x, range.y);
            return range;
        }
    }
}
