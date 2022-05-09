using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited LairFeature that has a sprite 'animation'
// Ran out of time but Hero interaction here should allow them leave the way they came in
// Sometimes they just run to the doorway and get stuck there lol

[RequireComponent(typeof(BoxCollider2D))]
public class LairEntrance : LairFeature
{

    [SerializeField] Sprite[] stateSprites;
    SpriteRenderer sr; // Animation references
    BoxCollider2D bc;

    protected override void Start()
    {
        base.Start();

        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
        ToggleEntranceState(false);
    }

    // Function to set sprite state and toggle blocking collider
    // I could extend this doorway mechanic to include updating the A* graph as a placeable / interactable object hmmmm
    public void ToggleEntranceState(bool open)
    {
        if (open)
        {
            sr.sprite = stateSprites[1];
            bc.enabled = false;
        } else
        {
            sr.sprite = stateSprites[0];
            bc.enabled = true;
        }

    }

    public override void Interact()
    {
        //oops
    }
}
