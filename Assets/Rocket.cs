using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Weapon
{


    public GameObject rocketEffectsPrefab;
    public GameObject rocketEffectsObj;
    public Transform effectsCenter;


    private Rigidbody rbRef;
    private CombatFlow myFlow;

    public float impactDamage;

    public float motorBurnTime;
    public float motorForce;
    public bool burnActive = true;

    public Collider myCollider;


    public float tempTimerMax;
    public float tempTimerCurrent;

    private bool tempArmed = false;


    // Start is called before the first frame update
    void Start()
    {
        armed = false;
        setColliders(false);
        rocketEffectsObj = Instantiate(rocketEffectsPrefab);
        
        rocketEffectsObj.transform.position = effectsCenter.position;

        rbRef = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();



        tempTimerCurrent = tempTimerMax;

    }


    private void Update()
    {
        //Debug.Log("RocketArmed: " + armed + ", with " + armTimeRemaining + " time remaining");
        if (armed)
        {
            //Debug.Log("armed");

            if (motorBurnTime > 0f)
            {
                burnActive = true;
                motorBurnTime -= Time.deltaTime;
                
            }
            else
            {
                burnActive = false;
                rocketEffectsObj.GetComponent<Light>().enabled = false;
            }



        }
        else
        {
            //Debug.Log("Attempting to arm with " + armTimeRemaining + "seconds remaining ");
            tryArm();// count down to arm



        }

        killIfBelowFloor();
        killIfLifetimeOver();

    }


    

    

    void FixedUpdate()
    {

        


        if (burnActive)
        {
            rbRef.AddForce(transform.forward * motorForce);
            rocketEffectsObj.transform.position = effectsCenter.transform.position;
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(rbRef.velocity);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Rocket colliding. Arming time: " + armingTime + ", arming timer: " + armTimeRemaining + ", isArmed: " + armed);

        GameObject otherRoot = other.transform.root.gameObject;

        

        CombatFlow rootFlow = otherRoot.GetComponent<CombatFlow>();

        bool doExplode = true;


        if(rootFlow != null)
        {
            if (rootFlow.team != myTeam || friendlyImpact)
            {
                
                rootFlow.currentHP -= impactDamage;
            }
            else
            {
                doExplode = false;
            }
        }

        if (doExplode)
        {
            myFlow.currentHP -= myFlow.currentHP;
            rocketEffectsObj.GetComponent<Light>().enabled = false;
            Debug.Log("Rocket HP after impact: " + myFlow.currentHP);
        }

        
    }


}
