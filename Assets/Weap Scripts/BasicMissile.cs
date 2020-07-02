using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;


// loading and detonation properties
public class BasicMissile : Weapon
{

    // inherits target property from Weapon

    public GameObject ownerObj;

    //public GameObject effectsOriginalObj;
    

    private Rigidbody rbRef;
    private CombatFlow myCombatFlow;

    public float proximityFuseRange; // automatically set to explodeStats radius
    public float fuseLeadTime;
    public float impactDamage;
    public float speed;

    private Vector3 targetPosition;
    public bool guidedLaunch;
    private GameObject impactVictimRoot;

    public float selfDestructSpeed;

    private bool doDestroy = false;

    public Radar radar;

    private int targetID;

    public float PASS_OWNERSHIP_DISTANCE = 350f;


    private bool hasPassed = false;
    

    void awake()
    {
        init();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //init();
        //Debug.LogWarning("Found radar of " + radar.gameObject.name);


        //photonView = PhotonView.Get(this);






    }

    private void init()
    {
        setRefs();
        setColliders(false);

        // RocketMotor instantiates effects obj

        myCombatFlow.isActive = false;
    }

    private void setRefs()
    {
        myCombatFlow = GetComponent<CombatFlow>();
        radar = GetComponent<Radar>();
        rbRef = GetComponent<Rigidbody>();
        flightSound = GetComponent<AudioSource>();
    }

    override
    public void linkToOwner(GameObject ownerObjArg)
    {
        ownerObj = ownerObjArg;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();

        myHardpoint.roundsMax = 1;
        myHardpoint.roundsRemain = 1;
    }
    

    




    void Update()
    {
        tryArm();
        //effectsObj.transform.position = effectsCenter.position;
        checkLinecastCollision();
    }

    

    private void FixedUpdate()
    {
        

        if ( launched )
        {
            if (myCombatFlow.localOwned)
            {

                if(myTarget != null && !hasPassed && myTarget.GetComponent<CreepControl>() == null &&
                    Vector3.Distance(myTarget.transform.position, 
                    transform.position) < PASS_OWNERSHIP_DISTANCE)
                {
                    // target's instance will own this missile and show accurate position
                    myCombatFlow.giveOwnership(targetID);
                    Debug.LogWarning("Giving missile ownership");
                }


                if(armed && rbRef.velocity.magnitude < selfDestructSpeed)
                {
                    myCombatFlow.dealLocalDamage(myCombatFlow.getHP());
                }

                if (myTarget != null)
                {
                    updateTargetPosition();
                }

                if (checkProximityFuse())
                {
                    // make this rpc
                    if (effectsObj != null)
                    {
                        //effectsObj.GetComponent<Light>().enabled = false;
                        photonView.RPC("rpcDisableLight", RpcTarget.All);
                    }
                    if (myCombatFlow != null && myCombatFlow.localOwned)
                    {
                        // blow up local instance. Death itself should be networked fine
                        myCombatFlow.dealLocalDamage(myCombatFlow.getHP()); 
                    }
                }
            }
        }
    }

    public void setHasPassed(bool hasPassed)
    {
        this.hasPassed = hasPassed;
    }

    // ugh, this is so inefficient, but at least isn't scaled up enough to really matter
    //  - more efficient to batch this into another RPC, so that each instance independently determines when to disable light
    [PunRPC]
    private void rpcDisableLight()
    {
        effectsObj.GetComponent<Light>().enabled = false;
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

    

    private void OnTriggerEnter(Collider other)
    {

        //Debug.Log(myCombatFlow.currentHP);

        if (myCombatFlow != null)
        {
            if (myCombatFlow.localOwned)
            {
                GameObject otherRoot = other.transform.root.gameObject;
                //CombatFlow otherFlow = otherRoot.GetComponent<CombatFlow>();
                int otherId = getVictimId(otherRoot);

                if (explodeOnOther(otherRoot))
                {
                    Debug.Log("Missile collider triggered");
                    //rpcContactProcess(transform.position, other.transform.root.gameObject.GetComponent<PhotonView>().ViewID);
                    photonView.RPC("rpcContactProcess", RpcTarget.All,
                        transform.position, otherId);
                }


            }
        }
    }

    
    

    [PunRPC]
    override
    public void rpcContactProcess(Vector3 position, int otherId)
    {
        //Debug.LogWarning("rpcContactProcess locally called");
        GameObject otherRoot = null;
        CombatFlow otherFlow = null;

        if(otherId != -1)
        {
            otherRoot = PhotonNetwork.GetPhotonView(otherId).gameObject;
            otherFlow = otherRoot.GetComponent<CombatFlow>();
        }

        transform.position = position;

        //Weapon otherWeapon = otherRoot.gameObject.GetComponent<Weapon>();

        
        // If other has a flow and is the one impact victim
        if (otherFlow != null && otherRoot != impactVictimRoot)
        {
            //myCombatFlow.explodeStats.doExplode = false;
            impactVictimRoot = otherRoot;

            // don't explode if victim will die and if victim is not a projectile
            if (impactDamage > otherFlow.getHP() && otherFlow.type != CombatFlow.Type.PROJECTILE)
            {
                myCombatFlow.explodeStats.doExplode = false; // death will only trigger enemy explosion
            }

            // =========  TRY TO DEAL IMPACT

            bool doDealImpact = false;

            // leaving this super obfuscated like this in case more complex conditions wanted later
            if (otherFlow != null)
            {
                doDealImpact = true;
            }


            // finally, deal the impact
            if (doDealImpact && myCombatFlow.localOwned)
            {

                otherFlow.dealDamage(impactDamage);
                Debug.Log("Impact dealing " + impactDamage + " damage to " + otherFlow);
            }



        }

        if (effectsObj != null)
        {
            effectsObj.GetComponent<Light>().enabled = false;

        }

        if (myCombatFlow != null)
        {
            myCombatFlow.dealLocalDamage(myCombatFlow.getHP());
        }

        
        
    }




    // only callable by the local player. No need to check photon ownership
    override
    public void launch()
    {
        init();
        myCombatFlow.localOwned = true; // NetPosition will propogate this instance to rest of clients


        photonView.RPC("rpcLaunch", RpcTarget.AllBuffered);

        guidedLaunch = myTarget != null;

        

        if (guidedLaunch)
        {
            int targetID = myTarget.GetComponent<PhotonView>().ViewID;
            photonView.RPC("rpcPingPlayer", RpcTarget.All, targetID);

            //Debug.LogError("Guided launch against " + PhotonNetwork.GetPhotonView(targetID).name);
        }

        radar.radarOn = true;

    }

    [PunRPC]
    private void rpcPingPlayer(int photonId)
    {
        PhotonView targetView = PhotonNetwork.GetPhotonView(photonId);
        if(targetView != null)
        {
            CombatFlow targetFlow = targetView.GetComponent<CombatFlow>();
            if(radar == null)
            {
                radar = GetComponent<Radar>();
            }

            // all instances will have target set
            //  - this allows us to pass around ownership, to give more accurate display of missile position
            targetID = photonId;
            myTarget = targetView.gameObject;
            guidedLaunch = true;
            setRefs();
            //init();


            //radar.radarOn = targetFlow.isLocalPlayer;
            radar.pingPlayer = targetFlow.isLocalPlayer;

            radar.radarOn = radar.pingPlayer || myCombatFlow.localOwned || targetFlow.localOwned;


            
        }
    }

    [PunRPC]
    private void rpcLaunch()
    {
        if (!doDestroy)
        {
            GetComponent<NetPosition>().active = true;

            if (myHardpoint != null)
            {
                myHardpoint.readyToFire = false;
                myHardpoint.loadedWeaponObj = null;

                if (launchSound != null)
                {
                    myHardpoint.launchSoundSource.clip = launchSound;
                    myHardpoint.launchSoundSource.Play();
                }
            }

            if(flightSound != null)
            {
                flightSound.loop = true;
                flightSound.Play();
            }

            FixedJoint joint = GetComponent<FixedJoint>();
            if (joint != null)
            {
                Destroy(joint);
            }


            effectsObj.GetComponent<Light>().enabled = true;
            effectsObj.GetComponent<TrailRenderer>().enabled = true;
            launched = true;
            armTimeRemaining = armingTime;

            if (myCombatFlow == null)
            {
                myCombatFlow = GetComponent<CombatFlow>();
            }

            myCombatFlow.isActive = true;
            if (myHardpoint != null)
            {
                myHardpoint.roundsRemain = 0;
            }
        }
    }


    private bool checkProximityFuse()
    {
        bool fuseTriggered = false;

        if (myCombatFlow.localOwned)
        {
            // explode if targetPosition has not been set yet
            if (guidedLaunch)
            {
                

                float distance = Vector3.Distance(targetPosition, transform.position);
                if (distance < proximityFuseRange)
                {
                    //Debug.LogError("Proximity fuse blew");
                    fuseTriggered = true;
                }
            }
        }

        return fuseTriggered;
    }

    override
    public void destroyWeapon()
    {
        

        if(myCombatFlow == null)
        {
            myCombatFlow = GetComponent<CombatFlow>();
        }

        if (myCombatFlow.localOwned)
        {
            photonView.RPC("rpcDestroyWeapon", RpcTarget.AllBuffered);
        }
        
    }

    [PunRPC]
    private void rpcDestroyWeapon()
    {
        doDestroy = true;

        //Debug.LogWarning("Destroying weapon");
        //Debug.LogWarning("Effects obj found: " + effectsObj.name);
        //Destroy(effectsObj.gameObject);

        effectsObj.GetComponent<Light>().enabled = false;

        effectsObj.GetComponent<EffectsBehavior>().doCount = true;
        //Debug.LogWarning("Destroy successful: " + effectsObj == null);
        
        
        Destroy(gameObject);
    }
}
