using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneAdmiral : MonoBehaviour
{
    public List<ShipNavigation> laneFleet;


    public List<Transform> wpts;

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
        for(int i = 0; i < laneFleet.Count; i++)
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 getWpt(int index)
    {
        return wpts[index].position;
    }


}
