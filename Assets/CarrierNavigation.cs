using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierNavigation : ShipNavigation
{

    public float farAheadStandoff = 0f; // any further ahead than this, we flank retreat
    public float slightAheadStandoff = 2700f; // ahead this till far, we halt
    //public float desiredStandoff = 3500f; // between far and slight, we cruise
    public float farBehindStandoff = 3700f; // any further behind than this, we flank ahead



    public float debugStandoffRead;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(admiral != null)
        {
            checkLeader();
            checkWaypoint();

            carrierNavModeAndSpeed();

        }
    }

    public void carrierLinktoAdmiral(LaneAdmiral admiral)
    {
        this.admiral = admiral;
        setWptIndexByPos();
    }

    private void carrierNavModeAndSpeed()
    {
        ShipNavigation leader = admiral.getLeader();
        float leaderAxisPos = admiral.laneAxisPos(leader);
        float myAxisPos = admiral.laneAxisPos(this);
        float axisStandoffToLeader = leaderAxisPos - myAxisPos;

        debugStandoffRead = axisStandoffToLeader;
        //NavMode navSelect = NavMode.ADVANCE;

        ShipPhysics.Speed speedSet;

        if (axisStandoffToLeader > farBehindStandoff)
        {
            // flank ahead
            changeNavmode(NavMode.ADVANCE);
            speedSet = ShipPhysics.Speed.FLANK;
            
        }
        else if(axisStandoffToLeader > slightAheadStandoff)
        {
            // cruise ahead
            changeNavmode(NavMode.ADVANCE);
            speedSet = ShipPhysics.Speed.CRUISE;

        }
        else if(axisStandoffToLeader > farAheadStandoff)
        {
            // halt
            changeNavmode(NavMode.HALT);
            speedSet = ShipPhysics.Speed.HALT;
        }
        else
        {
            // flank retreat
            changeNavmode(NavMode.RETREAT);
            speedSet = ShipPhysics.Speed.FLANK;
        }

        driveToWaypoint(speedSet);


        // FAR BEHIND --> advance flank
        // In position --> advance cruise
        // Slightly ahead position --> halt
        // far ahead position --> retreat flank


    }
}
