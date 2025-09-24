using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpAbility : AbilityParent
{

    public float warpDistance;

    Rigidbody myRb;


    // UI reference

    Quaternion storeRot;
    //Vector3 storeAngVel;

    //bool lockRotation = false;

    private int lockTickCounter = 0;

    public int lockRotTicks = 2;

    //public float warpCollisionCancelScale = .95f;

    //public float warpCollisionCancelTime = 1f;
    public float warpCollisionCancelMinDist = 150f;

    private void Awake()
    {
        base.init();
        //base.abilityName = "Warp";
        myRb = GetComponent<Rigidbody>();
        // spawn and activate UI element
        //  - load picture onto UI element
    }

    // Start is called before the first frame update
    void Start()
    {
        base.startProcess();
    }

    // Update is called once per frame
    void Update()
    {
        base.updateProcess();

    }

    private void FixedUpdate()
    {
        if (lockTickCounter > 0)
        {
            //if(lockTimer > 0)
            //{
            //    Debug.Log("********* LOCKING ROTATION ");
            //    myRb.rotation = storeRot;
            //    lockTimer -= Time.fixedDeltaTime;
            //}
            //else
            //{
            //    lockRot = false;
            //    lockTimer = lockDelayMax;
            //    //myRb.isKinematic = false;
            //}

            myRb.rotation = storeRot;
            //lockRotation = false;
            lockTickCounter--;
        }
    }

    override
    public void activate()
    {
        base.activate();

        //myRb.isKinematic = true;

        Vector3 initVelocity = myRb.velocity;
        Vector3 initAngularVel = myRb.angularVelocity;
        Quaternion initRot = myRb.rotation;

        storeRot = initRot; // unity physics rotation freak out from updating position instantly over long distances
        lockTickCounter = lockRotTicks; // so we store prior rotation and lock the rotation for 2 ticks

        Vector3 warpVect = transform.forward * warpDistance;

        RaycastHit hitInfo = new RaycastHit();
        short terrainLayer = 1 << 10; // only check collisions with terrain
        if (Physics.Linecast(transform.position, transform.position + warpVect,
            out hitInfo, terrainLayer))
        {
            Vector3 hitVect = hitInfo.point - transform.position;
            //hitVect *= warpCollisionCancelScale;


            // *** alternate idea -- time-based offset
            //  - Calculate player velocity IN warp direction
            //  - Retract warppoint by specified seconds
            //   > minimum offset required
            //   > do not change warpvector direction

            // EHHH....I'll just set a hard offset instead
            float hitVectMagnitude = hitVect.magnitude;

            // if cancelMinDist is greater than min offset, don't warp forward
            float effectiveCancelOffsetDist = Mathf.Min(hitVectMagnitude, warpCollisionCancelMinDist);

            Vector3 warpCancelOffsetVect = hitVect.normalized * effectiveCancelOffsetDist;

            hitVect -= warpCancelOffsetVect;

            warpVect = hitVect;

        }

        Vector3 newPos = myRb.position + warpVect;
        //myRb.position += transform.forward * warpDistance;

        myRb.transform.position = newPos;
        

        myRb.rotation = initRot;
        myRb.velocity = initVelocity;
        myRb.angularVelocity = initAngularVel;


        // activate any effects


    }

    override
    public void copyOther(AbilityParent other)
    {
        base.copyOther(other);

        WarpAbility otherWarp = (WarpAbility)other;
        warpDistance = otherWarp.warpDistance;
    }

    override
    public void equipAbilityToAircraftObject(GameObject aircraftObj)
    {
        // How to get unity editor values to pass into script?
        // tech object's attached script can have edited values

        // so we must add the raw script initially
        // and then copy values from the tech object onto the aircraft

        WarpAbility equippedWarp = aircraftObj.AddComponent<WarpAbility>(); // adds raw script instance
        equippedWarp.copyOther(this); // this should pass editor-set values from tech obj prefab onto equipped warp script
    }

}
