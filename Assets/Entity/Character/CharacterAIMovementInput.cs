using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAIMovementInput : MonoBehaviour
{
    GameObject target;

    NavMeshAgent navMeshAgent;

    CharacterHealth characterHealth;
    CharacterCombat characterCombat;

    private enum EMovementStatus
    {
        Wandering,
        FollowingEnemy
    }
    private EMovementStatus movementStatus = EMovementStatus.FollowingEnemy;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        characterHealth = GetComponent<CharacterHealth>();
        characterCombat = GetComponent<CharacterCombat>();

        UpdateTarget();
    }

    private void Update()
    {
        //navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navMeshAgent.SetDestination(target.transform.position);
        navMeshAgent.isStopped = Vector3.Distance(transform.position, navMeshAgent.destination) < 2f || characterHealth.IsOnGround;
        
        if (Vector3.Distance(target.transform.position, transform.position) <= 1f)
        {
            characterCombat.RequestAttack(EAttackType.Weak);
        }
    }
    
    void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        target = players.OrderBy(p => Vector3.Distance(p.transform.position, transform.position)).FirstOrDefault();
    }

}
