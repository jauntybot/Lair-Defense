using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Currently an exact clone of BaseObject
// For AI targeting purposes, a new base class to extend for the exit and entrance
public class LairFeature : MonoBehaviour
{
    LairManager lairManager;

    public bool interactable;
    public float interactionRadius, interactionDuration;

    protected virtual void Start()
    {
        if (LairManager.instance)
        {
            lairManager = LairManager.instance;
            lairManager.lairFeatures.Add(this);
            lairManager.UpdateObjectList();
        }
    }

    public virtual void Interact()
    {

        interactable = false;
        lairManager.lairFeatures.Remove(this);
        lairManager.UpdateObjectList();
    }

}
