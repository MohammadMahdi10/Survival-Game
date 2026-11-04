using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// Controls the mini-boss's movement, targeting and melee attacks.
/// Uses NavMesh navigation to chase the player when detected,
/// and scales attack damage with the current world difficulty.
public class MiniBossAI : MonoBehaviour
{

    [Header("Target Settings")]
    public Transform playerTarget;
    public string playerTag = "Player";

    [Header("Detection Settings")]
    public float detectionRange = 15f;

    [Header("Combat Settings")]
    public float attackRange = 3.2f;
    public float attackCooldown = 1f;
    public float attackDamage = 15f;
    private float nextAttackTime = 0f;

    [Header("Weapon Settings")]
    public Transform swordTransform;
    public Animator animator;
    private bool isAttacking = false;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;

    private NavMeshAgent agent;
    private Vector3 lastDestination;

    /// Initialises the NavMesh agent and finds the player if no
    /// target has been assigned, allowing the boss to track the player.
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find the player in the scene if not assigned
        FindPlayerTarget();

        // Match the stopping distance to attack range so the boss stops in front of the player
        if (agent != null)
        {
            agent.stoppingDistance = Mathf.Max(1.0f, attackRange - 0.5f);
        }

        // Grab the world difficulty manager if missing
        if (worldDifficultyManager == null)
        {
            worldDifficultyManager = FindFirstObjectByType<WorldDifficultyManager>();
        }

        // Make sure the boss starts properly grounded on the walking mesh
        SnapToNavMesh();
    }

    /// Checks the distance to the player each frame.
    /// Stops chasing outside detection range, approaches the player
    /// within detection range, and attacks when close enough.
    void Update()
    {
        if (playerTarget == null) {
            FindPlayerTarget();
            if (playerTarget == null) {
                return; // No player found, exit early
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > detectionRange)
        {
            StopAgent();
            return;
        }

        if (distanceToPlayer > attackRange)
        {
            ChaseTarget();
        }
        else
        {
            StopAgent();
            LookAtTarget();

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(SwingSwordRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        if (animator != null && agent != null)
        {
            bool running = agent.hasPath && agent.velocity.sqrMagnitude > 0.1f && !agent.isStopped;
            animator.SetBool("IsRunning", running);
        }
    }

    private void FindPlayerTarget()
    {
        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindWithTag(playerTag);
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
        }
    }

    private void SnapToNavMesh()
    {
        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                agent.enabled = false; // Temporarily disable the agent to move it
                transform.position = hit.position;
                agent.enabled = true; // Re-enable the agent
            }
        }
    }

    private void ChaseTarget()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) 
        {
            return;
        }
        agent.isStopped = false;

        if(Vector3.Distance(lastDestination, playerTarget.position) > 0.5f)
        {
            if (NavMesh.SamplePosition(playerTarget.position, out NavMeshHit hit, 5f, agent.areaMask))
            {
                agent.SetDestination(hit.position);
                lastDestination = hit.position;
            }
        }

        LookAtTarget();
    }

    private void StopAgent()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    /// Smoothly rotates the mini-boss to face the player.
    /// Ignores vertical direction so the boss stays upright
    /// rather than tilting towards the player's height.
    private void LookAtTarget() { 

        if (playerTarget == null) {
            return;
        }

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// Animates the sword swing and checks whether the player is in range.
    /// Uses a coroutine to spread the attack animation over multiple frames
    /// instead of instantly changing the sword's rotation.
    IEnumerator SwingSwordRoutine() {
        isAttacking = true;
        Quaternion originalRotation = swordTransform != null ? swordTransform.localRotation : Quaternion.identity;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(0.4f); // Wait for the animation to start before swinging
        }

        else if (swordTransform != null)
        {
            originalRotation = swordTransform.localRotation;
            Quaternion attackRotation = originalRotation * Quaternion.Euler(-90f, 0f, 0f);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(originalRotation, attackRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.1f); // Small pause at the peak of the swingw
        }
        // Check range when the swing reaches its target so the player
        // can avoid damage by moving away before the hit connects.

        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.8f)
        {
            PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
            if (stats != null) {
                float difficulty = worldDifficultyManager != null ? worldDifficultyManager.worldDifficulty : 1f;


                // Scale boss damage with world difficulty to increase
                // the challenge as the player progresses.
                float finalDamage = attackDamage * difficulty;

                stats.TakeDamage(finalDamage);
            }
        }

        if (animator == null && swordTransform != null)
        {
            Quaternion attackRotation = swordTransform.localRotation;
            float duration = 0.35f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(attackRotation, originalRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            swordTransform.localRotation = originalRotation;
        }
        isAttacking = false;
    }

    // Draws yellow (detection) and red (attack) range circles in Unity Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
