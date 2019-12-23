using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPod : Weapon
{
    public GameObject ownerObj;

    public CombatFlow myCombatFlow;

    public Rigidbody rbRef;

    public GameObject rocketPrefab;

    public float rocketArmingTime;
    
    public float rocketForce;
    public float fireRateTimer;
    public float rocketFireRateDelay;
    public float rocketMotorBurntime;


    public float rocketMagazineSize;
    public float rocketMagazineReloadSize;



    public float rocketSpawnAheadDistance;

    bool doLaunch = false;


    private void Awake()
    {
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //bool readyToFire = countDownFireRate();
        if (countDownFireRate())
        {
            if (doLaunch)
            {
                fireRocket();
            }
        }
    }

    bool countDownFireRate()
    {
        bool doFire = false;
        if (fireRateTimer > 0f)
        {
            
            fireRateTimer -= Time.deltaTime;
        }
        else // timer ran out
        {
            doFire = true;
        }
        
        return doFire;
    }


    private GameObject fireRocket()
    {
        // spawn, set references
        GameObject newObj = Instantiate(rocketPrefab);
        Rocket newRocket = newObj.GetComponent<Rocket>();
        CombatFlow newFlow = newObj.GetComponent<CombatFlow>();
        Rigidbody newRB = newObj.GetComponent<Rigidbody>();

        // positioning
        newObj.transform.position = transform.position + ownerObj.transform.forward * rocketSpawnAheadDistance;
        newObj.transform.rotation = ownerObj.transform.rotation;
        newRB.velocity += ownerObj.GetComponent<Rigidbody>().velocity;


        // initial rocket data
        newRocket.setArmTime(rocketArmingTime);
        newRocket.motorBurnTime = rocketMotorBurntime;
        newRocket.launched = true;

        // resetTimer
        fireRateTimer = rocketFireRateDelay;

        // initial combatFlow data
        newFlow.team = myTeam;
        

        

        return newObj;
    }


    override
    public void launch()
    {
        doLaunch = true;
    }

    override
    public void launchEnd()
    {
        doLaunch = false;
    }


    override
    public void linkToOwner(GameObject ownerObjArg)
    {
        ownerObj = ownerObjArg;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();
        Debug.Log("Linking to owner: " + ownerObjArg.name);
    }
}
