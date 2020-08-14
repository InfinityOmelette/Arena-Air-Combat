using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Aircraft : MonoBehaviour
{
    public enum NAV_MODE
    {
        WAYPOINT_MISSION,
        DOGFIGHT
    }


    public CombatFlow myFlow;
    public HardpointController hardpoints;
    public CannonControl cannon;
    public WheelsControl wheels;
    public DirectionAI dirAI;
    public EngineControl engine;
    public Rigidbody myRb;

    public Vector3 currWpt;

    public List<Vector3> waypoints;
    private int waypointIndex;

    public float waypointRadius;
    public float timeToCrashOverride;
    //public float crashPitchOverride;

    public CombatFlow targetFlow;
    public bool dogfightMode;

    public NAV_MODE navMode;


    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        dirAI = GetComponent<DirectionAI>();
        engine = GetComponent<EngineControl>();
        myRb = GetComponent<Rigidbody>();
    }


    // Start is called before the first frame update
    void Start()
    {
        dirAI.isApplied = true;
        engine.currentThrottlePercent = 100f;

        GameObject wptContainer = GameObject.Find("JeffWaypoints");

        fillWaypointList(wptContainer);

        currWpt = waypoints[waypointIndex];

        //Debug.LogWarning("====================================== Curr wpt: " + currWpt);

    }

    private void fillWaypointList(GameObject wptContainer)
    {
        waypoints = new List<Vector3>();

        for (int i = 0; i < wptContainer.transform.childCount; i++)
        {
            waypoints.Add(wptContainer.transform.GetChild(i).position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //engine.input_throttleAxis = 1.0f;
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (navMode == NAV_MODE.DOGFIGHT)
            {
                navMode = NAV_MODE.WAYPOINT_MISSION;
            }
            else
            {
                navMode = NAV_MODE.DOGFIGHT;
                targetFlow = GameManager.getGM().localPlayer.GetComponent<CombatFlow>();
            }
        }

    }


    void FixedUpdate()
    {

        wheels.setGearEnabled(transform.position.y < 10f);

        Vector3 dir = transform.forward;

        if (navMode == NAV_MODE.DOGFIGHT)
        {
            if(targetFlow != null)
            {
                dir = offsetForAoA( targetFlow.transform.position - transform.position);
            }
            else
            {
                navMode = NAV_MODE.WAYPOINT_MISSION;
            }
        }

        if (navMode == NAV_MODE.WAYPOINT_MISSION)
        {
            if (Vector3.Distance(transform.position, currWpt) < waypointRadius)
            {
                nextWaypoint();
            }
            else
            {
                dir = offsetForAoA(currWpt - transform.position);
            }
        }

        dirAI.targetDir = groundAvoid(dir);
        
    }

    public Vector3 groundAvoid(Vector3 dir)
    {
        if (myRb.velocity.y < 0f)
        {
            float estimatedCrashTime = Mathf.Abs(transform.position.y / myRb.velocity.y);


            float overrideMod = Mathf.Clamp((timeToCrashOverride - estimatedCrashTime) / timeToCrashOverride,0.0f,  1.0f);

            Vector3 overrideDir = (myRb.velocity - new Vector3(0.0f, myRb.velocity.y, 0.0f)).normalized; // remove y component from vel vector
            overrideDir = offsetForAoA(overrideDir);
            dir = Vector3.Lerp(dir.normalized, overrideDir, overrideMod);

            //Debug.Log("EstimatedCrashTime: " + estimatedCrashTime + ", overrideMod: " + overrideMod);
        }
        return dir;
    }

    Vector3 pitchOffset(Vector3 dir, float pitchBy)
    {
        Vector3 offsetAxis = Vector3.Cross(dir, Vector3.up);
        Quaternion rotBy = Quaternion.AngleAxis(pitchBy, offsetAxis);
        return rotBy * dir;
    }

    Vector3 offsetForAoA(Vector3 targetDir)
    {
        float myPitch = getPitch(transform.rotation);
        float velPitch = getPitch(Quaternion.LookRotation(myRb.velocity));
        float vertAoA = myPitch - velPitch;

        ////Debug.Log("MyPitch: " + myPitch + ", velPitch: " + velPitch + ", vertAoA: " + vertAoA);

        //Vector3 offsetAxis = -Vector3.Cross(targetDir, Vector3.up);
        //Quaternion rotBy = Quaternion.AngleAxis(vertAoA, offsetAxis);


        return pitchOffset(targetDir, -vertAoA);
    }

    void nextWaypoint()
    {
        Debug.Log("Next waypoint");

        if (waypoints != null)
        {

            waypointIndex++;

            if (waypointIndex >= waypoints.Count)
            {
                waypointIndex = 0;
            }

            currWpt = waypoints[waypointIndex];
        }
        else
        {
            Debug.Log("Unable to find waypoints");
        }
    }

    private float getPitch(Quaternion rot)
    {
        return unEulerize(Quaternion.ToEulerAngles(rot).x * Mathf.Rad2Deg);
    }

    private float unEulerize(float angle)
    {
        if(angle > 180)
        {
            angle -= 360f;
        }

        return angle;
    }


}
