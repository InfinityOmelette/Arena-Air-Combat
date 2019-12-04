using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBehavior : MonoBehaviour
{


    private ParticleSystem pSystem;
    private ParticleCollisionEvent[] collisionEvents;

    public long collisionCount = 0;
    public float impactFuseVelocity;

    // Start is called before the first frame update
    void Start()
    {
        pSystem = GetComponent<ParticleSystem>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnParticleCollision(GameObject other)
    {
        int collCount = pSystem.GetSafeCollisionEventSize();

        if(collisionEvents == null)
            collisionEvents = new ParticleCollisionEvent[collCount];

        if (collCount > collisionEvents.Length)
            collisionEvents = new ParticleCollisionEvent[collCount];


        int eventCount = pSystem.GetCollisionEvents(other, collisionEvents);

        // whenever a collision event is triggered, this loops through and processes every one
        for(int i = 0; i < eventCount; i++)
        {


            Vector3 incidentVelocity = collisionEvents[i].velocity;
            Vector3 normal = collisionEvents[i].normal;
            Vector3 incidentNormal = Vector3.Project(incidentVelocity, normal);

            Debug.Log("Impact incidence: " + incidentNormal.magnitude.ToString());

            if (incidentNormal.magnitude > 300) // if impact velocity is high enough
            {
                Debug.Log("Heavy impact, exploding...");
                Explosion.createExplosionAt(collisionEvents[i].intersection, 3, 0);
            }

            
        }
        

       // ParticlePhysicsExtensions.
        collisionCount++;
        
    }


}
