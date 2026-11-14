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
    private List<Vector2> plainsTreePositions = new List<Vector2>();

    [Header("Plains Environment Prefabs")]
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] bushPrefabs;

    [Header("Environment Object Counts")]
    [SerializeField] private int rockCount = 250;
    [SerializeField] private int bushCount = 500;

    [SerializeField] private float environmentMinimumSpacing = 6f;

    [Header("Plains Trees")]
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
    [SerializeField] private int snowRockCount = 250;
    [SerializeField] private float snowMaximumSlope = 28f;
    [SerializeField] private float snowTreeLineHeight = 550f;
    [SerializeField] private float snowForestNoiseScale = 0.0016f;
    [SerializeField] private float snowForestThreshold = 0.45f;
    [SerializeField] private float snowTreeSpacing = 10f;
    [SerializeField] private float snowRockSpacing = 9f;
    private List<Vector2> snowTreePositions = new List<Vector2>();

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

    [Header("Swamp Reeds")]
    [SerializeField] private GameObject[] swampReedPrefabs;
    [SerializeField] private int swampReedCount = 450;
    [SerializeField] private float swampReedSpacing = 4f;
    [SerializeField] private float swampReedMinWaterMask = 0.2f;
    [SerializeField] private float swampReedMaxWaterMask = 0.75f;
    [SerializeField] private float swampReedScaleMin = 8f;
    [SerializeField] private float swampReedScaleMax = 12f;

    [Header("Swamp Trees")]
    [SerializeField] private GameObject[] swampTreePrefabs;
    [SerializeField] private int swampTreeCount = 350;
    [SerializeField] private float swampTreeSpacing = 22f;
    [SerializeField] private float swampTreeScaleMin = 2.5f;
    [SerializeField] private float swampTreeScaleMax = 4.5f;

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
    public void GenerateVegetation() {
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
        plainsTreePositions.Clear();

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

                    float combinedDensity = Mathf.Max(forestDensity, woodlandMask * woodlandDensity);
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

                    float scale = Mathf.Lerp(0.8f, 2.8f, (float)random.NextDouble());
                    float heightMultiplier = Mathf.Lerp(0.85f, 1.4f, (float)random.NextDouble());

                    tree.transform.localScale = Vector3.Scale(tree.transform.localScale, new Vector3(scale, scale * heightMultiplier, scale));

                    placedPositions.Add(point);
                    plainsTreePositions.Add(point);
                    chunkSpawned++;
                }
            }
        }

        /// Generate the remaining vegetation using the existing biome rules.
        SpawnEnvironmentObjects(container.transform, terrains, worldCentre, random, rockPrefabs, rockCount, 0.25f, environmentMinimumSpacing);
        SpawnEnvironmentObjects(container.transform, terrains, worldCentre, random, bushPrefabs, bushCount, 0.85f, environmentMinimumSpacing);

        SpawnSnowEnvironment(container.transform, terrains, worldCentre);
        SpawnDesertEnvironment(container.transform, terrains, worldCentre);

        GenerateSwampReeds(terrains, container.transform, random, worldCentre, minX, maxX, minZ, maxZ);
        GenerateSwampTrees(terrains, container.transform, random, worldCentre, minX, maxX, minZ, maxZ);

        UpdateChunkVisibility();
    }



    /// Spawns Plains rocks and bushes in clusters rather than
    /// scattering individual objects across the whole biome.
    /// Bush clusters favour forests; rock clusters favour open and hilly areas.
    private void SpawnEnvironmentObjects(Transform parent, Terrain[] terrains, Vector2 worldCentre, System.Random random, GameObject[] prefabs, int targetCount, float forestPreference, float minimumSpacing) {
        if (prefabs == null || prefabs.Length == 0 || targetCount <= 0) {
            return;
        }

        bool isBush = forestPreference > 0.5f;

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + terrainSize.x);
            maxZ = Mathf.Max(maxZ, origin.z + terrainSize.z);
        }

        int spawned = 0;
        int clusterAttempts = 0;
        int maxClusterAttempts = targetCount * 20;

        while (spawned < targetCount && clusterAttempts < maxClusterAttempts) {
            clusterAttempts++;

            float centreX = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float centreZ = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

            Vector2 clusterCentre = new Vector2(centreX, centreZ);

            float centreDistance = Vector2.Distance(clusterCentre, worldCentre);

            if (centreDistance < kingdomExclusionRadius || centreDistance > 1900f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(centreX, centreZ, out float snow, out float plains, out float desert, out float swamp);

            if (plains < 0.85f || biomeGenerator.IsInsidePlainsBossArena(centreX, centreZ)) {
                continue;
            }

            int chunkType = GetPlainsChunkType(centreX, centreZ);

            float clusterChance;

            if (isBush) {
                clusterChance = chunkType == 2 ? 0.9f : chunkType == 1 ? 0.55f : 0.05f;
            }
            else {
                clusterChance = chunkType == 0 ? 0.9f : chunkType == 1 ? 0.5f : 0.15f;
            }

            if ((float)random.NextDouble() > clusterChance) {
                continue;
            }

            int clusterSize = isBush ? random.Next(5, 13) : random.Next(2, 13);
            float clusterRadius = isBush ? 18f : 28f;

            List<Vector2> clusterPositions = new List<Vector2>();

            for (int i = 0; i < clusterSize && spawned < targetCount; i++) {
                bool foundPosition = false;
                Vector2 point = Vector2.zero;
                Terrain currentTerrain = null;
                float y = 0f;

                for (int attempt = 0; attempt < 20; attempt++) {
                    float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                    float distance = Mathf.Sqrt((float)random.NextDouble()) * clusterRadius;

                    float x = centreX + Mathf.Cos(angle) * distance;
                    float z = centreZ + Mathf.Sin(angle) * distance;

                    point = new Vector2(x, z);
                    if (!isBush) {
                        bool nearTree = false;

                        foreach (Vector2 treePosition in plainsTreePositions) {
                            if (Vector2.Distance(point, treePosition) < 12f) {
                                nearTree = true;
                                break;
                            }
                        }

                        if (nearTree) {
                            continue;
                        }
                    }

                    float distanceFromWorldCentre = Vector2.Distance(point, worldCentre);

                    if (distanceFromWorldCentre < kingdomExclusionRadius || distanceFromWorldCentre > 1950f) {
                        continue;
                    }

                    biomeGenerator.GetBiomeBlend(x, z, out float pointSnow, out float pointPlains, out float pointDesert, out float pointSwamp);

                    if (pointPlains < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
                        continue;
                    }

                    currentTerrain = null;

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

                    if (isBush && slope > 15f) {
                        continue;
                    }

                    if (!isBush && slope > 35f) {
                        continue;
                    }

                    bool tooClose = false;

                    foreach (Vector2 placed in clusterPositions) {
                        if (Vector2.Distance(point, placed) < minimumSpacing) {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) {
                        continue;
                    }

                    if (!isBush) {
                        float slopeChance = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0f, 25f, slope));

                        if ((float)random.NextDouble() > slopeChance) {
                            continue;
                        }
                    }

                    y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;
                    foundPosition = true;
                    break;
                }

                if (!foundPosition) {
                    continue;
                }

                GameObject prefab = prefabs[random.Next(prefabs.Length)];

                if (prefab == null) {
                    continue;
                }

                GameObject instance = Instantiate(prefab, new Vector3(point.x, y, point.y), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(point.x, point.y, false));

                float scale = Mathf.Lerp(0.8f, 1.4f, (float)random.NextDouble());
                instance.transform.localScale *= scale;

                clusterPositions.Add(point);
                spawned++;
            }
        }
    }

    /// Generates Snow pine forests using individual chunk budgets.
    /// Snow rocks continue using their existing spawning rules.
    private void SpawnSnowEnvironment(Transform parent, Terrain[] terrains, Vector2 worldCentre) {
        SpawnSnowTreesByChunk(terrains, worldCentre);

        SpawnSnowRockClusters(terrains, worldCentre);
    }


    /// Creates small groups of Snow rocks rather than scattering them randomly.
    /// Rocky slopes have higher spawn rate, open snowfields remain clear,
    /// and rocks avoid pine trunks and the Kingdom.
    private void SpawnSnowRockClusters(Terrain[] terrains, Vector2 worldCentre) {
        if (snowRockPrefabs == null || snowRockPrefabs.Length == 0 || snowRockCount <= 0) {
            return;
        }

        System.Random random = new System.Random(biomeGenerator.seed + 9427);

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 candidateSize = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + candidateSize.x);
            maxZ = Mathf.Max(maxZ, origin.z + candidateSize.z);
        }

        List<Vector2> clusterCentres = new List<Vector2>();
        List<Vector2> rockPositions = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = snowRockCount * 40;

        while (spawned < snowRockCount && attempts < maxAttempts) {
            attempts++;

            float centreX = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float centreZ = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

            Vector2 clusterCentre = new Vector2(centreX, centreZ);

            float worldDistance = Vector2.Distance(clusterCentre, worldCentre);

            if (worldDistance < kingdomExclusionRadius || worldDistance > 1900f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(centreX, centreZ, out float snow, out float plains, out float desert, out float swamp);

            if (snow < 0.85f) {
                continue;
            }

            bool tooCloseToCluster = false;

            foreach (Vector2 existingCentre in clusterCentres) {
                if (Vector2.Distance(clusterCentre, existingCentre) < 65f) {
                    tooCloseToCluster = true;
                    break;
                }
            }

            if (tooCloseToCluster) {
                continue;
            }

            Terrain centreTerrain = null;

            foreach (Terrain terrain in terrains) {
                Vector3 origin = terrain.transform.position;
                Vector3 candidateSize = terrain.terrainData.size;

                if (centreX >= origin.x && centreX <= origin.x + candidateSize.x && centreZ >= origin.z && centreZ <= origin.z + candidateSize.z) {
                    centreTerrain = terrain;
                    break;
                }
            }

            if (centreTerrain == null) {
                continue;
            }

            Vector3 centreOrigin = centreTerrain.transform.position;
            Vector3 centreTerrainSize = centreTerrain.terrainData.size;

            float centreU = Mathf.Clamp01((centreX - centreOrigin.x) / centreTerrainSize.x);
            float centreV = Mathf.Clamp01((centreZ - centreOrigin.z) / centreTerrainSize.z);
            float centreSlope = centreTerrain.terrainData.GetSteepness(centreU, centreV);

            float clusterChance = Mathf.Lerp(0.12f, 0.9f, Mathf.InverseLerp(3f, 30f, centreSlope));

            if ((float)random.NextDouble() > clusterChance) {
                continue;
            }

            int clusterSize = random.Next(2, 9);
            float clusterRadius = Mathf.Lerp(12f, 30f, (float)random.NextDouble());

            clusterCentres.Add(clusterCentre);

            for (int i = 0; i < clusterSize && spawned < snowRockCount; i++) {
                for (int placementAttempt = 0; placementAttempt < 25; placementAttempt++) {
                    float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                    float distance = Mathf.Sqrt((float)random.NextDouble()) * clusterRadius;

                    float x = centreX + Mathf.Cos(angle) * distance;
                    float z = centreZ + Mathf.Sin(angle) * distance;

                    Vector2 point = new Vector2(x, z);

                    if (Vector2.Distance(point, worldCentre) < kingdomExclusionRadius || Vector2.Distance(point, worldCentre) > 1950f) {
                        continue;
                    }

                    biomeGenerator.GetBiomeBlend(x, z, out float pointSnow, out float pointPlains, out float pointDesert, out float pointSwamp);

                    if (pointSnow < 0.85f) {
                        continue;
                    }

                    bool tooClose = false;

                    foreach (Vector2 rockPosition in rockPositions) {
                        if (Vector2.Distance(point, rockPosition) < snowRockSpacing) {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) {
                        continue;
                    }

                    foreach (Vector2 treePosition in snowTreePositions) {
                        if (Vector2.Distance(point, treePosition) < 12f) {
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

                    if (slope > 42f) {
                        continue;
                    }

                    float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;

                    GameObject prefab = snowRockPrefabs[random.Next(snowRockPrefabs.Length)];

                    if (prefab == null) {
                        continue;
                    }

                    GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(x, z, false));

                    float scale = Mathf.Lerp(0.8f, 1.8f, (float)random.NextDouble());
                    instance.transform.localScale *= scale;

                    rockPositions.Add(point);
                    spawned++;
                    break;
                }
            }
        }
    }

    /// Generates pine trees separately within each Snow chunk.
    /// Forest noise determines the tree limit for each region.
    /// Placement respects biome boundaries, slopes, treeline and Kingdom clearing.
    private void SpawnSnowTreesByChunk(Terrain[] terrains, Vector2 worldCentre) {
        snowTreePositions.Clear();
        if (snowTreePrefabs == null || snowTreePrefabs.Length == 0) {
            return;
        }

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + terrainSize.x);
            maxZ = Mathf.Max(maxZ, origin.z + terrainSize.z);
        }

        float size = Mathf.Max(1f, chunkSize);

        int minChunkX = Mathf.FloorToInt(minX / size);
        int maxChunkX = Mathf.CeilToInt(maxX / size);
        int minChunkZ = Mathf.FloorToInt(minZ / size);
        int maxChunkZ = Mathf.CeilToInt(maxZ / size);

        List<Vector2> placedPositions = new List<Vector2>();

        for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++) {
            for (int chunkZ = minChunkZ; chunkZ < maxChunkZ; chunkZ++) {
                float startX = Mathf.Max(minX, chunkX * size);
                float endX = Mathf.Min(maxX, (chunkX + 1) * size);
                float startZ = Mathf.Max(minZ, chunkZ * size);
                float endZ = Mathf.Min(maxZ, (chunkZ + 1) * size);

                float centreX = (startX + endX) * 0.5f;
                float centreZ = (startZ + endZ) * 0.5f;

                bool containsSnow = false;

                for (int sample = 0; sample < 5; sample++) {
                    float sampleX = sample == 0 ? centreX : sample == 1 || sample == 3 ? Mathf.Lerp(startX, endX, 0.15f) : Mathf.Lerp(startX, endX, 0.85f);
                    float sampleZ = sample == 0 ? centreZ : sample == 1 || sample == 2 ? Mathf.Lerp(startZ, endZ, 0.15f) : Mathf.Lerp(startZ, endZ, 0.85f);

                    biomeGenerator.GetBiomeBlend(sampleX, sampleZ, out float snow, out float plains, out float desert, out float swamp);

                    if (snow >= 0.85f) {
                        containsSnow = true;
                        break;
                    }
                }

                if (!containsSnow) {
                    continue;
                }

                float forestNoise = Mathf.PerlinNoise(centreX * snowForestNoiseScale + biomeGenerator.seed * 0.131f + 241f, centreZ * snowForestNoiseScale + biomeGenerator.seed * 0.217f + 617f);

                float forestDensity = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(snowForestThreshold - 0.1f, snowForestThreshold + 0.2f, forestNoise));
                int targetTrees = Mathf.RoundToInt(Mathf.Lerp(3f, 85f, forestDensity));

                System.Random random = new System.Random(unchecked(biomeGenerator.seed + chunkX * 73856093 + chunkZ * 19349663 + 9427));

                int chunkSpawned = 0;
                int attempts = 0;
                int maxAttempts = targetTrees * 60;

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

                    if (snow < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
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
                    float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;
                    float elevation = y - terrainOrigin.y;

                    if (slope > snowMaximumSlope || elevation > snowTreeLineHeight) {
                        continue;
                    }

                    bool tooClose = false;

                    foreach (Vector2 placed in placedPositions) {
                        if (Vector2.Distance(point, placed) < snowTreeSpacing) {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) {
                        continue;
                    }

                    GameObject prefab = snowTreePrefabs[random.Next(snowTreePrefabs.Length)];

                    if (prefab == null) {
                        continue;
                    }

                    GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(x, z, true));

                    float scale;
                    float heightMultiplier;

                    if (random.NextDouble() < 0.03) {
                        scale = Mathf.Lerp(5f, 7f, (float)random.NextDouble());
                        heightMultiplier = Mathf.Lerp(1.3f, 1.7f, (float)random.NextDouble());
                    }
                    else {
                        scale = Mathf.Lerp(1.2f, 4f, (float)random.NextDouble());
                        heightMultiplier = Mathf.Lerp(1f, 1.6f, (float)random.NextDouble());
                    }

                    instance.transform.localScale = Vector3.Scale(instance.transform.localScale, new Vector3(scale, scale * heightMultiplier, scale));

                    placedPositions.Add(point);
                    snowTreePositions.Add(point);
                    chunkSpawned++;
                }
            }
        }
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
        SpawnDesertCactusClusters(terrains, worldCentre);
        SpawnDesertObjects(parent, terrains, worldCentre, desertBushPrefabs, desertBushCount, desertBushSpacing, 1);
        SpawnDesertObjects(parent, terrains, worldCentre, desertRockPrefabs, desertRockCount, desertRockSpacing, 2);
    }


    /// Generates actual cactus groups within Desert chunks.
    /// Each chunk has a noise based cactus amount, creating patches
    /// of vegetation separated by large areas of open sand.
    private void SpawnDesertCactusClusters(Terrain[] terrains, Vector2 worldCentre) {
        if (cactusPrefabs == null || cactusPrefabs.Length == 0 || cactusCount <= 0) {
            return;
        }

        System.Random random = new System.Random(biomeGenerator.seed + 12037);

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 candidateSize = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + candidateSize.x);
            maxZ = Mathf.Max(maxZ, origin.z + candidateSize.z);
        }

        float size = Mathf.Max(1f, chunkSize);

        int minChunkX = Mathf.FloorToInt(minX / size);
        int maxChunkX = Mathf.CeilToInt(maxX / size);
        int minChunkZ = Mathf.FloorToInt(minZ / size);
        int maxChunkZ = Mathf.CeilToInt(maxZ / size);

        List<Vector2> cactusPositions = new List<Vector2>();
        int totalSpawned = 0;

        for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++) {
            for (int chunkZ = minChunkZ; chunkZ < maxChunkZ; chunkZ++) {
                float startX = Mathf.Max(minX, chunkX * size);
                float endX = Mathf.Min(maxX, (chunkX + 1) * size);
                float startZ = Mathf.Max(minZ, chunkZ * size);
                float endZ = Mathf.Min(maxZ, (chunkZ + 1) * size);

                if (startX >= endX || startZ >= endZ) {
                    continue;
                }

                float centreX = (startX + endX) * 0.5f;
                float centreZ = (startZ + endZ) * 0.5f;

                float noise = Mathf.PerlinNoise(centreX * desertNoiseScale + biomeGenerator.seed * 0.137f + 173f, centreZ * desertNoiseScale + biomeGenerator.seed * 0.293f + 291f);

                float density = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.72f, noise));

                int clusters = random.Next(0, 5);

                if ((float)random.NextDouble() > Mathf.Lerp(0.15f, 0.9f, density)) {
                    clusters = 0;
                }

                for (int cluster = 0; cluster < clusters && totalSpawned < cactusCount; cluster++) {
                    float groupX = Mathf.Lerp(startX, endX, (float)random.NextDouble());
                    float groupZ = Mathf.Lerp(startZ, endZ, (float)random.NextDouble());

                    biomeGenerator.GetBiomeBlend(groupX, groupZ, out float snow, out float plains, out float desert, out float swamp);

                    if (desert < 0.85f) {
                        continue;
                    }

                    int groupSize = random.Next(1, 12);
                    float groupRadius = Mathf.Lerp(8f, 45f, (float)random.NextDouble());

                    for (int i = 0; i < groupSize && totalSpawned < cactusCount; i++) {
                        for (int attempt = 0; attempt < 20; attempt++) {
                            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                            float distance = Mathf.Pow((float)random.NextDouble(), 1.5f) * groupRadius;

                            float x = groupX + Mathf.Cos(angle) * distance;
                            float z = groupZ + Mathf.Sin(angle) * distance;

                            if (x < startX || x >= endX || z < startZ || z >= endZ) {
                                continue;
                            }

                            Vector2 point = new Vector2(x, z);
                            float centreDistance = Vector2.Distance(point, worldCentre);

                            if (centreDistance < kingdomExclusionRadius || centreDistance > 1950f) {
                                continue;
                            }

                            biomeGenerator.GetBiomeBlend(x, z, out float pointSnow, out float pointPlains, out float pointDesert, out float pointSwamp);

                            if (pointDesert < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
                                continue;
                            }

                            bool tooClose = false;

                            foreach (Vector2 placed in cactusPositions) {
                                if (Vector2.Distance(point, placed) < cactusSpacing) {
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

                            if (slope > desertMaximumSlope) {
                                continue;
                            }

                            float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;

                            GameObject prefab = cactusPrefabs[random.Next(cactusPrefabs.Length)];

                            if (prefab == null) {
                                continue;
                            }

                            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), GetVegetationParent(x, z, false));

                            float scale = Mathf.Lerp(2.5f, 4f, (float)random.NextDouble());
                            instance.transform.localScale *= scale;

                            cactusPositions.Add(point);
                            totalSpawned++;
                            break;
                        }
                    }
                }
            }
        }
    }


    /// Spawns Desert bushes and rocks in natural clusters.
    /// Bushes favour scrubland patches, while rocks favour slopes.
    /// Keeps large areas of open sand between clusters.
    private void SpawnDesertObjects(Transform parent, Terrain[] terrains, Vector2 worldCentre, GameObject[] prefabs, int targetCount, float minimumSpacing, int objectType) {
        if (prefabs == null || prefabs.Length == 0 || targetCount <= 0) {
            return;
        }

        bool isBush = objectType == 1;
        System.Random random = new System.Random(biomeGenerator.seed + 12037 + objectType * 7919);

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 candidateSize = terrain.terrainData.size;

            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + candidateSize.x);
            maxZ = Mathf.Max(maxZ, origin.z + candidateSize.z);
        }

        List<Vector2> clusterCentres = new List<Vector2>();
        List<Vector2> placedPositions = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 40;

        while (spawned < targetCount && attempts < maxAttempts) {
            attempts++;

            float centreX = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float centreZ = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

            Vector2 clusterCentre = new Vector2(centreX, centreZ);
            float centreDistance = Vector2.Distance(clusterCentre, worldCentre);

            if (centreDistance < kingdomExclusionRadius || centreDistance > 1900f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(centreX, centreZ, out float snow, out float plains, out float desert, out float swamp);

            if (desert < 0.85f || biomeGenerator.IsInsidePlainsBossArena(centreX, centreZ)) {
                continue;
            }

            bool nearCluster = false;

            foreach (Vector2 existingCentre in clusterCentres) {
                if (Vector2.Distance(clusterCentre, existingCentre) < (isBush ? 95f : 130f)) {
                    nearCluster = true;
                    break;
                }
            }

            if (nearCluster) {
                continue;
            }

            Terrain centreTerrain = null;

            foreach (Terrain terrain in terrains) {
                Vector3 origin = terrain.transform.position;
                Vector3 candidateSize = terrain.terrainData.size;

                if (centreX >= origin.x && centreX <= origin.x + candidateSize.x && centreZ >= origin.z && centreZ <= origin.z + candidateSize.z) {
                    centreTerrain = terrain;
                    break;
                }
            }

            if (centreTerrain == null) {
                continue;
            }

            Vector3 centreOrigin = centreTerrain.transform.position;
            Vector3 centreSize = centreTerrain.terrainData.size;

            float centreU = Mathf.Clamp01((centreX - centreOrigin.x) / centreSize.x);
            float centreV = Mathf.Clamp01((centreZ - centreOrigin.z) / centreSize.z);
            float centreSlope = centreTerrain.terrainData.GetSteepness(centreU, centreV);

            float regionNoise = Mathf.PerlinNoise(centreX * desertNoiseScale + biomeGenerator.seed * 0.137f + 173f, centreZ * desertNoiseScale + biomeGenerator.seed * 0.293f + 291f);
            float scrubDensity = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(desertClusterThreshold - 0.12f, desertClusterThreshold + 0.15f, regionNoise));

            float patchNoise = Mathf.PerlinNoise(centreX * 0.0018f + biomeGenerator.seed * 0.17f, centreZ * 0.0018f + biomeGenerator.seed * 0.23f);

            float clusterChance = isBush ? Mathf.Lerp(0.02f, 0.75f, scrubDensity) : Mathf.Lerp(0.08f, 0.85f, Mathf.InverseLerp(2f, 25f, centreSlope));

            clusterChance *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.3f, 0.7f, patchNoise));

            if ((float)random.NextDouble() > clusterChance) {
                continue;
            }

            int clusterSize = isBush ? random.Next(1, 13) : random.Next(1, 10);
            float clusterRadius = isBush ? Mathf.Lerp(8f, 40f, (float)random.NextDouble()) : Mathf.Lerp(10f, 55f, (float)random.NextDouble());

            int clusterSpawned = 0;

            for (int i = 0; i < clusterSize && spawned < targetCount; i++) {
                for (int placementAttempt = 0; placementAttempt < 25; placementAttempt++) {
                    float angle = (float)random.NextDouble() * Mathf.PI * 2f;

                    float distance = Mathf.Pow((float)random.NextDouble(), 1.5f) * clusterRadius;

                    float stretchX = Mathf.Lerp(0.6f, 1.5f, (float)random.NextDouble());
                    float stretchZ = Mathf.Lerp(0.6f, 1.5f, (float)random.NextDouble());

                    float x = centreX + Mathf.Cos(angle) * distance * stretchX;
                    float z = centreZ + Mathf.Sin(angle) * distance * stretchZ;

                    Vector2 point = new Vector2(x, z);
                    float worldDistance = Vector2.Distance(point, worldCentre);

                    if (worldDistance < kingdomExclusionRadius || worldDistance > 1950f) {
                        continue;
                    }

                    biomeGenerator.GetBiomeBlend(x, z, out float pointSnow, out float pointPlains, out float pointDesert, out float pointSwamp);

                    if (pointDesert < 0.85f || biomeGenerator.IsInsidePlainsBossArena(x, z)) {
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

                    if (isBush && slope > 18f) {
                        continue;
                    }

                    if (!isBush && slope > 38f) {
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
                    clusterSpawned++;
                    break;
                }
            }

            if (clusterSpawned > 0) {
                clusterCentres.Add(clusterCentre);
            }
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

    /// Calculates a gradual tree amount for each Plains chunk.
    /// Neighbouring chunks tend to have similar chunks as
    /// the amount follows the continuous forest noise pattern.
    private int GetPlainsChunkTreeBudget(float x, float z) {
        float noise = Mathf.PerlinNoise(x * forestNoiseScale + biomeGenerator.seed * 0.137f + 173f, z * forestNoiseScale + biomeGenerator.seed * 0.293f + 291f);

        float density = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 0.65f, noise));

        return Mathf.RoundToInt(Mathf.Lerp(8f, 110f, density));
    }

    /// Spawns reeds around swamp water edges so the swamp feels wetter
    /// and more natural without covering the whole biome uniformly.
    private void GenerateSwampReeds(Terrain[] terrains, Transform parent, System.Random random, Vector2 worldCentre, float minX, float maxX, float minZ, float maxZ) {
        if (swampReedPrefabs == null || swampReedPrefabs.Length == 0) {
            return;
        }

        List<Vector2> placed = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = swampReedCount * 60;

        while (spawned < swampReedCount && attempts < maxAttempts) {
            attempts++;

            float x = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float z = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());
            Vector2 point = new Vector2(x, z);

            if (Vector2.Distance(point, worldCentre) < kingdomExclusionRadius) {
                continue;
            }

            if (Vector2.Distance(point, worldCentre) > 1950f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            if (swamp < 0.8f) {
                continue;
            }

            float waterMask = biomeGenerator.GetSwampWaterMask(x, z);

            if (waterMask < swampReedMinWaterMask || waterMask > swampReedMaxWaterMask) {
                continue;
            }

            bool tooClose = false;

            foreach (Vector2 p in placed) {
                if (Vector2.Distance(point, p) < swampReedSpacing) {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) {
                continue;
            }

            Terrain currentTerrain = GetTerrainAtPosition(terrains, x, z);

            if (currentTerrain == null) {
                continue;
            }

            Vector3 terrainOrigin = currentTerrain.transform.position;
            Vector3 terrainSize = currentTerrain.terrainData.size;

            float u = Mathf.Clamp01((x - terrainOrigin.x) / terrainSize.x);
            float v = Mathf.Clamp01((z - terrainOrigin.z) / terrainSize.z);

            if (currentTerrain.terrainData.GetSteepness(u, v) > maximumSlope) {
                continue;
            }

            float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y + 0.3f;

            GameObject prefab = swampReedPrefabs[random.Next(swampReedPrefabs.Length)];

            if (prefab == null) {
                continue;
            }

            GameObject reed = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, parent);
            reed.transform.Rotate(0f, (float)random.NextDouble() * 360f, 0f);

            float scale = Mathf.Lerp(swampReedScaleMin, swampReedScaleMax, (float)random.NextDouble());
            reed.transform.localScale *= scale;

            placed.Add(point);
            spawned++;
        }
    }

    /// Spawns swamp trees on wet ground and shallow water areas
    /// to create a Moisty Mire style swamp feel.
    private void GenerateSwampTrees(Terrain[] terrains, Transform parent, System.Random random, Vector2 worldCentre, float minX, float maxX, float minZ, float maxZ) {
        if (swampTreePrefabs == null || swampTreePrefabs.Length == 0) {
            return;
        }

        List<Vector2> placed = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = swampTreeCount * 80;

        while (spawned < swampTreeCount && attempts < maxAttempts) {
            attempts++;

            float x = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
            float z = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());
            Vector2 point = new Vector2(x, z);

            if (Vector2.Distance(point, worldCentre) < kingdomExclusionRadius) {
                continue;
            }

            if (Vector2.Distance(point, worldCentre) > 1950f) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            if (swamp < 0.82f) {
                continue;
            }

            float waterMask = biomeGenerator.GetSwampWaterMask(x, z);


            float waterSpawnChance = Mathf.Lerp(0.95f, 0.3f, waterMask);

            if ((float)random.NextDouble() > waterSpawnChance) {
                continue;
            }

            float clusterNoise = Mathf.PerlinNoise(x * 0.003f + biomeGenerator.seed * 0.19f + 211f, z * 0.003f + biomeGenerator.seed * 0.27f + 509f);

            float treeSpawnChance = Mathf.Lerp(0.2f, 0.85f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.3f, 0.7f, clusterNoise)));

            if ((float)random.NextDouble() > treeSpawnChance) {
                continue;
            }

            bool tooClose = false;

            foreach (Vector2 p in placed) {
                if (Vector2.Distance(point, p) < swampTreeSpacing) {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) {
                continue;
            }

            Terrain currentTerrain = GetTerrainAtPosition(terrains, x, z);

            if (currentTerrain == null) {
                continue;
            }

            Vector3 terrainOrigin = currentTerrain.transform.position;
            Vector3 terrainSize = currentTerrain.terrainData.size;

            float u = Mathf.Clamp01((x - terrainOrigin.x) / terrainSize.x);
            float v = Mathf.Clamp01((z - terrainOrigin.z) / terrainSize.z);

            if (currentTerrain.terrainData.GetSteepness(u, v) > maximumSlope) {
                continue;
            }

            float y = currentTerrain.SampleHeight(new Vector3(x, 0f, z)) + terrainOrigin.y;

            GameObject prefab = swampTreePrefabs[random.Next(swampTreePrefabs.Length)];

            if (prefab == null) {
                continue;
            }

            GameObject tree = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, parent);
            tree.transform.Rotate(0f, (float)random.NextDouble() * 360f, 0f);

            float scale = Mathf.Lerp(swampTreeScaleMin, swampTreeScaleMax, (float)random.NextDouble());
            tree.transform.localScale *= scale;

            placed.Add(point);
            spawned++;
        }
    }

    private Terrain GetTerrainAtPosition(Terrain[] terrains, float x, float z) {
        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            if (x >= origin.x && x <= origin.x + terrainSize.x && z >= origin.z && z <= origin.z + terrainSize.z) {
                return terrain;
            }
        }

        return null;
    }
}

