using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSwampWater : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private Material swampWaterMaterial;

    [Header("Water Generation")]
    [SerializeField] private float meshSpacing = 10f;
    [SerializeField] private float waterMaskThreshold = 0.45f;
    [SerializeField] private float minimumSwampWeight = 0.9f;
    [SerializeField] private float waterLevel = 10f;

    [Header("World Limits")]
    [SerializeField] private float kingdomExclusionRadius = 350f;
    [SerializeField] private float playableRadius = 1950f;

    [ContextMenu("Generate Swamp Water")]
    public void GenerateSwampWater() {
        if (biomeGenerator == null || swampWaterMaterial == null || meshSpacing <= 0f) {
            return;
        }

        Terrain[] terrains = { biomeGenerator.snowTerrain, biomeGenerator.plainsTerrain, biomeGenerator.desertTerrain, biomeGenerator.swampTerrain };

        foreach (Terrain terrain in terrains) {
            if (terrain == null || terrain.terrainData == null) {
                return;
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

        Vector2 worldCentre = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);

        Transform previous = transform.Find("GeneratedSwampWater");

        if (previous != null) {
            if (Application.isPlaying) {
                Destroy(previous.gameObject);
            }
            else
            {
                DestroyImmediate(previous.gameObject);
            }
        }

        GameObject container = new GameObject("GeneratedSwampWater");
        container.transform.SetParent(transform, false);

        foreach (Terrain terrain in terrains) {
            GenerateWaterTile(terrain, container.transform, worldCentre);
        }
    }


    /// Generates flat, horizontal water inside the swamp's existing
    /// gaps and pond masks. Water only appears where the terrain
    /// is below the chosen water level, leaving raised islands dry.
    private void GenerateWaterTile(Terrain terrain, Transform parent, Vector2 worldCentre) {
        Vector3 origin = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        int cellsX = Mathf.CeilToInt(terrainSize.x / meshSpacing);
        int cellsZ = Mathf.CeilToInt(terrainSize.z / meshSpacing);

        float stepX = terrainSize.x / cellsX;
        float stepZ = terrainSize.z / cellsZ;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float localWaterY = waterLevel - origin.y;

        for (int z = 0; z < cellsZ; z++) {
            for (int x = 0; x < cellsX; x++) {
                float x0 = origin.x + x * stepX;
                float x1 = x0 + stepX;
                float z0 = origin.z + z * stepZ;
                float z1 = z0 + stepZ;

                float centreX = (x0 + x1) * 0.5f;
                float centreZ = (z0 + z1) * 0.5f;

                Vector2 point = new Vector2(centreX, centreZ);
                float distanceFromCentre = Vector2.Distance(point, worldCentre);

                if (distanceFromCentre < kingdomExclusionRadius || distanceFromCentre > playableRadius) {
                    continue;
                }

                biomeGenerator.GetBiomeBlend(centreX, centreZ, out float snow, out float plains, out float desert, out float swamp);

                if (swamp < minimumSwampWeight) {
                    continue;
                }

                float waterMask = biomeGenerator.GetSwampWaterMask(centreX, centreZ);

                if (waterMask < waterMaskThreshold) {
                    continue;
                }

                float u = Mathf.Clamp01((centreX - origin.x) / terrainSize.x);
                float v = Mathf.Clamp01((centreZ - origin.z) / terrainSize.z);

                float groundY = terrain.terrainData.GetInterpolatedHeight(u, v) + origin.y;

                if (groundY >= waterLevel - 0.05f) {
                    continue;
                }

                int index = vertices.Count;

                vertices.Add(new Vector3(x0 - origin.x, localWaterY, z0 - origin.z));
                vertices.Add(new Vector3(x1 - origin.x, localWaterY, z0 - origin.z));
                vertices.Add(new Vector3(x0 - origin.x, localWaterY, z1 - origin.z));
                vertices.Add(new Vector3(x1 - origin.x, localWaterY, z1 - origin.z));

                uvs.Add(new Vector2(x0 / 10f, z0 / 10f));
                uvs.Add(new Vector2(x1 / 10f, z0 / 10f));
                uvs.Add(new Vector2(x0 / 10f, z1 / 10f));
                uvs.Add(new Vector2(x1 / 10f, z1 / 10f));

                triangles.Add(index);
                triangles.Add(index + 2);
                triangles.Add(index + 1);

                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }
        }

        if (vertices.Count == 0) {
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "SwampWaterMesh";
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject waterObject = new GameObject("SwampWater");
        waterObject.transform.SetParent(parent, false);
        waterObject.transform.position = origin;

        MeshFilter filter = waterObject.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = waterObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = swampWaterMaterial;
    }

}


