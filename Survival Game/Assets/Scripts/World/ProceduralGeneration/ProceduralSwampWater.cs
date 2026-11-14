using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSwampWater : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private Material swampWaterMaterial;

    [Header("Water Generation")]
    [SerializeField] private float meshSpacing = 6f;
    [SerializeField] private float minimumSwampWeight = 0.6f;

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

    private const float DepthForFullOpacity = 3f;
    private const float GroundClearance = 0.05f;

    private float GetGroundY(Terrain terrain, float worldX, float worldZ)
    {
        Vector3 origin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;

        float u = Mathf.Clamp01((worldX - origin.x) / size.x);
        float v = Mathf.Clamp01((worldZ - origin.z) / size.z);

        return terrain.terrainData.GetInterpolatedHeight(u, v) + origin.y;
    }

    private Color GetWaterVertexColour(float groundY, float level)
    {
        float alpha = Mathf.Clamp01((level - groundY) / DepthForFullOpacity);
        return new Color(1f, 1f, 1f, alpha);
    }

    private bool IsInsideWaterRange(Vector2 point, Vector2 worldCentre)
    {
        float distance = Vector2.Distance(point, worldCentre);
        return distance >= kingdomExclusionRadius && distance <= playableRadius;
    }

    /// Generates flat water wherever the terrain dips below the shared
    /// water level. Vertex fades with depth so shorelines blend softly.
    private void GenerateWaterTile(Terrain terrain, Transform parent, Vector2 worldCentre) {
        Vector3 origin = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        int cellsX = Mathf.CeilToInt(terrainSize.x / meshSpacing);
        int cellsZ = Mathf.CeilToInt(terrainSize.z / meshSpacing);

        float stepX = terrainSize.x / cellsX;
        float stepZ = terrainSize.z / cellsZ;

        float level = biomeGenerator.SwampWaterLevel;
        float localWaterY = level - origin.y;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colours = new List<Color>();
        List<int> triangles = new List<int>();

        for (int z = 0; z < cellsZ; z++) {
            for (int x = 0; x < cellsX; x++) {
                float x0 = origin.x + x * stepX;
                float x1 = x0 + stepX;
                float z0 = origin.z + z * stepZ;
                float z1 = z0 + stepZ;

                Vector2 centre = new Vector2((x0 + x1) * 0.5f, (z0 + z1) * 0.5f);

                if (!IsInsideWaterRange(centre, worldCentre)) {
                    continue;
                }

                biomeGenerator.GetBiomeBlend(centre.x, centre.y, out float snow, out float plains, out float desert, out float swamp);

                if (swamp < minimumSwampWeight) {
                    continue;
                }

                float y00 = GetGroundY(terrain, x0, z0);
                float y10 = GetGroundY(terrain, x1, z0);
                float y01 = GetGroundY(terrain, x0, z1);
                float y11 = GetGroundY(terrain, x1, z1);

                float lowest = Mathf.Min(Mathf.Min(y00, y10), Mathf.Min(y01, y11));

                if (lowest >= level - GroundClearance) {
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

                colours.Add(GetWaterVertexColour(y00, level));
                colours.Add(GetWaterVertexColour(y10, level));
                colours.Add(GetWaterVertexColour(y01, level));
                colours.Add(GetWaterVertexColour(y11, level));

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
        mesh.SetColors(colours);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject waterObject = new GameObject("SwampWater");
        waterObject.transform.SetParent(parent, false);
        waterObject.transform.position = origin;

        waterObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        waterObject.AddComponent<MeshRenderer>().sharedMaterial = swampWaterMaterial;
    }
}


