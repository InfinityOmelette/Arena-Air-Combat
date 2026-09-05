using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipNavigation : MonoBehaviour
{
    public enum NavMode
    {
        ADVANCE, // go to next waypoint
        HALT,       
        RETREAT, // go to previous waypoint
        FOLLOW,   // follow lane naval leader
        DEBUG
    }

    public static float LEADER_RADIUS = 3500f;

    public static float DRIVE_POINT_RADIUS = 100f;

    public NavMode navMode;

   // public List<Transform> waypoints;

    public int currentWptIndex;

    protected ShipPhysics shipPhysics;

    public LaneAdmiral admiral;


    public float maxHeadingErrorDegrees;

    [Tooltip ("Degrees")]
    public float maxFollowAngleCorrection;

    public float maxFollowLateralError;
    public float maxFollowLongitudinalError;

    public float wptRadius = 300f;

    public int formationIndex = 0;

    public Transform followerOffset;

    public bool isCarrier = false;

    private void Awake()
    {
        shipPhysics = GetComponent<ShipPhysics>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void linktoAdmiral(LaneAdmiral admiral)
    {
        this.admiral = admiral;
        //waypoints = admiral.wpts;
        formationIndex = admiral.getFormationIndex(this);
        setWptIndexByPos();

        if(formationIndex == 0)
        {
            changeNavmode(NavMode.ADVANCE);
        }
        else
        {
            changeNavmode(NavMode.FOLLOW);
        }
    }

    public void changeNavmode(NavMode navMode, ShipPhysics.Speed speed = ShipPhysics.Speed.CRUISE)
    {
        if(navMode != this.navMode)
        {
            

            switch (navMode)
            {
                case NavMode.ADVANCE:
                    currentWptIndex = admiral.closestForwardWaypointIndex(this);
                    shipPhysics.setSpeed(speed);
                    break;
                case NavMode.RETREAT:
                    currentWptIndex = admiral.closestRetreatWaypointIndex(this);
                    shipPhysics.setSpeed(speed);
                    break;
                case NavMode.HALT:
                    shipPhysics.setSpeed(ShipPhysics.Speed.HALT);
                    break;
            }

            this.navMode = navMode;
        }

        
    }

    protected void setWptIndexByPos()
    {
        currentWptIndex = admiral.closestForwardWaypointIndex(this);
    }

    private void FixedUpdate()
    {
        if(admiral != null)
        {
            checkLeader();
            checkWaypoint();

            // waypoint steer process
            switch (navMode)
            {
                case NavMode.FOLLOW:
                    followLeader();
                    break;
                default:
                    driveToWaypoint(ShipPhysics.Speed.CRUISE);
                    break;

            }
            
        }

    }



    protected void checkLeader()
    {
        if(admiral.getLeader() == null)
        {
            admiral.reassessFormation();
        }

        // check if we are close enough to leader to follow? otherwise, just follow wpts independently?

    }

    // HOW TO HANDLE LEADER STOPPED CASE?
    //   -> if case 
    // HOW TO HANDLE ROUNDING CORNER (ex: new ship at base, rest of fleet near enemy base)
    //   -> only follow leader if within follow radius, otherwise follow waypoints at flank speed?
    protected void followLeader()
    {
        ShipNavigation leader = admiral.getLeader();

        if(withinLeaderRadius())
        {
            if (leader.shipPhysics.speedSet == ShipPhysics.Speed.HALT)
            {
                // just drive directly to form position as if waypoint, stop there
                Vector3 formPos = leader.offsetPos(formationIndex);
                driveToPoint(formPos, ShipPhysics.Speed.CRUISE);

            }
            else // leader is NOT halted
            {

                // convert position to leader's space
                Transform followTransform = leader.transform;

                Vector3 currentFormPos = followTransform.InverseTransformPoint(transform.position);
                currentFormPos = currentFormPos - leader.followerOffset.localPosition * formationIndex;

                // steer based on lateral error FROM LEADER HEADING LINE
                steerFollow(currentFormPos);

                // speed based on longitudinal error from point FROM LEADER HEADING LINE
                speedFollow(currentFormPos);
            }
        }
        else // drive to waypoints at flank speed to catch up
        {
            driveToWaypoint(ShipPhysics.Speed.FLANK);
        }

        

        

    }


    public Vector3 offsetPos(int index)
    {
        Vector3 oneOffset = followerOffset.position - transform.position;

        return transform.position + oneOffset * index;
    }

    protected void speedFollow(Vector3 formPos)
    {
        float longitudinalError = formPos.z;

        if(longitudinalError > maxFollowLongitudinalError)
        {
            shipPhysics.setSpeed(ShipPhysics.Speed.SLOW);
        }
        else if (longitudinalError < -maxFollowLongitudinalError)
        {
            shipPhysics.setSpeed(ShipPhysics.Speed.FLANK);
        }
        else
        {
            shipPhysics.setSpeed(ShipPhysics.Speed.CRUISE);
        }


    }

    // formpos is our position in leader's local space (w/ formation offset baked in)
    protected void steerFollow(Vector3 formPos)
    {
        // max lateral error --> max angle correction
        float errorCoeff = Mathf.Clamp(formPos.x / maxFollowLateralError, -1f, 1f);
        float angleCorrectionSigned = -errorCoeff * maxFollowAngleCorrection;

        Debug.Log("SteerFollow angleCorrectionSigned: " 
            + angleCorrectionSigned + ", formPos x error: " + formPos.x);

        // angle --> direction
        Vector3 leaderDir = admiral.getLeader().transform.forward;
        Vector3 dir = Quaternion.AngleAxis(angleCorrectionSigned, Vector3.up) * leaderDir;

        // steer to direction
        shipPhysics.setRudder( steerToDir(dir));
    }

    protected void checkWaypoint()
    {
        float distToWpt = Vector3.Distance(transform.position, getCurrentWpt());

        if(distToWpt < wptRadius)
        {
            setNextWaypoint();
        }
    }

    public void setNextWaypoint(NavMode newNavMode)
    {
        navMode = newNavMode;
        setNextWaypoint();
    }

    protected void setNextWaypoint()
    {
        switch (navMode)
        {
            case NavMode.ADVANCE:
                currentWptIndex++;
                break;
            case NavMode.RETREAT:
                currentWptIndex--;
                break;
        }

        clampWptIndex();

        // orient all follower vessels to leader's waypoint
        if (checkIfIAmLeader())
        {
            admiral.propagateWptIndex(currentWptIndex);
        }
        
    }

    public bool withinLeaderRadius()
    {
        return Vector3.Distance(admiral.getLeader().transform.position, transform.position) < LEADER_RADIUS;
    }

    protected bool checkIfIAmLeader()
    {
        return admiral.getLeader() == this;
    }

    protected void clampWptIndex()
    {
        if(currentWptIndex > admiral.wpts.Count - 1)
        {
            currentWptIndex = admiral.wpts.Count - 1;
        }
        else if (currentWptIndex < 0)
        {
            currentWptIndex = 0;
        }
    }

    protected Vector3 getCurrentWpt()
    {
        return admiral.getWpt(currentWptIndex);
    }

    protected void driveToPoint(Vector3 wpt, ShipPhysics.Speed speed)
    {

        Vector3 dirToWpt = wpt - transform.position;

        // angle off nose --> set rudder
        float rudder = steerToDir(dirToWpt);

        shipPhysics.setRudder(rudder);


        if(dirToWpt.magnitude < DRIVE_POINT_RADIUS)
        {
            speed = ShipPhysics.Speed.HALT;
        }


        shipPhysics.setSpeed(speed);
    }

    protected void driveToWaypoint(ShipPhysics.Speed defaultSpeed)
    {
        // current wpt
        if(admiral != null)
        {
            Vector3 wpt = admiral.wpts[currentWptIndex].position;

            driveToPoint(wpt, defaultSpeed);
            
        }

        
    }

    protected float steerToDir(Vector3 dir)
    {

        float signedAngleError = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        float errorScale = Mathf.Clamp(signedAngleError / maxHeadingErrorDegrees, -1f, 1f);

        return errorScale;
    }

    private void OnDestroy()
    {
        if(admiral != null)
        {
            admiral.laneFleet.Remove(this);
            admiral.reassessFormation();
        }
    }

}
