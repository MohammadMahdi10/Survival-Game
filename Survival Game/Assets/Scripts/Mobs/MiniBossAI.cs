using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;

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

    void Start()
    {
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

    void Update()
    {
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
        // ATTACK RANGE: Player is close enough to swing
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

    void LookAtTarget() { 
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        // Keep the y-axis rotation unchanged

        // Slerp: Smoothly rotate the MiniBoss towards the player
        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

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

            if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.5f)
            {
                PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
                if (stats != null) {
                    float difficulty = worldDifficultyManager != null ? worldDifficultyManager.worldDifficulty : 1f;

                    float finalDamage = attackDamage * difficulty;

                    stats.TakeDamage(finalDamage);
                }
            }

            yield return new WaitForSeconds(0.2f); // Wait for a short moment before returning to original position

            elapsed = 0f;
            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(attackRotation, originalRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            swordTransform.localRotation = originalRotation; // Ensure the sword returns to its original rotation
        }
        isAttacking = false;
    }
}
