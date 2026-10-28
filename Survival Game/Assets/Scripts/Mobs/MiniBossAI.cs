
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// Controls the mini-boss's movement, targeting and melee attacks.
/// Uses NavMesh pathfinding to chase the player within detection range,
/// and scales attack damage with the current world difficulty.
[RequireComponent(typeof(NavMeshAgent))]
public class MiniBossAI : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform playerTarget;

    [Header("Detection Settings")]
    public float detectionRange = 15f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 5f;
    public float navMeshSampleRadius = 5f;
    public float pathUpdateInterval = 0.5f;

    [Header("Combat Settings")]
    public float attackRange = 3.2f;
    public float attackCooldown = 1f;
    public float attackDamage = 15f;
    private float nextAttackTime = 0f;

    [Header("Weapon Settings")]
    public Transform swordTransform;
    private bool isAttacking = false;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;

    private NavMeshAgent agent;
    private NavMeshPath testPath;
    private float nextPathUpdateTime = 0f;
    private float nextDebugTime;

    /// Initialises NavMesh navigation and finds the player if no
    /// target has been assigned in the Inspector.
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        testPath = new NavMeshPath();

        agent.speed = moveSpeed;

        // Navigation stops near the sampled destination.
        // Combat distance is controlled separately by attackRange.
        agent.stoppingDistance = 0.5f;

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("MiniBoss is not positioned on a NavMesh.");
        }
    }

    /// Checks the player's distance every frame.
    /// Chases only within detection range and attacks when close enough.
    void Update()
    {
        if (playerTarget == null || agent == null ||
            !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0f;

        float distanceToPlayer = direction.magnitude;

        // Stop chasing if the player moves outside the detection radius.
        // Clear the old path so the boss cannot follow across the map.
        if (distanceToPlayer > detectionRange)
        {
            StopChasing();
            return;
        }

        if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            // Pause movement during combat without deleting the existing path.
            agent.isStopped = true;

            LookAtTarget();

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(SwingSwordRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void LateUpdate()
    {
        if (agent == null || !agent.isOnNavMesh || playerTarget == null)
            return;

        if (Time.time < nextDebugTime)
            return;

        nextDebugTime = Time.time + 1f;

        Vector3 offset = playerTarget.position - transform.position;
        offset.y = 0f;

        Debug.Log(
            "BOSS CHASE DEBUG: " +
            "PlayerDistance=" + offset.magnitude.ToString("F2") +
            " | Stopped=" + agent.isStopped +
            " | HasPath=" + agent.hasPath +
            " | Pending=" + agent.pathPending +
            " | Path=" + agent.pathStatus +
            " | Remaining=" + agent.remainingDistance.ToString("F2") +
            " | Velocity=" + agent.velocity.magnitude.ToString("F2") +
            " | PlayerPos=" + playerTarget.position.ToString("F2") +
            " | Destination=" + agent.destination.ToString("F2") +
            " | TargetToDestination=" +
            Vector3.Distance(playerTarget.position, agent.destination).ToString("F2")
        );
    }

    /// Finds a reachable NavMesh point near the player.
    /// Updates the destination periodically to avoid repeated
    /// path calculations interrupting the boss's movement.
    void ChasePlayer()
    {
        agent.isStopped = false;

        if (agent.pathPending || Time.time < nextPathUpdateTime)
            return;

        nextPathUpdateTime = Time.time + pathUpdateInterval;

        Vector3 sampleOrigin = playerTarget.position;

        if (Physics.Raycast(
            playerTarget.position + Vector3.up * 2f,
            Vector3.down,
            out RaycastHit groundHit,
            10f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            sampleOrigin = groundHit.point;
        }

        if (!NavMesh.SamplePosition(
            sampleOrigin,
            out NavMeshHit hit,
            navMeshSampleRadius,
            agent.areaMask))
        {
            Debug.LogWarning("BOSS DESTINATION: Sampling failed");
            return;
        }

        bool calculated = agent.CalculatePath(hit.position, testPath);

        // Log the sampled destination separately from the current destination
        // so we can see whether Unity is rejecting an updated target.
        Debug.Log(
            "BOSS DESTINATION: " +
            "Player=" + playerTarget.position.ToString("F2") +
            " | Sampled=" + hit.position.ToString("F2") +
            " | SampleDistance=" +
            Vector3.Distance(playerTarget.position, hit.position).ToString("F2") +
            " | Calculated=" + calculated +
            " | Status=" + testPath.status
        );

        if (calculated && testPath.status == NavMeshPathStatus.PathComplete)
        {
            if (!agent.hasPath ||
                Vector3.Distance(agent.destination, hit.position) > 0.25f)
            {
                agent.SetDestination(hit.position);
            }
        }
    }


    /// Stops the boss when the player is outside detection range
    /// or close enough for combat.
    void StopChasing()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;

        // Remove the destination so the boss doesn't keep moving
        // towards a player it can no longer detect.
        if (agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    /// Smoothly rotates the mini-boss to face the player.
    /// Ignores vertical direction so the boss remains upright.
    void LookAtTarget()
    {
        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    /// Animates the sword swing and checks whether the player is in range.
    /// Uses a coroutine to spread the animation over multiple frames
    /// rather than instantly changing the sword's rotation.
    IEnumerator SwingSwordRoutine()
    {
        isAttacking = true;

        if (swordTransform != null)
        {
            Quaternion originalRotation = swordTransform.localRotation;
            Quaternion attackRotation =
                originalRotation * Quaternion.Euler(-90f, 0f, 0f);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(
                    originalRotation,
                    attackRotation,
                    elapsed / duration
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Check range when the swing reaches its target so the player
            // can avoid damage by moving away before the hit connects.
            if (playerTarget != null &&
                Vector3.Distance(transform.position, playerTarget.position)
                    <= attackRange + 0.5f)
            {
                PlayerStats stats = playerTarget.GetComponent<PlayerStats>();

                if (stats != null)
                {
                    float difficulty = worldDifficultyManager != null
                        ? worldDifficultyManager.worldDifficulty
                        : 1f;

                    // Scale boss damage with world difficulty to increase
                    // the challenge as the player progresses.
                    float finalDamage = attackDamage * difficulty;

                    stats.TakeDamage(finalDamage);
                }
            }

            // Brief pause before returning the sword to its starting position.
            yield return new WaitForSeconds(0.2f);

            elapsed = 0f;

            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(
                    attackRotation,
                    originalRotation,
                    elapsed / duration
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restore the exact starting rotation to avoid animation drift.
            swordTransform.localRotation = originalRotation;
        }

        isAttacking = false;
    }


    void OnDrawGizmosSelected()
    {
        if (playerTarget == null)
            return;

        // Draw the player's position so we can compare it
        // with the nearest navigable point in the Scene view.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTarget.position, 0.5f);

        if (NavMesh.SamplePosition(
            playerTarget.position,
            out NavMeshHit hit,
            5f,
            NavMesh.AllAreas))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hit.position, 0.5f);
            Gizmos.DrawLine(playerTarget.position, hit.position);
        }
    }

}
