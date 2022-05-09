using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Utility class for calling damage from parent BaseCharacter component
// Seperate class so we can use seperate collision detection

public class HitBox : MonoBehaviour
{
    [Header("References")] // Must define in inspector!
    public LayerMask damageLayer; // What hurts us

    public delegate void OnHitboxDetection(float d, float f, Vector3 dir);
    public event OnHitboxDetection DamageCallback; // Callback for BaseCharacter to apply hit detected

    // Check for collision, if it damages us, signal to BaseCharacter
    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.layer == (int)Mathf.Log(damageLayer.value, 2))
        {
            GameObject incomingHit = trigger.gameObject;
            if (incomingHit.GetComponent<ColliderArc>())
            {
                ColliderArc melee = incomingHit.GetComponent<ColliderArc>();
                DamageCallback?.Invoke(melee.damage, melee.force, incomingHit.transform.up);
            }
        }
    }

}