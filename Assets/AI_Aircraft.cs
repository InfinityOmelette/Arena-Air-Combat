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

    public float lowLoopSpeed;
    public float highLoopSpeed;

    public float groundAvoidSensitivity;

    public float fwdCheckPitchOffset;
    public float fwdCheckTimeToCrash;
    public float fwdCheckSensitivity;
    public float lowCheckTimeToCrash;
    public float groundCheckRayPitchOffset;
    public float groundAvoidPitchOffset;

    public float sideCheckTimeToCrash;
    public float sideCheckRayYawOffset;
    public float wallAvoidRollOffset;

    public float maxClimbOffset;

    public float baseClimbAngle;
    public float climbAngleOffset;
    public float climbSpeedCoeff;

    public float VERTICAL_ANGLE;
    public float maxHorizOffset;

    public float canZoomSpeedCoeff;

    public float maxDirAngle;
    public float maxCorrectionAngle;

    //float pitchOffset = -40f;
    //float pitchCoeff = 0.2f;
    //float hardMaxPitchOffset = 20f;

    public float minWallAvoidAlt;

    public float groundAvoidHighPitchOffset;
    public float groundAvoidPitchCoeff;
    public float groundAvoidHardMaxPitchOffset;

    public float verticalBuffer;
    public float minPitchAboveDown;

    //public float crashPitchOverride;

    public CombatFlow targetFlow;
    public bool dogfightMode;

    public NAV_MODE navMode;

    float MS_2_KPH = 3.6f;

    private int terrainLayer = 1 << 10; // line only collides with terrain layer


    public Vector3 targetDir;

    private Vector3 currentDir;
    public float dirLerpRate;

    public float minTurnCircleTime;


    private AI_MissileEvade mslAvoid;
    

    void Awake()
    {
        // start these vectors with some magnitude, helps facilitate rotation lerping
        targetDir = transform.forward;
        currentDir = transform.forward;

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

        mslAvoid = GetComponent<AI_MissileEvade>();

        mslAvoid.enabled = true;

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

        if (!dirAI.useAi)
        {


            wheels.setGearEnabled(transform.position.y < 10f);

            

            if (navMode == NAV_MODE.DOGFIGHT)
            {
                if (targetFlow != null)
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

            targetDir = targetPos - transform.position;
        }

        
        targetDir = offsetForAoA(targetDir);

        //Debug.DrawRay(transform.position, dir * 10f, Color.green);

        //Debug.DrawRay(transform.position, dir * 10f, Color.yellow);

        //Vector3 debugOffset = setClimbOffsets(dir, 90f, 45f);
        //Debug.DrawRay(transform.position, debugOffset * 15f, Color.cyan);


        //Debug.Log("GearIsDown: " + wheels.gearIsDown);

        bool climbApplied = false;

        
        // climbProcess is applied for all navigation modes
        if (!dirAI.useAi && targetDir.y > 0f && !wheels.gearIsDown && !canZoomClimb(targetPos)) // don't mess with takeoff
        {
            targetDir = climbProcess(targetDir);
            //Debug.DrawRay(transform.position, dir * 10f, Color.green);
            climbApplied = true;
        }

        targetDir = mslAvoid.tryMissileEvade(targetDir);

        //Debug.Log("Climb applied: " + climbApplied);


        //Debug.Log("Speed: " + MS_2_KPH * myRb.velocity.magnitude);

        if (Vector3.Angle(transform.forward, targetDir) > 90f)
        {
            targetDir = preventLoop(targetDir);
        }


        targetDir = targetDir.normalized;

        if(navMode == NAV_MODE.WAYPOINT_MISSION && waypointIndex == 0 && !dirAI.useAi)
        {
            //dirAI.targetDir = targetDir;
            currentDir = targetDir;
        }
        else
        {
            targetDir = terrainAvoid(targetDir);
            currentDir = lerpRotateVect(currentDir, targetDir, dirLerpRate * Time.fixedDeltaTime);
            //dirAI.targetDir = targetDir;
        }

        
        dirAI.targetDir = currentDir;

        //dirAI.targetDir = terrainAvoid(dir);

       // Debug.DrawRay(transform.position, targetDir, Color.cyan, 5);
        //Debug.DrawRay(transform.position, currentDir, Color.green, 5);
        //Debug.DrawLine(transform.position, targetPos, Color.white, 5);

    }

    Vector3 preventLoop(Vector3 dir)
    {
        
        dir = lerpRotateVect(transform.forward, dir, 0.5f);

        if(dir.y > 0f)
        {
            dir.y *= calculateLoopability();
        }

        return dir;
    }

    public Vector3 lerpRotateVect(Vector3 from, Vector3 to, float lerp)
    {
        Quaternion fromRot = Quaternion.LookRotation(from, Vector3.up);
        Quaternion toRot = Quaternion.LookRotation(to, Vector3.up);
        Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, lerp);

        return lerpRot * Vector3.forward;
    }

    private float calculateLoopability()
    {
        float currSpd = myRb.velocity.magnitude * MS_2_KPH;
        float ratio = Mathf.InverseLerp(lowLoopSpeed, highLoopSpeed, currSpd);

        return Mathf.Clamp(ratio, 0.0f, 1.0f);
    }

    public Vector3 climbProcess(Vector3 dir)
    {
        //Debug.Log("Climb process called");
        

        if (dir.y > 0f)
        {
            float dirPitch = getPitch(dir);

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

                
                dir = setClimbOffsets(dir, horizOffsetResult, climbAngle);
                
                //Debug.DrawRay(transform.position, dir * 50, Color.blue);

                dir = offsetForAoA(dir);

                //Debug.DrawRay(transform.position, dir * 10f, Color.green);

            }
        }


        return dir;
    }

    private bool canLoop()
    {
        return (myRb.velocity.magnitude * MS_2_KPH) > highLoopSpeed;
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

    // look at desmos for equation
    // roughly, horizontal starts at 0 degrees at 200kph, 20 degrees at 300 kph
    private float calculateMaxPitch()
    {
        // always at least zero
        return Mathf.Max( myRb.velocity.magnitude * MS_2_KPH * groundAvoidPitchCoeff + groundAvoidHighPitchOffset,
            0.0f);
    }

    private float calculatePitchOvershootMod(float newPitch, float highPitch, float maxPitch, Vector3 dir)
    {
        //dir = dir.normalized * myRb.velocity.magnitude;
        //RaycastHit dirHit;
        //bool dirIntersect = Physics.Raycast(transform.position, dir, out dirHit,
        //    dir.magnitude, terrainLayer);

        //float timeToHit = 1000f; // arbitrarily large number signaling no intersect

        //// avoid turning into a wall if there's no room
        //if (dirIntersect)
        //{
        //    timeToHit = Vector3.Distance(transform.position, dirHit.point) / myRb.velocity.magnitude;

        //}

        //if (timeToHit < minTurnCircleTime)
        //{
        //    return 1.0f;
        //}
        //else
        {
            return Mathf.Clamp(Mathf.InverseLerp(highPitch, maxPitch, newPitch), 0.0f, 1.0f);
        }
    }

    private float calculateClimbAngle(float spd)
    {
        
        spd *= MS_2_KPH;

        float val = spd * climbSpeedCoeff + climbAngleOffset;
        val = Mathf.Clamp(val, baseClimbAngle, 85f); // staying away from direct vertical just because weird things happen with azimuth
        return val;
    }

    public float pitchAboveDown(Vector3 dir)
    {
        float angle = Vector3.Angle(-Vector3.up, dir) - verticalBuffer;
        return angle;
    }

    public Vector3 terrainAvoid(Vector3 dir)
    {
        // constrain desired direction within cone
        dir = rotateDirFromTo(transform.forward, dir, maxDirAngle);

        float dirPitchAboveDown = pitchAboveDown(dir);
        float underPitchOvershoot = minPitchAboveDown - dirPitchAboveDown;
        underPitchOvershoot = Mathf.Max(underPitchOvershoot, 0.0f);


        Debug.DrawRay(transform.position, dir, Color.red);

        dir = pitchOffset(dir, underPitchOvershoot);
        Debug.DrawRay(transform.position, dir, Color.red);

        int terrainLayer = 1 << 10; // line only collides with terrain layer

        // check both forward and low raycast. Prioritize low

        float angleDownToFwdCheck = pitchAboveDown(myRb.velocity);

        

        // FORWARD CHECK RAY -- JUST A FEW DEGREES BELOW VELOCITY
        Vector3 fwdCheckRay = myRb.velocity * fwdCheckTimeToCrash;
        fwdCheckRay = pitchOffset(fwdCheckRay, Mathf.Max(fwdCheckPitchOffset, -angleDownToFwdCheck));
        RaycastHit fwdHit;
        bool fwdIntersect = Physics.Raycast(transform.position, fwdCheckRay, out fwdHit,
            fwdCheckRay.magnitude, terrainLayer);

        // GROUND CHECK RAY -- EXTREME DOWN ANGLE
        Vector3 groundCheckRay = myRb.velocity * lowCheckTimeToCrash;
        groundCheckRay = pitchOffset(groundCheckRay, Mathf.Max(groundCheckRayPitchOffset, -angleDownToFwdCheck));
        RaycastHit lowHit;
        bool groundIntersect = Physics.Raycast(transform.position, groundCheckRay, out lowHit,
            groundCheckRay.magnitude,  terrainLayer);

        Debug.DrawRay(transform.position, myRb.velocity, Color.yellow);
        Debug.DrawRay(transform.position, transform.forward, Color.white);
        Debug.DrawRay(transform.position, fwdCheckRay, Color.blue);
        Debug.DrawRay(transform.position, groundCheckRay, Color.green);

        //Debug.DrawRay(transform.position, )

        // ground avoid
        if (groundIntersect)
        {

            float fwdCheckMod = 0.0f;
            
            if (fwdIntersect)
            {
                fwdCheckMod = fwdCheckSensitivity;
                
            }

            // prioritize the low raycast
            Vector3 intersectPos;
            RaycastHit hit;

            intersectPos = lowHit.point;
            hit = lowHit;

            
            


            float estimatedCrashTime = Vector3.Distance(transform.position, intersectPos) / myRb.velocity.magnitude;
            float overrideMod = Mathf.Clamp(
                ((lowCheckTimeToCrash - estimatedCrashTime) / lowCheckTimeToCrash) * groundAvoidSensitivity + fwdCheckMod
                ,0.0f,  1.0f);




            Vector3 overrideDir = dir;

            
            // this whole pitch block is super ugly
            float currentDirPitch = getPitch(dir);


            float pitchCorrectionRaw = overrideMod * maxCorrectionAngle;

            float newPitch = currentDirPitch + pitchCorrectionRaw;

            float highPitch = calculateMaxPitch();
            float hardMaxPitch = highPitch + groundAvoidHardMaxPitchOffset;

            float pitchOvershootMod = calculatePitchOvershootMod(newPitch, highPitch, hardMaxPitch, dir);

            newPitch = Mathf.Min(newPitch, hardMaxPitch);

            float maxPitchOffset = hardMaxPitch - currentDirPitch;
            float pitchCorrection = Mathf.Min(pitchCorrectionRaw, maxPitchOffset);

            overrideDir = rotateDirFromTo(overrideDir, Vector3.up, pitchCorrection);


            

            //Debug.Log("CurrentDirPitch: " + currentDirPitch + ", newPitch: " +)

            float horizDirection = wallAvoidDirection(hit.normal);

            bool canWallAvoid = transform.position.y > minWallAvoidAlt && (wallAvoidIntersect(fwdCheckRay) || fwdIntersect);

            if (canWallAvoid)
            {
                overrideDir = yawOffset(overrideDir, horizDirection * pitchOvershootMod * maxCorrectionAngle);
            }

            Debug.Log("currentDirPitch: " + currentDirPitch + ", newPitch: " + newPitch + ", highPitch: " + highPitch +
                ", hardMaxPitch: " + hardMaxPitch + ", pitchOvershootMod: " + pitchOvershootMod + ", fwdIntersect: " + fwdIntersect +
                ", canWallAvoid: " + canWallAvoid);
            //Debug.DrawRay(transform.position, hit.normal, Color.red);

            //overrideDir = pitchOffset(dir, overrideMod * maxCorrectionAngle);

            dir = overrideDir;

            //if (horizDirection > 0.0f)
            //{
            //    Debug.Log("========= TURNING RIGHT, " + pitchOvershootMod + " horizontal ");
            //}
            //else
            //{
            //    Debug.Log("========= TURNING LEFT, " + pitchOvershootMod + " horizontal");
            //}

            //overrideDir = wallAvoid(groundCheckRay, overrideDir);



            //Debug.Log("========= GROUND INTERSECTED , estimatedCrashTime: " + estimatedCrashTime + ", overrideMod: " + overrideMod);
            //Debug.DrawRay(transform.position, groundCheckRay, Color.blue, 5);
            //Debug.DrawLine(transform.position, lowHit.point, Color.red, 5);
            //Debug.DrawRay(transform.position, overrideDir, Color.yellow, 5);

            //dir = lerpRotateVect(dir.normalized, overrideDir, overrideMod);
        }

        Debug.DrawRay(transform.position, dir * 10f, Color.cyan);

        return dir;
    }

    //private float calculateGroundAvoidPitch(float timeToCrash)
    //{

    //}

    private float wallAvoidDirection(Vector3 hitNormal)
    {
        Vector3 myRight = Vector3.Cross(Vector3.up, transform.forward).normalized;
        Vector3 turnDir = Vector3.Project(hitNormal, myRight).normalized;

        // 10 degrees arbitrarily chosen
        //  If both vectors pointing in the same direction, angle will be small
        //  both in same direction means turn right (positive)
        //  opposite directions means turn left (negative)
        if(Vector3.Angle(myRight, turnDir) < 10f)
        {
            // small angle, vectors pointing same direction, turn right, positive
            return 1f;
        }
        else
        {
            return -1f;
        }
    }
    
    bool wallAvoidIntersect(Vector3 fwdAxis)
    {
        
        Vector3 fwd = fwdAxis.normalized * myRb.velocity.magnitude * sideCheckTimeToCrash;

        Vector3 leftCheckRay = yawOffset(fwd, -sideCheckRayYawOffset);
        Vector3 rightCheckRay = yawOffset(fwd, sideCheckRayYawOffset);

        Debug.DrawRay(transform.position, leftCheckRay, Color.red, 5);
        Debug.DrawRay(transform.position, rightCheckRay, Color.yellow, 5);

        float leftHitDist = getRaycheckDist(leftCheckRay);
        float rightHitDist = getRaycheckDist(rightCheckRay);

        // only activate avoidance if one of the rays hit
        return leftHitDist > 0f || rightHitDist > 0f;

        
    }

    float getRaycheckDist(Vector3 ray)
    {
        int terrainLayer = 1 << 10; // line only collides with terrain layer
        float dist = -1f;

        RaycastHit hit;
        bool intersect = Physics.Raycast(transform.position, ray, out hit,
            ray.magnitude, terrainLayer);

        if (intersect)
        {
            dist = Vector3.Distance(transform.position, hit.point);
        }

        return dist;
    }

    public Vector3 yawOffset(Vector3 dir, float yawBy)
    {
        Quaternion rotBy = Quaternion.AngleAxis(yawBy, Vector3.up);
        return rotBy * dir;
    }

    public Vector3 pitchOffset(Vector3 dir, float pitchBy)
    {
        if (!pitchBy.Equals(0.0f))
        {
            Vector3 offsetAxis = Vector3.Cross(dir, Vector3.up);
            Quaternion rotBy = Quaternion.AngleAxis(pitchBy, offsetAxis);
            return rotBy * dir;
        }
        else
        {
            return dir;
        }
    }

    public Vector3 offsetForAoA(Vector3 targetDir)
    {
        float myPitch = getPitch(transform.rotation);
        float velPitch = getPitch(Quaternion.LookRotation(myRb.velocity));
        float vertAoA = myPitch - velPitch;

        ////Debug.Log("MyPitch: " + myPitch + ", velPitch: " + velPitch + ", vertAoA: " + vertAoA);

        //Vector3 offsetAxis = -Vector3.Cross(targetDir, Vector3.up);
        //Quaternion rotBy = Quaternion.AngleAxis(vertAoA, offsetAxis);


        return pitchOffset(targetDir, vertAoA);
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

    public float getPitch(Vector3 dir)
    {
        return getPitch(Quaternion.LookRotation(dir, Vector3.up));
    }

    public float getPitch(Quaternion rot)
    {
        return -unEulerize(Quaternion.ToEulerAngles(rot).x * Mathf.Rad2Deg);
    }

    public float unEulerize(float angle)
    {
        if(angle > 180)
        {
            angle -= 360f;
        }

        return angle;
    }

    public Vector3 dir2Euler(Vector3 dir)
    {
        Quaternion localOldDirRotation = Quaternion.LookRotation(dir);
        return Quaternion.ToEulerAngles(localOldDirRotation) * Mathf.Rad2Deg;
    }


    public Vector3 rotateDirFromTo(Vector3 from, Vector3 to, float angle)
    {
        // don't overshoot
        float angleBetween = Vector3.Angle(from, to);
        angle = Mathf.Clamp(angle, -angleBetween, angleBetween);


        Vector3 axis = Vector3.Cross(from, to);
        Quaternion rot = Quaternion.AngleAxis(angle, axis);
        return rot * from;
    }

}
