using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

//[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class BasicPathfindingAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform target;
    [SerializeField] Transform lookTransform;
    ColliderArc weapon;

    Animator animator;
    Rigidbody2D rb;

    Seeker seeker;

    [Header("Pathfinding")]
    [SerializeField] float moveSpeed;
    [SerializeField] float nextWaypointDistance, pathCheckInterval;
    float pathTime;

    Path currentPath;
    int currentWaypoint = 0;
    bool completedPath = false;

    [SerializeField] Vector2 dir;
    [Header("AI State Dependencies")]

    public AIState currentState;
    public enum AIState { Idle, Roaming, Tracking, Engaging, Retracing, Dead };
    [SerializeField] LayerMask lineOfSightMask;
    [SerializeField] float trackingRadius, engagmentRadius;
    [SerializeField] float attackSpeed;
    [SerializeField] bool targetedPlayer, attacking = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        weapon = GetComponent<ColliderArc>();


        currentState = AIState.Tracking;
        StartCoroutine(RunAI());
        StartCoroutine(weapon.ArcWipe());
    }

    IEnumerator RunAI()
    {
        while (currentState != AIState.Dead) {

            CheckPathToTarget();
            switch (currentState)
            {
                default:
                    yield return null;
                    break;
                case AIState.Idle:
                    yield return null;
                    break;
                case AIState.Roaming:
                    yield return null;
                    break;
                case AIState.Tracking:
                    if (seeker.IsDone())
                        seeker.StartPath(rb.position, target.position, OnPathComplete);
                    yield return StartCoroutine(FollowPath());
                    break;
                case AIState.Engaging:
                    if (!attacking)
                    {
                        StartCoroutine(weapon.ArcWipe());
                        attacking = true;
                        StartCoroutine(AttackCooldown());
                    }
                    if (seeker.IsDone())
                        seeker.StartPath(rb.position, target.position, OnPathComplete);
                    yield return StartCoroutine(FollowPath());
                    break;
                case AIState.Retracing:
                    yield return null;
                    break;


            }
        }
    }

    void CheckPathToTarget()
    {
        if (target)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position, distance +.2f, lineOfSightMask);
            Debug.DrawRay(transform.position, target.position - transform.position);
            if (hit) {
                Debug.Log(hit.transform.name);
                if (hit.transform == target)
                {
                    if (distance < trackingRadius && distance > engagmentRadius)
                    {
                        currentState = AIState.Tracking;
                        targetedPlayer = true;
                        DebugDrawLineOfSight(2, distance);
                    }
                    else if (distance <= engagmentRadius)
                    {
                        currentState = AIState.Engaging;
                        DebugDrawLineOfSight(3, distance);
                    }
                    else
                        DebugDrawLineOfSight(1, distance);
                }
                else
                {
                    DebugDrawLineOfSight(0, distance);
                    if (currentState != AIState.Retracing)
                    {
                        if (targetedPlayer)
                            currentState = AIState.Retracing;
                        else
                            currentState = AIState.Roaming;
                    }
                }
            }
        }
       
    }

    IEnumerator FollowPath()
    {
        if (currentPath == null)
            yield break;

        if (currentWaypoint >= currentPath.vectorPath.Count)
        {
            completedPath = true;
            yield break;
        } else
        {
            completedPath = false;
        }

        dir = ((Vector2)currentPath.vectorPath[currentWaypoint] - rb.position).normalized;
        animator.SetBool("IsMoving", dir.magnitude > 0);
        Vector2 force = dir * moveSpeed * Time.deltaTime;

        rb.AddForce(force);

        Vector2 lookDir = dir.normalized;
        if (Mathf.Abs(lookDir.y) >= Mathf.Abs(lookDir.x))
        {
            if (lookDir.y <= 0) 
            { 
                animator.SetInteger("Direction", 0);
                lookTransform.rotation = Quaternion.Euler(0, 0, 180);
            } 
            else if (lookDir.y > 0) 
            { 
                animator.SetInteger("Direction", 1);
                lookTransform.rotation = Quaternion.Euler(0, 0, 0);
            }       
        } 
        else if (Mathf.Abs(lookDir.y) < Mathf.Abs(lookDir.x))
        {
            if (lookDir.x >= 0)
            {
                animator.SetInteger("Direction", 2);
                lookTransform.rotation = Quaternion.Euler(0, 0, 270);
            }
            else if (lookDir.x < 0)
            {
                animator.SetInteger("Direction", 3);
                lookTransform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        
        yield return new WaitForEndOfFrame();

        float distance = Vector2.Distance(rb.position, currentPath.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
            currentWaypoint++;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(weapon.arcWipeDuration + attackSpeed);
        attacking = false;
    }

    //set path from seeker calculation when completed
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
        }
    }


    void DebugDrawLineOfSight(int index, float distance)
    {
        Color color;
        switch (index)
        {
            default:
                color = Color.white;
                break;
            case 0: //enemy does not have line sight
                color = Color.gray;
                break;
            case 1: //enemy has line of sight but is too far away
                color = Color.red;
                break;
            case 2: //enemy has line of sight and is in tracking distance
                color = Color.yellow;
                break;
            case 3: //enemy has line of sight and is in engagement distance
                color = Color.green;
                break;
        }
        Debug.DrawRay(transform.position, (target.transform.position - transform.position).normalized * distance, color);
    }
}
