using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralEnvironmentSpawner : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private GameObject[] plainsTreePrefabs;

    [Header("Plains Environment Prefabs")]
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] bushPrefabs;

    [Header("Environment Object Counts")]
    [SerializeField] private int rockCount = 250;
    [SerializeField] private int bushCount = 500;

    [SerializeField] private float environmentMinimumSpacing = 6f;

    [Header("Plains Trees")]
    [SerializeField] private int plainsTreeCount = 900;
    [SerializeField] private float maximumSlope = 30f;
    [SerializeField] private float kingdomExclusionRadius = 350f;

    [Header("Forest Generation")]
    [SerializeField] private float forestNoiseScale = 0.0018f;
    [SerializeField] private float forestThreshold = 0.52f;
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
    [SerializeField] private int snowTreeCount = 450;
    [SerializeField] private int snowRockCount = 250;
    [SerializeField] private float snowMaximumSlope = 28f;
    [SerializeField] private float snowTreeLineHeight = 550f;
    [SerializeField] private float snowForestNoiseScale = 0.0016f;
    [SerializeField] private float snowForestThreshold = 0.52f;
    [SerializeField] private float snowTreeSpacing = 14f;
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
            Vector3 size = terrain.terrainData.size;

            minX = Mathf.Min(minX, position.x);
            minZ = Mathf.Min(minZ, position.z);
            maxX = Mathf.Max(maxX, position.x + size.x);
            maxZ = Mathf.Max(maxZ, position.z + size.z);
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

        List<Vector2> placedPositions = new List<Vector2>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = plainsTreeCount * 100;

        while (spawned < plainsTreeCount && attempts < maxAttempts) {
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
                Vector3 size = terrain.terrainData.size;

                if (x >= origin.x && x <= origin.x + size.x &&
                    z >= origin.z && z <= origin.z + size.z)
                {
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

            GameObject tree = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, container.transform);

            tree.transform.Rotate(0f, (float)random.NextDouble() * 360f, 0f);

            float scale = Mathf.Lerp(0.85f, 1.2f, (float)random.NextDouble());
            tree.transform.localScale *= scale;

            placedPositions.Add(point);
            spawned++;
        }

        SpawnEnvironmentObjects(container.transform, terrains, worldCentre, random, rockPrefabs, rockCount, 0.25f, environmentMinimumSpacing);

        SpawnEnvironmentObjects(container.transform, terrains, worldCentre, random, bushPrefabs, bushCount, 0.85f, environmentMinimumSpacing);

        SpawnSnowEnvironment(container.transform, terrains, worldCentre);

        SpawnDesertEnvironment(container.transform, terrains, worldCentre);
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

            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);

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

            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);

            float scale = Mathf.Lerp(0.85f, 1.25f, (float)random.NextDouble());
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

            GameObject instance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);

            float scale = Mathf.Lerp(2.5f, 4f, (float)random.NextDouble());
            instance.transform.localScale *= scale;

            placedPositions.Add(point);
            spawned++;
        }
    }



}

