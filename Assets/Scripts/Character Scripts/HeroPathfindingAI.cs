using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extended from BasicPathfindingAI, will extend further into specific character class logic down the line
// For more detailed comments on everything covered here, refer to the base class
// Not impartial, now alignment specific, so be explicit in what Heroes can do
public class HeroPathfindingAI : BasicPathfindingAI
{
    // More data please!
    [SerializeField] protected List<BaseObject> validObjects;
    [SerializeField] protected List<LairFeature> lairFeatures;

    public enum Priority {  Minion, Object, LairFeature } // Smart Heroes, look for specific lair contents based on their priority

    [Header("Hero Settings")]
    public Priority currentPriority;
    bool teleporting = false; // Used for LairExit delay, probably will revise and remove it here

    // Override called in base.Start(), requests more data from LairManager.instance than base does
    protected override void InitializeLair()
    {
        if (LairManager.instance)
        {
            lairManager = LairManager.instance;
            lairManager.CharacterUpdateCallback += UpdateValidTargets;
            lairManager.ObjectUpdateCallback += UpdateValidTargets; // smash tha sub
            UpdateValidTargets();
            foreach (LairFeature lf in lairManager.lairFeatures)
                lairFeatures.Add(lf);
        }
    }

    // Oops heroes are smart enough to auto roam to exit <.< 
    protected override IEnumerator RandomRoam() {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, lairFeatures.Find(t => t.GetComponent<LairExit>()).transform.position, OnPathComplete);
        yield return StartCoroutine(FollowPath());
    }
    
    // Override pathfinding EngageTarget state coroutine to allow Heroes to interact w/ objects
    protected override IEnumerator EngageTarget()
    {
        stateRunning = true;
        if (currentTarget) {
            if (seeker.IsDone())
                seeker.StartPath(rb.position, currentTarget.position, OnPathComplete);

            if (currentTarget.GetComponent<BaseCharacter>()) // If I swing my sword
            {
                if (Vector3.Distance(rb.position, currentTarget.position) < weapon.range && !attacking)
                {
                    attacking = true;
                    StartCoroutine(AttackCooldown());
                    StartCoroutine(weapon.ArcWipe());

                }
            }
            else if (currentTarget.GetComponent<BaseObject>()) // If I interact
            {
                if (Vector3.Distance(rb.position, currentTarget.position)
                    < currentTarget.GetComponent<BaseObject>().interactionRadius)
                {
                    yield return new WaitForSeconds(currentTarget.GetComponent<BaseObject>().interactionDuration);
                    if (currentTarget)
                        currentTarget.GetComponent<BaseObject>().Interact();
                }
            }

            yield return StartCoroutine(FollowPath());
            stateRunning = false;
        }
    }

    // Overrides pathfinding PrioritizeTarget function to allow Heroes to target objects and lair entrance/exit based on priority
    // More detailed breakdown of function skeleton in BasicPathfindingAI, this adds more specifics through a switch statement
    protected override void PrioritizeTarget()
    {
        float closestDist = Mathf.Infinity;

        switch (currentPriority) {
            case Priority.Object:
                if (validObjects.Count == 0 || !validObjects.Find(o => o.targeted == false)) //If there are no objects left change priority
                {   
                    //If there are minions and hero stands a chance
                    if (self.currentHP < self.maxHP / 4 && validCharacters.Count != 0)
                        currentPriority = Priority.Minion;
                    else //gtfo
                        currentPriority = Priority.LairFeature;
                    PrioritizeTarget();
                    return;
                } else { 
                    // Target the closest object
                    foreach (BaseObject target in validObjects)
                    {
                        float dist = Vector2.Distance(transform.position, target.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;                   
                            currentTarget = target.transform;
                        }  
                        
                    }     
                    break;
                }

            case Priority.Minion:
                if (validCharacters.Count == 0) { //If there are no minions left
                    currentPriority = Priority.Object;
                    PrioritizeTarget();
                    return;
                } else { 
                    // Target the closest minion
                    foreach (BaseCharacter target in validCharacters)
                    {
                        float dist = Vector2.Distance(transform.position, target.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targeted = true;
                            currentTarget = target.transform;                       
                        }                   
                    }
                    break;
                }

            // last case state, does not transition out from here
            // This state uses the Find method a lot, feels more efficient than my foreach loops but is it just a wrapper for the same cost function ?
            case Priority.LairFeature:
                // If the hero is close to dying 
                if (self.currentHP < self.maxHP / 5)
                {
                    // Go to the closest lair feature
                    if (Vector3.Distance(rb.position, lairFeatures.Find(t => t.GetComponent<LairEntrance>()).transform.position)
                        < (Vector3.Distance(rb.position, lairFeatures.Find(t => t.GetComponent<LairExit>()).transform.position)))
                        
                        currentTarget = lairFeatures.Find(t => t.GetComponent<LairEntrance>()).transform;
                    else
                        currentTarget = lairFeatures.Find(t => t.GetComponent<LairExit>()).transform;
                }
                else { 
                    currentTarget = lairFeatures.Find(t => t.GetComponent<LairExit>()).transform;
                }
                break;
        }       

        // Check if a hostile Character is closer than current target, always prioritize an attacker
        foreach (BaseCharacter target in validCharacters)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;               
                currentTarget = target.transform;
            }
        }

    }

    // Override pathfinding UpdateValidTargets to enable Heroes to register objects as targets
    // Should break this down further so I can execute the base rather than rewriting ? hmmm...
    protected override void UpdateValidTargets()
    {

        currentTarget = null;
        validCharacters = new List<BaseCharacter>();
        validObjects = new List<BaseObject>();
        foreach (BaseCharacter character in lairManager.charactersInLair) {
            if (character.alignment == targetAlignment)          
                validCharacters.Add(character);
            
        }
        foreach(BaseObject obj in lairManager.objectsInLair) {
            if (obj.interactable)
                validObjects.Add(obj);
        }
    }



    // Pathfinding delay to keep Hero in teleporter collider, then interact
    IEnumerator ProgressToNextLair(LairExit exit)
    {
        float t = 0;
        while (teleporting)
        {
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
            if (t > exit.interactionDuration) {
                exit.Interact();
                lairManager.charactersInLair.Remove(gameObject.GetComponent<BaseCharacter>());
                lairManager.UpdateCharacterList();
                Destroy(gameObject);
            }              
        }       
    }

    // Hero teleporting detection
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "LairExit")
        {
            if (!teleporting) { 
                teleporting = true;
                StartCoroutine(ProgressToNextLair(collision.GetComponent<LairExit>()));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "LairExit")
            if (teleporting)
                teleporting = false;
    }
}
