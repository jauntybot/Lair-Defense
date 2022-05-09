using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited object w/ sprite 'animation'

[RequireComponent(typeof(SpriteRenderer))]
public class ChestObject : BaseObject
{

    SpriteRenderer sr;
    [SerializeField] Sprite[] sprites;

    protected override void Start()
    {
        base.Start();

        sr = GetComponent<SpriteRenderer>();
    }


    public override void Interact()
    {
        sr.sprite = sprites[1];

        base.Interact();

    }


}
