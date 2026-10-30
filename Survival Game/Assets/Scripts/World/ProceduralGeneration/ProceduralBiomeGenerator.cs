using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWorldGenerator : MonoBehaviour {
    [Header("Four Connected Terrains")]
    public Terrain snowTerrain;
    public Terrain plainsTerrain;
    public Terrain desertTerrain;
    public Terrain swampTerrain;

    [Header("Generation Settings")]
    public int seed = 12345;
    public float noiseScale = 0.008f;
    public float detailScale = 0.035f;

    [Header("Biome Heights (World Units)")]
    public float snowHeight = 65f;
    public float plainsHeight = 12f;
    public float desertHeight = 22f;
    public float swampHeight = 4f;

    [Header("Future Kingdom Clearing")]
    public float kingdomRadius = 60f;
    public float kingdomFlatHeight = 8f;

    [ContextMenu("Generate World")]
    public void GenerateWorld() {
        if (snowTerrain == null || plainsTerrain == null || desertTerrain == null || swampTerrain == null) {
            return;
        }

        Terrain[] terrains = { snowTerrain, plainsTerrain, desertTerrain, swampTerrain };

        float baseY = terrains[0].transform.position.y;

        foreach (Terrain terrain in terrains) {
            if (Mathf.Abs(terrain.transform.position.y - baseY) > 0.01f) {
                return;
            }

            foreach (Terrain other in terrains) {
                if (other != terrain && other.terrainData == terrain.terrainData) {
                    return;
                }
            }
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

        float centreX = (minX + maxX) * 0.5f;
        float centreZ = (minZ + maxZ) * 0.5f;

        System.Random random = new System.Random(seed);
        float offsetX = random.Next(1000, 100000);
        float offsetZ = random.Next(1000, 100000);

        foreach (Terrain terrain in terrains) {
            TerrainData data = terrain.terrainData;
            int resolution = data.heightmapResolution;
            float[,] heights = new float[resolution, resolution];

            Vector3 origin = terrain.transform.position;
            Vector3 size = data.size;

            for (int z = 0; z < resolution; z++) {
                for (int x = 0; x < resolution; x++) {
                    float worldX = origin.x + x / (float)(resolution - 1) * size.x;

                    float worldZ = origin.z + z / (float)(resolution - 1) * size.z;

                    float largeNoise = Mathf.PerlinNoise(worldX * noiseScale + offsetX, worldZ * noiseScale + offsetZ);

                    float smallNoise = Mathf.PerlinNoise(worldX * detailScale + offsetX + 137f, worldZ * detailScale + offsetZ + 271f);

                    float nx = (worldX - centreX) / Mathf.Max(1f, (maxX - minX) * 0.5f);

                    float nz = (worldZ - centreZ) / Mathf.Max(1f, (maxZ - minZ) * 0.5f);

                    float snowWeight = Mathf.Exp(nz * 7f);
                    float plainsWeight = Mathf.Exp(-nx * 7f);
                    float desertWeight = Mathf.Exp(nx * 7f);
                    float swampWeight = Mathf.Exp(-nz * 7f);

                    float total = snowWeight + plainsWeight + desertWeight + swampWeight;

                    float amplitude = (snowWeight * snowHeight + plainsWeight * plainsHeight + desertWeight * desertHeight + swampWeight * swampHeight) / total;

                    float worldHeight = largeNoise * amplitude + (smallNoise - 0.5f) * amplitude * 0.18f;

                    float distance = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(centreX, centreZ));

                    float blend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distance - kingdomRadius) / Mathf.Max(1f, kingdomRadius * 0.5f)));

                    worldHeight = Mathf.Lerp(kingdomFlatHeight, worldHeight, blend);

                    heights[z, x] = Mathf.Clamp01(worldHeight / size.y);
                }
            }

            data.SetHeights(0, 0, heights);
        }
    }
}

