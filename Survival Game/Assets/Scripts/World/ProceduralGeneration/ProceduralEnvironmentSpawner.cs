using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralEnvironmentSpawner : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private GameObject[] plainsTreePrefabs;

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
    }
}

