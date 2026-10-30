using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWorldGenerator : MonoBehaviour {
    [Header("Four Terrain Tiles")]
    public Terrain snowTerrain;
    public Terrain plainsTerrain;
    public Terrain desertTerrain;
    public Terrain swampTerrain;

    [Header("World Settings")]
    public int seed = 12345;
    public float biomeTransitionWidth = 120f;

    [Header("Kingdom Clearing")]
    public float kingdomRadius = 80f;
    public float kingdomBlendWidth = 100f;
    public float kingdomHeight = 8f;

    private enum Biome
    {
        Snow,
        Plains,
        Desert,
        Swamp
    }

    [ContextMenu("Generate World")]
    public void GenerateWorld() {
        Terrain[] terrains =
        { snowTerrain, plainsTerrain, desertTerrain, swampTerrain };

        foreach (Terrain terrain in terrains) {
            if (terrain == null || terrain.terrainData == null) {
                return;
            }
        }

        for (int i = 0; i < terrains.Length; i++) {
            for (int j = i + 1; j < terrains.Length; j++) {
                if (terrains[i] == terrains[j] || terrains[i].terrainData == terrains[j].terrainData) {
                    return;
                }
            }
        }

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        float baseY = terrains[0].transform.position.y;
        Vector3 tileSize = terrains[0].terrainData.size;

        foreach (Terrain terrain in terrains) {
            Vector3 position = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            if (Mathf.Abs(position.y - baseY) > 0.01f || Vector3.Distance(size, tileSize) > 0.01f ||
                terrain.terrainData.heightmapResolution != terrains[0].terrainData.heightmapResolution) {
                return;
            }

            minX = Mathf.Min(minX, position.x);
            minZ = Mathf.Min(minZ, position.z);
            maxX = Mathf.Max(maxX, position.x + size.x);
            maxZ = Mathf.Max(maxZ, position.z + size.z);
        }

        float centreX = (minX + maxX) * 0.5f;
        float centreZ = (minZ + maxZ) * 0.5f;

        Biome[] shuffled = { Biome.Snow, Biome.Plains, Biome.Desert, Biome.Swamp };

        System.Random random = new System.Random(seed);

        for (int i = shuffled.Length - 1; i > 0; i--) {
            int j = random.Next(i + 1);

            Biome temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        float offsetX = random.Next(1000, 100000);
        float offsetZ = random.Next(1000, 100000);

        Terrain[] quadrantTerrains = new Terrain[4];

        foreach (Terrain terrain in terrains) {
            Vector3 p = terrain.transform.position;
            Vector3 s = terrain.terrainData.size;

            bool east = p.x + s.x * 0.5f > centreX;
            bool north = p.z + s.z * 0.5f > centreZ;

            int quadrant = north ? (east ? 1 : 0) : (east ? 3 : 2);

            if (quadrantTerrains[quadrant] != null) {
                return;
            }

            quadrantTerrains[quadrant] = terrain;
        }

        foreach (Terrain terrain in quadrantTerrains) {
            if (terrain == null)
            {
                return;
            }
        }

        foreach (Terrain terrain in quadrantTerrains) {
            TerrainData data = terrain.terrainData;
            int resolution = data.heightmapResolution;

            float[,] heights = new float[resolution, resolution];

            Vector3 origin = terrain.transform.position;
            Vector3 size = data.size;

            for (int z = 0; z < resolution; z++) {
                for (int x = 0; x < resolution; x++) {
                    float worldX = origin.x + x / (float)(resolution - 1) * size.x;

                    float worldZ = origin.z + z / (float)(resolution - 1) * size.z;

                    float snow = GetBiomeHeight(Biome.Snow, worldX, worldZ, offsetX, offsetZ);

                    float plains = GetBiomeHeight(Biome.Plains, worldX, worldZ, offsetX, offsetZ);

                    float desert = GetBiomeHeight(Biome.Desert, worldX, worldZ, offsetX, offsetZ);

                    float swamp = GetBiomeHeight(Biome.Swamp, worldX, worldZ, offsetX, offsetZ);

                    float[] biomeHeights = { snow, plains, desert, swamp };

                    float eastBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(centreX - biomeTransitionWidth, centreX + biomeTransitionWidth, worldX));

                    float northBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(centreZ - biomeTransitionWidth, centreZ + biomeTransitionWidth, worldZ));

                    float nw = (1f - eastBlend) * northBlend;
                    float ne = eastBlend * northBlend;
                    float sw = (1f - eastBlend) * (1f - northBlend);
                    float se = eastBlend * (1f - northBlend);

                    float height = biomeHeights[(int)shuffled[0]] * nw + biomeHeights[(int)shuffled[1]] * ne + biomeHeights[(int)shuffled[2]] * sw + biomeHeights[(int)shuffled[3]] * se;

                    float distance = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(centreX, centreZ));

                    float kingdomBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(kingdomRadius, kingdomRadius + kingdomBlendWidth, distance));

                    height = Mathf.Lerp(kingdomHeight, height, kingdomBlend);

                    heights[z, x] = Mathf.Clamp01(height / size.y);
                }
            }

            data.SetHeights(0, 0, heights);
        }
    }

    private float GetBiomeHeight(Biome biome, float x, float z, float offsetX, float offsetZ) {
        float large = Mathf.PerlinNoise(x * 0.0025f + offsetX, z * 0.0025f + offsetZ);

        float medium = Mathf.PerlinNoise(x * 0.009f + offsetX + 173f, z * 0.009f + offsetZ + 291f);

        float detail = Mathf.PerlinNoise(x * 0.028f + offsetX + 431f, z * 0.028f + offsetZ + 619f);

        switch (biome) {
            case Biome.Snow: {
                    float mountainZone = Mathf.PerlinNoise(x * 0.0016f + offsetX + 827f, z * 0.0016f + offsetZ + 391f);

                    float mountainMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.68f, mountainZone));

                    float hillsNoise = Mathf.PerlinNoise(x * 0.0055f + offsetX + 51f, z * 0.0055f + offsetZ + 73f);

                    float hills = hillsNoise * 18f;

                    float ridgeNoise1 = Mathf.PerlinNoise(x * 0.0028f + offsetX + 173f, z * 0.0028f + offsetZ + 291f);

                    float ridge1 = 1f - Mathf.Abs(ridgeNoise1 * 2f - 1f);
                    ridge1 = Mathf.Pow(ridge1, 2.2f) * 280f;

                    float ridgeNoise2 = Mathf.PerlinNoise(x * 0.0042f + offsetX + 611f, z * 0.0042f + offsetZ + 147f);

                    float ridge2 = 1f - Mathf.Abs(ridgeNoise2 * 2f - 1f);
                    ridge2 = Mathf.Pow(ridge2, 3.0f) * 140f;

                    float detailNoise = Mathf.PerlinNoise(x * 0.014f + offsetX + 431f, z * 0.014f + offsetZ + 619f);

                    float mountainDetail = detailNoise * 20f;

                    float baseUndulationNoise = Mathf.PerlinNoise(x * 0.009f + offsetX + 900f, z * 0.009f + offsetZ + 240f);

                    float baseUndulation = baseUndulationNoise * 8f;

                    float lowlands = 10f + hills + baseUndulation;
                    float mountains = 28f + ridge1 + ridge2 + mountainDetail;

                    return Mathf.Lerp(lowlands, mountains, mountainMask);
                }


            case Biome.Plains:
                return 7f + large * 12f + medium * 4f;

            case Biome.Desert:
                return 9f + large * 15f + medium * 11f + detail * 3f;

            case Biome.Swamp:
                return 5f + large * 3f + medium * 1.5f;

            default:
                return 0f;
        }
    }
}


