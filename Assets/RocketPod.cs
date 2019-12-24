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
    public float rocketFireRateDelay;
    public float rocketMotorBurntime;


    public short magazineSize;
    

    public float reloadTimerMax;
    public float reloadTimerCurrent;
    



    public float rocketSpawnAheadDistance;

    bool doLaunch = false;

    public float rocketSpreadHoriz;
    public float rocketSpreadVert;



    private void Awake()
    {
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        roundsRemain = magazineSize;
    }

    // Update is called once per frame
    void Update()
    {
        
        //bool readyToFire = countDownFireRate();
        if (countDownFireRate())
        {
            if (doLaunch) // launch command pressed
            {
                if (roundsRemain > 0)
                {
                    Debug.Log("doLaunch is " + doLaunch + ", fireRocket() called");
                    fireRocket();
                }
                else
                {
                    myHardpoint.readyToFire = false;
                    Debug.Log("No rockets remaining. Ignoring launch");
                }
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

        roundsRemain--;

        // spawn, set references
        GameObject newObj = Instantiate(rocketPrefab);
        Rocket newRocket = newObj.GetComponent<Rocket>();
        CombatFlow newFlow = newObj.GetComponent<CombatFlow>();
        Rigidbody newRB = newObj.GetComponent<Rigidbody>();

        // positioning
        newObj.transform.position = transform.position + ownerObj.transform.forward * rocketSpawnAheadDistance;

        // rotation -- with randomized spread
        Quaternion rocketRotation = ownerObj.transform.rotation;
        rocketRotation *= getRocketSpreadRotation(rocketSpreadHoriz, rocketSpreadVert);
        newObj.transform.rotation = rocketRotation;




        newRB.velocity += ownerObj.GetComponent<Rigidbody>().velocity;


        // initial rocket data
        newRocket.setArmTime(rocketArmingTime);
        newRocket.motorForce = rocketForce;
        newRocket.motorBurnTime = rocketMotorBurntime;
        newRocket.launched = true;

        // resetTimer
        fireRateTimer = rocketFireRateDelay;

        // initial combatFlow data
        newFlow.team = myTeam;
        

        

        return newObj;
    }


    private Quaternion getRocketSpreadRotation(float horizSpread, float vertSpread)
    {

        horizSpread = Random.Range(-horizSpread, horizSpread);
        vertSpread = Random.Range(-vertSpread, vertSpread);

        Vector3 newEuler = new Vector3(vertSpread, horizSpread, 0.0f); // rocket rotation will change by this
        return Quaternion.Euler(newEuler);
    }

    override
    public void launch()
    {
        Debug.Log("Rocket pod launch() called");
        doLaunch = true;
    }

    override
    public void launchEnd()
    {
        Debug.Log("Ending rocket pod launch");
        doLaunch = false;
    }


    // called repeatedly from update until reload is complete
    override
    public void reloadProcess()
    {
        // count down reload timer
        if(reloadTimerCurrent > 0)
        {
            reloadTimerCurrent -= Time.deltaTime;
            myHardpoint.currentReloadTimer = reloadTimerCurrent; // communicate reload time to hardpoint
        }
        else  // if reload timer done, make ready to fire again
        {
            myHardpoint.readyToFire = true; // prevent this from being called again until next reload cycle
            roundsRemain = magazineSize;
        }
        
    }

    override
    public void linkToOwner(GameObject ownerObjArg)
    {
        ownerObj = ownerObjArg;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();
        myHardpoint.reloadTimeMax = reloadTimerMax;
        myHardpoint.fireRateDelayRaw = rocketFireRateDelay;

        Debug.Log("Linking " + gameObject.name + " to owner: " + ownerObjArg.name);
    }
}
