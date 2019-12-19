using System.Collections;
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


    public float speed;
    

    

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
                transform.LookAt(myTarget.transform);
            }
            rbRef.velocity = transform.forward * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        effectsObj.GetComponent<Light>().enabled = false;
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
        rbRef.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        effectsObj.GetComponent<Light>().enabled = true;
        effectsObj.GetComponent<TrailRenderer>().enabled = true;
        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;
        
    }
}
