using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBomb : Weapon
{

    public GameObject ownerObj;

    public CombatFlow myCombatFlow;

    public Rigidbody rbRef;



    // Start is called before the first frame update
    void Start()
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


    override
    public void launch()
    {
        Destroy(GetComponent<FixedJoint>());

        rbRef.velocity = ownerObj.GetComponent<Rigidbody>().velocity;

        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bomb collided");
        myCombatFlow.currentHP -= 100f; // die immediately on collision
        CombatFlow otherFlow = other.gameObject.GetComponent<CombatFlow>();
        if (otherFlow != null)
        {
            otherFlow.currentHP -= 100f;
        }
    }

}
