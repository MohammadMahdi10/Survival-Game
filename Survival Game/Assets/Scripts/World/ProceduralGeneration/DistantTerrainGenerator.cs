using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Generates terrain beyond the playable world.
/// Uses the existing biome generator so distant landscape
/// continues naturally from the four main Terrain tiles.
public class DistantTerrainGenerator : MonoBehaviour {

    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private ProceduralEnvironmentSpawner environmentSpawner;
    [SerializeField] private Material distantTerrainMaterial;

    [Header("Distant Biome Textures")]
    [SerializeField] private Texture2D distantPlainsTexture;
    [SerializeField] private Texture2D distantSwampMudTexture;

    [Header("Distant Swamp Appearance")]
    [SerializeField, Range(0f, 1f)] private float distantMudStrength = 0.8f;
    [SerializeField] private float distantMudNoiseScale = 0.003f;

    [Header("Distant Terrain Settings")]
    [SerializeField] private int meshResolution = 256;
    [SerializeField] private int textureResolution = 512;

    [Header("Outer Terrain Variation")]
    [SerializeField] private float variationBlendDistance = 600f;
    [SerializeField] private float largeVariationScale = 0.0005f;
    [SerializeField] private float mediumVariationScale = 0.0015f;
    [SerializeField] private float variationStrength = 60f;

    [Header("Distant Snow Mountains")]
    [SerializeField] private float mountainScale = 0.00035f;
    [SerializeField] private float mountainHeight = 2200f;
    [SerializeField] private float mountainThreshold = 0.38f;
    [SerializeField] private float mountainFullHeightThreshold = 0.68f;

    [Header("Distant Plains Mountains")]
    [SerializeField] private float plainsMountainHeight = 400f;
    [SerializeField] private float plainsMountainScale = 0.0006f;
    [SerializeField] private float plainsMountainThreshold = 0.55f;
    [SerializeField] private float plainsMountainFullHeightThreshold = 0.82f;

    [Header("Distant Vegetation Prefabs")]
    [SerializeField] private GameObject[] distantPlainsTrees;
    [SerializeField] private GameObject[] distantSnowTrees;
    [SerializeField] private GameObject[] distantSwampTrees;
    [SerializeField] private GameObject[] distantCacti;
    [SerializeField] private GameObject[] distantBushes;
    [SerializeField] private GameObject[] distantRocks;

    [Header("Distant Vegetation Density")]
    [SerializeField] private int distantTreeCount = 180;
    [SerializeField] private int distantCactusCount = 55;
    [SerializeField] private int distantBushCount = 45;
    [SerializeField] private int distantRockCount = 35;

    [Header("Distant Vegetation Fade")]
    [SerializeField] private float vegetationFullDensityDistance = 200f;
    [SerializeField] private float vegetationFadeDistance = 1400f;
    [SerializeField] private float farDensityMultiplier = 0.35f;
    [SerializeField] private float farSpacingMultiplier = 1.8f;

    [Header("Distant Swamp Water")]
    [SerializeField] private Material distantSwampWaterMaterial;
    [SerializeField] private float distantWaterMeshSpacing = 10f;

    [Header("Distant Swamp Trees")]
    [SerializeField] private int distantSwampTreeCount = 55;


    private float worldMinX;
    private float worldMaxX;
    private float worldMinZ;
    private float worldMaxZ;

    /// Creates twelve terrain meshes surrounding the four main tiles.
    /// Samples the same world heights to keep the landscape connected
    /// without adding colliders or expensive terrain details.
    [ContextMenu("Generate Distant Terrain")]
    public void GenerateDistantTerrain() {
        if (biomeGenerator == null || distantTerrainMaterial == null || meshResolution < 2) {
            return;
        }

        Terrain[] terrains = { biomeGenerator.snowTerrain, biomeGenerator.plainsTerrain, biomeGenerator.desertTerrain, biomeGenerator.swampTerrain };

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;

        foreach (Terrain terrain in terrains) {
            if (terrain == null || terrain.terrainData == null) {
                return;
            }

            minX = Mathf.Min(minX, terrain.transform.position.x);
            minZ = Mathf.Min(minZ, terrain.transform.position.z);
        }

        float tileWidth = terrains[0].terrainData.size.x;
        float tileLength = terrains[0].terrainData.size.z;

        worldMinX = minX;
        worldMaxX = minX + tileWidth * 2f;
        worldMinZ = minZ;
        worldMaxZ = minZ + tileLength * 2f;

        Transform previous = transform.Find("GeneratedDistantTerrain");

        if (previous != null) {
            if (Application.isPlaying) {
                Destroy(previous.gameObject);
            }
            else {
                DestroyImmediate(previous.gameObject);
            }
        }

        GameObject container = new GameObject("GeneratedDistantTerrain");
        container.transform.SetParent(transform, false);

        for (int z = -1; z <= 2; z++) {
            for (int x = -1; x <= 2; x++) {
                if (x >= 0 && x <= 1 && z >= 0 && z <= 1) {
                    continue;
                }

                float startX = minX + x * tileWidth;
                float startZ = minZ + z * tileLength;

                CreateTerrainMesh(container.transform, startX, startZ, tileWidth, tileLength);
            }
        }
    }


    /// Extends the existing procedural terrain into the distance.
    /// Preserves the original terrain height at the world edges
    /// and gradually introduces new large-scale landforms farther away.
    private float SampleDistantHeight(float x, float z) {
        float originalHeight = biomeGenerator.SampleWorldHeight(x, z);

        float outsideX = Mathf.Max(worldMinX - x, x - worldMaxX, 0f);
        float outsideZ = Mathf.Max(worldMinZ - z, z - worldMaxZ, 0f);
        float outsideDistance = Mathf.Max(outsideX, outsideZ);

        float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, variationBlendDistance, outsideDistance));

        float offsetX = biomeGenerator.seed * 0.137f;
        float offsetZ = biomeGenerator.seed * 0.293f;

        float largeNoise = Mathf.PerlinNoise(x * largeVariationScale + offsetX + 173f, z * largeVariationScale + offsetZ + 291f);
        float mediumNoise = Mathf.PerlinNoise(x * mediumVariationScale + offsetX + 431f, z * mediumVariationScale + offsetZ + 619f);

        float relief = Mathf.Clamp((originalHeight - 50f) * 0.2f, 0f, 80f);
        float variation = ((largeNoise - 0.5f) * 2f + (mediumNoise - 0.5f) * 0.5f) * (variationStrength + relief);

        float snowMountains = GetDistantMountainHeight(x, z);
        float plainsMountains = GetDistantPlainsMountainHeight(x, z);

        float height = originalHeight + (variation + snowMountains + plainsMountains) * blend;

        biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

        if (swamp > 0.5f) {
            float waterMask = biomeGenerator.GetSwampWaterMask(x, z);
            float creekStrength = waterMask * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 0.9f, swamp));

            float creekBed = Mathf.Min(height, biomeGenerator.SwampWaterLevel - 2f);
            height = Mathf.Lerp(height, creekBed, creekStrength);
        }

        if (float.IsNaN(height) || float.IsInfinity(height)) {
            return 0f;
        }

        return height;
    }


    /// Bakes biome textures for the distant terrain meshes.
    /// Exposes rock on steep Snow slopes, restores snow on high
    /// summits, and adds narrow snow streaks across rocky faces.
    private Texture2D GenerateBiomeTexture(float startX, float startZ, float width, float length, Mesh mesh) {
        Texture2D texture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, true);
        texture.wrapMode = TextureWrapMode.Clamp;

        TerrainLayer[] layers = { biomeGenerator.snowLayer, biomeGenerator.plainsLayer, biomeGenerator.desertLayer, biomeGenerator.swampLayer, biomeGenerator.rockLayer, biomeGenerator.mudLayer };

        Color[] pixels = new Color[textureResolution * textureResolution];

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        int vertexCount = meshResolution + 1;

        for (int z = 0; z < textureResolution; z++) {
            for (int x = 0; x < textureResolution; x++) {
                float u = x / (float)(textureResolution - 1);
                float v = z / (float)(textureResolution - 1);

                float worldX = startX + u * width;
                float worldZ = startZ + v * length;

                biomeGenerator.GetBiomeBlend(worldX, worldZ, out float snow, out float plains, out float desert, out float swamp);

                float meshX = u * meshResolution;
                float meshZ = v * meshResolution;

                int ix = Mathf.Min(Mathf.FloorToInt(meshX), meshResolution - 1);
                int iz = Mathf.Min(Mathf.FloorToInt(meshZ), meshResolution - 1);

                float fx = meshX - ix;
                float fz = meshZ - iz;

                int a = iz * vertexCount + ix;
                int b = a + 1;
                int c = a + vertexCount;
                int d = c + 1;

                Vector3 normalBottom = Vector3.Lerp(normals[a], normals[b], fx);
                Vector3 normalTop = Vector3.Lerp(normals[c], normals[d], fx);
                Vector3 normal = Vector3.Lerp(normalBottom, normalTop, fz).normalized;

                float slope = Vector3.Angle(normal, Vector3.up);

                float heightBottom = Mathf.Lerp(vertices[a].y, vertices[b].y, fx);
                float heightTop = Mathf.Lerp(vertices[c].y, vertices[d].y, fx);
                float mountainHeight = Mathf.Lerp(heightBottom, heightTop, fz);

                float rockMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(28f, 52f, slope));

                float summitSnow = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(900f, 1750f, mountainHeight));

                float warp = (Mathf.PerlinNoise(worldX * 0.003f + biomeGenerator.seed * 0.017f + 317f, worldZ * 0.003f + biomeGenerator.seed * 0.029f + 593f) - 0.5f) * 110f;

                float streakNoise = Mathf.PerlinNoise((worldX + warp) * 0.017f + biomeGenerator.seed * 0.013f + 719f, worldZ * 0.00374f + biomeGenerator.seed * 0.031f + 283f);

                float streaks = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.64f, 0.77f, streakNoise));

                float slopeMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(25f, 45f, slope));

                float elevationMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(350f, 850f, mountainHeight));

                float streakMask = streaks * slopeMask * elevationMask;

                float snowRestoration = Mathf.Max(summitSnow * 0.98f, streakMask * 0.85f);

                float rock = snow * rockMask * (1f - snowRestoration);
                snow -= rock;

                float mudWeight = swamp * biomeGenerator.GetSwampMudMask(worldX, worldZ, mountainHeight);
                swamp -= mudWeight;

                float[] weights = { snow, plains, desert, swamp, rock, mudWeight };

                Color colour = Color.black;

                for (int i = 0; i < layers.Length; i++) {
                    if (layers[i] == null || layers[i].diffuseTexture == null || weights[i] <= 0f) {
                        continue;
                    }

                    Texture2D diffuse = i == 1 && distantPlainsTexture != null ? distantPlainsTexture : layers[i].diffuseTexture;

                    float bakeTileSizeX = Mathf.Max(layers[i].tileSize.x, width / (textureResolution - 1f) * 4f);
                    float bakeTileSizeZ = Mathf.Max(layers[i].tileSize.y, length / (textureResolution - 1f) * 4f);

                    float textureU = Mathf.Repeat((worldX + layers[i].tileOffset.x) / bakeTileSizeX, 1f);
                    float textureV = Mathf.Repeat((worldZ + layers[i].tileOffset.y) / bakeTileSizeZ, 1f);

                    colour += diffuse.GetPixelBilinear(textureU, textureV) * weights[i];
                }

                pixels[z * textureResolution + x] = colour;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(true, true);

        return texture;
    }


    /// Builds one terrain mesh using the same height
    /// calculations as the main procedural biome generator.
    /// Mesh coordinates match the neighbouring Terrain edges.
    private void CreateTerrainMesh(Transform parent, float startX, float startZ, float width, float length) {
        int vertexCount = meshResolution + 1;

        Vector3[] vertices = new Vector3[vertexCount * vertexCount];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[meshResolution * meshResolution * 6];

        for (int z = 0; z < vertexCount; z++) {
            for (int x = 0; x < vertexCount; x++) {
                float localX = x / (float)meshResolution * width;
                float localZ = z / (float)meshResolution * length;

                float worldX = startX + localX;
                float worldZ = startZ + localZ;
                float height = SampleDistantHeight(worldX, worldZ);

                int index = z * vertexCount + x;

                vertices[index] = new Vector3(localX, height, localZ);
                uvs[index] = new Vector2(x / (float)meshResolution, z / (float)meshResolution);
            }
        }

        int triangleIndex = 0;

        for (int z = 0; z < meshResolution; z++) {
            for (int x = 0; x < meshResolution; x++) {
                int a = z * vertexCount + x;
                int b = a + 1;
                int c = a + vertexCount;
                int d = c + 1;

                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = b;

                triangles[triangleIndex++] = b;
                triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = d;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "DistantTerrainMesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject terrainObject = new GameObject("DistantTerrain");
        terrainObject.transform.SetParent(parent);
        terrainObject.transform.position = new Vector3(startX, 0f, startZ);
        terrainObject.transform.rotation = Quaternion.identity;
        terrainObject.transform.localScale = Vector3.one;

        MeshFilter filter = terrainObject.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = terrainObject.AddComponent<MeshRenderer>();

        Material material = new Material(distantTerrainMaterial);
        Texture2D biomeTexture = GenerateBiomeTexture(startX, startZ, width, length, mesh);

        if (material.HasProperty("_MainTex")) {
            material.SetTexture("_MainTex", biomeTexture);
        }

        if (material.HasProperty("_BaseMap")) {
            material.SetTexture("_BaseMap", biomeTexture);
        }

        renderer.sharedMaterial = material;

        GenerateDistantSwampWater(terrainObject.transform, startX, startZ, width, length);
        SpawnDistantVegetation(terrainObject.transform, mesh, startX, startZ, width, length);
    }

    /// Generates simplified vegetation on each distant terrain mesh.
    /// Uses the biome weights to choose which objects belong in each region.
    private void SpawnDistantVegetation(Transform parent, Mesh mesh, float startX, float startZ, float width, float length)
    {
        GameObject container = new GameObject("DistantVegetation");
        container.transform.SetParent(parent, false);

        int tileX = Mathf.RoundToInt(startX / width);
        int tileZ = Mathf.RoundToInt(startZ / length);

        System.Random random = new System.Random(biomeGenerator.seed + tileX * 73856093 + tileZ * 19349663);

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        SpawnDistantForestChunks(container.transform, vertices, normals, startX, startZ, width, length, distantPlainsTrees, false);
        SpawnDistantForestChunks(container.transform, vertices, normals, startX, startZ, width, length, distantSnowTrees, true);
        SpawnDistantObjects(container.transform, vertices, normals, startX, startZ, width, length, distantSwampTrees, distantSwampTreeCount, 2, random);
        SpawnDistantObjects(container.transform, vertices, normals, startX, startZ, width, length, distantCacti, distantCactusCount, 3, random);
        SpawnDistantObjects(container.transform, vertices, normals, startX, startZ, width, length, distantBushes, distantBushCount, 4, random);
        SpawnDistantObjects(container.transform, vertices, normals, startX, startZ, width, length, distantRocks, distantRockCount, 5, random);
    }

    private void SpawnDistantObjects(Transform parent, Vector3[] vertices, Vector3[] normals, float startX, float startZ, float width, float length, GameObject[] prefabs, int targetCount, int objectType, System.Random random) {
        if (prefabs == null || prefabs.Length == 0 || targetCount <= 0) {
            return;
        }

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 30;
        int vertexCount = meshResolution + 1;

        float groupX = 0f;
        float groupZ = 0f;
        float groupRadius = 0f;
        int groupRemaining = 0;

        List<Vector2> placedPositions = new List<Vector2>();

        while (spawned < targetCount && attempts < maxAttempts) {
            attempts++;

            float x;
            float z;

            if (objectType >= 3) {
                if (groupRemaining <= 0) {
                    groupX = startX + (float)random.NextDouble() * width;
                    groupZ = startZ + (float)random.NextDouble() * length;

                    groupRemaining = objectType == 3 ? random.Next(1, 12) : objectType == 4 ? random.Next(1, 13) : random.Next(1, 10);
                    groupRadius = objectType == 3 ? Mathf.Lerp(8f, 45f, (float)random.NextDouble()) : objectType == 4 ? Mathf.Lerp(8f, 40f, (float)random.NextDouble()) : Mathf.Lerp(10f, 55f, (float)random.NextDouble());
                }

                groupRemaining--;

                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = Mathf.Pow((float)random.NextDouble(), 1.5f) * groupRadius;

                x = groupX + Mathf.Cos(angle) * distance;
                z = groupZ + Mathf.Sin(angle) * distance;

                if (x < startX || x >= startX + width || z < startZ || z >= startZ + length) {
                    continue;
                }
            }
            else
            {
                x = startX + (float)random.NextDouble() * width;
                z = startZ + (float)random.NextDouble() * length;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            float biomeWeight = objectType == 0 ? plains : objectType == 1 ? snow : objectType == 2 ? swamp : objectType == 3 ? desert : objectType == 4 ? desert : desert;

            if (biomeWeight < 0.8f) {
                continue;
            }

            float patchNoise = Mathf.PerlinNoise(x * 0.0012f + biomeGenerator.seed * 0.13f, z * 0.0012f + biomeGenerator.seed * 0.27f);

            float spawnChance = Mathf.Lerp(0.05f, 0.9f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.3f, 0.7f, patchNoise)));

            if (objectType == 2 && environmentSpawner != null) {
                spawnChance = environmentSpawner.GetDistantSwampTreeChance(x, z);
            }

            if (objectType == 0 && environmentSpawner != null) {
                spawnChance = environmentSpawner.GetDistantPlainsTreeChance(x, z);
            }

            spawnChance *= GetDistantDensityMultiplier(x, z);

            if ((float)random.NextDouble() > spawnChance) {
                continue;
            }

            float gridX = Mathf.Clamp(x - startX, 0f, width - 0.001f) / width * meshResolution;
            float gridZ = Mathf.Clamp(z - startZ, 0f, length - 0.001f) / length * meshResolution;

            int ix = Mathf.FloorToInt(gridX);
            int iz = Mathf.FloorToInt(gridZ);

            float fx = gridX - ix;
            float fz = gridZ - iz;

            int a = iz * vertexCount + ix;
            int b = a + 1;
            int c = a + vertexCount;
            int d = c + 1;

            float y;
            Vector3 normal;

            if (fx + fz <= 1f) {
                y = vertices[a].y * (1f - fx - fz) + vertices[b].y * fx + vertices[c].y * fz;
                normal = (normals[a] * (1f - fx - fz) + normals[b] * fx + normals[c] * fz).normalized;
            }
            else {
                y = vertices[d].y * (fx + fz - 1f) + vertices[b].y * (1f - fz) + vertices[c].y * (1f - fx);
                normal = (normals[d] * (fx + fz - 1f) + normals[b] * (1f - fz) + normals[c] * (1f - fx)).normalized;
            }

            if (float.IsNaN(y) || float.IsInfinity(y)) {
                continue;
            }

            if (float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z)) {
                continue;
            }

            float slope = Vector3.Angle(normal, Vector3.up);

            if (objectType == 2 && environmentSpawner != null && slope > environmentSpawner.GetDistantPlainsMaximumSlope()) {
                continue;
            }

            if (objectType == 5 ? slope > 45f : slope > 28f) {
                continue;
            }

            if (objectType == 1 && y > 550f) {
                continue;
            }

            if ((objectType == 0 || objectType == 4) && y > 550f) {
                continue;
            }

            float minimumSpacing = objectType == 2 ? environmentSpawner.GetDistantSwampTreeSpacing() : objectType == 3 ? 14f : objectType == 4 ? 8f : 10f;

            minimumSpacing *= Mathf.Lerp(1f, farSpacingMultiplier, GetDistantVegetationFade(x, z));

            bool tooClose = false;

            foreach (Vector2 placed in placedPositions) {
                if (Vector2.Distance(new Vector2(x, z), placed) < minimumSpacing) {
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

            float scale = objectType <= 2 ? Mathf.Lerp(2f, 3.5f, (float)random.NextDouble()) : Mathf.Lerp(1.5f, 3f, (float)random.NextDouble());

            if (objectType == 2 && environmentSpawner != null) {
                Vector2 swampScale = environmentSpawner.GetDistantSwampTreeScale();
                scale = Mathf.Lerp(swampScale.x, swampScale.y, (float)random.NextDouble());
            }

            instance.transform.localScale *= scale;

            foreach (Collider collider in instance.GetComponentsInChildren<Collider>()) {
                collider.enabled = false;
            }

            foreach (Renderer objectRenderer in instance.GetComponentsInChildren<Renderer>()) {
                objectRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                objectRenderer.receiveShadows = false;
            }

            spawned++;
            placedPositions.Add(new Vector2(x, z));
        }
    }

    /// Generates large rounded mountain formations in distant Plains.
    /// Uses low frequency noise to create broad slopes rather than
    /// the sharper, ridged mountains found in the Snow biome.
    private float GetDistantPlainsMountainHeight(float x, float z) {
        float offsetX = biomeGenerator.seed * 0.137f;
        float offsetZ = biomeGenerator.seed * 0.293f;

        float noise = Mathf.PerlinNoise(x * plainsMountainScale + offsetX + 347f, z * plainsMountainScale + offsetZ + 683f);

        float mountainMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(plainsMountainThreshold, plainsMountainFullHeightThreshold, noise));

        float plainsWeight = biomeGenerator.GetPlainsBiomeWeight(x, z);

        return mountainMask * plainsMountainHeight * plainsWeight;
    }

    /// Generates enormous snowy mountains with jagged ridges,
    /// smaller peaks and irregular slopes. Keeps their massive
    /// scale while adding detail similar to the main Snow mountains.
    private float GetDistantMountainHeight(float x, float z) {
        float offsetX = biomeGenerator.seed * 0.137f;
        float offsetZ = biomeGenerator.seed * 0.293f;

        float regionNoise = Mathf.PerlinNoise(x * mountainScale + offsetX + 173f, z * mountainScale + offsetZ + 291f);

        float mountainMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(mountainThreshold, mountainFullHeightThreshold, regionNoise));

        float largeNoise = Mathf.PerlinNoise(x * 0.0012f + offsetX + 431f, z * 0.0012f + offsetZ + 619f);
        float largeRidges = 1f - Mathf.Abs(largeNoise * 2f - 1f);

        float mediumNoise = Mathf.PerlinNoise(x * 0.0035f + offsetX + 733f, z * 0.0035f + offsetZ + 887f);
        float mediumRidges = 1f - Mathf.Abs(mediumNoise * 2f - 1f);

        float fineNoise = Mathf.PerlinNoise(x * 0.009f + offsetX + 127f, z * 0.009f + offsetZ + 397f);
        float fineRidges = 1f - Mathf.Abs(fineNoise * 2f - 1f);

        float mountainBody = Mathf.Pow(largeRidges, 1.5f) * mountainHeight;

        float mediumDetails = Mathf.Pow(mediumRidges, 2f) * 350f;
        float fineDetails = Mathf.Pow(fineRidges, 3f) * 110f;

        float snowWeight = biomeGenerator.GetSnowBiomeWeight(x, z);

        return (mountainBody + mediumDetails + fineDetails) * mountainMask * snowWeight;
    }

    private void SpawnDistantForestChunks(Transform parent, Vector3[] vertices, Vector3[] normals, float startX, float startZ, float width, float length, GameObject[] prefabs, bool isSnow) {
        if (environmentSpawner == null || prefabs == null || prefabs.Length == 0) {
            return;
        }

        float size = Mathf.Max(1f, environmentSpawner.GetDistantChunkSize());

        int minChunkX = Mathf.FloorToInt(startX / size);
        int maxChunkX = Mathf.CeilToInt((startX + width) / size);
        int minChunkZ = Mathf.FloorToInt(startZ / size);
        int maxChunkZ = Mathf.CeilToInt((startZ + length) / size);

        List<Vector2> placedPositions = new List<Vector2>();

        for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++) {
            for (int chunkZ = minChunkZ; chunkZ < maxChunkZ; chunkZ++) {
                float minX = Mathf.Max(startX, chunkX * size);
                float maxX = Mathf.Min(startX + width, (chunkX + 1) * size);
                float minZ = Mathf.Max(startZ, chunkZ * size);
                float maxZ = Mathf.Min(startZ + length, (chunkZ + 1) * size);

                if (minX >= maxX || minZ >= maxZ) {
                    continue;
                }

                float centreX = (minX + maxX) * 0.5f;
                float centreZ = (minZ + maxZ) * 0.5f;

                int targetTrees = isSnow ? environmentSpawner.GetDistantSnowTreeBudget(centreX, centreZ) : environmentSpawner.GetDistantPlainsTreeBudget(centreX, centreZ);

                float chunkDensityMultiplier = GetDistantDensityMultiplier(centreX, centreZ);
                targetTrees = Mathf.RoundToInt(targetTrees * chunkDensityMultiplier);

                if (targetTrees <= 0) {
                    continue;
                }

                System.Random random = new System.Random(unchecked(biomeGenerator.seed + chunkX * 73856093 + chunkZ * 19349663 + (isSnow ? 9427 : 8421)));

                int spawned = 0;
                int attempts = 0;
                int maxAttempts = targetTrees * (isSnow ? 60 : 100);

                while (spawned < targetTrees && attempts < maxAttempts) {
                    attempts++;

                    float x = Mathf.Lerp(minX, maxX, (float)random.NextDouble());
                    float z = Mathf.Lerp(minZ, maxZ, (float)random.NextDouble());

                    biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

                    if (isSnow ? snow < 0.65f : plains < 0.85f) {
                        continue;
                    }

                    float spawnChance = isSnow ? 1f : environmentSpawner.GetDistantPlainsTreeChance(x, z);

                    float distanceFade = GetDistantVegetationFade(x, z);

                    if ((float)random.NextDouble() > spawnChance) {
                        continue;
                    }

                    if (!TrySampleDistantMesh(vertices, normals, x, z, startX, startZ, width, length, out float y, out float slope)) {
                        continue;
                    }
                    float maxSlope = isSnow ? 38f : environmentSpawner.GetDistantPlainsMaximumSlope();

                    if (slope > maxSlope) {
                        continue;
                    }

                    if (isSnow && y > 1200f) {
                        continue;
                    }

                    float spacing = isSnow ? environmentSpawner.GetDistantSnowTreeSpacing() : environmentSpawner.GetDistantPlainsTreeSpacing(x, z);

                    spacing *= Mathf.Lerp(1f, farSpacingMultiplier, distanceFade);

                    Vector2 point = new Vector2(x, z);
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

                    GameObject prefab = prefabs[random.Next(prefabs.Length)];

                    if (prefab == null) {
                        continue;
                    }

                    GameObject tree = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);

                    float scale;
                    float heightMultiplier;

                    if (isSnow && random.NextDouble() < 0.03) {
                        scale = Mathf.Lerp(5f, 7f, (float)random.NextDouble());
                        heightMultiplier = Mathf.Lerp(1.3f, 1.7f, (float)random.NextDouble());
                    }

                    else if (isSnow) {
                        scale = Mathf.Lerp(1.2f, 4f, (float)random.NextDouble());
                        heightMultiplier = Mathf.Lerp(1f, 1.6f, (float)random.NextDouble());
                    }

                    else {
                        scale = Mathf.Lerp(0.8f, 2.8f, (float)random.NextDouble());
                        heightMultiplier = Mathf.Lerp(0.85f, 1.4f, (float)random.NextDouble());
                    }

                    tree.transform.localScale = Vector3.Scale(tree.transform.localScale, new Vector3(scale, scale * heightMultiplier, scale));

                    foreach (Collider collider in tree.GetComponentsInChildren<Collider>()) {
                        collider.enabled = false;
                    }

                    foreach (Renderer treeRenderer in tree.GetComponentsInChildren<Renderer>()) {
                        treeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        treeRenderer.receiveShadows = false;
                    }

                    placedPositions.Add(point);
                    spawned++;
                }
            }
        }
    }

    private bool TrySampleDistantMesh(Vector3[] vertices, Vector3[] normals, float x, float z, float startX, float startZ, float width, float length, out float y, out float slope) {
        y = 0f;
        slope = 0f;

        float gx = Mathf.Clamp((x - startX) / width * meshResolution, 0f, meshResolution - 0.0001f);
        float gz = Mathf.Clamp((z - startZ) / length * meshResolution, 0f, meshResolution - 0.0001f);

        int ix = Mathf.FloorToInt(gx);
        int iz = Mathf.FloorToInt(gz);

        float fx = gx - ix;
        float fz = gz - iz;

        int row = meshResolution + 1;
        int a = iz * row + ix;
        int b = a + 1;
        int c = a + row;
        int d = c + 1;

        Vector3 normal;

        if (fx + fz <= 1f) {
            y = vertices[a].y * (1f - fx - fz) + vertices[b].y * fx + vertices[c].y * fz;
            normal = (normals[a] * (1f - fx - fz) + normals[b] * fx + normals[c] * fz).normalized;
        }

        else
        {
            y = vertices[d].y * (fx + fz - 1f) + vertices[b].y * (1f - fz) + vertices[c].y * (1f - fx);
            normal = (normals[d] * (fx + fz - 1f) + normals[b] * (1f - fz) + normals[c] * (1f - fx)).normalized;
        }

        if (float.IsNaN(y) || float.IsInfinity(y) || float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z)) {
            return false;
        }

        slope = Vector3.Angle(normal, Vector3.up);

        return !float.IsNaN(slope) && !float.IsInfinity(slope);
    }

    private float GetDistanceOutsideMainWorld(float x, float z) {
        float outsideX = Mathf.Max(worldMinX - x, x - worldMaxX, 0f);
        float outsideZ = Mathf.Max(worldMinZ - z, z - worldMaxZ, 0f);
        return Mathf.Max(outsideX, outsideZ);
    }

    private float GetDistantVegetationFade(float x, float z) {
        float distance = GetDistanceOutsideMainWorld(x, z);
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(vegetationFullDensityDistance, vegetationFadeDistance, distance));
    }

    private float GetDistantDensityMultiplier(float x, float z) {
        float fade = GetDistantVegetationFade(x, z);
        return Mathf.Lerp(1f, farDensityMultiplier, fade);
    }

    private void GenerateDistantSwampWater(Transform parent, float startX, float startZ, float width, float length) {
        if (distantSwampWaterMaterial == null) {
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float spacing = Mathf.Max(2f, distantWaterMeshSpacing);

        for (float z = 0f; z < length; z += spacing) {
            for (float x = 0f; x < width; x += spacing) {
                float cellWidth = Mathf.Min(spacing, width - x);
                float cellLength = Mathf.Min(spacing, length - z);

                float worldX = startX + x + cellWidth * 0.5f;
                float worldZ = startZ + z + cellLength * 0.5f;

                biomeGenerator.GetBiomeBlend(worldX, worldZ, out float snow, out float plains, out float desert, out float swamp);

                if (swamp < 0.8f || biomeGenerator.GetSwampWaterMask(worldX, worldZ) < 0.65f) {
                    continue;
                }

                float groundY = SampleDistantHeight(worldX, worldZ);

                if (float.IsNaN(groundY) || float.IsInfinity(groundY) || groundY >= biomeGenerator.SwampWaterLevel) {
                    continue;
                }

                int index = vertices.Count;
                float y = biomeGenerator.SwampWaterLevel;

                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y, z + cellLength));
                vertices.Add(new Vector3(x + cellWidth, y, z));
                vertices.Add(new Vector3(x + cellWidth, y, z + cellLength));

                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);

                triangles.Add(index + 2);
                triangles.Add(index + 1);
                triangles.Add(index + 3);
            }
        }

        if (vertices.Count == 0) {
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "DistantSwampWaterMesh";
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject waterObject = new GameObject("DistantSwampWater");
        waterObject.transform.SetParent(parent, false);

        MeshFilter filter = waterObject.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer waterRenderer = waterObject.AddComponent<MeshRenderer>();
        waterRenderer.sharedMaterial = distantSwampWaterMaterial;
        waterRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
}

