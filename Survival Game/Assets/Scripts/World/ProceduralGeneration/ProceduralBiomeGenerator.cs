using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralBiomeGenerator : MonoBehaviour {
    [Header("Four Terrain Tiles")]
    public Terrain snowTerrain;
    public Terrain plainsTerrain;
    public Terrain desertTerrain;
    public Terrain swampTerrain;

    [Header("World Settings")]
    public int seed = 12345;

    [Header("Kingdom Clearing")]
    public float kingdomRadius = 80f;
    public float kingdomBlendWidth = 100f;
    public float kingdomHeight = 8f;

    [Header("Biome Terrain Layers")]
    public TerrainLayer snowLayer;
    public TerrainLayer plainsLayer;
    public TerrainLayer desertLayer;
    public TerrainLayer swampLayer;

    [Header("Radial Biome Generation")]
    [Range(1f, 40f)] public float biomeAngleBlend = 12f;
    [Range(0f, 45f)] public float biomeAngleWarp = 25f;
    public float biomeAngleNoiseScale = 0.0018f;
    [Range(0f, 20f)] public float biomeAngleDetail = 8f;
    public float biomeAngleDetailScale = 0.005f;

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
    private const float BiomeWarpFrequency = 0.003f;

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

    private const float PlainsLargeFrequency = 0.0012f;
    private const float PlainsMediumFrequency = 0.0035f;
    private const float PlainsSmallFrequency = 0.010f;
    private const float PlainsBase = 12f;
    private const float PlainsBaseVariation = 25f;
    private const float PlainsRollingHillHeight = 22f;
    private const float PlainsHillRegionFrequency = 0.0018f;
    private const float PlainsHillRegionStart = 0.52f;
    private const float PlainsHillRegionEnd = 0.75f;
    private const float PlainsLargerHillFrequency = 0.0025f;
    private const float PlainsLargerHillPower = 1.8f;
    private const float PlainsLargerHillHeight = 55f;
    private const float PlainsDetailAmplitude = 5f;
    private const float PlainsWarpStrength = 80f;
    private const float PlainsBorderFadeWidth = 200f;

    private const float DesertLargeFrequency = 0.0007f;
    private const float DesertBase = 15f;
    private const float DesertBaseVariation = 60f;
    private const float DesertMinHeight = 4f;
    private const float DesertFloorSoftness = 8f;
    private const float DesertBorderFadeWidth = 260f;

    private const float DesertHillFrequency = 0.0009f;
    private const int DesertHillOctaves = 3;
    private const float DesertHillGain = 0.5f;
    private const float DesertHillLacunarity = 2.1f;
    private const float DesertHillContrastLow = 0.2f;
    private const float DesertHillContrastHigh = 0.8f;
    private const float DesertHillWarpStrength = 140f;
    private const float DesertHillHeight = 100f;

    private const float DesertBasinFrequency = 0.002f;
    private const float DesertBasinStart = 0.55f;
    private const float DesertBasinEnd = 0.95f;
    private const float DesertBasinDepth = 22f;

    private const float DuneRegionFrequency = 0.0008f;
    private const float DuneRegionStart = 0.28f;
    private const float DuneRegionEnd = 0.62f;
    private const float DuneLargeFrequency = 0.0012f;
    private const float DuneLargeHeight = 120f;
    private const float DuneSmallFrequency = 0.0032f;
    private const float DuneSmallHeight = 22f;
    private const float DuneCrestFloor = 0.3f;
    private const float DuneSharpness = 1.2f;
    private const float DuneStretch = 0.35f;
    private const float DuneWarpStrength = 200f;
    private const float DuneWindAngleDegrees = 35f;

    private static readonly Vector2 DuneWindDirection = new Vector2(Mathf.Cos(DuneWindAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(DuneWindAngleDegrees * Mathf.Deg2Rad));

    private const float SwampLargeFrequency = 0.0015f;
    private const float SwampMediumFrequency = 0.006f;
    private const float SwampDetailFrequency = 0.02f;
    private const float SwampBase = 9f;
    private const float SwampLargeHeight = 7f;
    private const float SwampMediumHeight = 3f;
    private const float SwampDepressionFrequency = 0.0035f;
    private const float SwampDepressionStart = 0.42f;
    private const float SwampDepressionEnd = 0.65f;
    private const float SwampDepressionDepth = 7f;
    private const float SwampRaisedGroundFrequency = 0.0045f;
    private const float SwampRaisedGroundStart = 0.58f;
    private const float SwampRaisedGroundEnd = 0.78f;
    private const float SwampRaisedGroundHeight = 8f;
    private const float SwampDetailAmplitude = 2f;
    private const float SwampMinHeight = 2f;
    private const float SwampWarpStrength = 60f;
    private const float SwampCreekFrequency = 0.004f;
    private const float SwampCreekInnerWidth = 0.01f;
    private const float SwampCreekOuterWidth = 0.06f;
    private const float SwampCreekDepth = 5f;

    private float noiseOffsetX;
    private float noiseOffsetZ;
    private Vector2 worldCentre;
    private Vector2 snowDirection;
    private Vector3 tileExtent;

    /// Generates matching terrain heights and biome textures across four tiles.
    /// Shuffles the radial biome sectors using the seed and preserves the Kingdom clearing.
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

            if (Mathf.Abs(position.y - baseY) > 0.01f || Vector3.Distance(size, tileSize) > 0.01f || terrain.terrainData.heightmapResolution != heightmapResolution) {
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

        int snowIndex = System.Array.IndexOf(shuffled, Biome.Snow);

        worldCentre = new Vector2(centreX, centreZ);
        snowDirection = GetSectorDirection(snowIndex);
        tileExtent = tileSize;

        if (snowLayer == null || plainsLayer == null || desertLayer == null || swampLayer == null) {
            return;
        }

        float maxSnowHeight = GetMaxSnowHeight();

        if (maxSnowHeight > tileSize.y) {
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

                    float snow = GetBiomeHeight(Biome.Snow, worldX, worldZ);
                    float plains = GetBiomeHeight(Biome.Plains, worldX, worldZ);
                    float desert = GetBiomeHeight(Biome.Desert, worldX, worldZ);
                    float swamp = GetBiomeHeight(Biome.Swamp, worldX, worldZ);

                    float[] biomeHeights = { snow, plains, desert, swamp };

                    GetBiomeWeights(worldX, worldZ, centreX, centreZ, out float nw, out float ne, out float sw, out float se);

                    float height = biomeHeights[(int)shuffled[0]] * nw + biomeHeights[(int)shuffled[1]] * ne + biomeHeights[(int)shuffled[2]] * sw + biomeHeights[(int)shuffled[3]] * se;

                    float distance = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(centreX, centreZ));

                    float kingdomBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(kingdomRadius, kingdomRadius + kingdomBlendWidth, distance));

                    height = Mathf.Lerp(kingdomHeight, height, kingdomBlend);

                    heights[z, x] = Mathf.Clamp01(height / size.y);
                }
            }

            data.SetHeights(0, 0, heights);
            PaintBiomeTextures(terrain, shuffled, centreX, centreZ);
        }
    }


    /// Calculates biome weights using four pie shaped regions around the Kingdom.
    /// Seeded noise bends their shared boundaries without creating square divisions.
    /// The same weights are used for terrain heights and terrain textures.
    private void GetBiomeWeights(float x, float z, float centreX, float centreZ, out float nw, out float ne, out float sw, out float se) {
        float dx = x - centreX;
        float dz = z - centreZ;
        float distance = Mathf.Sqrt(dx * dx + dz * dz);
        float angle = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;

        float warp = (SampleNoise(x, z, biomeAngleNoiseScale, 143f, 287f) - 0.5f) * 2f * biomeAngleWarp;
        float detail = (SampleNoise(x, z, biomeAngleDetailScale, 619f, 911f) - 0.5f) * 2f * biomeAngleDetail;
        float centreFade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(kingdomRadius, kingdomRadius + kingdomBlendWidth + 100f, distance));

        angle += (warp + detail) * centreFade;

        nw = GetSectorWeight(angle, 90f);
        ne = GetSectorWeight(angle, 0f);
        sw = GetSectorWeight(angle, 180f);
        se = GetSectorWeight(angle, 270f);

        float total = nw + ne + sw + se;

        nw /= total;
        ne /= total;
        sw /= total;
        se /= total;
    }

    /// Smoothly blends neighbouring radial sectors along their angular borders.
    /// Each sector covers approximately one quarter of the world.
    private float GetSectorWeight(float angle, float sectorAngle) {
        float angularDistance = Mathf.Abs(Mathf.DeltaAngle(angle, sectorAngle));
        return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(45f - biomeAngleBlend, 45f + biomeAngleBlend, angularDistance));
    }

    /// Paints the terrain using the same radial weights as height generation.
    /// Keeps biome textures aligned with the curved boundaries across all tiles.
    private void PaintBiomeTextures(Terrain terrain, Biome[] shuffled, float centreX, float centreZ) {
        TerrainData data = terrain.terrainData;

        data.terrainLayers = new TerrainLayer[] { snowLayer, plainsLayer, desertLayer, swampLayer };

        int width = data.alphamapWidth;
        int height = data.alphamapHeight;

        float[,,] splatmap = new float[height, width, 4];

        Vector3 origin = terrain.transform.position;
        Vector3 size = data.size;

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                float worldX = origin.x + x / (float)(width - 1) * size.x;
                float worldZ = origin.z + z / (float)(height - 1) * size.z;

                GetBiomeWeights(worldX, worldZ, centreX, centreZ, out float nw, out float ne, out float sw, out float se);

                splatmap[z, x, (int)shuffled[0]] += nw;
                splatmap[z, x, (int)shuffled[1]] += ne;
                splatmap[z, x, (int)shuffled[2]] += sw;
                splatmap[z, x, (int)shuffled[3]] += se;
            }
        }

        data.SetAlphamaps(0, 0, splatmap);
    }

    private float GetBiomeHeight(Biome biome, float x, float z) {
        switch (biome) {
            case Biome.Snow:
                return GetSnowHeight(x, z);

            case Biome.Plains:
                return GetPlainsHeight(x, z);

            case Biome.Desert:
                return GetDesertHeight(x, z);

            case Biome.Swamp:
                return GetSwampHeight(x, z);

            default:
                return 0f;
        }
    }

    private float GetPlainsHeight(float x, float z) {
        Vector2 p = GetWarpedPosition(x, z, PlainsWarpStrength);

        float largeNoise = SampleNoise(p.x, p.y, PlainsLargeFrequency, 173f, 291f);
        float mediumNoise = SampleNoise(p.x, p.y, PlainsMediumFrequency, 431f, 619f);
        float smallNoise = SampleNoise(p.x, p.y, PlainsSmallFrequency, 157f, 337f);

        float baseTerrain = PlainsBase + largeNoise * PlainsBaseVariation;

        float rollingHills = Mathf.SmoothStep(0f, 1f, mediumNoise) * PlainsRollingHillHeight;

        float hillRegionNoise = SampleNoise(p.x, p.y, PlainsHillRegionFrequency, 761f, 123f);
        float hillMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(PlainsHillRegionStart, PlainsHillRegionEnd, hillRegionNoise));

        float borderFade = GetBiomeBorderFade(x, z, PlainsBorderFadeWidth);
        float largerHillNoise = SampleNoise(p.x, p.y, PlainsLargerHillFrequency, 547f, 839f);
        float largerHills = Mathf.Pow(largerHillNoise, PlainsLargerHillPower) * PlainsLargerHillHeight * hillMask * borderFade;

        float gentleDetails = (smallNoise - 0.5f) * PlainsDetailAmplitude;

        float valleys = (1f - hillMask) * rollingHills;

        return baseTerrain + valleys + largerHills + gentleDetails;
    }

    private float GetDesertHeight(float x, float z) {
        float largeNoise = SampleNoise(x, z, DesertLargeFrequency, 247f, 581f);
        float baseTerrain = DesertBase + largeNoise * DesertBaseVariation;

        float borderFade = GetBiomeBorderFade(x, z, DesertBorderFadeWidth);

        float hills = GetDesertHills(x, z) * DesertHillHeight * borderFade;
        float basins = GetDesertBasins(x, z) * DesertBasinDepth * borderFade;

        float duneRegionNoise = SampleNoise(x, z, DuneRegionFrequency, 379f, 941f);
        float duneMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(DuneRegionStart, DuneRegionEnd, duneRegionNoise));

        float dunes = 0f;

        if (duneMask > 0f) {
            dunes = GetDuneHeight(x, z) * duneMask * borderFade;
        }

        return SmoothMax(DesertMinHeight, baseTerrain + hills - basins + dunes, DesertFloorSoftness);
    }
    private float GetDesertHills(float x, float z) {
        Vector2 p = GetWarpedPosition(x, z, DesertHillWarpStrength);

        float sum = 0f;
        float totalAmplitude = 0f;
        float amplitude = 1f;
        float frequency = DesertHillFrequency;

        for (int octave = 0; octave < DesertHillOctaves; octave++) {
            sum += SampleNoise(p.x, p.y, frequency, 29f + octave * 173f, 47f + octave * 311f) * amplitude;
            totalAmplitude += amplitude;

            amplitude *= DesertHillGain;
            frequency *= DesertHillLacunarity;
        }

        float hills = Mathf.InverseLerp(DesertHillContrastLow, DesertHillContrastHigh, sum / totalAmplitude);

        return Mathf.SmoothStep(0f, 1f, hills);
    }

    private float GetDesertBasins(float x, float z) {
        float basinNoise = SampleNoise(x, z, DesertBasinFrequency, 233f, 877f);

        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(DesertBasinStart, DesertBasinEnd, basinNoise));
    }


    private float GetDuneHeight(float x, float z) {
        float largeDunes = GetDuneLayer(x, z, DuneLargeFrequency, 613f, 827f) * DuneLargeHeight;
        float smallerDunes = GetDuneLayer(x, z, DuneSmallFrequency, 137f, 419f) * DuneSmallHeight;

        return largeDunes + smallerDunes;
    }

    private float GetDuneLayer(float x, float z, float frequency, float offsetA, float offsetB) {
        Vector2 p = GetWarpedPosition(x, z, DuneWarpStrength);

        float acrossWind = p.x * DuneWindDirection.x + p.y * DuneWindDirection.y;
        float alongCrest = -p.x * DuneWindDirection.y + p.y * DuneWindDirection.x;

        float noise = Mathf.PerlinNoise(acrossWind * frequency + noiseOffsetX + offsetA, alongCrest * frequency * DuneStretch + noiseOffsetZ + offsetB);

        float crest = 1f - Mathf.Abs(noise * 2f - 1f);
        crest = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(DuneCrestFloor, 1f, crest));

        return Mathf.Pow(crest, DuneSharpness);
    }

    private float GetSwampHeight(float x, float z) {
        Vector2 p = GetWarpedPosition(x, z, SwampWarpStrength);

        float largeNoise = SampleNoise(p.x, p.y, SwampLargeFrequency, 193f, 487f);
        float mediumNoise = SampleNoise(p.x, p.y, SwampMediumFrequency, 557f, 821f);
        float detailNoise = SampleNoise(p.x, p.y, SwampDetailFrequency, 311f, 671f);

        float lowlands = SwampBase + largeNoise * SwampLargeHeight + mediumNoise * SwampMediumHeight;

        float depressionNoise = SampleNoise(p.x, p.y, SwampDepressionFrequency, 853f, 149f);
        float depressionMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(SwampDepressionStart, SwampDepressionEnd, depressionNoise));
        float depressions = depressionMask * SwampDepressionDepth;

        float raisedGroundNoise = SampleNoise(p.x, p.y, SwampRaisedGroundFrequency, 467f, 739f);
        float raisedGroundMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(SwampRaisedGroundStart, SwampRaisedGroundEnd, raisedGroundNoise));
        float raisedGround = raisedGroundMask * SwampRaisedGroundHeight;

        float smallDetails = (detailNoise - 0.5f) * SwampDetailAmplitude;

        float creeks = GetSwampCreekMask(p.x, p.y) * SwampCreekDepth;

        return Mathf.Max(SwampMinHeight, lowlands - depressions + raisedGround + smallDetails - creeks);
    }

    private float GetSwampCreekMask(float x, float z) {
        float creekNoise = SampleNoise(x, z, SwampCreekFrequency, 613f, 727f);
        float distanceFromCentre = Mathf.Abs(creekNoise - 0.5f);

        return 1f - Mathf.SmoothStep(SwampCreekInnerWidth, SwampCreekOuterWidth, distanceFromCentre);
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

        for (int i = 0; i < RidgeOctaves; i++) {
            float n = SampleNoise(x, z, frequency, i * 137f, i * 211f);

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
        return GetBiomeBorderFade(x, z, MountainBorderFadeWidth);
    }


    private Vector2 GetPeakWorldPosition(MountainPeak peak) {
        float lateral = (peak.position.x - 0.5f) * tileExtent.x * 1.5f;
        float outward = peak.position.y * tileExtent.z;
        Vector2 perpendicular = new Vector2(snowDirection.y, -snowDirection.x);

        return worldCentre + snowDirection * outward + perpendicular * lateral;
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


    private Vector2 GetWarpedPosition(float x, float z, float strength) {
        float warpX = (SampleNoise(x, z, BiomeWarpFrequency, 151f, 263f) - 0.5f) * strength;
        float warpZ = (SampleNoise(x, z, BiomeWarpFrequency, 487f, 599f) - 0.5f) * strength;

        return new Vector2(x + warpX, z + warpZ);
    }

    /// Reduces large hills and mountains close to the central Kingdom.
    /// Radial fading avoids straight cuts along the old quadrant boundaries.
    private float GetBiomeBorderFade(float x, float z, float fadeWidth) {
        float distance = Vector2.Distance(new Vector2(x, z), worldCentre);
        float fade = Mathf.InverseLerp(kingdomRadius + kingdomBlendWidth, kingdomRadius + kingdomBlendWidth + fadeWidth, distance);

        return Mathf.SmoothStep(0f, 1f, fade);
    }


    private static float SmoothMax(float a, float b, float k) {
        float h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
        return Mathf.Lerp(a, b, h) + k * h * (1f - h);
    }

    /// Returns the outward direction of the radial sector containing Snow.
    /// Sector order matches the original shuffled biome indices.
    private static Vector2 GetSectorDirection(int sector) {
        switch (sector) {
            case 0: { 
                    return Vector2.up; 
                }
            case 1: { 
                    return Vector2.right; 
                }
            case 2: { 
                    return Vector2.left; 
                }
            default: { 
                    return Vector2.down; 
                }
        }
    }
}
