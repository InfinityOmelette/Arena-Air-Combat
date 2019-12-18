using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMissile : Weapon
{

    // inherits target property from Weapon

    public GameObject ownerObj;


    public GameObject missileModel;
    public GameObject missileActive;
    private Rigidbody rbRef;
    private CombatFlow myCombatFlow;


    public float speed;
    public float armingTime;
    private float armTimeRemaining;
    public bool launched = false;
    public bool armed = false;

    

    // Start is called before the first frame update
    void Start()
    {
        myCombatFlow = GetComponent<CombatFlow>();
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);

        GetComponent<CombatFlow>().isActive = false;
        

    }

    override
    public void linkToOwner(GameObject ownerObjArg)
    {
        ownerObj = ownerObjArg;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();
    }
    

    // consider making this recursive to go into all of the children's children as well?
    bool setColliders(bool enableColl)
    {
        //Debug.Log("SET COLLIDERS BEGINS");
        return setChildColliders(gameObject, enableColl);
    }


    bool setChildColliders(GameObject obj, bool enableColl)
    {
        //Debug.Log("SetChildColliders called for: " + obj + ", which has " + obj.transform.childCount + " children");
        if (obj.transform.childCount > 0)
        {
            
            for (short i = 0; i < obj.transform.childCount; i++)
            {
                GameObject childObj = obj.transform.GetChild(i).gameObject;
                Collider childColl = childObj.GetComponent<Collider>();
                if (childColl != null)
                    childColl.enabled = enableColl;
                setChildColliders(childObj, enableColl);
            }
        }
        return enableColl;
    }




    void Update()
    {
        if (launched)
        {

            if(armTimeRemaining < 0)
            {
                armed = true;
                setColliders(true);
            }

            armTimeRemaining -= Time.deltaTime;

        }
    }

    private void FixedUpdate()
    {
        if ( launched)
        {
            if(myTarget != null)
            {
                transform.LookAt(myTarget.transform);
            }
            rbRef.velocity = transform.forward * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        myCombatFlow.currentHP -= 100f; // die immediately on collision
        CombatFlow otherFlow = collision.gameObject.GetComponent<CombatFlow>();
        if(otherFlow != null)
        {
            otherFlow.currentHP -= 100f;
        }
    }


    override
    public void launch()
    {
        Destroy(GetComponent<FixedJoint>());
        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;
        
    }
}
