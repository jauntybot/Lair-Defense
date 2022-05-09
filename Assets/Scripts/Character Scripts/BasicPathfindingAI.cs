using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// This class is the base for all hero and minion auto-battlers
/// The AI isn't roboust or all that great, but with the frame of the game as is it will be easily improved
/// State machine ran in coroutine, finds target, navigates by A*, and engages
/// IMPORTANTLY, both alignments, Heroes and Minions, derive from this logic, keep it impartial
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class BasicPathfindingAI : MonoBehaviour
{
    #region Class Variables
    [Header("References")] // Must define in inspector!
    [SerializeField] Transform lookTransform; // local up set to dir <.< idk, is 2D convention local right? 
    [SerializeField] protected ColliderArc weapon;

    protected BaseCharacter self;

    [SerializeField] Animator animator;
    protected Rigidbody2D rb;
    Collider2D rootCollider;

    protected Seeker seeker;
    protected LairManager lairManager;

    [Header("Pathfinding")] // Settings in inspector
    [SerializeField] float moveSpeed;

    // A* Seeker variables
    [SerializeField] float nextWaypointDistance;
    Path currentPath;
    bool completedPath = true;
    int currentWaypoint = 0;

    [SerializeField] Vector2 dir;

    public enum AIState { Idle, Roaming, Tracking, Engaging, Retracing, Dead }; // AI states!

    [Header("AI Inputs")]
    public AIState currentState;
    [SerializeField] protected bool stateRunning; // a futile failsafe, coroutines are hard

    public BaseCharacter.Alignment targetAlignment; // who we don't like
    public Transform currentTarget; // where we navigate
    [SerializeField] protected List<BaseCharacter> validCharacters; // possible targets, currentTarget assigned from here

    [SerializeField] LayerMask lineOfSightMask; // What we ignore when raycasting line of sight
    [SerializeField] protected float trackingRadius, engagmentRadius; // How close we need to be to enter states
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected bool targeted, attacking = false;
    public bool alive;
    #endregion

    #region Initialization
    protected virtual void Start()
    {
        // Set required component references
        if (GetComponent<BaseCharacter>())
            self = GetComponent<BaseCharacter>();
        rb = GetComponent<Rigidbody2D>();
        rootCollider = GetComponent<Collider2D>();
        seeker = GetComponent<Seeker>();

        // Sneaky override for extended classes that want to know more
        InitializeLair();

        animator.SetBool("Alive", true); 

        if (!weapon) // Cheeky reference failsafe, if unarmed, can't attack!
            attacking = false;
        else
            engagmentRadius = weapon.range + weapon.offsetFromOrigin;

        StartCoroutine(RunAI()); // run that sweet, sweet while loop
    }

    // That failsafe, inheriting classes will include more lists 
    protected virtual void InitializeLair()
    {
        // Get reference to lair, subscribe to callback, read and save data
        if (LairManager.instance)
        {
            lairManager = LairManager.instance;
            lairManager.CharacterUpdateCallback += UpdateValidTargets;
            UpdateValidTargets();
        }
    }
    #endregion

    #region Determine and Run currentState
    // While loops! Who needs Update()? Switch statement of currentState and run (you guessed it) more coroutines!
    // Coroutines are so cool, you can return a coroutine in another coroutine and the first won't continue executing until the second finishes
    // Which is what I do here, it's like a more complicated Update(), but tailored to our state machine and not running superflous checks
    IEnumerator RunAI()
    {
        // If created during Setup, wait for the Lair to be Active
        while (lairManager.currentState == LairManager.LairState.Setup)
        {
            yield return null;
        }
        currentState = AIState.Roaming;
        while (currentState != AIState.Dead) {           
            PrioritizeTarget(); // Set currentTarget
            CheckPathToTarget(); // Set currentState         
            switch (currentState)
            {              
                default:
                    yield return null;
                    break;
                case AIState.Idle:
                    yield return null;
                    break;
                case AIState.Roaming:
                    yield return StartCoroutine(RandomRoam());
                    break;
                case AIState.Tracking:
                    yield return StartCoroutine(TrackTarget());
                    break;
                case AIState.Engaging:
                    if (!stateRunning)
                    yield return StartCoroutine(EngageTarget());
                    break;
                case AIState.Retracing: // Sad, this can't be implemented until line of sight targeting is fully integrated
                    yield return null;
                    break;
            }
        }
        if (currentState == AIState.Dead)
        {
            if (alive)
            {
                StartCoroutine(DeathAnimation());
                alive = false;
            }
        }
    }

    //Sets currentState based on line of sight and distance between this and currentTarget
    void CheckPathToTarget()
    {
        if (currentTarget)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            // Cast ray to currentTarget
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentTarget.position - transform.position, distance + .2f, lineOfSightMask);
            if (hit)
            {                
                if (hit.transform == currentTarget) // Nothing is in between us
                {
                    if (distance < trackingRadius && distance > engagmentRadius) // Within tracking radius
                    {
                        currentState = AIState.Tracking;
                        DebugDrawLineOfSight(2, distance); // Draw ray for editor, see at bottom
                    }
                    else if (distance <= engagmentRadius) // Within engaging radius
                    {
                        currentState = AIState.Engaging;
                        DebugDrawLineOfSight(3, distance);

                    }
                    else // We're too far away 
                        currentState = AIState.Roaming;
                        DebugDrawLineOfSight(1, distance);
                }
                else // Needs development
                {
                    if (currentState != AIState.Retracing)
                    {
                        //if (targeted)
                        //    currentState = AIState.Retracing;
                        //else
                            //currentState = AIState.Roaming;
                    }
                }
            }
        }
        else // Needs development
            currentState = AIState.Roaming;
    }
    #endregion

    #region State Coroutines
    //These all follow the same forumla, set path, start follow path coroutine, do state specific stuff inb/w
    protected virtual IEnumerator RandomRoam()
    {
        stateRunning = true;
        if (completedPath) {
            if (seeker.IsDone()) 
            seeker.StartPath(rb.position, RandomPosSphere(rb.position, trackingRadius), OnPathComplete);
        }
        yield return StartCoroutine(FollowPath());
        stateRunning = false;
    }

    IEnumerator TrackTarget()
    {
        if (currentTarget) { // Can't be too safe, this was giving me trouble in the past
            stateRunning = true;
            if (seeker.IsDone())
                seeker.StartPath(rb.position, currentTarget.position, OnPathComplete);
            yield return StartCoroutine(FollowPath());
            stateRunning = false;
        }
    }

    protected virtual IEnumerator EngageTarget()
    {
        if (currentTarget) {
            stateRunning = true;
            if (Vector3.Distance(rb.position, currentTarget.position) < weapon.range && !attacking)
            {
                attacking = true;
                StartCoroutine(AttackCooldown());
                StartCoroutine(weapon.ArcWipe());

            }
            if (seeker.IsDone())
                seeker.StartPath(rb.position, currentTarget.position, OnPathComplete);
            yield return StartCoroutine(FollowPath());
            stateRunning = false;
        }
    }
    #endregion

    #region Navigation

    // Seeker data read, applied to character movement, next waypoint check
    protected IEnumerator FollowPath()
    {     
        if (currentPath == null) // can't follow nothing !
            yield break;

        // tbh these are artifacts of the tutorial I watched to start this, not currently used but probably useful later
        if (currentWaypoint >= currentPath.vectorPath.Count)
        {
            completedPath = true;
            yield break;
        } else
        {
            completedPath = false;
        }

        // Calculate direction
        dir = ((Vector2)currentPath.vectorPath[currentWaypoint] - rb.position).normalized;

        // lol rigidbody character movement? weeee'lll seee
        Vector2 force = dir * moveSpeed * Time.deltaTime;
        rb.AddForce(force);

        // Calculate look direction, translate to 2D, set local up transform
        Vector2 lookDir = dir.normalized;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        lookTransform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        // Set animator parameters
        SetAnimator(lookDir);

        yield return new WaitForFixedUpdate(); // This is the collapse point of all State based coroutines, they all wait for this FixedUpdate call, cool right?
                                                // They probably shouldn't, admittedly, and if they were to it probably shouldn't happen here lol
        // Check if Seeker should look to next waypoint
        float distance = Vector2.Distance(rb.position, currentPath.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
            currentWaypoint++;
    }

    // Set path from seeker calculation when completed
    protected void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
        }
    }

    //Generates a random Vector3 within sphere w/ radius of dist
    static Vector3 RandomPosSphere(Vector3 origin, float dist)
    {
        Vector3 randPos = Random.insideUnitSphere * dist;
        randPos += origin;

        return new Vector3 (randPos.x, randPos.y, 0);
    }
    #endregion

    #region Targeting
    // Overriden check, base will target closest character, extended will focus priorities
    protected virtual void PrioritizeTarget() {
        float closestDist = Mathf.Infinity; // over 9000 !
        foreach (BaseCharacter character in validCharacters)
        {
            // Check if this character is closer than the last
            float dist = Vector2.Distance(transform.position, character.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                currentTarget = character.transform; // Set this character as our target
            }
        }
    }

    // Delegate void, subscribed to LairManager.instance callback event
    // Overriden, more lair contents will be tracked by smarter AIs
    protected virtual void UpdateValidTargets()
    {
        // zero our current record
        currentTarget = null; // especially this guy, gave me a lot of trouble
        validCharacters = new List<BaseCharacter>(); 
        foreach (BaseCharacter character in lairManager.charactersInLair)
        {
            if (character.alignment == targetAlignment) // add characters we don't like
                validCharacters.Add(character); 
            
        }
    }
    #endregion

    #region Animation

    // These came standard w/ the sprite pack I'm using. I don't make the rules. sometimes
    void SetAnimator(Vector2 lookDir)
    {

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
        animator.SetBool("IsMoving", lookDir.magnitude > 0);
    }

    // like here, I made these rules
    IEnumerator DeathAnimation()
    {
        animator.SetLayerWeight(0, 0);
        animator.SetLayerWeight(1, 0);
        animator.SetLayerWeight(2, 1); // Set death animation layer weight
        animator.SetBool("Alive", true);
        yield return new WaitForSeconds(1); //animation / collision detection delay
        rootCollider.enabled = false;
    }
    #endregion

    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(weapon.attackDuration + attackSpeed);
        attacking = false;
    }

    // Funny little switch statement, have to look here to know how to use it so
    void DebugDrawLineOfSight(int index, float distance)
    {
        Color color;
        switch (index)
        {
            default:
                color = Color.white;
                break;
            case 0: //we don't have line sight
                color = Color.gray;
                break;
            case 1: //we have line of sight but are too far away
                color = Color.red;
                break;
            case 2: //we have line of sight and are in tracking distance
                color = Color.yellow;
                break;
            case 3: //we have line of sight and are in engagement distance
                color = Color.green;
                break;
        }
        Debug.DrawRay(transform.position, (currentTarget.transform.position - transform.position).normalized * distance, color);
    }
}
