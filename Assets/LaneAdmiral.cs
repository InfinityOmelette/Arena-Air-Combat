using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneAdmiral : MonoBehaviour
{
    public List<ShipNavigation> laneFleet;

    public CarrierNavigation laneCarrier;


    public List<Transform> wpts;


    //public GameObject carrierPrefab;
    //public GameObject cruiserPrefab;

    // Only push fleet past wp1 once fleet size is appropriate
    //public bool fleetReady = false;

    public int fleetReadySize = 3;

    //private bool wasReady = false;

    public ShipNavigation.NavMode fleetNavOrder;

    // fleet considered "deployed" if leader x meters past first wpt
    public float isDeployedThreshold = 500f;


    public float fleetTickDelay = 3f;
    public float fleetTickTimer;

    public bool invertFormationOffset = false;

    private float formationInversionCoeff = 1.0f;

    private void Awake()
    {
        generateWaypointsFromChildren();
        if (invertFormationOffset)
        {
            formationInversionCoeff = -1.0f;
        }
    }

    public float getFormationInversion()
    {
        return formationInversionCoeff;
    }

    public bool isFleetReady()
    {
        return laneFleet.Count >= fleetReadySize;
    }

    private void generateWaypointsFromChildren()
    {
        int childCount = transform.childCount;
        wpts = new List<Transform>(childCount);
        for(int i = 0; i < childCount; i++)
        {
            wpts.Add(transform.GetChild(i));
        }
    }

    public ShipNavigation.NavMode getFleetNavOrder()
    {
        return fleetNavOrder;
    }

    public void setLeaderNavMode(ShipNavigation.NavMode navMode)
    {
        if(getLeader() != null)
        {
            getLeader().changeNavmode(navMode);
        }
    }

    public void setFleetNavMode(ShipNavigation.NavMode navMode)
    {
        for(int i = 0; i < laneFleet.Count; i++)
        {
            laneFleet[i].changeNavmode(navMode);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        reassessFormation();
    }

    private void FixedUpdate()
    {

        if(fleetTickTimer < 0f)
        {
            fleetTick();
            fleetTickTimer = fleetTickDelay;
        }
        else
        {
            fleetTickTimer -= Time.fixedDeltaTime;
        }

    } 

    // put any other intermittent fleet decisionmaking processing here
    private void fleetTick()
    {
        assessFleetNavOrder();
    }

    private void assessFleetNavOrder()
    {
        if (isFleetReady() || isFleetDeployed())
        {
            fleetNavOrder = ShipNavigation.NavMode.ADVANCE;
        }
        else
        {
            fleetNavOrder = ShipNavigation.NavMode.RETREAT;
        }
    }

    public bool isFleetDeployed()
    {
        ShipNavigation leader = getLeader();

        bool deployed = false;

        if(leader != null)
        {
            float leaderPos = laneAxisPos(leader);
            float wpt1Pos = laneAxisPos(wpts[0].position);

            deployed = leaderPos > wpt1Pos + isDeployedThreshold;
        }


        return deployed;
    }

    public void reassessFormation()
    {
        if(laneFleet.Count > 0)
        {
            cleanShipList();
            linkAllShips();
        }
        
    }

    void linkAllShips()
    {
        for (int i = 0; i < laneFleet.Count; i++)
        {
            linkShip(laneFleet[i]);
        }

        if(laneCarrier != null)
        {
            laneCarrier.carrierLinktoAdmiral(this);
        }
    }

    //public void 

    public void linkShip(ShipNavigation ship)
    {
        
        // hhhhh this is crusty, i really should just use an inherited method lol
        if(ship is CarrierNavigation)
        {
            ((CarrierNavigation)ship).carrierLinktoAdmiral(this);
        }
        else
        {
            if (!laneFleet.Contains(ship))
            {
                laneFleet.Add(ship);
            }
            ship.linktoAdmiral(this);
        }
        
    }

    void cleanShipList()
    {
        for(int i = 0; i < laneFleet.Count; i++)
        {
            if(laneFleet[i] == null)
            {
                laneFleet.RemoveAt(i);
                i--;
            }
        }
    }

    public ShipNavigation getShip(int index)
    {
        return laneFleet[index];
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 getWpt(int index)
    {
        index = clampWtpIndex(index);

        return wpts[index].position;
    }

    public ShipNavigation getLeader()
    {
        if(laneFleet[0] == null)
        {
            reassessFormation();
        }

        return laneFleet[0];
    }

    private int clampWtpIndex(int index)
    {
        

        if (index > wpts.Count - 1)
        {
            index = wpts.Count - 1;
        }
        else if (index < 0)
        {
            index = 0;
        }

        return index;
    }

    public int getFormationIndex(ShipNavigation ship)
    {
        return laneFleet.IndexOf(ship);
    }

    // Z axis of admiral object points towards enemy base
    //  this axis is used to determine progress
    public int closestForwardWaypointIndex(ShipNavigation ship)
    {
        int nextIndex = 0;
        bool wptFound = false;

        for(int i = 0; i < wpts.Count && !wptFound; i++)
        {
            float shipDistFromBase = laneAxisPos(ship);
            float wptDistFromBase = laneAxisPos(getWpt(i));

            // assign next index once wpt farther from base
            // OR we have reached final index
            if(wptDistFromBase > shipDistFromBase || i == wpts.Count - 1)
            {
                // exit loop
                nextIndex = i;
                wptFound = true;
            }

        }


        return nextIndex;
    }

    public float laneAxisPos(ShipNavigation ship)
    {
        if(ship == null)
        {
            return laneAxisPos(transform.position);
        }
        return laneAxisPos(ship.transform.position);
    }

    public float laneAxisPos(Vector3 pos)
    {
        return transform.InverseTransformPoint(pos).z;
    }

    public int closestRetreatWaypointIndex(ShipNavigation ship)
    {
        int backIndex = closestForwardWaypointIndex(ship) - 1;

        return clampWtpIndex(backIndex);
    }

    // Ensure all ships formed with fleet share same waypoint orientation
    public void propagateWptIndex(int index)
    {
        for(int i = 0; i < laneFleet.Count; i++)
        {
            if (laneFleet[i].withinLeaderRadius())
            {
                laneFleet[i].currentWptIndex = index;
            }
            
        }
    }
}
