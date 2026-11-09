using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSwampWater : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private Material swampWaterMaterial;

    [Header("Water Generation")]
    [SerializeField] private float meshSpacing = 10f;
    [SerializeField] private float waterSurfaceOffset = 0.15f;
    [SerializeField] private float waterMaskThreshold = 0.45f;
    [SerializeField] private float minimumSwampWeight = 0.9f;
    [SerializeField] private float maximumSlope = 12f;

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

    /// Builds connected water patches using the Swamp terrain's
    /// existing creek and depression masks rather than random ponds.
    private void GenerateWaterTile(Terrain terrain, Transform parent, Vector2 worldCentre) {
        Vector3 origin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;

        int cellsX = Mathf.CeilToInt(size.x / meshSpacing);
        int cellsZ = Mathf.CeilToInt(size.z / meshSpacing);

        float stepX = size.x / cellsX;
        float stepZ = size.z / cellsZ;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        for (int z = 0; z < cellsZ; z++) {
            for (int x = 0; x < cellsX; x++) {
                float x0 = origin.x + x * stepX;
                float x1 = x0 + stepX;
                float z0 = origin.z + z * stepZ;
                float z1 = z0 + stepZ;

                float centreX = (x0 + x1) * 0.5f;
                float centreZ = (z0 + z1) * 0.5f;

                float distance = Vector2.Distance(new Vector2(centreX, centreZ), worldCentre);

                if (distance < kingdomExclusionRadius || distance > playableRadius) {
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

                float u = Mathf.Clamp01((centreX - origin.x) / size.x);
                float v = Mathf.Clamp01((centreZ - origin.z) / size.z);

                if (terrain.terrainData.GetSteepness(u, v) > maximumSlope) {
                    continue;
                }

                float y0 = terrain.SampleHeight(new Vector3(x0, 0f, z0)) + waterSurfaceOffset;
                float y1 = terrain.SampleHeight(new Vector3(x1, 0f, z0)) + waterSurfaceOffset;
                float y2 = terrain.SampleHeight(new Vector3(x0, 0f, z1)) + waterSurfaceOffset;
                float y3 = terrain.SampleHeight(new Vector3(x1, 0f, z1)) + waterSurfaceOffset;

                int index = vertices.Count;

                vertices.Add(new Vector3(x0 - origin.x, y0, z0 - origin.z));
                vertices.Add(new Vector3(x1 - origin.x, y1, z0 - origin.z));
                vertices.Add(new Vector3(x0 - origin.x, y2, z1 - origin.z));
                vertices.Add(new Vector3(x1 - origin.x, y3, z1 - origin.z));

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
        waterObject.transform.SetParent(parent);
        waterObject.transform.position = new Vector3(origin.x, origin.y, origin.z);

        MeshFilter filter = waterObject.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = waterObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = swampWaterMaterial;
    }
}


