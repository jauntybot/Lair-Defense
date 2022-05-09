using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stupid particle sys, why no self-destruct

public class ParticleSysDestoryer : MonoBehaviour
{
    ParticleSystem ps;
    float delay;

    private void Start()
    {
        ps = gameObject.GetComponent<ParticleSystem>();
        delay = ps.main.duration;
        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
