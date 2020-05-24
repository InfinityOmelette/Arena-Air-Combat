using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DummyFlight : MonoBehaviourPunCallbacks
{

    // move towards waypoint
    // OR
    // turn in circle

    public enum BehaviorType
    {
        WAYPOINT_MISSION,
        TURN
    }

    public BehaviorType behaviorType;

    public float waypointHitRange;

    public float forwardSpeed;

    public float centripetalVelocity;

    public bool repeatWaypoints;

    bool waypointMissionFinished = false;

    public GameObject waypointsContainer;

    private GameObject[] waypoints;

    public short activeWaypointIndex = 0;


    public GameObject activeWaypointObj;

    private Rigidbody rbRef;

    CombatFlow myFlow;

    private void Awake()
    {
        rbRef = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
        
    }

    private void checkIfLocalOwns()
    {
        if (!myFlow.localOwned && PhotonNetwork.PlayerList.Length == 1)
        {
            myFlow.localOwned = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (waypointsContainer != null)
        {

            fillWaypoints();

            if (waypoints != null)
                activeWaypointObj = waypoints[0];
        }
    }


    void fillWaypoints()
    {
        waypoints = new GameObject[waypointsContainer.transform.childCount];
        for(int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointsContainer.transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        checkIfLocalOwns();

        if (myFlow.localOwned)
        {
            rbRef.velocity = transform.forward * forwardSpeed + transform.up * centripetalVelocity;

            if (behaviorType == BehaviorType.WAYPOINT_MISSION)
            {

                float distance = Vector3.Distance(activeWaypointObj.transform.position, transform.position);
                //Debug.Log("currently flying to: " + activeWaypointObj.name);

                if (!waypointMissionFinished)
                {

                    if (distance < waypointHitRange)
                    {
                        nextWaypoint();
                    }

                    // transform points towards activeWaypoint
                    transform.rotation = Quaternion.LookRotation(activeWaypointObj.transform.position - transform.position, transform.up);
                }

            }
            else if (behaviorType == BehaviorType.TURN)
            {
                // rbRef.AddForce(transform.up * centripetalVelocity);
                transform.rotation = Quaternion.LookRotation(rbRef.velocity, transform.up);
            }


        }

    }


    // called when waypoint is hit
    //  - set new waypoint index
    //  - set reference to active waypoint obj
    //  - determine if waypoint mission is finished
    void nextWaypoint()
    {
        activeWaypointIndex++;

        // if at end of waypoint array
        if(activeWaypointIndex > waypoints.Length - 1)
        {
            // determine if object will continue or reset to first waypoint

            if (!repeatWaypoints)
            {
                waypointMissionFinished = true;
            }

            activeWaypointIndex = 0; // reset this either way to keep code below from throwing exception

        }


        activeWaypointObj = waypoints[activeWaypointIndex];

    }


    
}
