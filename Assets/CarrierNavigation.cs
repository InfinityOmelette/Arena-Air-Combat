using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierNavigation : ShipNavigation
{

    public float farAheadThresh = 800f; // any further ahead than this from desire, we halt
    public float slightAheadThresh = 500f; // ahead this till far, we slow
    public float desiredStandoff = 3500f; // between far and slight, we cruise
    public float slightBehindThresh = -500f; // any further behind than this, we flank ahead

    public float independentNavThreshold = 6000f; // farther than this either direction, independent

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
            
            if(admiral.getLeader() == null)
            {
                changeNavmode(NavMode.RETREAT);
                driveToWaypoint(ShipPhysics.Speed.CRUISE);
            }
            else
            {
                carrierFollowLeader();
            }
            

        }
    }

    public void carrierLinktoAdmiral(LaneAdmiral admiral)
    {
        this.admiral = admiral;
        admiral.laneCarrier = this;
        setWptIndexByPos();

    }

    private void carrierFollowLeader()
    {
        ShipNavigation leader = admiral.getLeader();
        float leaderAxisPos = admiral.laneAxisPos(leader);
        float myAxisPos = admiral.laneAxisPos(this);
        //float axisStandoffToLeader = leaderAxisPos - myAxisPos;

        float desireAxisPos = leaderAxisPos - desiredStandoff;
        float myDeltaToStandoff = desireAxisPos - myAxisPos;

        //if(admiral.getFleetNavOrder() == NavMode.RETREAT)
        //{
        //    //axisStandoffToLeader *= -1;
        //    myDeltaToStandoff *= -1;

        //}

        //debugStandoffRead = axisStandoffToLeader;
        //NavMode navSelect = NavMode.ADVANCE;

        ShipPhysics.Speed speedSet;

        // close enough to leader to follow
        if (Mathf.Abs(myDeltaToStandoff) < independentNavThreshold) 
        {
            receiveFleetNavOrder();

            if(admiral.getFleetNavOrder() == NavMode.RETREAT)
            {
                myDeltaToStandoff *= -1;
            }

            speedSet = followSpeed(myDeltaToStandoff);
            

        }
        else // far away from leader, independently drive towards leader
        {
            speedSet = ShipPhysics.Speed.FLANK;
            if(myDeltaToStandoff > 0)
            {
                changeNavmode(NavMode.ADVANCE);
            }
            else
            {
                changeNavmode(NavMode.RETREAT);
            }

        }



        //if (myDeltaToStandoff > farBehindStandoff)
        //{
        //    // flank
        //    receiveFleetNavOrder();
        //    speedSet = ShipPhysics.Speed.FLANK;
            
        //}
        //else if(myDeltaToStandoff > slightAheadStandoff)
        //{
        //    // cruise
        //    receiveFleetNavOrder();
        //    speedSet = ShipPhysics.Speed.CRUISE;

        //}
        //else if(myDeltaToStandoff > farAheadStandoff)
        //{
        //    // halt
        //    changeNavmode(NavMode.STOP);
        //    speedSet = ShipPhysics.Speed.HALT;
        //}
        //else // if very very far ahead of leader, go in opposite direction to meet them
        //{
        //    // flank

        //    if(admiral.getFleetNavOrder() == NavMode.ADVANCE)
        //    {
        //        changeNavmode(NavMode.RETREAT);
        //    }
        //    else
        //    {
        //        changeNavmode(NavMode.ADVANCE);
        //    }
            


        //    speedSet = ShipPhysics.Speed.FLANK;
        //}

        driveToWaypoint(speedSet);



    }


    // hhhh i really don't like this but eh
    private ShipPhysics.Speed followSpeed(float deltaFromDesirePos)
    {
        ShipPhysics.Speed speedSet;

        if (deltaFromDesirePos < slightBehindThresh)
        {
            speedSet = ShipPhysics.Speed.FLANK;
        }
        else if (deltaFromDesirePos < slightAheadThresh)
        {
            speedSet = ShipPhysics.Speed.CRUISE;
        }
        else if (deltaFromDesirePos < farAheadThresh)
        {
            speedSet = ShipPhysics.Speed.SLOW;
        }
        else
        {
            speedSet = ShipPhysics.Speed.HALT;
        }

        return speedSet;
    }

    private void OnDestroy()
    {
        
    }
}
