using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyDecalOnParticleCollision : MonoBehaviour
{
    public GameObject decal;

    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if ((LayerMask.NameToLayer("Level") & 1 << other.layer) == 0) return;

        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        int i = 0;
        while (i < numCollisionEvents)
        {
            ParticleCollisionEvent evnt = collisionEvents[i];
            Instantiate(decal, evnt.intersection, Quaternion.Euler(evnt.normal));
            i++;
        }
    }
}
