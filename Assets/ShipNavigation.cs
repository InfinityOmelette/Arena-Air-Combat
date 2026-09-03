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

    public NavMode navMode;

    public List<Transform> waypoints;

    public int currentWptIndex;

    private ShipPhysics shipPhysics;

    public LaneAdmiral admiral;


    public float maxHeadingErrorDegrees;

    [Tooltip ("Degrees")]
    public float maxFollowAngleCorrection;

    public float maxFollowLateralError;
    public float maxFollowLongitudinalError;

    public float wptRadius = 300f;

    public int formationIndex = 0;

    public Transform followerOffset;

    public static float LEADER_RADIUS = 3500f;

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
        waypoints = admiral.wpts;
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

    public void changeNavmode(NavMode navMode)
    {
        if(navMode != this.navMode)
        {
            

            switch (navMode)
            {
                case NavMode.ADVANCE:
                    currentWptIndex = admiral.closestForwardWaypointIndex(this);
                    shipPhysics.setSpeed(ShipPhysics.Speed.CRUISE);
                    break;
                case NavMode.RETREAT:
                    currentWptIndex = admiral.closestRetreatWaypointIndex(this);
                    shipPhysics.setSpeed(ShipPhysics.Speed.CRUISE);
                    break;
            }

            this.navMode = navMode;
        }

        
    }

    private void setWptIndexByPos()
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
                    driveToWaypoint(Time.fixedDeltaTime);
                    break;

            }
            
        }
        
        

        // Follow leader process
    }


    void checkLeader()
    {
        if(admiral.getLeader() == null)
        {
            admiral.reassessFormation();
        }
    }

    // need to slightly rework to dynamically resize error based on INDEX of this ship,
    // rather than directly referencing leader transform
    private void followLeader()
    {
        ShipNavigation leader = admiral.getLeader();

        // convert position to leader's space
        Transform followTransform = leader.transform;


        Vector3 currentFormPos = followTransform.InverseTransformPoint(transform.position);
        currentFormPos = currentFormPos - leader.followerOffset.localPosition * formationIndex;
        
        
        //Debug.Log("Leader follower offset: " + (leader.followerOffset.localPosition * formationIndex));
        
        
        // steer based on lateral error FROM LEADER HEADING LINE
        steerFollow(currentFormPos);

        // speed based on longitudinal error from point FROM LEADER HEADING LINE
        speedFollow(currentFormPos);

    }

    private void speedFollow(Vector3 formPos)
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
    private void steerFollow(Vector3 formPos)
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

    private void checkWaypoint()
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

    private void setNextWaypoint()
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

    private bool checkIfIAmLeader()
    {
        return admiral.getLeader() == this;
    }

    private void clampWptIndex()
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

    private Vector3 getCurrentWpt()
    {
        return admiral.getWpt(currentWptIndex);
    }

    private void driveToWaypoint(float deltaTime)
    {
        // current wpt
        if(admiral != null)
        {
            Vector3 wpt = admiral.wpts[currentWptIndex].position;
            Vector3 dirToWpt = wpt - transform.position;

            // angle off nose --> set rudder
            float rudder = steerToDir(dirToWpt);

            shipPhysics.setRudder(rudder);

            // speed setting always cruise
            shipPhysics.setSpeed(ShipPhysics.Speed.CRUISE);
        }

        
    }

    private float steerToDir(Vector3 dir)
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
