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

    private CombatFlow rootFlow;

    


    public static float impactFuseVelocity = 100f; // impact incident velocity must be at least this value to explode

    // Start is called before the first frame update
    void Start()
    {
        pSystem = GetComponent<ParticleSystem>();
        myParticles = new ParticleSystem.Particle[500];
        rootFlow = transform.root.GetComponent<CombatFlow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    


    private void OnParticleCollision(GameObject other) // other is target hit by emitter
    {

        // CONSTRUCT ARRAY OF ALL COLLISION EVENTS

        int collCount = pSystem.GetSafeCollisionEventSize();

        if(collisionEvents == null)
            collisionEvents = new ParticleCollisionEvent[collCount];

        if (collCount > collisionEvents.Length)
            collisionEvents = new ParticleCollisionEvent[collCount];


        int eventCount = pSystem.GetCollisionEvents(other, collisionEvents);




        // whenever a collision event is triggered, this loops through and processes every one
        for (int i = 0; i < eventCount; i++)
        {

            // Get velocity of (I'm assuming) particle 
            Vector3 incidentVelocity = collisionEvents[i].velocity;

            // If other object has rigidbody, subtract its velocity to get relative velocity
            Rigidbody otherRBref = other.GetComponent<Rigidbody>();
            if (otherRBref != null)
                incidentVelocity -= otherRBref.velocity;

            // Calculate component of velocity along normal
            Vector3 normal = collisionEvents[i].normal;
            Vector3 incidentNormal = Vector3.Project(incidentVelocity, normal);

            // Reference to particle emitter
            ParticleSystem emitterForThisCollision = GetComponent<ParticleSystem>();
            var coll = emitterForThisCollision.collision;

            // Target information
            GameObject target = other.transform.root.gameObject;
            CombatFlow targetFlow = target.GetComponent<CombatFlow>();
            float currentDamage = 0f;


            if (incidentNormal.magnitude > ParticleBehavior.impactFuseVelocity) // if impact velocity is high enough, impact
            {
                // set emitter to have all its projectiles lose 100% of lifetime upon collision
                coll.lifetimeLoss = 1f;

                // create impact explosion
                impactExplosionProperties.explode(collisionEvents[i].intersection);
                
                // damage
                currentDamage = impactDamage;

            }
            else // low impact velocity, bounce
            {
                // set emitter to have all its projectiles lose 40% of lifetime upon collision
                coll.lifetimeLoss = .4f;

                // create bounce explosion at intersection
                bounceExplosionProperties.explode(collisionEvents[i].intersection);

                // damage
                currentDamage = bounceDamage;
            }

            // only attempt to sent HP subtraction if target has CombatFlow script component
            if (targetFlow != null && rootFlow.isLocalPlayer)
            {
                targetFlow.dealDamage(currentDamage);
            }

        }
        

       // ParticlePhysicsExtensions.
        collisionCount++;
        
    }


}
