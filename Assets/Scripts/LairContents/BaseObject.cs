using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for all interactable objects to inherit from, so that LairManager can store them all as BaseObjects

public class BaseObject : MonoBehaviour
{
    LairManager lairManager;

    public bool interactable, targeted = false;
    public float interactionRadius, interactionDuration;

    protected virtual void Start()
    {
        // Get LairManager instance, update it's object list, call event <.<
        if (LairManager.instance)
        {
            lairManager = LairManager.instance;
            lairManager.objectsInLair.Add(this);
            lairManager.UpdateObjectList();
        }
    }

    // Need to rethink this a bit <.< don't really need this functionality here, but need the function
    public virtual void Interact()
    {
        interactable = false;
        targeted = false;
        lairManager.objectsInLair.Remove(this);
        lairManager.UpdateObjectList();
    }
}
