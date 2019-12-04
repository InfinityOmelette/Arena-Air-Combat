using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBehavior : MonoBehaviour
{


    private ParticleSystem pSystem;
    private ParticleCollisionEvent[] collisionEvents;

    public long collisionCount = 0;

    private ParticleSystem.Particle[] myParticles;


    public static float impactFuseVelocity = 100f; // impact incident velocity must be at least this value to explode

    // Start is called before the first frame update
    void Start()
    {
        pSystem = GetComponent<ParticleSystem>();
        myParticles = new ParticleSystem.Particle[500];
    }

    // Update is called once per frame
    void Update()
    {
        //pSystem.GetParticles(myParticles);
    }

    void initializeArrayIfNeeded()
    {
        

    }


    private void OnParticleCollision(GameObject other) // other is target hit by emitter
    {

        initializeArrayIfNeeded();
        pSystem.GetParticles(myParticles);

        Debug.Log("Other's name: " + other.name);

        int collCount = pSystem.GetSafeCollisionEventSize();

        if(collisionEvents == null)
            collisionEvents = new ParticleCollisionEvent[collCount];

        if (collCount > collisionEvents.Length)
            collisionEvents = new ParticleCollisionEvent[collCount];


        int eventCount = pSystem.GetCollisionEvents(other, collisionEvents);


        for (int i = 0; i < myParticles.Length; i++)
        {
            myParticles[i].remainingLifetime = 0f;
        }

        // whenever a collision event is triggered, this loops through and processes every one
        for (int i = 0; i < eventCount; i++)
        {

            
            Vector3 incidentVelocity = collisionEvents[i].velocity;
            Vector3 normal = collisionEvents[i].normal;
            Vector3 incidentNormal = Vector3.Project(incidentVelocity, normal);

            ParticleSystem emitterForThisCollision = GetComponent<ParticleSystem>();
            var coll = emitterForThisCollision.collision;

            if (incidentNormal.magnitude > ParticleBehavior.impactFuseVelocity) // if impact velocity is high enough
            {
                Debug.Log("Exploding at impact incidence: " + incidentNormal.magnitude.ToString());


                coll.lifetimeLoss = 1f;


                Explosion.createExplosionAt(collisionEvents[i].intersection, 3, 0);
                

            }
            else
            {
                coll.lifetimeLoss = .4f;
            }

        }
        

       // ParticlePhysicsExtensions.
        collisionCount++;
        
    }


}
