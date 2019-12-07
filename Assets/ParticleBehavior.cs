using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBehavior : MonoBehaviour
{


    private ParticleSystem pSystem;
    private ParticleCollisionEvent[] collisionEvents;

    public long collisionCount = 0;
    public float impactDamage;
    public float bounceDamage;

    private ParticleSystem.Particle[] myParticles;

    public ExplodeStats impactExplosionProperties;
    public ExplodeStats bounceExplosionProperties;

    


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

    //void initializeArrayIfNeeded()
    //{
        

    //}

    


    private void OnParticleCollision(GameObject other) // other is target hit by emitter
    {

       // initializeArrayIfNeeded();
        //pSystem.GetParticles(myParticles);

        //Debug.Log("Other's name: " + other.name);

        int collCount = pSystem.GetSafeCollisionEventSize();

        if(collisionEvents == null)
            collisionEvents = new ParticleCollisionEvent[collCount];

        if (collCount > collisionEvents.Length)
            collisionEvents = new ParticleCollisionEvent[collCount];


        int eventCount = pSystem.GetCollisionEvents(other, collisionEvents);


        //// test loop. This does nothing of actual value
        //for (int i = 0; i < myParticles.Length; i++)
        //{
        //    myParticles[i].remainingLifetime = 0f;
        //}

        // whenever a collision event is triggered, this loops through and processes every one
        for (int i = 0; i < eventCount; i++)
        {

            
            Vector3 incidentVelocity = collisionEvents[i].velocity;
            Vector3 normal = collisionEvents[i].normal;
            Vector3 incidentNormal = Vector3.Project(incidentVelocity, normal);

            ParticleSystem emitterForThisCollision = GetComponent<ParticleSystem>();
            var coll = emitterForThisCollision.collision;

            // Target information
            GameObject target = other.transform.root.gameObject;
            CombatFlow targetFlow = target.GetComponent<CombatFlow>();
            float currentDamage = 0f;
            //Debug.Log("Bullet from: " + gameObject.name + " hit target: " + other.name);

            if (incidentNormal.magnitude > ParticleBehavior.impactFuseVelocity) // if impact velocity is high enough, impact
            {
                //Debug.Log("Exploding at impact incidence: " + incidentNormal.magnitude.ToString());

                //Debug.Log("Explode");

                coll.lifetimeLoss = 1f;


                //Explosion.createExplosionAt(collisionEvents[i].intersection, 3, 0, false, 4, Color.yellow, true, Color.grey);
                impactExplosionProperties.explode(collisionEvents[i].intersection);
                

                currentDamage = impactDamage;

                

                
                

            }
            else // low impact velocity, bounce
            {
                //Debug.Log("Bounce");
                coll.lifetimeLoss = .4f;
                //Explosion.createExplosionAt(collisionEvents[i].intersection, 1.5f, 0, false, 2, Color.grey, false, Color.grey);
                bounceExplosionProperties.explode(collisionEvents[i].intersection);
                currentDamage = bounceDamage;
            }

            if (targetFlow != null)
                targetFlow.currentHP -= currentDamage;

        }
        

       // ParticlePhysicsExtensions.
        collisionCount++;
        
    }


}
