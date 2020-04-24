using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Rocket : Weapon
{


    public GameObject rocketEffectsPrefab;//blah


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


    //PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        armed = false;
        setColliders(false);
        effectsObj = Instantiate(rocketEffectsPrefab);
        
        effectsObj.transform.position = effectsCenter.position;

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
                if(effectsObj != null)
                    effectsObj.GetComponent<Light>().enabled = false;
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

        checkLinecastCollision();


        if (burnActive)
        {
            rbRef.AddForce(transform.forward * motorForce);
            effectsObj.transform.position = effectsCenter.transform.position;
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(rbRef.velocity);
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        //photonView.RPC("rpcContactProcess")

        if (myFlow != null)
        {

            if (myFlow.localOwned)
            {

                if (explodeOnOther(other.gameObject))
                {
                    rpcContactProcess(transform.position, other.transform.root.GetComponent<PhotonView>().ViewID);
                }
            }
        }

    }

    [PunRPC]
    override
    public void rpcContactProcess(Vector3 position, int otherId)
    {
        //Debug.Log("Rocket colliding. Arming time: " + armingTime + ", arming timer: " + armTimeRemaining + ", isArmed: " + armed);

        GameObject otherRoot = null;
        CombatFlow otherFlow = null;

        if (otherId != -1)
        {
            otherRoot = PhotonNetwork.GetPhotonView(otherId).gameObject;
            otherFlow = otherRoot.GetComponent<CombatFlow>();
        }

        transform.position = position;

        if (otherRoot != null && !otherRoot.CompareTag("Effects")) // do not do anything against effects
        {
            bool doExplode = true;

            if (otherFlow != null)
            {
                if (otherFlow.team != myTeam || friendlyImpact)
                {

                    otherFlow.currentHP -= impactDamage;
                }
                else
                {
                    doExplode = false;
                }
            }

            if (doExplode)
            {
                myFlow.currentHP -= myFlow.currentHP;
                effectsObj.GetComponent<Light>().enabled = false;
                Debug.Log("Rocket HP after impact: " + myFlow.currentHP);
            }
        }
    }


}
