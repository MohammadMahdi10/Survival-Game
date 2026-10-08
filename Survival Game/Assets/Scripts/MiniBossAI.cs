using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniBossAI : MonoBehaviour
{

    [Header("Target Settings")]
    public Transform playerTarget;

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float nextAttackTime = 0f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null) {
            agent.stoppingDistance = attackRange;
        }
    }

    void Update()
    {
        if (playerTarget == null) { return; }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }

        else
        {
            agent.isStopped = true;

            LookAtTarget();

            if (Time.time >= nextAttackTime)
            {
                PerformAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void LookAtTarget() { 
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        // Keep the y-axis rotation unchanged

        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    // Placeholder for attack logic

    void PerformAttack() { 
        Debug.Log("MiniBoss attacks the player!");
    }
}
