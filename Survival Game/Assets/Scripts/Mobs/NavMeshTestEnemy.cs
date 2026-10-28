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
        if (playerTarget == null || !agent.isOnNavMesh)
            return;

        agent.SetDestination(playerTarget.position);
    }
}
