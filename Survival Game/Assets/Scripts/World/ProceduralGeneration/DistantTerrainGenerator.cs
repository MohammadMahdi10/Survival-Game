using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Generates terrain beyond the playable world.
/// Uses the existing biome generator so distant landscape
/// continues naturally from the four main Terrain tiles.
public class DistantTerrainGenerator : MonoBehaviour {

    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private Material distantTerrainMaterial;

    [Header("Distant Terrain Settings")]
    [SerializeField] private int meshResolution = 128;
    [SerializeField] private int textureResolution = 512;

    [Header("Outer Terrain Variation")]
    [SerializeField] private float variationBlendDistance = 600f;
    [SerializeField] private float largeVariationScale = 0.0005f;
    [SerializeField] private float mediumVariationScale = 0.0015f;
    [SerializeField] private float variationStrength = 60f;

    [Header("Distant Snow Mountains")]
    [SerializeField] private float mountainScale = 0.00035f;
    [SerializeField] private float mountainDetailScale = 0.0015f;
    [SerializeField] private float mountainHeight = 2200f;
    [SerializeField] private float mountainThreshold = 0.38f;
    [SerializeField] private float mountainFullHeightThreshold = 0.68f;

    [Header("Distant Plains Mountains")]
    [SerializeField] private float plainsMountainHeight = 400f;
    [SerializeField] private float plainsMountainScale = 0.0006f;
    [SerializeField] private float plainsMountainThreshold = 0.55f;
    [SerializeField] private float plainsMountainFullHeightThreshold = 0.82f;

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

        return originalHeight + (variation + snowMountains + plainsMountains) * blend;
    }


    /// Bakes biome textures for the distant terrain meshes.
    /// Exposes rock on steep Snow slopes, restores snow on high
    /// summits, and adds narrow snow streaks across rocky faces.
    private Texture2D GenerateBiomeTexture(float startX, float startZ, float width, float length, Mesh mesh) {
        Texture2D texture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, true);
        texture.wrapMode = TextureWrapMode.Clamp;

        TerrainLayer[] layers = { biomeGenerator.snowLayer, biomeGenerator.plainsLayer, biomeGenerator.desertLayer, biomeGenerator.swampLayer, biomeGenerator.rockLayer };

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

                float[] weights = { snow, plains, desert, swamp, rock };

                Color colour = Color.black;

                for (int i = 0; i < layers.Length; i++) {
                    if (layers[i] == null || layers[i].diffuseTexture == null || weights[i] <= 0f) {
                        continue;
                    }

                    Texture2D diffuse = layers[i].diffuseTexture;

                    float textureU = Mathf.Repeat((worldX + layers[i].tileOffset.x) / layers[i].tileSize.x, 1f);

                    float textureV = Mathf.Repeat((worldZ + layers[i].tileOffset.y) / layers[i].tileSize.y, 1f);

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
}

