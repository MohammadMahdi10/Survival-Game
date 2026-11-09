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
    public float detectionRange = 10f;

    [Header("Combat Settings")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.2f;
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
        if (playerTarget == null)
        {
            FindPlayerTarget();
            if (playerTarget == null)
            {
                return; // No player found, exit early
            }
        }

        Vector3 bossPos = transform.position;
        Vector3 playerPos = playerTarget.position;
        bossPos.y = 0f; // Ignore vertical distance for detection
        playerPos.y = 0f; // Ignore vertical distance for detection

        float horizontalDistance = Vector3.Distance(bossPos, playerPos);

        if (horizontalDistance > detectionRange)
        {
            StopAgent();
            UpdateAnimator(false);
            return;
        }

        if (horizontalDistance > attackRange)
        {
            ChaseTarget();
            UpdateAnimator(true);
        }
        else
        {
            StopAgent();
            LookAtTarget();
            UpdateAnimator(false);

            if (Time.time >= nextAttackTime && !isAttacking && horizontalDistance <= attackRange)
            {
                TriggerAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void UpdateAnimator(bool isRunning)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("IsRunning", isRunning);

        if (isRunning)
        {
            animator.ResetTrigger("Attack");
        }
    }

    private void TriggerAttack()
    {
        isAttacking = true;
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        StartCoroutine(ResetAttackFlag());
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

    public void HitPlayer()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= attackRange + 1.0f)
        {
            PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
            if (stats != null)
            {
                float difficulty = worldDifficultyManager != null ? worldDifficultyManager.worldDifficulty : 1f;
                float finalDamage = attackDamage * difficulty;
                stats.TakeDamage(finalDamage);
            }
        }
    }

    private IEnumerator ResetAttackFlag()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
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

        if (Vector3.Distance(lastDestination, playerTarget.position) > 0.5f)
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
    private void LookAtTarget()
    {

        if (playerTarget == null)
        {
            return;
        }

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// Animates the sword swing and checks whether the player is in range.
    /// Uses a coroutine to spread the attack animation over multiple frames
    /// instead of instantly changing the sword's rotation.
    //IEnumerator SwingSwordRoutine() {
    //    isAttacking = true;
    //    Quaternion originalRotation = swordTransform != null ? swordTransform.localRotation : Quaternion.identity;

    //    if (animator != null)
    //    {
    //        animator.SetTrigger("Attack");
    //        yield return new WaitForSeconds(0.4f); // Wait for the animation to start before swinging
    //    }

    //    else if (swordTransform != null)
    //    {
    //        originalRotation = swordTransform.localRotation;
    //        Quaternion attackRotation = originalRotation * Quaternion.Euler(-90f, 0f, 0f);

    //        float duration = 0.5f;
    //        float elapsed = 0f;

    //        while (elapsed < duration)
    //        {
    //            swordTransform.localRotation = Quaternion.Slerp(originalRotation, attackRotation, elapsed / duration);
    //            elapsed += Time.deltaTime;
    //            yield return null;
    //        }

    //        yield return new WaitForSeconds(0.1f); // Small pause at the peak of the swingw
    //    }
    //    // Check range when the swing reaches its target so the player
    //    // can avoid damage by moving away before the hit connects.

    //    if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.8f)
    //    {
    //        PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
    //        if (stats != null) {
    //            float difficulty = worldDifficultyManager != null ? worldDifficultyManager.worldDifficulty : 1f;


    //            // Scale boss damage with world difficulty to increase
    //            // the challenge as the player progresses.
    //            float finalDamage = attackDamage * difficulty;

    //            stats.TakeDamage(finalDamage);
    //        }
    //    }

    //    if (animator == null && swordTransform != null)
    //    {
    //        Quaternion attackRotation = swordTransform.localRotation;
    //        float duration = 0.35f;
    //        float elapsed = 0f;
    //        while (elapsed < duration)
    //        {
    //            swordTransform.localRotation = Quaternion.Slerp(attackRotation, originalRotation, elapsed / duration);
    //            elapsed += Time.deltaTime;
    //            yield return null;
    //        }
    //        swordTransform.localRotation = originalRotation;
    //    }
    //    isAttacking = false;
    //}
}