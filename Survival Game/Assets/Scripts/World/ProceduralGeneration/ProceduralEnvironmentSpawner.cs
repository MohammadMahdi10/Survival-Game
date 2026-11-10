using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralEnvironmentSpawner : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private GameObject[] plainsTreePrefabs;

    [Header("Chunk Settings")]
    [SerializeField] private float chunkSize = 250f;

    private Dictionary<Vector2Int, Transform> vegetationChunks = new Dictionary<Vector2Int, Transform>();
    private Transform chunkContainer;

    [Header("Chunk Rendering")]
    [SerializeField] private Transform player;
    [SerializeField] private float chunkRenderDistance = 1200f;
    [SerializeField] private float chunkUpdateInterval = 0.5f;

    [Header("Plains Environment Prefabs")]
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] bushPrefabs;

    [Header("Environment Object Counts")]
    [SerializeField] private int rockCount = 250;
    [SerializeField] private int bushCount = 500;

    [SerializeField] private float environmentMinimumSpacing = 6f;

    [Header("Plains Trees")]
    [SerializeField] private int plainsTreeCount = 1800;
    [SerializeField] private float maximumSlope = 8f;
    [SerializeField] private float kingdomExclusionRadius = 40f;

    [Header("Forest Generation")]
    [SerializeField] private float forestNoiseScale = 0.0018f;
    [SerializeField] private float forestThreshold = 0.45f;
    [SerializeField] private float forestSpacing = 12f;
    [SerializeField] private float scatteredTreeSpacing = 28f;
    [SerializeField] private float scatteredTreeChance = 0.04f;

    [Header("Small Woodlands")]
    [SerializeField] private float woodlandNoiseScale = 0.004f;
    [SerializeField] private float woodlandThreshold = 0.65f;
    [SerializeField] private float woodlandDensity = 0.6f;

    [Header("Snow Environment Prefabs")]
    [SerializeField] private GameObject[] snowTreePrefabs;
    [SerializeField] private GameObject[] snowRockPrefabs;

    [Header("Snow Generation")]
    [SerializeField] private int snowTreeCount = 900;
    [SerializeField] private int snowRockCount = 250;
    [SerializeField] private float snowMaximumSlope = 28f;
    [SerializeField] private float snowTreeLineHeight = 550f;
    [SerializeField] private float snowForestNoiseScale = 0.0016f;
    [SerializeField] private float snowForestThreshold = 0.45f;
    [SerializeField] private float snowTreeSpacing = 10f;
    [SerializeField] private float snowRockSpacing = 9f;

    [Header("Desert Environment Prefabs")]
    [SerializeField] private GameObject[] cactusPrefabs;
    [SerializeField] private GameObject[] desertBushPrefabs;
    [SerializeField] private GameObject[] desertRockPrefabs;

    [Header("Desert Generation")]
    [SerializeField] private int cactusCount = 220;
    [SerializeField] private int desertBushCount = 180;
    [SerializeField] private int desertRockCount = 260;

    [SerializeField] private float desertMaximumSlope = 30f;
    [SerializeField] private float desertNoiseScale = 0.002f;
    [SerializeField] private float desertClusterThreshold = 0.62f;

    [SerializeField] private float cactusSpacing = 14f;
    [SerializeField] private float desertBushSpacing = 8f;
    [SerializeField] private float desertRockSpacing = 10f;

    /// Starts automatically updating chunk visibility during gameplay.
    /// Finds the chunks created earlier using Generate Vegetation.
    private void Start() {
        chunkContainer = transform.Find("GeneratedVegetation/Chunks");

        if (chunkContainer == null || player == null) {
            return;
        }

        vegetationChunks.Clear();

        foreach (Transform chunk in chunkContainer) {
            string[] parts = chunk.name.Split('_');

            if (parts.Length != 3) {
                continue;
            }

            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int z)) {
                vegetationChunks[new Vector2Int(x, z)] = chunk;
            }
        }

        UpdateChunkVisibility();
        InvokeRepeating(nameof(UpdateChunkVisibility), chunkUpdateInterval, chunkUpdateInterval);
    }

    [ContextMenu("Generate Vegetation")]
    public void GenerateVegetation()
    {
        if (biomeGenerator == null || plainsTreePrefabs == null || plainsTreePrefabs.Length == 0) {
            return;
        }

        Terrain[] terrains = { biomeGenerator.snowTerrain, biomeGenerator.plainsTerrain, biomeGenerator.desertTerrain, biomeGenerator.swampTerrain };

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            if (terrain == null || terrain.terrainData == null) {
                return;
            }

            Vector3 position = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            minX = Mathf.Min(minX, position.x);
            minZ = Mathf.Min(minZ, position.z);
            maxX = Mathf.Max(maxX, position.x + terrainSize.x);
            maxZ = Mathf.Max(maxZ, position.z + terrainSize.z);
        }

        Vector2 worldCentre = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        System.Random random = new System.Random(biomeGenerator.seed + 8421);

        Transform previous = transform.Find("GeneratedVegetation");

        if (previous != null) {
            if (Application.isPlaying) {
                Destroy(previous.gameObject);
            }
            else {
                DestroyImmediate(previous.gameObject);
            }
        }

        GameObject container = new GameObject("GeneratedVegetation");
        container.transform.SetParent(transform, false);

        vegetationChunks.Clear();

        GameObject chunks = new GameObject("Chunks");
        chunks.transform.SetParent(container.transform, false);
        chunkContainer = chunks.transform;

        List<Vector2> placedPositions = new List<Vector2>();

        float size = Mathf.Max(1f, chunkSize);

        int minChunkX = Mathf.FloorToInt(minX / size);
        int maxChunkX = Mathf.CeilToInt(maxX / size);
        int minChunkZ = Mathf.FloorToInt(minZ / size);
        int maxChunkZ = Mathf.CeilToInt(maxZ / size);

        for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++) {
            for (int chunkZ = minChunkZ; chunkZ < maxChunkZ; chunkZ++) {
                float startX = Mathf.Max(minX, chunkX * size);
                float endX = Mathf.Min(maxX, (chunkX + 1) * size);
                float startZ = Mathf.Max(minZ, chunkZ * size);
                float endZ = Mathf.Min(maxZ, (chunkZ + 1) * size);

                float centreX = (startX + endX) * 0.5f;
                float centreZ = (startZ + endZ) * 0.5f;

                int targetTrees = GetPlainsChunkTreeBudget(centreX, centreZ);
                int chunkSpawned = 0;
                int attempts = 0;
                int maxAttempts = targetTrees * 100;

                while (chunkSpawned < targetTrees && attempts < maxAttempts) {
                    attempts++;

                    float x = Mathf.Lerp(startX, endX, (float)random.NextDouble());
                    float z = Mathf.Lerp(startZ, endZ, (float)random.NextDouble());

                    Vector2 point = new Vector2(x, z);
                    float centreDistance = Vector2.Distance(point, worldCentre);

                    if (centreDistance < kingdomExclusionRadius || centreDistance > 1950f) {
                        continue;
                    }

                    biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

                    if (plains < 0.85f) {
                        continue;
                    }

                    if (biomeGenerator.IsInsidePlainsBossArena(x, z)) {
                        continue;
                    }

                    float forestNoise = Mathf.PerlinNoise(x * forestNoiseScale + biomeGenerator.seed * 0.137f + 173f, z * forestNoiseScale + biomeGenerator.seed * 0.293f + 291f);
                    float forestDensity = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(forestThreshold, forestThreshold + 0.2f, forestNoise));

                    float woodlandNoise = Mathf.PerlinNoise(x * woodlandNoiseScale + biomeGenerator.seed * 0.173f + 391f, z * woodlandNoiseScale + biomeGenerator.seed * 0.227f + 617f);
                    float woodlandMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(woodlandThreshold, woodlandThreshold + 0.15f, woodlandNoise));

                    int chunkType = GetPlainsChunkType(x, z);

                    float combinedDensity = chunkType == 2 ? Mathf.Lerp(0.75f, 1f, forestDensity) : chunkType == 1 ? Mathf.Lerp(0.25f, 0.6f, woodlandMask) : 0f;
                    float spawnChance = Mathf.Lerp(scatteredTreeChance, 0.95f, combinedDensity);

                    if ((float)random.NextDouble() > spawnChance) {
                        continue;
                    }

                    float spacing = Mathf.Lerp(scatteredTreeSpacing, forestSpacing, combinedDensity);

                    bool tooClose = false;

                    foreach (Vector2 placed in placedPositions) {
                        if (Vector2.Distance(point, placed) < spacing) {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) {
                        continue;
                    }

                    Terrain currentTerrain = null;

                    foreach (Terrain terrain in terrains) {
                        Vector3 origin = terrain.transform.position;
                        Vector3 candidateSize = terrain.terrainData.size;

                        if (x >= origin.x && x <= origin.x + candidateSize.x && z >= origin.z && z <= origin.z + candidateSize.z) {
                            currentTerrain = terrain;
                            break;
                        }
                    }

                    if (currentTerrain == null) {
                        continue;
                    }

                    Vector3 terrainOrigin = currentTerrain.transform.position;
                    Vector3 terrainSize = currentTerrain.terrainData.size;

                    float u = Mathf.Clamp01((x - terrainOrigin.x) / terrainSize.x);
                    float v = Mathf.Clamp01((z - terrainOrigin.z) / terrainSize.z);
                    float slope = currentTerrain.terrainData.GetSteepness(u, v);

                    if (slope > maximumSlope) {
                        continue;
                    }

                    float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;

                    GameObject prefab = plainsTreePrefabs[random.Next(plainsTreePrefabs.Length)];

                    if (prefab == null) {
                        continue;
                    }

                    GameObject tree = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, GetVegetationParent(x, z, true));

                    tree.transform.Rotate(0f, (float)random.NextDouble() * 360f, 0f);

                    float scale = Mathf.Lerp(1.6f, 2.3f, (float)random.NextDouble());
                    tree.transform.localScale *= scale;

                    placedPositions.Add(point);
                    chunkSpawned++;
                }
            }
        }

        /// Generate the remaining vegetation using the existing biome rules.
        SpawnEnvironmentObjects(container.transform, terrains, worldCentre, random, rockPrefabs, rockCount, 0.25f, environmentMinimumSpacing);
        SpawnEnvironmentObjects(container.transform, terrains, worldCentre, random, bushPrefabs, bushCount, 0.85f, environmentMinimumSpacing);

        SpawnSnowEnvironment(container.transform, terrains, worldCentre);
        SpawnDesertEnvironment(container.transform, terrains, worldCentre);

        UpdateChunkVisibility();
    }


    /// Places environmental objects using the same forest noise
    /// as tree generation. Bushes and logs favour within forest regions,
    /// while rocks can also appear in open Plains.
    private void SpawnEnvironmentObjects(Transform parent, Terrain[] terrains, Vector2 worldCentre, System.Random random, GameObject[] prefabs, int targetCount, float forestPreference, float minimumSpacing) {
        if (prefabs == null || prefabs.Length == 0 || targetCount <= 0) {
            return;
        }

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + size.x);
            maxZ = Mathf.Max(maxZ, origin.z + size.z);
        }

        List<Vector2> placedPositions = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 150;

        while (spawned < targetCount && attempts < maxAttempts) {
            attempts++;

            float x = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float z = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

            Vector2 point = new Vector2(x, z);
            float centreDistance = Vector2.Distance(point, worldCentre);

            if (centreDistance < kingdomExclusionRadius || centreDistance > 1950f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            if (plains < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
                continue;
            }

            float forestNoise = Mathf.PerlinNoise(x * forestNoiseScale + biomeGenerator.seed * 0.137f + 173f, z * forestNoiseScale + biomeGenerator.seed * 0.293f + 291f);

            float forestDensity = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(forestThreshold, forestThreshold + 0.2f, forestNoise));

            float woodlandNoise = Mathf.PerlinNoise(x * woodlandNoiseScale + biomeGenerator.seed * 0.173f + 391f, z * woodlandNoiseScale + biomeGenerator.seed * 0.227f + 617f);

            float woodlandMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(woodlandThreshold, woodlandThreshold + 0.15f, woodlandNoise));

            float density = Mathf.Max(forestDensity, woodlandMask * woodlandDensity);

            float spawnChance = Mathf.Lerp(0.25f, 1f, density);
            spawnChance = Mathf.Lerp(1f, spawnChance, forestPreference);

            if ((float)random.NextDouble() > spawnChance) {
                continue;
            }

            bool tooClose = false;

            foreach (Vector2 placed in placedPositions) {
                if (Vector2.Distance(point, placed) < minimumSpacing) {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) {
                continue;
            }

            Terrain currentTerrain = null;

            foreach (Terrain terrain in terrains) {
                Vector3 origin = terrain.transform.position;
                Vector3 size = terrain.terrainData.size;

                if (x >= origin.x && x <= origin.x + size.x && z >= origin.z && z <= origin.z + size.z) {
                    currentTerrain = terrain;
                    break;
                }
            }

            if (currentTerrain == null) {
                continue;
            }

            Vector3 terrainOrigin = currentTerrain.transform.position;
            Vector3 terrainSize = currentTerrain.terrainData.size;

            float u = Mathf.Clamp01((x - terrainOrigin.x) / terrainSize.x);
            float v = Mathf.Clamp01((z - terrainOrigin.z) / terrainSize.z);

            float slope = currentTerrain.terrainData.GetSteepness(u, v);

            if (slope > maximumSlope) {
                continue;
            }

            float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;

            GameObject prefab = prefabs[random.Next(prefabs.Length)];

            if (prefab == null) {
                continue;
            }

            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(x, z, false));

            float scale = Mathf.Lerp(0.8f, 1.3f, (float)random.NextDouble());
            instance.transform.localScale *= scale;

            placedPositions.Add(point);
            spawned++;
        }
    }


    /// Generates clustered pine forests and scattered rocks in the Snow biome.
    /// Tree density gradually decreases towards the treeline while
    /// rocks can appear higher up on the mountain slopes.
    private void SpawnSnowEnvironment(Transform parent, Terrain[] terrains, Vector2 worldCentre) {
        System.Random random = new System.Random(biomeGenerator.seed + 9427);

        SpawnSnowObjects(parent, terrains, worldCentre, random, snowTreePrefabs, snowTreeCount, true);
        SpawnSnowObjects(parent, terrains, worldCentre, random, snowRockPrefabs, snowRockCount, false);
    }

    /// Distributes Snow objects in seeded patches, respecting biome
    /// boundaries, mountain slopes, elevation and the Kingdom clearing.
    private void SpawnSnowObjects(Transform parent, Terrain[] terrains, Vector2 worldCentre, System.Random random, GameObject[] prefabs, int targetCount, bool isTree) {
        
        if (prefabs == null || prefabs.Length == 0 || targetCount <= 0) {
            return;
        }

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + size.x);
            maxZ = Mathf.Max(maxZ, origin.z + size.z);
        }

        List<Vector2> placedPositions = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 150;

        while (spawned < targetCount && attempts < maxAttempts) {
            attempts++;

            float x = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float z = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

            Vector2 point = new Vector2(x, z);
            float centreDistance = Vector2.Distance(point, worldCentre);

            if (centreDistance < kingdomExclusionRadius || centreDistance > 1950f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            if (snow < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
                continue;
            }

            Terrain currentTerrain = null;

            foreach (Terrain terrain in terrains) {
                Vector3 origin = terrain.transform.position;
                Vector3 size = terrain.terrainData.size;

                if (x >= origin.x && x <= origin.x + size.x && z >= origin.z && z <= origin.z + size.z) {
                    currentTerrain = terrain;
                    break;
                }
            }

            if (currentTerrain == null) {
                continue;
            }

            Vector3 originPosition = currentTerrain.transform.position;
            Vector3 terrainSize = currentTerrain.terrainData.size;

            float u = Mathf.Clamp01((x - originPosition.x) / terrainSize.x);
            float v = Mathf.Clamp01((z - originPosition.z) / terrainSize.z);

            float slope = currentTerrain.terrainData.GetSteepness(u, v);
            float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + originPosition.y;
            float elevation = y - originPosition.y;

            if (isTree && slope > snowMaximumSlope) {
                continue;
            }

            if (isTree && elevation > snowTreeLineHeight) {
                continue;
            }

            if (!isTree && slope > 48f) {
                continue;
            }

            float forestNoise = Mathf.PerlinNoise(x * snowForestNoiseScale + biomeGenerator.seed * 0.131f + 241f, z * snowForestNoiseScale + biomeGenerator.seed * 0.217f + 617f);

            float forestDensity = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(snowForestThreshold, snowForestThreshold + 0.2f, forestNoise));

            float treelineFade = isTree ? 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(snowTreeLineHeight * 0.6f, snowTreeLineHeight, elevation)) : 1f;

            float spawnChance = isTree ? Mathf.Lerp(0.04f, 0.95f, forestDensity) * treelineFade : Mathf.Lerp(0.35f, 0.85f, forestDensity);

            if ((float)random.NextDouble() > spawnChance) {
                continue;
            }

            float minimumSpacing = isTree ? snowTreeSpacing : snowRockSpacing;

            bool tooClose = false;

            foreach (Vector2 placed in placedPositions) {
                if (Vector2.Distance(point, placed) < minimumSpacing) {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) {
                continue;
            }

            GameObject prefab = prefabs[random.Next(prefabs.Length)];

            if (prefab == null) {
                continue;
            }

            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(x, z, isTree));

            float scale = isTree ? Mathf.Lerp(2f, 3.5f, (float)random.NextDouble()) : Mathf.Lerp(0.85f, 1.25f, (float)random.NextDouble());
            instance.transform.localScale *= scale;

            placedPositions.Add(point);
            spawned++;
        }
    }

    
    /// Generates cactus clusters, dry bushes and scattered rocks.
    /// Uses seeded noise to create natural patches and large open
    /// areas of sand rather than uniform object placement.
    private void SpawnDesertEnvironment(Transform parent, Terrain[] terrains, Vector2 worldCentre) {
        SpawnDesertObjects(parent, terrains, worldCentre, cactusPrefabs, cactusCount, cactusSpacing, 0);
        SpawnDesertObjects(parent, terrains, worldCentre, desertBushPrefabs, desertBushCount, desertBushSpacing, 1);
        SpawnDesertObjects(parent, terrains, worldCentre, desertRockPrefabs, desertRockCount, desertRockSpacing, 2);
    }

    /// Places objects only in the Desert biome, avoiding steep slopes,
    /// the Kingdom and the Plains boss summit.
    /// Each object type uses a separate deterministic random seed.
    private void SpawnDesertObjects(Transform parent, Terrain[] terrains, Vector2 worldCentre, GameObject[] prefabs, int targetCount, float minimumSpacing, int objectType) {
        
        if (prefabs == null || prefabs.Length == 0 || targetCount <= 0) {
            return;
        }

        System.Random random = new System.Random(biomeGenerator.seed + 12037 + objectType * 7919);

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + size.x);
            maxZ = Mathf.Max(maxZ, origin.z + size.z);
        }

        List<Vector2> placedPositions = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 150;

        while (spawned < targetCount && attempts < maxAttempts) {
            attempts++;

            float x = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float z = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

            Vector2 point = new Vector2(x, z);
            float centreDistance = Vector2.Distance(point, worldCentre);

            if (centreDistance < kingdomExclusionRadius || centreDistance > 1950f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            if (desert < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
                continue;
            }

            float regionNoise = Mathf.PerlinNoise(x * desertNoiseScale + biomeGenerator.seed * 0.137f + 173f, z * desertNoiseScale + biomeGenerator.seed * 0.293f + 291f);

            float clusterDensity = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(desertClusterThreshold, desertClusterThreshold + 0.2f, regionNoise));

            float spawnChance;

            switch (objectType) {
                case 0: {
                        spawnChance = Mathf.Lerp(0.015f, 0.8f, clusterDensity);
                        break;
                    }
                case 1: {
                        spawnChance = Mathf.Lerp(0.025f, 0.55f, clusterDensity);
                        break;
                    }
                default: {
                        spawnChance = Mathf.Lerp(0.25f, 0.8f, clusterDensity);
                        break;
                    }
            }

            if ((float)random.NextDouble() > spawnChance) {
                continue;
            }

            bool tooClose = false;

            foreach (Vector2 placed in placedPositions) {
                if (Vector2.Distance(point, placed) < minimumSpacing) {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) {
                continue;
            }

            Terrain currentTerrain = null;

            foreach (Terrain terrain in terrains) {
                Vector3 origin = terrain.transform.position;
                Vector3 size = terrain.terrainData.size;

                if (x >= origin.x && x <= origin.x + size.x && z >= origin.z && z <= origin.z + size.z) {
                    currentTerrain = terrain;
                    break;
                }
            }

            if (currentTerrain == null) {
                continue;
            }

            Vector3 terrainOrigin = currentTerrain.transform.position;
            Vector3 terrainSize = currentTerrain.terrainData.size;

            float u = Mathf.Clamp01((x - terrainOrigin.x) / terrainSize.x);
            float v = Mathf.Clamp01((z - terrainOrigin.z) / terrainSize.z);

            float slope = currentTerrain.terrainData.GetSteepness(u, v);

            if (slope > desertMaximumSlope) {
                continue;
            }

            float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;

            GameObject prefab = prefabs[random.Next(prefabs.Length)];

            if (prefab == null) {
                continue;
            }

            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(x, z, false));

            float scale = Mathf.Lerp(2.5f, 4f, (float)random.NextDouble());
            instance.transform.localScale *= scale;

            placedPositions.Add(point);
            spawned++;
        }
    }

    /// Finds the chunk containing a world position.
    /// Creates the chunk if it does not already exist.
    private Transform GetVegetationChunk(float x, float z) {
        Vector2Int chunkCoordinate = new Vector2Int(Mathf.FloorToInt(x / chunkSize), Mathf.FloorToInt(z / chunkSize));

        if (vegetationChunks.TryGetValue(chunkCoordinate, out Transform existingChunk)) {
            return existingChunk;
        }

        GameObject chunk = new GameObject($"Chunk_{chunkCoordinate.x}_{chunkCoordinate.y}");
        chunk.transform.SetParent(chunkContainer, false);

        vegetationChunks.Add(chunkCoordinate, chunk.transform);

        return chunk.transform;
    }

    /// Returns a separate parent for trees or smaller environment objects
    /// within the same vegetation chunk.
    private Transform GetVegetationParent(float x, float z, bool isTree) {
        Transform chunk = GetVegetationChunk(x, z);
        string folderName = isTree ? "Trees" : "Details";

        Transform folder = chunk.Find(folderName);

        if (folder == null) {
            GameObject group = new GameObject(folderName);
            group.transform.SetParent(chunk, false);
            folder = group.transform;
        }

        return folder;
    }

    /// Enables nearby vegetation chunks and disables distant chunks.
    /// Distance is measured to each chunk's closest edge, not its centre.
    private void UpdateChunkVisibility() {
        if (player == null || chunkContainer == null) {
            return;
        }

        float viewDistanceSqr = chunkRenderDistance * chunkRenderDistance;

        foreach (KeyValuePair<Vector2Int, Transform> entry in vegetationChunks) {
            Vector2Int coordinate = entry.Key;
            Transform chunk = entry.Value;

            if (chunk == null) {
                continue;
            }

            float minX = coordinate.x * chunkSize;
            float minZ = coordinate.y * chunkSize;

            float closestX = Mathf.Clamp(player.position.x, minX, minX + chunkSize);
            float closestZ = Mathf.Clamp(player.position.z, minZ, minZ + chunkSize);

            float dx = player.position.x - closestX;
            float dz = player.position.z - closestZ;

            bool shouldBeActive = dx * dx + dz * dz <= viewDistanceSqr;

            if (chunk.gameObject.activeSelf != shouldBeActive) {
                chunk.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    /// Classifies a Plains chunk using seeded Perlin noise.
    /// Every position inside the same chunk receives the same environment type.
    /// 0 = Open Plains, 1 = Woodland, 2 = Dense Forest.
    private int GetPlainsChunkType(float x, float z) {
        float size = Mathf.Max(1f, chunkSize);

        float centreX = (Mathf.Floor(x / size) + 0.5f) * size;
        float centreZ = (Mathf.Floor(z / size) + 0.5f) * size;

        float noise = Mathf.PerlinNoise(centreX * forestNoiseScale + biomeGenerator.seed * 0.137f + 173f, centreZ * forestNoiseScale + biomeGenerator.seed * 0.293f + 291f);

        if (noise >= 0.55f)
        {
            return 2;
        }

        if (noise >= 0.42f)
        {
            return 1;
        }

        return 0;
    }

    /// Returns the number of trees to attempt in each Plains chunk.
    /// Dense forests receive more trees, while open areas remain sparse.
    private int GetPlainsChunkTreeBudget(float x, float z) {
        int chunkType = GetPlainsChunkType(x, z);

        switch (chunkType) {
            case 2: {
                    return 110;
                }
            case 1: {
                    return 50;
                }
            default: {
                    return 12;
                }
        }
    }
}

