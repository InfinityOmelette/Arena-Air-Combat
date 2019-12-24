using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBomb : Weapon
{

    public GameObject ownerObj;

    public CombatFlow myCombatFlow;

    public Rigidbody rbRef;

    public float impactDamage;


    private GameObject oneImpactVictim;
    
    void Awake()
    {
        myCombatFlow = GetComponent<CombatFlow>();
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);

        myCombatFlow.isActive = false;
    }

    override
    public void linkToOwner(GameObject newOwner)
    {
        ownerObj = newOwner;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        tryArm();

        if (armed)
        {
            transform.rotation = Quaternion.LookRotation(rbRef.velocity, transform.up);
        }
    }


    private void FixedUpdate()
    {
        checkLinecastCollision();
    }


    override
    public void launch()
    {
        myHardpoint.readyToFire = false;
        myHardpoint.loadedWeaponObj = null; // no longer loaded -- remove reference

        Destroy(GetComponent<FixedJoint>());

        rbRef.velocity = ownerObj.GetComponent<Rigidbody>().velocity;

        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        contactProcess(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        contactProcess(collision.gameObject);
    }

    override
    public void contactProcess(GameObject other)
    {
        if (oneImpactVictim == null)
        {


            bool doAct = true; // various conditions will try to make this false


            // impact with effects
            if (other.CompareTag("Effects") && !impactOnEffects)
                doAct = false;


            // friendly fire
            CombatFlow otherFlow = other.gameObject.GetComponent<CombatFlow>();
            if (otherFlow != null)
            {
                if (otherFlow.team == myTeam && !friendlyImpact)
                    doAct = false;
            }

            // don't act if touching explosion

            if (doAct)
            {
                oneImpactVictim = other.transform.root.gameObject;

                Debug.Log("Bomb collided");
                myCombatFlow.currentHP -= myCombatFlow.currentHP; // die immediately on collision
                                                                  //CombatFlow otherFlow = other.gameObject.GetComponent<CombatFlow>();
                if (otherFlow != null)
                {
                    otherFlow.currentHP -= impactDamage;
                }
            }
        }
    }

    

}
