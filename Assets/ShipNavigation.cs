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
    }

    private void FixedUpdate()
    {
        if(navMode == NavMode.DEBUG && admiral != null)
        {
            // Waypoint steer process
            driveToWaypoint(Time.fixedDeltaTime);
        }
        

        // Follow process
    }

    private void driveToWaypoint(float deltaTime)
    {
        // current wpt
        Vector3 wpt = admiral.wpts[currentWptIndex].position;
        Vector3 dirToWpt = wpt - transform.position;

        // angle off nose --> set rudder
        float signedAngleError = Vector3.SignedAngle(transform.forward, dirToWpt, Vector3.up);
        float errorScale = Mathf.Clamp(signedAngleError / maxHeadingErrorDegrees, -1f, 1f);

        shipPhysics.setRudder(errorScale);



        // speed setting always cruise
        shipPhysics.setSpeed(ShipPhysics.Speed.CRUISE);
    }

}
