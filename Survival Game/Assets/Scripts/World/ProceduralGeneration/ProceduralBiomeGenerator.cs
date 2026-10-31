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

    private enum Biome {
        Snow,
        Plains,
        Desert,
        Swamp
    }
    private struct MountainPeak {
        public Vector2 position;
        public float radius;
        public float height;

        public MountainPeak(float u, float v, float radius, float height) {
            this.position = new Vector2(u, v);
            this.radius = radius;
            this.height = height;
        }
    }

    private static readonly MountainPeak[] MountainPeaks = {
        new MountainPeak(0.55f, 0.45f, 0.20f, 600f),
        new MountainPeak(0.75f, 0.75f, 0.24f, 850f),
        new MountainPeak(0.85f, 0.40f, 0.18f, 700f),
        new MountainPeak(0.50f, 0.85f, 0.15f, 420f),
        new MountainPeak(0.92f, 0.92f, 0.12f, 300f),
    };

    private const int NoiseOffsetRange = 1000;

    private const float SnowLowlandBase = 18f;
    private const float SnowLowlandNoiseHeight = 30f;
    private const float SnowLowlandHillHeight = 12f;
    private const float SnowDetailAmplitude = 45f;
    private const float SnowDetailFullStrengthHeight = 250f;

    private const float PeakBlendSharpness = 40f;  
    private const float PeakBodyPower = 1.5f;       
    private const float PeakWarpFrequency = 0.0025f;
    private const float PeakWarpStrength = 120f;
    private const float PeakMinRadiusScale = 0.75f;
    private const float PeakMaxRadiusScale = 1.35f;
    private const float PeakAngularNoiseScale = 1.7f;

    private const float RidgeBaseFrequency = 0.004f;
    private const int RidgeOctaves = 4;
    private const float RidgeLacunarity = 2.03f;
    private const float RidgeGain = 0.5f;
    private const float RidgeMinScale = 0.5f;
    private const float RidgeMaxScale = 1.3f;

    private const float MountainBorderFadeWidth = 150f;

    private float noiseOffsetX;
    private float noiseOffsetZ;
    private Vector2 worldCentre;
    private Vector2 snowDirection;
    private Vector3 tileExtent;

    [ContextMenu("Generate World")]

    public void GenerateWorld() {
        Terrain[] terrains = { snowTerrain, plainsTerrain, desertTerrain, swampTerrain };

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
        int heightmapResolution = terrains[0].terrainData.heightmapResolution;

        foreach (Terrain terrain in terrains) {
            Vector3 position = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            if (Mathf.Abs(position.y - baseY) > 0.01f || Vector3.Distance(size, tileSize) > 0.01f ||
                terrain.terrainData.heightmapResolution != heightmapResolution) {
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

        noiseOffsetX = random.Next(0, NoiseOffsetRange);
        noiseOffsetZ = random.Next(0, NoiseOffsetRange);

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
            if (terrain == null) {
                return;
            }
        }

        for (int i = 0; i < quadrantTerrains.Length; i++) {
            quadrantTerrains[i].gameObject.name = shuffled[i] + "Terrain";
        }
        int snowIndex = System.Array.IndexOf(shuffled, Biome.Snow);

        worldCentre = new Vector2(centreX, centreZ);
        snowDirection = GetQuadrantDirection(snowIndex);
        tileExtent = tileSize;

        float maxSnowHeight = GetMaxSnowHeight();

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

                    float snow = GetBiomeHeight(Biome.Snow, worldX, worldZ);
                    float plains = GetBiomeHeight(Biome.Plains, worldX, worldZ);
                    float desert = GetBiomeHeight(Biome.Desert, worldX, worldZ);
                    float swamp = GetBiomeHeight(Biome.Swamp, worldX, worldZ);

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



    private float GetBiomeHeight(Biome biome, float x, float z) {
        switch (biome) {
            case Biome.Snow:
                return GetSnowHeight(x, z);


            case Biome.Plains: {
                    float largeNoise = SampleNoise(x, z, 0.0012f, 173f, 291f);
                    float mediumNoise = SampleNoise(x, z, 0.0035f, 431f, 619f);
                    float smallNoise = SampleNoise(x, z, 0.010f, 157f, 337f);

                    float baseTerrain = 12f + largeNoise * 25f;

                    float rollingHills = Mathf.SmoothStep(0f, 1f, mediumNoise) * 22f;

                    float hillRegionNoise = SampleNoise(x, z, 0.0018f, 761f, 123f);

                    float hillMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.52f, 0.75f, hillRegionNoise));

                    float largerHillNoise = SampleNoise(x, z, 0.0025f, 547f, 839f);

                    float largerHills = Mathf.Pow(largerHillNoise, 1.8f) * 55f * hillMask;

                    float gentleDetails = (smallNoise - 0.5f) * 5f;

                    float valleys = (1f - hillMask) * rollingHills;

                    return baseTerrain + valleys + largerHills + gentleDetails;
                }


            case Biome.Desert: {
                    float largeNoise = SampleNoise(x, z, 0.0008f, 247f, 581f);
                    float mediumNoise = SampleNoise(x, z, 0.0018f, 613f, 827f);
                    float smallNoise = SampleNoise(x, z, 0.004f, 137f, 419f);

                    float baseTerrain = 15f + largeNoise * 45f;

                    float duneRegionNoise = SampleNoise(x, z, 0.001f, 379f, 941f);

                    float duneMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.32f, 0.70f, duneRegionNoise));

                    float largeDunes = Mathf.SmoothStep(0f, 1f, mediumNoise) * 140f;
                    float smallerDunes = Mathf.SmoothStep(0f, 1f, smallNoise) * 12f;

                    float rollingHills = largeNoise * 30f + mediumNoise * 18f;

                    float valleys = (1f - duneMask) * rollingHills;
                    float dunes = (largeDunes + smallerDunes) * duneMask;

                    return baseTerrain + valleys + dunes;
                }

            case Biome.Swamp: {
                    float largeNoise = SampleNoise(x, z, 0.0015f, 193f, 487f);
                    float mediumNoise = SampleNoise(x, z, 0.006f, 557f, 821f);
                    float detailNoise = SampleNoise(x, z, 0.02f, 311f, 671f);

                    float lowlands = 9f + largeNoise * 7f + mediumNoise * 3f;

                    float depressionNoise = SampleNoise(x, z, 0.0035f, 853f, 149f);

                    float depressionMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.65f, depressionNoise));

                    float depressions = depressionMask * 7f;

                    float raisedGroundNoise = SampleNoise(x, z, 0.0045f, 467f, 739f);

                    float raisedGroundMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.58f, 0.78f, raisedGroundNoise));

                    float raisedGround = raisedGroundMask * 8f;

                    float smallDetails = (detailNoise - 0.5f) * 2f;

                    return Mathf.Max(2f, lowlands - depressions + raisedGround + smallDetails);
                }

            default:
                return 0f;
        }
    }

    private float GetSnowHeight(float x, float z) {
        float lowlandNoise = SampleNoise(x, z, 0.003f, 51f, 73f);
        float hillNoise = SampleNoise(x, z, 0.008f, 91f, 143f);
        float lowlands = SnowLowlandBase + lowlandNoise * SnowLowlandNoiseHeight + hillNoise * SnowLowlandHillHeight;

        float mountains = GetMountainHeight(x, z);

        float detailNoise = SampleNoise(x, z, 0.012f, 431f, 619f);
        float detailStrength = Mathf.Clamp01(mountains / SnowDetailFullStrengthHeight);
        float mountainDetail = (detailNoise - 0.5f) * SnowDetailAmplitude * detailStrength;

        return lowlands + mountains + mountainDetail;
    }


    private float GetMountainHeight(float x, float z) {
        float ridges = GetRidgedNoise(x, z);

        float mountains = 0f;

        foreach (MountainPeak peak in MountainPeaks) {
            mountains = SmoothMax(mountains, GetPeakHeight(x, z, peak, ridges), PeakBlendSharpness);
        }

        return mountains * GetBorderFade(x, z);
    }


    private float GetPeakHeight(float x, float z, MountainPeak peak, float ridges) {
        Vector2 centre = GetPeakWorldPosition(peak);
        float radius = peak.radius * tileExtent.x;

        float warpX = (SampleNoise(x, z, PeakWarpFrequency, 117f, 219f) - 0.5f) * PeakWarpStrength;
        float warpZ = (SampleNoise(x, z, PeakWarpFrequency, 337f, 419f) - 0.5f) * PeakWarpStrength;

        float dx = x + warpX - centre.x;
        float dz = z + warpZ - centre.y;

        float angle = Mathf.Atan2(dz, dx);
        float angularNoise = Mathf.PerlinNoise(Mathf.Cos(angle) * PeakAngularNoiseScale + noiseOffsetX + peak.position.x * 10f, Mathf.Sin(angle) * PeakAngularNoiseScale + noiseOffsetZ + peak.position.y * 10f);

        float irregularRadius = radius * Mathf.Lerp(PeakMinRadiusScale, PeakMaxRadiusScale, angularNoise);
        float normalizedDistance = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dz * dz) / irregularRadius);

        float foundation = 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);
        float body = Mathf.Pow(foundation, PeakBodyPower);

        return peak.height * body * Mathf.Lerp(RidgeMinScale, RidgeMaxScale, ridges);
    }

    private float GetRidgedNoise(float x, float z) {
        float sum = 0f;
        float totalAmplitude = 0f;
        float amplitude = 1f;
        float frequency = RidgeBaseFrequency;

        for (int octave = 0; octave < RidgeOctaves; octave++) {
            float n = SampleNoise(x, z, frequency, octave * 137f, octave * 211f);

            n = 1f - Mathf.Abs(n * 2f - 1f);
            n *= n;

            sum += n * amplitude;
            totalAmplitude += amplitude;

            amplitude *= RidgeGain;
            frequency *= RidgeLacunarity;
        }

        return sum / totalAmplitude;
    }

    private float GetBorderFade(float x, float z) {
        float distanceToBorder = Mathf.Min(Mathf.Abs(x - worldCentre.x), Mathf.Abs(z - worldCentre.y));

        float fade = Mathf.InverseLerp(biomeTransitionWidth, biomeTransitionWidth + MountainBorderFadeWidth, distanceToBorder);

        return Mathf.SmoothStep(0f, 1f, fade);
    }


    private Vector2 GetPeakWorldPosition(MountainPeak peak) {
        return new Vector2(worldCentre.x + snowDirection.x * peak.position.x * tileExtent.x, worldCentre.y + snowDirection.y * peak.position.y * tileExtent.z);
    }


    private float GetMaxSnowHeight() {
        float tallestPeak = 0f;

        foreach (MountainPeak peak in MountainPeaks) {
            tallestPeak = Mathf.Max(tallestPeak, peak.height);
        }

        float maxLowlands = SnowLowlandBase + SnowLowlandNoiseHeight + SnowLowlandHillHeight;

        return maxLowlands + tallestPeak * RidgeMaxScale + SnowDetailAmplitude * 0.5f + PeakBlendSharpness;
    }

    private float SampleNoise(float x, float z, float frequency, float offsetA, float offsetB) {
        return Mathf.PerlinNoise(x * frequency + noiseOffsetX + offsetA, z * frequency + noiseOffsetZ + offsetB);
    }


    private static float SmoothMax(float a, float b, float k) {
        float h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
        return Mathf.Lerp(a, b, h) + k * h * (1f - h);
    }

    private static Vector2 GetQuadrantDirection(int quadrant) {
        float xSign = (quadrant == 1 || quadrant == 3) ? 1f : -1f;
        float zSign = (quadrant == 0 || quadrant == 1) ? 1f : -1f;

        return new Vector2(xSign, zSign);
    }
}