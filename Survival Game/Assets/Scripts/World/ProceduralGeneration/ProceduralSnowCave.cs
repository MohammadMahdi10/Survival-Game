using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ProceduralSnowCave : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private ProceduralEnvironmentSpawner environmentSpawner;

    [Header("Vegetation Clearing")]
    [SerializeField] private float entranceClearRadius = 35f;
    [SerializeField] private float approachClearLength = 50f;
    [SerializeField] private float approachClearWidth = 16f;

    [Header("Placement")]
    [SerializeField] private int placementAttempts = 1500;
    [SerializeField] private float minimumKingdomDistance = 650f;
    [SerializeField] private float minimumBoundaryClearance = 350f;
    [SerializeField] private float minimumSnowWeight = 0.92f;
    [SerializeField] private float minimumMountainElevation = 35f;
    [SerializeField] private float maximumMountainElevation = 220f;
    [SerializeField] private float minimumSlope = 8f;
    [SerializeField] private float maximumSlope = 25f;
    [SerializeField] private float maximumApproachSlope = 18f;

    [Header("Low Poly Entrance")]
    [SerializeField] private float openingWidth = 18f;
    [SerializeField] private float openingHeight = 15f;
    [SerializeField] private float facadeDepth = 5f;
    [SerializeField] private float downhillOffset = 14f;

    [Header("Cave Materials")]
    [SerializeField] private Material rockMaterial;
    [SerializeField] private Material darkRockMaterial;
    [SerializeField] private Material caveOpeningMaterial;
    [SerializeField] private Material snowMaterial;

    private const string GeneratedName = "GeneratedSnowCaveEntrance";
    private readonly List<Mesh> generatedMeshes = new List<Mesh>();
    private Mesh sharedRockMesh;

    [ContextMenu("Generate Snow Cave Entrance")]
    public void GenerateSnowCaveEntrance() {
        if (biomeGenerator == null) {
            return;
        }

        if (rockMaterial == null || darkRockMaterial == null || caveOpeningMaterial == null || snowMaterial == null) {
            Debug.LogWarning("Snow cave: assign all four Cave Materials in the Inspector.", this);
            return;
        }

        Terrain[] terrains = { biomeGenerator.snowTerrain, biomeGenerator.plainsTerrain, biomeGenerator.desertTerrain, biomeGenerator.swampTerrain };

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            if (terrain == null || terrain.terrainData == null) {
                return;
            }

            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            minX = Mathf.Min(minX, origin.x);
            maxX = Mathf.Max(maxX, origin.x + size.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxZ = Mathf.Max(maxZ, origin.z + size.z);
        }

        Vector2 centre = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        System.Random random = new System.Random(unchecked(biomeGenerator.seed * 397 + 19043));

        Vector3 bestPosition = Vector3.zero;
        Vector3 bestDownhill = Vector3.forward;
        bool found = false;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < placementAttempts; i++) {
            float x = Mathf.Lerp(minX + minimumBoundaryClearance, maxX - minimumBoundaryClearance, (float)random.NextDouble());
            float z = Mathf.Lerp(minZ + minimumBoundaryClearance, maxZ - minimumBoundaryClearance, (float)random.NextDouble());

            if (Vector2.Distance(new Vector2(x, z), centre) < minimumKingdomDistance) {
                continue;
            }

            biomeGenerator.GetBiomeBlend(x, z, out float snow, out float plains, out float desert, out float swamp);

            if (snow < minimumSnowWeight) {
                continue;
            }

            Terrain terrain = GetTerrainAtPosition(terrains, x, z);

            if (terrain == null) {
                continue;
            }

            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            float u = Mathf.Clamp01((x - origin.x) / size.x);
            float v = Mathf.Clamp01((z - origin.z) / size.z);
            float slope = terrain.terrainData.GetSteepness(u, v);
            float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + origin.y;
            float elevation = y - origin.y;

            if (slope < minimumSlope || slope > maximumSlope) {
                continue;
            }

            if (elevation < minimumMountainElevation || elevation > maximumMountainElevation) {
                continue;
            }

            Vector3 downhill = CalculateDownhill(terrains, x, z);

            if (downhill.sqrMagnitude < 0.01f) {
                continue;
            }

            Vector3 entrancePosition = new Vector3(x, y, z);

            Terrain entranceTerrain = GetTerrainAtPosition(terrains, entrancePosition.x, entrancePosition.z);

            if (entranceTerrain == null) {
                continue;
            }

            entrancePosition.y = SampleHeight(terrains, entrancePosition.x, entrancePosition.z);

            if (!HasMountainBehind(terrains, entrancePosition, downhill, out float mountainRise)) {
                continue;
            }

            Vector3 nearMountainPoint = entrancePosition - downhill * 25f;

            if (GetTerrainAtPosition(terrains, nearMountainPoint.x, nearMountainPoint.z) == null) {
                continue;
            }

            float nearMountainRise = SampleHeight(terrains, nearMountainPoint.x, nearMountainPoint.z) - entrancePosition.y;

            if (nearMountainRise < 7f) {
                continue;
            }

            if (!HasWalkableApproach(terrains, entrancePosition, downhill)) {
                continue;
            }

            float approachSlope = GetSlope(entranceTerrain, entrancePosition.x, entrancePosition.z);
            float score = nearMountainRise * 3f + mountainRise * 0.1f - elevation * 0.15f - approachSlope * 2f + (float)random.NextDouble() * 3f;

            if (score > bestScore) {
                bestScore = score;
                bestPosition = entrancePosition;
                bestDownhill = downhill;
                found = true;
            }
        }

        if (!found) {
            return;
        }

        ClearGeneratedEntrance();

        GameObject root = new GameObject(GeneratedName);
        root.transform.SetParent(transform, true);
        root.transform.SetPositionAndRotation(bestPosition, Quaternion.LookRotation(bestDownhill, Vector3.up));
        
        BuildEntrance(root.transform, random, terrains);
        ClearSnowTreesAroundEntrance(bestPosition, bestDownhill);
    }

    private void BuildEntrance(Transform root, System.Random random, Terrain[] terrains) {
        float halfWidth = openingWidth * 0.5f;
        float shoulderHeight = openingHeight * 0.42f;
        float archRise = openingHeight - shoulderHeight;
        float frontZ = facadeDepth * 0.38f;

        CreateDarkOpening(root, new Vector3(0f, 0.15f, frontZ - 1.2f), openingWidth * 0.98f, openingHeight * 0.98f);

        for (int side = -1; side <= 1; side += 2) {
            CreateBlock(root, "Recess Side", new Vector3(side * (halfWidth + 1.5f), shoulderHeight * 0.5f, -0.6f), new Vector3(3.5f, shoulderHeight + 2f, facadeDepth + 3f), darkRockMaterial);

            for (int i = 0; i < 3; i++) {
                float x = side * (halfWidth + 1.15f + RandomRange(random, -0.5f, 0.6f));
                float z = frontZ + RandomRange(random, -0.5f, 0.5f);
                float groundY = GetLocalGroundHeight(root, terrains, x, z);
                float y = groundY + (i + 0.5f) * shoulderHeight / 3f;

                CreateLowPolyRock(root, "Side Arch Rock", new Vector3(x, y, z), new Vector3(4.2f, shoulderHeight / 2.3f, facadeDepth * 1.25f), i % 2 == 0 ? rockMaterial : darkRockMaterial, random);
            }
        }

        for (int i = 0; i <= 10; i++) {
            float angle = Mathf.PI * (1f - i / 10f);
            float x = Mathf.Cos(angle) * (halfWidth + 0.8f);
            float y = shoulderHeight + Mathf.Sin(angle) * archRise;
            float z = frontZ + RandomRange(random, -0.35f, 0.35f);

            CreateLowPolyRock(root, "Cave Arch Rock", new Vector3(x, y, z), new Vector3(4.1f, 4.4f, facadeDepth * 1.3f), i % 3 == 0 ? darkRockMaterial : rockMaterial, random);

            if (i >= 3 && i <= 7 && i % 2 == 1) {
                CreateLowPolyRock(root, "Snow on Arch", new Vector3(x, y + 1.85f, z + 0.2f), new Vector3(3f, 1.1f, 3.4f), snowMaterial, random);
            }
        }

        for (int i = 0; i < 12; i++) {
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * RandomRange(random, halfWidth + 2.5f, halfWidth + 10f);
            float z = RandomRange(random, -2f, 8f);
            float scale = RandomRange(random, 4f, 8f);
            float groundY = GetLocalGroundHeight(root, terrains, x, z);

            CreateLowPolyRock(root, "Mountain Base Rock", new Vector3(x, groundY + scale * 0.2f, z), new Vector3(scale * 1.45f, scale, scale * 1.2f), i % 4 == 0 ? snowMaterial : rockMaterial, random);
        }

        for (int side = -1; side <= 1; side += 2) {
            for (int i = 0; i < 5; i++) {
                float z = -3f - i * 6f;
                float x = side * (halfWidth + 2f + i * 0.8f);

                float groundY = GetLocalGroundHeight(root, terrains, x, z);
                float rockHeight = Mathf.Lerp(openingHeight * 0.8f, 4f, i / 4f);

                CreateLowPolyRock(root, "Mountain Attachment Rock", new Vector3(x, groundY + rockHeight * 0.25f, z), new Vector3(10f, rockHeight, 15f), i % 3 == 0 ? darkRockMaterial : rockMaterial, random);
            }
        }

        for (int i = 0; i < 3; i++) {
            float z = -9f - i * 8f;
            float groundY = GetLocalGroundHeight(root, terrains, 0f, z);
            float y = Mathf.Max(openingHeight + 1f, groundY + 2f);

            CreateLowPolyRock(root, "Mountain Roof Rock", new Vector3(0f, y, z), new Vector3(openingWidth + 5f, 8f, 12f), rockMaterial, random);
        }

        for (int side = -1; side <= 1; side += 2) {
            for (int i = 0; i < 8; i++) {
                float z = -2f - i * 4f;
                float x = side * (halfWidth + RandomRange(random, 1f, 5f));
                float groundY = GetLocalGroundHeight(root, terrains, x, z);
                float scale = RandomRange(random, 7f, 11f);

                CreateLowPolyRock(root, "Cave Side Filler", new Vector3(x, groundY + scale * 0.3f, z), new Vector3(scale, scale * 1.3f, scale * 1.5f), i % 3 == 0 ? darkRockMaterial : rockMaterial, random);
            }
        }

        for (int i = 0; i < 9; i++) {
            float x = Mathf.Lerp(-halfWidth, halfWidth, i / 8f);
            float z = RandomRange(random, -8f, -2f);
            float y = openingHeight + RandomRange(random, 3f, 6f);

            CreateLowPolyRock(root, "Cave Roof Filler", new Vector3(x, y, z), new Vector3(9f, 7f, 12f), i % 3 == 0 ? darkRockMaterial : rockMaterial, random);
        }
    }

    private void CreateDarkOpening(Transform parent, Vector3 position, float width, float height) {
        float halfWidth = width * 0.5f;
        float shoulderHeight = height * 0.42f;
        const int segments = 10;

        List<Vector3> outline = new List<Vector3> { new Vector3(-halfWidth, 0f, 0f), new Vector3(halfWidth, 0f, 0f) };

        for (int i = 0; i <= segments; i++) {
            float angle = Mathf.PI * i / segments;
            outline.Add(new Vector3(Mathf.Cos(angle) * halfWidth, shoulderHeight + Mathf.Sin(angle) * (height - shoulderHeight), 0f));
        }

        Vector3[] vertices = new Vector3[outline.Count + 1];
        vertices[0] = new Vector3(0f, height * 0.5f, 0f);

        for (int i = 0; i < outline.Count; i++) {
            vertices[i + 1] = outline[i];
        }

        int[] triangles = new int[outline.Count * 3];

        for (int i = 0; i < outline.Count; i++) {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % outline.Count + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = "SnowCaveOpeningMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        generatedMeshes.Add(mesh);

        GameObject opening = new GameObject("Dark Cave Opening");
        opening.transform.SetParent(parent, false);
        opening.transform.localPosition = position;
        opening.AddComponent<MeshFilter>().sharedMesh = mesh;
        opening.AddComponent<MeshRenderer>().sharedMaterial = caveOpeningMaterial;
    }


    private Mesh GetRockMesh() {
        if (sharedRockMesh != null) {
            return sharedRockMesh;
        }

        Vector3 top = new Vector3(0f, 0.55f, 0f);
        Vector3 bottom = new Vector3(0.03f, -0.52f, -0.02f);

        Vector3[] ring = {
        new Vector3(-0.55f, 0.04f, 0.34f),
        new Vector3(0.03f, 0.08f, 0.55f),
        new Vector3(0.55f, -0.04f, 0.28f),
        new Vector3(0.48f, -0.08f, -0.44f),
        new Vector3(-0.18f, 0.02f, -0.55f),
        new Vector3(-0.6f, -0.06f, -0.14f) };

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < ring.Length; i++) {
            int next = (i + 1) % ring.Length;
            AddRockTriangle(vertices, triangles, top, ring[i], ring[next]);
            AddRockTriangle(vertices, triangles, bottom, ring[next], ring[i]);
        }

        sharedRockMesh = new Mesh();
        sharedRockMesh.name = "SnowCaveLowPolyRockMesh";
        sharedRockMesh.SetVertices(vertices);
        sharedRockMesh.SetTriangles(triangles, 0);
        sharedRockMesh.RecalculateNormals();
        generatedMeshes.Add(sharedRockMesh);

        return sharedRockMesh;
    }

    private static void AddRockTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c) {
        int start = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);

        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
    }

    private GameObject CreateLowPolyRock(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material, System.Random random) {
        GameObject rock = new GameObject(objectName);
        rock.transform.SetParent(parent, false);
        rock.transform.localPosition = position;
        rock.transform.localRotation = Quaternion.Euler(RandomRange(random, -12f, 12f), RandomRange(random, -180f, 180f), RandomRange(random, -15f, 15f));
        rock.transform.localScale = scale;

        rock.AddComponent<MeshFilter>().sharedMesh = GetRockMesh();
        rock.AddComponent<MeshRenderer>().sharedMaterial = material;

        return rock;
    }


    private static float GetLocalGroundHeight(Transform root, Terrain[] terrains, float localX, float localZ) {
        Vector3 world = root.TransformPoint(new Vector3(localX, 0f, localZ));
        return SampleHeight(terrains, world.x, world.z) - root.position.y;
    }

    private bool HasMountainBehind(Terrain[] terrains, Vector3 entrance, Vector3 downhill, out float mountainRise) {
        mountainRise = 0f;

        float[] distances = { 40f, 100f, 180f };
        float[] minimumRises = { 12f, 40f, 85f };

        for (int i = 0; i < distances.Length; i++) {
            Vector3 point = entrance - downhill * distances[i];
            Terrain terrain = GetTerrainAtPosition(terrains, point.x, point.z);

            if (terrain == null) {
                return false;
            }

            biomeGenerator.GetBiomeBlend(point.x, point.z, out float snow, out float plains, out float desert, out float swamp);

            if (snow < 0.8f) {
                return false;
            }

            float height = SampleHeight(terrains, point.x, point.z);
            float rise = height - entrance.y;

            if (rise < minimumRises[i]) {
                return false;
            }

            mountainRise = rise;
        }

        return true;
    }



    private GameObject CreateBlock(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material) {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = objectName;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localScale = scale;
        block.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = block.GetComponent<Collider>();

        if (Application.isPlaying) {
            Destroy(collider);
        }
        else {
            DestroyImmediate(collider);
        }

        return block;
    }

    private static float RandomRange(System.Random random, float minimum, float maximum) {
        return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }

    private static Terrain GetTerrainAtPosition(Terrain[] terrains, float x, float z) {
        foreach (Terrain terrain in terrains) {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            if (x >= origin.x && x <= origin.x + size.x && z >= origin.z && z <= origin.z + size.z) {
                return terrain;
            }
        }
        return null;
    }

    private static float SampleHeight(Terrain[] terrains, float x, float z) {
        Terrain terrain = GetTerrainAtPosition(terrains, x, z);
        if (terrain == null) {
            return 0f;
        }
        return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
    }

    private void ClearGeneratedEntrance() {
        Transform previous = transform.Find(GeneratedName);
        if (previous != null) {
            if (Application.isPlaying) {
                Destroy(previous.gameObject);
            }
            else {
                DestroyImmediate(previous.gameObject);
            }
        }

        foreach (Mesh mesh in generatedMeshes) {
            if (mesh == null) {
                continue;
            }

            if (Application.isPlaying) {
                Destroy(mesh);
            }
            else {
                DestroyImmediate(mesh);
            }
        }

        generatedMeshes.Clear();
        sharedRockMesh = null;
    }

    private static Vector3 CalculateDownhill(Terrain[] terrains, float x, float z) {
        const float distance = 12f;

        float left = SampleHeight(terrains, x - distance, z);
        float right = SampleHeight(terrains, x + distance, z);
        float backward = SampleHeight(terrains, x, z - distance);
        float forward = SampleHeight(terrains, x, z + distance);

        return new Vector3(left - right, 0f, backward - forward).normalized;
    }

    private bool HasWalkableApproach(Terrain[] terrains, Vector3 entrance, Vector3 downhill) {
        float lastHeight = entrance.y;

        for (int i = 0; i <= 3; i++) {
            float distance = i * 12f;
            Vector3 point = entrance + downhill * distance;

            Terrain terrain = GetTerrainAtPosition(terrains, point.x, point.z);

            if (terrain == null || GetSlope(terrain, point.x, point.z) > maximumApproachSlope) {
                return false;
            }

            biomeGenerator.GetBiomeBlend(point.x, point.z, out float snow, out float plains, out float desert, out float swamp);

            if (snow < minimumSnowWeight) {
                return false;
            }

            float height = SampleHeight(terrains, point.x, point.z);

            if (i > 0 && Mathf.Abs(height - lastHeight) > 12f * Mathf.Tan(maximumApproachSlope * Mathf.Deg2Rad)) {
                return false;
            }

            lastHeight = height;
        }

        return true;
    }

    private static float GetSlope(Terrain terrain, float x, float z) {
        Vector3 origin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;

        float u = Mathf.Clamp01((x - origin.x) / size.x);
        float v = Mathf.Clamp01((z - origin.z) / size.z);

        return terrain.terrainData.GetSteepness(u, v);
    }

    private void ClearSnowTreesAroundEntrance(Vector3 entrancePosition, Vector3 downhill) {
        if (environmentSpawner == null) {
            return;
        }

        Transform chunks = environmentSpawner.transform.Find("GeneratedVegetation/Chunks");

        if (chunks == null) {
            return;
        }

        Vector2 entrance = new Vector2(entrancePosition.x, entrancePosition.z);
        Vector2 forward = new Vector2(downhill.x, downhill.z).normalized;

        int removed = 0;

        foreach (Transform chunk in chunks) {
            Transform trees = chunk.Find("Trees");

            if (trees == null) {
                continue;
            }

            for (int i = trees.childCount - 1; i >= 0; i--) {
                Transform tree = trees.GetChild(i);

                if (tree == null) {
                    continue;
                }

                Vector2 position = new Vector2(tree.position.x, tree.position.z);
                Vector2 offset = position - entrance;

                bool nearEntrance = offset.sqrMagnitude <= entranceClearRadius * entranceClearRadius;

                float forwardDistance = Vector2.Dot(offset, forward);
                float sidewaysDistance = Mathf.Abs(forward.x * offset.y - forward.y * offset.x);

                bool blocksApproach = forwardDistance >= 0f &&
                                      forwardDistance <= approachClearLength &&
                                      sidewaysDistance <= approachClearWidth;

                if (!nearEntrance && !blocksApproach) {
                    continue;
                }

                biomeGenerator.GetBiomeBlend(tree.position.x, tree.position.z, out float snow, out float plains, out float desert, out float swamp);

                if (snow < 0.8f) {
                    continue;
                }

                if (Application.isPlaying) {
                    Destroy(tree.gameObject);
                }
                else {
                    DestroyImmediate(tree.gameObject);
                }

                removed++;
            }
        }
    }
}


