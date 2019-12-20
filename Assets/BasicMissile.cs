﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMissile : Weapon
{

    // inherits target property from Weapon

    public GameObject ownerObj;


    public GameObject missileModel;
    public GameObject missileActive;
    public GameObject effectsPrefab;
    public Transform effectsCenter;
    public GameObject effectsObj;
    private Rigidbody rbRef;
    private CombatFlow myCombatFlow;

    public float proximityFuseRange; // automatically set to explodeStats radius
    public float fuseLeadTime;
    public float impactDamage;
    public float speed;

    private Vector3 targetPosition;

    private GameObject impactVictimRoot;
    

    // Start is called before the first frame update
    void Start()
    {
        myCombatFlow = GetComponent<CombatFlow>();
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);

        effectsObj = Instantiate(effectsPrefab);
        effectsObj.transform.position = effectsCenter.position;

        effectsObj.GetComponent<Light>().enabled = false;
        effectsObj.GetComponent<TrailRenderer>().enabled = false;

        myCombatFlow.isActive = false;


        
    }

    override
    public void linkToOwner(GameObject ownerObjArg)
    {
        ownerObj = ownerObjArg;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();
    }
    

    




    void Update()
    {
        tryArm();
        effectsObj.transform.position = effectsCenter.position;
    }

    

    private void FixedUpdate()
    {
        if ( launched)
        {
            if(myTarget != null)
            {
                updateTargetPosition();
                

                transform.LookAt(targetPosition);
            }
            rbRef.velocity = transform.forward * speed;


            if (checkProximityFuse())
            {
                effectsObj.GetComponent<Light>().enabled = false;
                myCombatFlow.currentHP -= myCombatFlow.currentHP;
            }
        }
    }

    void updateTargetPosition()
    {
        if(myTarget != null)
        {
            Vector3 targetPosTemp = myTarget.transform.position;

            Rigidbody targetRB = myTarget.GetComponent<Rigidbody>();
            if (targetRB != null)
                targetPosTemp += targetRB.velocity * fuseLeadTime;

            targetPosition = targetPosTemp;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        effectsObj.GetComponent<Light>().enabled = false;

        GameObject otherRoot = collision.gameObject.transform.root.gameObject;
        CombatFlow otherFlow = otherRoot.GetComponent<CombatFlow>();
        
        Weapon otherWeapon = otherRoot.gameObject.GetComponent<Weapon>();

        bool doDealImpact = true;

        // If other has a flow and is the one impact victim
        if (otherFlow != null && otherRoot != impactVictimRoot)
        {
            //myCombatFlow.explodeStats.doExplode = false;
            impactVictimRoot = otherRoot;

            // don't explode if victim will die and if victim is not a projectile
            if (impactDamage > otherFlow.currentHP && otherFlow.type != CombatFlow.Type.PROJECTILE)
            {
                myCombatFlow.explodeStats.doExplode = false; // death will only trigger enemy explosion
            }

            // =========  TRY TO DEAL IMPACT -- FOLLOWING CONDITIONS FAIL IMPACT


            //  Can do damage if victim has Weapon component, and this weapon is set to destroy projectiles
            if(otherWeapon == null && !impactDestroysProjectiles)
            {
                doDealImpact = false;
            }

            // obviously can't deal impact if there is no combatFlow to do it to
            if(otherFlow == null)
            {
                doDealImpact = false;
            }

            // finally, deal the impact
            if (doDealImpact)
            {
                otherFlow.currentHP -= impactDamage;
                Debug.Log("Impact dealing " + impactDamage + " damage to " + otherFlow);
            }
            
            
            
        }

        myCombatFlow.die(); // die immediately on collision



    }




    override
    public void launch()
    {
        Destroy(GetComponent<FixedJoint>());
        rbRef.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        effectsObj.GetComponent<Light>().enabled = true;
        effectsObj.GetComponent<TrailRenderer>().enabled = true;
        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;
        
    }


    private bool checkProximityFuse()
    {
        bool fuseTriggered = false;
        

        if (myTarget != null)
        {
            float distance = Vector3.Distance(targetPosition, transform.position);
            if(distance < proximityFuseRange)
            {
                Debug.Log("Proximity fuse blew");
                fuseTriggered = true;
            }
        }

        return fuseTriggered;
    }
}