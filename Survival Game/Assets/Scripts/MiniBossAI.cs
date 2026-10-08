using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniBossAI : MonoBehaviour
{

    [Header("Target Settings")]
    public Transform playerTarget;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
        }
    }
}
