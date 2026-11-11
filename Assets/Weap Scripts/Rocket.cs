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


    //private TrailRenderer effectTrail;
    public bool doSmokeForWholeFlight = false;

    //PhotonView photonView;

    //private EffectsBehavior effects;

    void Awake()
    {
        flightSound = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (flightSound != null)
        {
            flightSound.Play();
        }

        armed = false;
        setColliders(false);
        

        rbRef = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();

        createEffect();
        effectsBehavior.activate();

        tempTimerCurrent = tempTimerMax;

        rbRef.velocity += transform.forward * motorBurnTime * motorForce;

    }

    void createEffect()
    {
        effectsObj = Instantiate(rocketEffectsPrefab);
        effectsBehavior = effectsObj.GetComponent<EffectsBehavior>();
        //effectTrail = effectsObj.GetComponent<TrailRenderer>();
        //effectsObj.transform.position = effectsCenter.position;
        //effectsBehavior.updatePos(getEffectCentPos());
        updateEffectPos();
    }


    private void Update()
    {
        //Debug.Log("RocketArmed: " + armed + ", with " + armTimeRemaining + " time remaining");

        checkLinecastCollision();
        if (Input.GetKeyDown(KeyCode.F8))
        {
            //impactLocal(transform.position, gameObject);
            //myFlow.die();
            Destroy(gameObject);
        }
    }


    
    Vector3 getEffectCentPos()
    {
        Vector3 returnVect;
        if(effectsCenter != null && effectsCenter.transform != null)
        {
            returnVect = effectsCenter.transform.position;
        }
        else
        {
            returnVect = new Vector3();
        }

        return returnVect;
    }
    

    void updateEffectPos()
    {
        if(effectsBehavior != null)
        {
            effectsBehavior.updatePos(getEffectCentPos());
        }
    }

    void FixedUpdate()
    {

        //effectsObj.transform.position = effectsCenter.transform.position;
        updateEffectPos();

        if (armed)
        {
            //Debug.Log("armed");

            if (motorBurnTime > 0f)
            {
                burnActive = true;
                motorBurnTime -= Time.fixedDeltaTime;

            }
            else
            {
                burnActive = false;


                if (effectsObj != null)
                {
                    effectsBehavior.emitSmoke(doSmokeForWholeFlight);
                    
                }
                    //effectTrail.emitting = doSmokeForWholeFlight;
            }

        }
        else
        {
            //Debug.Log("Attempting to arm with " + armTimeRemaining + "seconds remaining ");
            tryArm();// count down to arm


        }



        killIfBelowFloor();
        killIfLifetimeOver();


        

        if (!burnActive)
        {
            transform.rotation = Quaternion.LookRotation(rbRef.velocity);
            //rbRef.AddForce(transform.forward * motorForce);

        }
        

    }

    private void OnDestroy()
    {
        effectsBehavior.beginDissipation();
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {

        //photonView.RPC("rpcContactProcess")

        Debug.Log("Rocket trigger enter");

        if (myFlow != null)
        {

            CombatFlow otherFlow = other.transform.root.GetComponent<CombatFlow>();
            if (otherFlow != null)
            {
                if (myFlow.localOwned)
                {

                    if (explodeOnOther(other.gameObject))
                    {
                        Debug.Log("Rocket collider triggered... exploding");
                        //rpcContactProcess(transform.position, getVictimId(other.transform.root.gameObject));
                        impactLocal(transform.position, other.transform.root.gameObject);
                    }
                }
            }
        }

    }

    override
    public void impactLocal(Vector3 position, GameObject other)
    {
        GameObject otherRoot = other;
        CombatFlow otherFlow = other.GetComponent<CombatFlow>();


        transform.position = position;

        Debug.Log("Impact local begin");

        if (otherRoot != null) // do not do anything against effects
        {
            bool doExplode = !otherRoot.CompareTag("Effects");

            //Debug.LogWarning("NOTE: ROCKET FOUND OTHER ROOT GAMEOBJECT");

            if (otherFlow != null)
            {
                //if (otherFlow.team != myTeam || friendlyImpact)
                if(friendlyImpact)
                {
                    if (myFlow.localOwned)
                    {
                        //otherFlow.currentHP -= impactDamage;
                        otherFlow.dealDamage(impactDamage);
                    }
                }
                else
                {
                    doExplode = false;
                }
            }

            if (doExplode)
            {
               // myFlow.currentHP -= myFlow.currentHP;
                //myFlow.dealLocalDamage(myFlow.getHP());

                if (effectsObj != null)
                {
                    //effectsObj.GetComponent<Light>().enabled = false;
                    effectsBehavior.emitLight(false);
                }
                //Debug.Log("Rocket HP after impact: " + myFlow.currentHP);
                myFlow.die();
            }
        }

        Debug.Log("Impact local end");
    }

    [PunRPC]
    override
    public void rpcContactProcess(Vector3 position, int otherId)
    {
        //Debug.Log("Rocket colliding. Arming time: " + armingTime + ", arming timer: " + armTimeRemaining + ", isArmed: " + armed);

        Debug.LogWarning("NOTE: ROCKET CONTACT TRIGGERED");

        GameObject otherRoot = null;
        CombatFlow otherFlow = null;

        if (otherId != -1)
        {
            otherRoot = PhotonNetwork.GetPhotonView(otherId).gameObject;
            otherFlow = otherRoot.GetComponent<CombatFlow>();
        }

        transform.position = position;

        if (otherRoot != null) // do not do anything against effects
        {
            bool doExplode = !otherRoot.CompareTag("Effects");
            doExplode = true;

            Debug.LogWarning("NOTE: ROCKET FOUND OTHER ROOT GAMEOBJECT");

            if (otherFlow != null)
            {
                if (otherFlow.team != myTeam || friendlyImpact)
                {
                    if (myFlow.localOwned)
                    {
                        //otherFlow.currentHP -= impactDamage;
                        otherFlow.dealDamage(impactDamage);
                    }
                    
                }
                else
                {
                    doExplode = false;
                }
            }

            if (doExplode)
            {
                //myFlow.currentHP -= myFlow.currentHP;

                myFlow.dealLocalDamage(myFlow.getHP());

                //effectsObj.GetComponent<Light>().enabled = false;
                effectsBehavior.emitLight(false);
                //effectsBehavior.enableDissipationTimer = true;

                effectsBehavior.beginDissipation();

                //Debug.Log("Rocket HP after impact: " + myFlow.getHP());
            }
        }
    }


}
