using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatLaunchGear : MonoBehaviour
{
    CombatFlow rootFlow;
    EngineControl engine;

    public static int CATAPULT_LAYER = 15;

    Catapult linkedCat;


    public float launchTimerMax = 2f;
    private float launchTimer;

    public bool doAttach = false;
    public bool doLaunch = false;

    public float launchThrottlePercent = 85f;
    public float launchReadyDistance = 1f;

    public float moveToCatSpeed = 3f;
    public float rotateToCatRate;

    public float shootTimerMax = 3f;
    private float shootTimer;

    public float launchAcceleration = 30f; // m/s per second

    public float maxLinkInitiateSpeed = 20;

    Vector3 launchAxis;

    public float maxRotSpeedDEG = 15f; // degrees per second

    public WheelsControl wheelControl;

    //public float angleCloseEnoughThreshold = .5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    private void FixedUpdate()
    {
        // if (doAttach)
        //  move velocity and rotation towards linked cat launch center

        if (doAttach && linkedCat != null)
        {
            moveToLaunchPoint();
            tryCountToLaunch();
        }

        if(doLaunch && linkedCat != null)
        {
            doShoot();
        }
    }

    private void doShoot()
    {
        shootTimer -= Time.fixedDeltaTime;
        // manipulate velocity
        //rootFlow.myRb.velocity += launchAxis * launchAcceleration * Time.fixedDeltaTime;
        // must prevent rudder influence

        // follow velocity schedule for controlled launch
        //  - ensure movement strictly along track
        float timeSinceLaunch = shootTimerMax - shootTimer;
        rootFlow.myRb.velocity = launchAxis * launchAcceleration * timeSinceLaunch;

        rotateToLaunchAxis(); // prevent rudder influence

        if(shootTimer < 0f)
        {
            doLaunch = false;
            linkedCat.release(this);
            wheelControl.setSteeringLock(false);

        }
    }

    
    private void tryCountToLaunch()
    {
        if (launchTimerConditions())
        {
            launchTimer -= Time.fixedDeltaTime;

            if(launchTimer < 0f)
            {

                beginShoot();
                
            }
        }
    }

    private void beginShoot()
    {
        Debug.Log("Beginning launch");
        // Begin launch
        launchTimer = launchTimerMax; // reset timer
        doAttach = false;
        doLaunch = true;
        shootTimer = shootTimerMax;
        launchAxis = linkedCat.launchCenter.forward;

        //if (wheelControl != null)
        //{
        //    wheelControl.externApplyBrake(0f);
        //    wheelControl.endSlide();
        //}
    }

    //  - close to launch point
    //  - engine throttle % high
    private bool launchTimerConditions()
    {
        return linkedCat != null && getEngine().currentBaseThrustPercent > launchThrottlePercent
            && Vector3.Distance(transform.position, linkedCat.launchCenter.position) < launchReadyDistance;
    }

    private void moveToLaunchPoint()
    {
        Debug.Log("Moving to launch point");
        Vector3 dirToCat = linkedCat.launchCenter.position - transform.position;
        dirToCat = new Vector3(dirToCat.x, 0f, dirToCat.z);

        getRootFlow().myRb.velocity = dirToCat * moveToCatSpeed;

        //getRootFlow().myRb.position = Vector3.MoveTowards(getRootFlow().transform.position, linkedCat.launchCenter.position,
        //    Mathf.Min( moveToCatSpeed * Time.deltaTime, dirToCat.magnitude));


        rotateToLaunchAxis();
    }

    private void rotateToLaunchAxis()
    {
        Vector3 aircraftFwd = getRootFlow().transform.forward;
        Vector3 launchFwd = linkedCat.launchCenter.forward;


        float rotAngleDeg = Vector3.SignedAngle(aircraftFwd, launchFwd, Vector3.up);
        float rotAngleRAD = Mathf.Deg2Rad * rotAngleDeg;



        float rotSpeed = Mathf.Clamp(rotAngleRAD * rotateToCatRate,
            -maxRotSpeedDEG * Mathf.Deg2Rad, maxRotSpeedDEG * Mathf.Deg2Rad);



        getRootFlow().myRb.angularVelocity = new Vector3(0f, rotSpeed, 0f);





    }

    public CombatFlow getRootFlow()
    {
        if (rootFlow == null)
        {
            rootFlow = transform.root.GetComponent<CombatFlow>();
        }
        return rootFlow;
    }

    public EngineControl getEngine()
    {
        if(engine == null)
        {
            engine = getRootFlow().GetComponent<EngineControl>();
        }
        return engine;
    }


    // Triggering into launchbox
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == CATAPULT_LAYER 
            && getRootFlow().myRb.velocity.magnitude < maxLinkInitiateSpeed)
        {
            Debug.Log("Cat layer triggered");
            CatapultLaunchbox catLaunchBox = other.GetComponent<CatapultLaunchbox>();
            if(catLaunchBox != null && !doLaunch && !doAttach)
            {
                Debug.Log("Linking to cat");
                linkedCat = catLaunchBox.chooseCat(this);

                linkedCat.linkToGear(this);

                doAttach = true;

                launchTimer = launchTimerMax;

                if (wheelControl != null)
                {
                    //wheelControl.externApplyBrake(1.0f); // apply brake to dampen vibrations
                    //wheelControl.beginSlide();
                    wheelControl.setSteeringLock(true);
                }
            }

        }
        //isCatching = other.gameObject.layer == 14;
    }
}
