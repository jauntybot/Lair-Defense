using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Base class for all characters to inherit from
// Been thinking I should move character animation here instead of the pathfinding class

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BasicPathfindingAI))]
public class BaseCharacter : MonoBehaviour
{
    [Header("References")] // Must define in inspector!
    [SerializeField] List<HitBox> hitBoxes;

    BasicPathfindingAI ai;
    Rigidbody2D rb;
    LairManager lairManager;
    ColliderArc arc;

    public enum Alignment { Hero, Minion }; // Determines which characters are friendly or mean
    [Header("Base Stats")]
    public Alignment alignment;

    [SerializeField] Slider hpSlider;
    public int currentHP, maxHP;
    public int damage;



    private void Start()
    {
        // Get required component references
        rb = GetComponent<Rigidbody2D>();
        ai = GetComponent<BasicPathfindingAI>();
        ai.alive = true;
        if (GetComponentInChildren<ColliderArc>())
            GetComponentInChildren<ColliderArc>().damage = damage;

        foreach (HitBox box in hitBoxes) // Subsribe to referenced hit box events
            box.DamageCallback += TakeDamage;

        // Set base stats
        currentHP = maxHP;
        if (hpSlider)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // Get LairManager instance, update it's object list, call event <.<
        if (LairManager.instance) {
            lairManager = LairManager.instance;
            lairManager.charactersInLair.Add(this);
            lairManager.UpdateCharacterList();
        }


    }

    // Delegate void called from HitBox event
    void TakeDamage(float damage, float force, Vector3 dir)
    {
        currentHP -= (int) damage;
        if (hpSlider)
            hpSlider.value = currentHP;
        rb.AddForce(force * dir); // Feels like I should keep all locomotion in the same script, but this class requires the other locomotive class so

        if (currentHP <= 0) // Character has passed on
        {
            ai.currentState = BasicPathfindingAI.AIState.Dead;

            foreach (HitBox box in hitBoxes)
                box.gameObject.SetActive(false); // w/o this corpses get yeeted it's p funny actually

            lairManager.charactersInLair.Remove(this); // Update LairManager lists
            lairManager.UpdateCharacterList();

            if (hpSlider)
                hpSlider.gameObject.SetActive(false);        
        }
    }



}
