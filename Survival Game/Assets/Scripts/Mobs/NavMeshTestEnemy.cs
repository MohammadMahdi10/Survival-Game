using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// Tests NavMesh movement independently from combat and other enemy systems.
[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshTestEnemy : MonoBehaviour
{
    public Transform playerTarget;

    private NavMeshAgent agent;

    private float nextPathUpdateTime = 0f;
    private NavMeshPath testPath;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerTarget = player.transform;
        }

        Debug.Log("TEST ENEMY ON NAVMESH: " + agent.isOnNavMesh);
    }

    void Update()
    {
        if (playerTarget == null || agent == null || !agent.isOnNavMesh)
            return;

        if (testPath == null)
            testPath = new NavMeshPath();

        if (Time.time < nextPathUpdateTime || agent.pathPending)
            return;

        nextPathUpdateTime = Time.time + 0.5f;

        // Find a navigable point near the player.
        if (!NavMesh.SamplePosition(
            playerTarget.position,
            out NavMeshHit hit,
            5f,
            agent.areaMask))
        {
            Debug.Log("TEST NAV: No NavMesh near player");
            return;
        }

        // Reject destinations that cannot be reached fully.
        if (agent.CalculatePath(hit.position, testPath) &&
            testPath.status == NavMeshPathStatus.PathComplete)
        {
            if (!agent.hasPath ||
                Vector3.Distance(agent.destination, hit.position) > 1f)
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            Debug.Log("TEST NAV: Player destination is unreachable");
        }
    }

    private float nextDebugTime;

    void LateUpdate()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (Time.time < nextDebugTime)
            return;

        nextDebugTime = Time.time + 1f;

        bool playerOnNavMesh = NavMesh.SamplePosition(
            playerTarget.position,
            out NavMeshHit playerHit,
            1f,
            agent.areaMask
        );

        Debug.Log(
            "TEST NAV: " +
            "Position=" + transform.position.ToString("F3") +
            " | Remaining=" + agent.remainingDistance.ToString("F2") +
            " | Stopped=" + agent.isStopped +
            " | Pending=" + agent.pathPending +
            " | Path=" + agent.pathStatus +
            " | Velocity=" + agent.velocity.magnitude.ToString("F2") +
            " | PlayerNearNavMesh=" + playerOnNavMesh
        );
    }
}
