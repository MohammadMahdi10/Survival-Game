using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCrashSpawn : MonoBehaviour {
    [Header("References")]
    [SerializeField] private ProceduralBiomeGenerator biomeGenerator;
    [SerializeField] private Transform player;
    [SerializeField] private Transform crashedSpaceship;
    [SerializeField] private ProceduralSnowCave snowCaveGenerator;

    [Header("Landmark Exclusion Zones")]
    [SerializeField] private float snowCaveClearance = 250f;
    [SerializeField] private float plainsMountainClearance = 200f;

    [Header("Playable Inner World")]
    [SerializeField] private float minimumRadius = 0f;
    [SerializeField] private float maximumRadius = 3300f;
    [SerializeField] private int placementAttempts = 3000;

    [Header("Flat Ground Requirements")]
    [SerializeField] private float spaceshipFootprintRadius = 20f;
    [SerializeField] private float maximumGroundSlope = 6f;
    [SerializeField] private float maximumHeightDifference = 2.5f;
    [SerializeField] private bool avoidSwampWater = true;

    [Header("Player Placement")]
    [SerializeField] private float playerDistanceFromSpaceship = 32f;
    [SerializeField] private float spaceshipGroundOffset = 0f;
    [SerializeField] private float playerGroundOffset = 0.05f;

    [ContextMenu("Place Player And Spaceship")]
    public void PlacePlayerAndSpaceship() {
        if (biomeGenerator == null || player == null || crashedSpaceship == null) {
            return;
        }

        Terrain[] terrains = { biomeGenerator.snowTerrain, biomeGenerator.plainsTerrain, biomeGenerator.desertTerrain, biomeGenerator.swampTerrain };

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Terrain terrain in terrains) {
            if (terrain == null || terrain.terrainData == null) {
                return;
            }

            Vector3 origin = terrain.GetPosition();
            Vector3 size = terrain.terrainData.size;
            minX = Mathf.Min(minX, origin.x);
            minZ = Mathf.Min(minZ, origin.z);
            maxX = Mathf.Max(maxX, origin.x + size.x);
            maxZ = Mathf.Max(maxZ, origin.z + size.z);
        }

        if (maximumRadius <= minimumRadius || spaceshipFootprintRadius <= 0f || playerDistanceFromSpaceship <= 0f)
        {
            return;
        }

        Vector2 centre = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        System.Random random = new System.Random(unchecked(biomeGenerator.seed + 71933));
        if (snowCaveGenerator == null)
        {
            return;
        }

        Transform caveEntrance = snowCaveGenerator.transform.Find("GeneratedSnowCaveEntrance");

        if (caveEntrance == null)
        {
            return;
        }

        for (int attempt = 0; attempt < placementAttempts; attempt++)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float radius = Mathf.Sqrt(Mathf.Lerp(minimumRadius * minimumRadius, maximumRadius * maximumRadius, (float)random.NextDouble()));
            Vector2 shipXZ = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            if (biomeGenerator.IsNearPlainsBossMountain(shipXZ.x, shipXZ.y, plainsMountainClearance + spaceshipFootprintRadius))
            {
                continue;
            }
            if (IsNearLandmark(shipXZ, spaceshipFootprintRadius, caveEntrance)) continue;
            if (!IsFlat(terrains, shipXZ, spaceshipFootprintRadius, out float shipY)) continue;

            float facing = (float)random.NextDouble() * Mathf.PI * 2f;
            Vector2 forwardXZ = new Vector2(Mathf.Sin(facing), Mathf.Cos(facing));
            Vector2 playerXZ = shipXZ + forwardXZ * playerDistanceFromSpaceship;

            if (biomeGenerator.IsNearPlainsBossMountain(playerXZ.x, playerXZ.y, plainsMountainClearance + 3f))
            {
                continue;
            }
            if (IsNearLandmark(playerXZ, 3f, caveEntrance)) continue;
            if (!IsFlat(terrains, playerXZ, 3f, out float playerY)) continue;


            if (Mathf.Abs(playerY - shipY) > maximumHeightDifference)
            { 
                continue;
            }


            crashedSpaceship.rotation = Quaternion.Euler(0f, facing * Mathf.Rad2Deg, 0f);

            float spaceshipOffset = GetGroundOffset(crashedSpaceship);
            crashedSpaceship.position = new Vector3(shipXZ.x, shipY + spaceshipOffset + spaceshipGroundOffset, shipXZ.y);

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            float playerOffset = GetGroundOffset(player);
            player.position = new Vector3(playerXZ.x, playerY + playerOffset + playerGroundOffset, playerXZ.y);

            Rigidbody playerBody = player.GetComponent<Rigidbody>();
            if (playerBody != null)
            {
                playerBody.position = player.position;
                if (!playerBody.isKinematic)
                {
                    playerBody.velocity = Vector3.zero;
                    playerBody.angularVelocity = Vector3.zero;
                }
            }

            if (controller != null) controller.enabled = true;

            return;
        }
    }


    private bool IsNearLandmark(Vector2 position, float objectRadius, Transform caveEntrance)
    {
        Vector2 caveXZ = new Vector2(caveEntrance.position.x, caveEntrance.position.z);

        if (Vector2.Distance(position, caveXZ) < snowCaveClearance + objectRadius)
        {
            return true;
        }

        if (biomeGenerator.IsNearPlainsBossMountain(position.x, position.y, plainsMountainClearance + objectRadius))
        {
            return true;
        }

        return false;
    }

    private float GetGroundOffset(Transform root)
    {
        CharacterController controller = root.GetComponent<CharacterController>();

        if (controller != null)
        {
            float feetY = root.TransformPoint(controller.center).y - controller.height * Mathf.Abs(root.lossyScale.y) * 0.5f;
            return root.position.y - feetY;
        }

        float lowestY = float.PositiveInfinity;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
        {
            if (!renderer.enabled) continue;
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;

            lowestY = Mathf.Min(lowestY, renderer.bounds.min.y);
        }

        if (float.IsPositiveInfinity(lowestY))
        {
            foreach (Collider collider in root.GetComponentsInChildren<Collider>())
            {
                if (!collider.enabled || collider.isTrigger) continue;
                lowestY = Mathf.Min(lowestY, collider.bounds.min.y);
            }
        }

        return float.IsPositiveInfinity(lowestY) ? 0f : root.position.y - lowestY;
    }

    private bool IsFlat(Terrain[] terrains, Vector2 centre, float radius, out float centreHeight)
    {
        centreHeight = 0f;
        if (!TrySample(terrains, centre, out centreHeight, out float centreSlope)) return false;
        if (centreSlope > maximumGroundSlope) return false;

        float lowest = centreHeight;
        float highest = centreHeight;

        for (int ring = 1; ring <= 2; ring++)
        {
            float sampleRadius = radius * ring * 0.5f;
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                Vector2 sampleXZ = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * sampleRadius;

                if (!TrySample(terrains, sampleXZ, out float height, out float slope)) return false;
                if (slope > maximumGroundSlope) return false;

                lowest = Mathf.Min(lowest, height);
                highest = Mathf.Max(highest, height);
                if (highest - lowest > maximumHeightDifference) return false;
            }
        }

        return true;
    }

    private bool TrySample(Terrain[] terrains, Vector2 position, out float height, out float slope)
    {
        foreach (Terrain terrain in terrains)
        {
            Vector3 origin = terrain.GetPosition();
            Vector3 size = terrain.terrainData.size;

            if (position.x < origin.x || position.x > origin.x + size.x ||
                position.y < origin.z || position.y > origin.z + size.z) continue;

            float u = Mathf.Clamp01((position.x - origin.x) / size.x);
            float v = Mathf.Clamp01((position.y - origin.z) / size.z);
            height = terrain.SampleHeight(new Vector3(position.x, 0f, position.y)) + origin.y;
            slope = terrain.terrainData.GetSteepness(u, v);

            if (avoidSwampWater && height <= biomeGenerator.SwampWaterLevel + 1f)
            {
                biomeGenerator.GetBiomeBlend(position.x, position.y, out _, out _, out _, out float swampWeight);
                if (swampWeight > 0.5f) return false;
            }

            return true;
        }

        height = 0f;
        slope = 0f;
        return false;
    }
}

