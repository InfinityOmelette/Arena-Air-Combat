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

    public Vector3 targetPos;

    public List<Vector3> waypoints;
    public int waypointIndex;

    public float waypointRadius;

    public float fwdCheckTimeMultiplier;
    public float timeToCrashOverride;
    public float groundCheckRayPitchOffset;
    public float groundAvoidPitchOffset;

    public float maxClimbOffset;

    public float baseClimbAngle;
    public float climbAngleOffset;
    public float climbSpeedCoeff;

    public float VERTICAL_ANGLE;
    public float maxHorizOffset;

    public float canZoomSpeedCoeff;

    //public float crashPitchOverride;

    public CombatFlow targetFlow;
    public bool dogfightMode;

    public NAV_MODE navMode;

    float MS_2_KPH = 3.6f;


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

        targetPos = waypoints[waypointIndex];

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
                targetPos = targetFlow.transform.position;
            }
            else
            {
                navMode = NAV_MODE.WAYPOINT_MISSION;
            }
        }

        if (navMode == NAV_MODE.WAYPOINT_MISSION)
        {
            if (Vector3.Distance(transform.position, targetPos) < waypointRadius)
            {
                nextWaypoint();
                targetPos = waypoints[waypointIndex];
            }
            else
            {
                targetPos = waypoints[waypointIndex];
            }
        }

        dir = offsetForAoA(targetPos - transform.position);

        //Debug.DrawRay(transform.position, dir * 10f, Color.green);

        //Debug.DrawRay(transform.position, dir * 10f, Color.yellow);

        //Vector3 debugOffset = setClimbOffsets(dir, 90f, 45f);
        //Debug.DrawRay(transform.position, debugOffset * 15f, Color.cyan);


        //Debug.Log("GearIsDown: " + wheels.gearIsDown);

        bool climbApplied = false;

        if (dir.y > 0f && !wheels.gearIsDown && !canZoomClimb(targetPos)) // don't mess with takeoff
        {
            dir = climbProcess(dir);
            //Debug.DrawRay(transform.position, dir * 10f, Color.green);
            climbApplied = true;
        }

        //Debug.Log("Climb applied: " + climbApplied);


        dirAI.targetDir = groundAvoid(dir);

        Debug.DrawRay(transform.position, dir * 5f, Color.cyan);
        Debug.DrawLine(transform.position, targetPos, Color.white);

    }

    public Vector3 climbProcess(Vector3 dir)
    {
        //Debug.Log("Climb process called");
        

        if (dir.y > 0f)
        {
            float dirPitch = -getPitch(dir);

            // calculate climb angle
            float climbAngle = calculateClimbAngle(myRb.velocity.magnitude);

            

            if (dirPitch > climbAngle)
            {
                Debug.DrawRay(transform.position, dir * 10f, Color.white);

                float angleToVertical = VERTICAL_ANGLE - climbAngle; // don't pitch past vertical
                float maxClimb = climbAngle + Mathf.Min(angleToVertical, maxClimbOffset); // mathf.min here to prevent pitching above VERTICAL_ANGLE


                // based on how high dir is above climb angle, set horiz offset
                // 0.0f -> direct, 1.0f -> fully perpendicular
                float offsetScale = Mathf.Clamp(Mathf.InverseLerp(climbAngle, maxClimb, dirPitch), 0.0f, 1.0f);

                // -1 or 1. Keep nose on same side of dir
                float offsetDirection = calculateOffsetDirection(dir);

                float horizOffsetResult = offsetDirection * offsetScale * maxHorizOffset;

                //Debug.Log("dirPitch: " + dirPitch + ", climbAngle: " + climbAngle +
                //    ", offsetScale: " + offsetScale + ", horizOffsetResult: " + horizOffsetResult +
                //    ", maxClimbOffset: " + angleToVertical + ", maxClimb: " + maxClimb);



                
                dir = setClimbOffsets(dir, horizOffsetResult, climbAngle);
                
                //Debug.DrawRay(transform.position, dir * 50, Color.blue);

                dir = offsetForAoA(dir);

                Debug.DrawRay(transform.position, dir * 10f, Color.green);

            }
        }


        return dir;
    }

    private bool canZoomClimb(Vector3 targetPos)
    {
        float vertDistance = (targetPos - transform.position).y;
        float speed = myRb.velocity.magnitude * MS_2_KPH;
        float speedMod = speed * canZoomSpeedCoeff;

        bool canZoom = vertDistance < speedMod;

        //Debug.Log("CanZoom: " + canZoom + ", vertDistance: " + vertDistance + ", speed: " + speed + ", speedMod: " + speedMod);

        return canZoom;
    }
    

    private float calculateOffsetDirection(Vector3 dir)
    {
        // project forward and targetDirection vectors onto horizontal plane

        Vector3 fwd = transform.forward;
        fwd -= new Vector3(0.0f, fwd.y, 0.0f);

        dir -= new Vector3(0.0f, dir.y, 0.0f);

        Vector3 torqueDir = Vector3.Cross(fwd, dir);

        return -Mathf.Sign(torqueDir.y);

    }

    private Vector3 setClimbOffsets(Vector3 dir, float horizOffset, float pitchOffset)
    {
        dir -= new Vector3(0.0f, dir.y, 0.0f);// remove y component

        //Quaternion rotBy = Quaternion.Euler(45f, 0.0f, 0.0f);
        Quaternion horizRot = Quaternion.AngleAxis(horizOffset, Vector3.up);
        Quaternion vertRot = Quaternion.AngleAxis(pitchOffset, -Vector3.Cross(Vector3.up, dir));
        Quaternion rotBy = horizRot * vertRot;

        //Debug.Log("HorizOffset: " + horizOffset + ", pitchOffset: " + pitchOffset);

        //Debug.DrawRay(transform.position, dir * 50f, Color.red);
        dir = rotBy * dir;
        //Debug.DrawRay(transform.position, dir * 10f, Color.white);

        return dir;
    }

    //private float 

    private float calculateClimbAngle(float spd)
    {
        
        spd *= MS_2_KPH;

        float val = spd * climbSpeedCoeff + climbAngleOffset;
        val = Mathf.Clamp(val, baseClimbAngle, 85f); // staying away from direct vertical just because weird things happen with azimuth
        return val;
    }

    public Vector3 groundAvoid(Vector3 dir)
    {
        int terrainLayer = 1 << 10; // line only collides with terrain layer
        
        // check both forward and low raycast. Prioritize low


        Vector3 fwdCheckRay = myRb.velocity * timeToCrashOverride;
        RaycastHit fwdHit;
        bool fwdIntersect = Physics.Raycast(transform.position, fwdCheckRay * fwdCheckTimeMultiplier, out fwdHit,
            myRb.velocity.magnitude * timeToCrashOverride, terrainLayer);


        Vector3 groundCheckRay = pitchOffset(fwdCheckRay, groundCheckRayPitchOffset);
        RaycastHit lowHit;
        bool groundIntersect = Physics.Raycast(transform.position, groundCheckRay, out lowHit, 
            myRb.velocity.magnitude*timeToCrashOverride,  terrainLayer);



        if (groundIntersect || fwdIntersect)
        {
            // prioritize the low raycast
            Vector3 intersectPos;
            if (groundIntersect)
            {
                intersectPos = lowHit.point;
            }
            else
            {
                intersectPos = fwdHit.point;
            }


            float estimatedCrashTime = Vector3.Distance(transform.position, intersectPos) / myRb.velocity.magnitude;
            float overrideMod = Mathf.Clamp((timeToCrashOverride - estimatedCrashTime) / timeToCrashOverride,0.0f,  1.0f);

            Vector3 overrideDir = intersectPos - transform.position;

            if (overrideDir.y < 0f)
            {
                overrideDir -= new Vector3(0.0f, overrideDir.y, 0.0f); // remove y component. Horizontal direction
            }

            overrideDir = pitchOffset(overrideDir, groundAvoidPitchOffset);

            overrideDir = offsetForAoA(overrideDir);

            Debug.Log("========= GROUND INTERSECTED , estimatedCrashTime: " + estimatedCrashTime + ", overrideMod: " + overrideMod);
            Debug.DrawLine(transform.position, lowHit.point, Color.red);
            Debug.DrawRay(transform.position, overrideDir, Color.yellow);

            dir = Vector3.Lerp(dir.normalized, overrideDir, overrideMod);
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

            //currWpt = waypoints[waypointIndex];
        }
        else
        {
            Debug.Log("Unable to find waypoints");
        }
    }

    private float getPitch(Vector3 dir)
    {
        return getPitch(Quaternion.LookRotation(dir, Vector3.up));
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

    private Vector3 dir2Euler(Vector3 dir)
    {
        Quaternion localOldDirRotation = Quaternion.LookRotation(dir);
        return Quaternion.ToEulerAngles(localOldDirRotation) * Mathf.Rad2Deg;
    }


}
