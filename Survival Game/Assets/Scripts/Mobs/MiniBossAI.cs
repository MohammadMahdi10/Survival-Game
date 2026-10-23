using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;

/// Controls the mini-boss's movement, targeting and melee attacks.
/// Uses NavMesh navigation to chase the player when detected,
/// and scales attack damage with the current world difficulty.
public class MiniBossAI : MonoBehaviour
{

    [Header("Target Settings")]
    public Transform playerTarget;

    [Header("Detection Settings")]
    public float detectionRange = 15f;

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

    /// Initialises the NavMesh agent and finds the player if no
    /// target has been assigned, allowing the boss to track the player.
    void Start()
    {
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }

        agent = GetComponent<NavMeshAgent>();

        if (agent != null) {
            agent.stoppingDistance = attackRange;
        }

        if (playerTarget == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                playerTarget = player.transform;
            }
        }
    }

    /// Checks the distance to the player each frame.
    /// Stops chasing outside detection range, approaches the player
    /// within detection range, and attacks when close enough.
    void Update()
    {
        if (playerTarget != null && agent != null && agent.isOnNavMesh)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= detectionRange)
            {
                agent.SetDestination(playerTarget.position);
            }
        }
        if (playerTarget == null) { return; }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > detectionRange)
        {
            if (agent != null && agent.isActiveAndEnabled && !agent.isStopped)
            {
                agent.isStopped = true;
            }
            return;
        }

        if(distanceToPlayer > attackRange)
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
            }
        }
        else
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
            }

            LookAtTarget();

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(SwingSwordRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    /// Smoothly rotates the mini-boss to face the player.
    /// Ignores vertical direction so the boss stays upright
    /// rather than tilting towards the player's height.
    void LookAtTarget() { 
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

        if (swordTransform != null)
        {
            Quaternion originalRotation = swordTransform.localRotation;
            Quaternion attackRotation = originalRotation * Quaternion.Euler(-90f, 0f, 0f);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(originalRotation, attackRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Check range when the swing reaches its target so the player
            // can avoid damage by moving away before the hit connects.

            if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.5f)
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


            // Brief pause before returning the sword to its starting position.
            yield return new WaitForSeconds(0.2f);

            elapsed = 0f;
            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(attackRotation, originalRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }


            // Restore the exact starting rotation to avoid animation drift.
            swordTransform.localRotation = originalRotation;
        }
        isAttacking = false;
    }
}
