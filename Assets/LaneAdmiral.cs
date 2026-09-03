using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneAdmiral : MonoBehaviour
{
    public List<ShipNavigation> laneFleet;


    public List<Transform> wpts;


    public GameObject carrierPrefab;
    public GameObject cruiserPrefab;

    private void Awake()
    {
        generateWaypointsFromChildren();
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

    // Start is called before the first frame update
    void Start()
    {

        reassessFormation();
    }

    public void reassessFormation()
    {
        cleanShipList();
        linkAllShips();
    }

    void linkAllShips()
    {
        for (int i = 0; i < laneFleet.Count; i++)
        {
            linkShip(laneFleet[i]);
        }
    }

    public void linkShip(ShipNavigation ship)
    {
        if (!laneFleet.Contains(ship))
        {
            laneFleet.Add(ship);
        }

        ship.linktoAdmiral(this);
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
        int nextIndex = -1;

        for(int i = 0; i < wpts.Count && nextIndex == -1; i++)
        {
            float shipDistFromBase = transform.InverseTransformPoint(ship.transform.position).z;
            float wptDistFromBase = transform.InverseTransformPoint(getWpt(i)).z;

            // assign next index once wpt farther from base
            // OR we have reached final index
            if(wptDistFromBase > shipDistFromBase || i == wpts.Count - 1)
            {
                // exit loop
                nextIndex = i;
            }

        }


        return nextIndex;
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
