using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPathfindingAI : MonoBehaviour
{
    public enum AIState { Idle, Roaming, Tracking, Engaging, Retracing, Dead };
    public AIState currentState;

    [Header("References")]
    Transform target;

    public virtual void CheckPathToTarget() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position);
        Debug.DrawRay(transform.position, target.position - transform.position);
        if (hit.transform == target) {



        }
    }



}
