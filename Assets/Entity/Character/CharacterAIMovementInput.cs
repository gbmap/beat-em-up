using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAIMovementInput : MonoBehaviour
{
    GameObject[] players;

    GameObject target;
    Vector3 targetLastPosition;

    Vector3[] pathToTarget;
    int pathPointIndex = 0;


    NavMeshPath navMeshPath;

    CharacterMovement movement;

    public float PathUpdateThreshold = 0.5f;
    public float NextPointUpdateThreshold = 0.1f;

    private enum EMovementStatus
    {
        Wandering,
        FollowingEnemy
    }
    private EMovementStatus movementStatus = EMovementStatus.FollowingEnemy;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        navMeshPath = new NavMeshPath();
        players = GameObject.FindGameObjectsWithTag("Player");

        UpdateTarget();
    }

    private Vector3 V3Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    // Update is called once per frame
    void Update()
    {
        if (movementStatus == EMovementStatus.FollowingEnemy)
        {
            if (target == null)
            {
                UpdateTarget();
            }

            // recalculate path if the target moved too far from destination
            if (Vector3.Distance(target.transform.position, pathToTarget[pathToTarget.Length - 1]) > PathUpdateThreshold) 
            {
                UpdatePath();
            }
        }

        else
        {
            // TODO: Wandering
        }

        pathPointIndex = MoveOnPath(pathToTarget, pathPointIndex);
    }

    void UpdateWandering()
    {

    }

    void UpdateTarget()
    {
        NavMeshPath tempPath = new NavMeshPath();
        target = players
            .Where(p => NavMesh.CalculatePath(transform.position, p.transform.position, NavMesh.AllAreas, tempPath))
            .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
            .FirstOrDefault();

        UpdatePath();
    }

    bool UpdatePath()
    {
        bool success = NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, navMeshPath);
        if (success)
        {
            pathToTarget = navMeshPath.corners;
            pathPointIndex = 0;
        }
        return success;
    }

    int MoveOnPath(Vector3[] points, int pointIndex)
    {
        Vector3 point = points[pointIndex];
        movement.direction = (point - transform.position).normalized;

        if (Vector3.Distance(transform.position, point) < NextPointUpdateThreshold)
        {
            pointIndex++;
        }

        return Mathf.Clamp(pointIndex, 0, points.Length-1);
    }

    private void OnDrawGizmos()
    {
        if (pathToTarget == null || pathToTarget.Length == 0) return;

        for (int i = 1; i < pathToTarget.Length; i++)
        {
            Vector3 a = pathToTarget[i - 1];
            Vector3 b = pathToTarget[i];
            Gizmos.DrawLine(a, b);
        }

        for (int i = 0; i < pathToTarget.Length; i++)
        {
            Vector3 a = pathToTarget[i];
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(a, 0.5f);
            Gizmos.color = Color.white;
        }
    }

    private void OnGUI()
    {
        GUILayout.TextArea("Path Index: " + pathPointIndex.ToString());
        GUILayout.TextArea("Path Target Position: " + pathToTarget[pathPointIndex]);
    }

}
