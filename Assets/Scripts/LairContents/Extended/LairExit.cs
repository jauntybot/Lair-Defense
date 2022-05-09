using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited LairFeature that has a sprite animation and particle system instantiation

public class LairExit : LairFeature
{
    public GameObject teleportParticles;

    public List<SpriteRenderer> runes;
    public float lerpSpeed;

    private Color curColor;
    private Color targetColor;

    // Sprite animation
    private IEnumerator LerpColor()
    {
        while (curColor != targetColor) { 
            curColor = Color.Lerp(curColor, targetColor, lerpSpeed * Time.deltaTime);

            foreach (var r in runes)
            {
                r.color = curColor;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    // HeroeAI triggers this when teleporting
    public override void Interact()
    {
        Instantiate(teleportParticles, transform.position + Vector3.back, Quaternion.identity);
    }

    
    // Collision detection to trigger sprite animation
    private void OnTriggerEnter2D(Collider2D other)
    {
        targetColor = new Color(1, 1, 1, 1);
        StopAllCoroutines();
        StartCoroutine(LerpColor());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        targetColor = new Color(1, 1, 1, 0);
        StopAllCoroutines();
        StartCoroutine(LerpColor());
    }
}
