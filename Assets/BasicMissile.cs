using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


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
    private bool guidedLaunch;
    private GameObject impactVictimRoot;

    private PhotonView photonView;
    

    // Start is called before the first frame update
    void Start()
    {
        photonView = PhotonView.Get(this);
        myCombatFlow = GetComponent<CombatFlow>();
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);

        //effectsObj = Instantiate(effectsOriginalObj);
        //Destroy(effectsOriginalObj);
        //effectsObj.transform.position = effectsCenter.position;

        //effectsObj.GetComponent<Light>().enabled = false;
        //effectsObj.GetComponent<TrailRenderer>().enabled = false;

        myCombatFlow.isActive = false;


        
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
    }

    

    private void FixedUpdate()
    {
        checkLinecastCollision();

        if ( launched)
        {
            if (myCombatFlow.isLocalPlayer)
            {
                if (myTarget != null)
                {
                    updateTargetPosition();


                    // transform.LookAt(targetPosition);
                }
                //rbRef.velocity = transform.forward * speed;


                if (checkProximityFuse())
                {
                    // make this rpc
                    effectsObj.GetComponent<Light>().enabled = false;
                    myCombatFlow.currentHP -= myCombatFlow.currentHP;
                }
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

    private void OnTriggerEnter(Collider other)
    {
        contactProcess(other.gameObject);
    }

    override
    public void contactProcess(GameObject other)
    {
        GameObject otherRoot = other.gameObject.transform.root.gameObject;
        CombatFlow otherFlow = otherRoot.GetComponent<CombatFlow>();

        Weapon otherWeapon = otherRoot.gameObject.GetComponent<Weapon>();

        bool doExplode = true; // various conditions will try to make this false

        if (other.CompareTag("Effects") && !impactOnEffects)
            doExplode = false;

        if (otherFlow != null)
        {
            if (otherFlow.team == myTeam && !friendlyImpact)
                doExplode = false;
        }

        if (doExplode)
        {

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

                // =========  TRY TO DEAL IMPACT

                bool doDealImpact = false;

                // leaving this super obfuscated like this in case more complex conditions wanted later
                if (otherFlow != null)
                {
                    doDealImpact = true;
                }


                // finally, deal the impact
                if (doDealImpact)
                {

                    otherFlow.currentHP -= impactDamage;
                    Debug.Log("Impact dealing " + impactDamage + " damage to " + otherFlow);
                }



            }


            effectsObj.GetComponent<Light>().enabled = false;
            myCombatFlow.currentHP -= myCombatFlow.currentHP;
        }
    }




    // only callable by the local player. No need to check photon ownership
    override
    public void launch()
    {
        myCombatFlow.isLocalPlayer = true; // NetPosition will propogate this instance to rest of clients

        photonView.RPC("rpcLaunch", RpcTarget.AllBuffered);

        guidedLaunch = myTarget != null;
        
    }

    [PunRPC]
    private void rpcLaunch()
    {
        GetComponent<NetPosition>().active = true;

        myHardpoint.readyToFire = false;
        myHardpoint.loadedWeaponObj = null;


        Destroy(GetComponent<FixedJoint>());


        effectsObj.GetComponent<Light>().enabled = true;
        effectsObj.GetComponent<TrailRenderer>().enabled = true;
        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;

        myHardpoint.roundsRemain = 0;
    }


    private bool checkProximityFuse()
    {
        bool fuseTriggered = false;
        
        // explode if targetPosition has not been set yet
        if (guidedLaunch)
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
