using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Aircraft : MonoBehaviour
{
    public CombatFlow myFlow;
    public HardpointController hardpoints;
    public CannonControl cannon;

    public DirectionAI dirAI;
    public EngineControl engine;

    public Vector3 currWpt;

    public List<Vector3> waypoints;
    private int waypointIndex;

    public float waypointRadius;

    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        dirAI = GetComponent<DirectionAI>();
        engine = GetComponent<EngineControl>();
    }


    // Start is called before the first frame update
    void Start()
    {
        dirAI.isApplied = true;
        engine.currentThrottlePercent = 100f;

        GameObject wptContainer = GameObject.Find("JeffWaypoints");

        fillWaypointList(wptContainer);

        currWpt = waypoints[waypointIndex];

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
    }


    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, currWpt) < waypointRadius)
        {
            nextWaypoint();
        }
        else
        {
            dirAI.inputDir = currWpt - transform.position;
        }
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

            currWpt = waypoints[waypointIndex];
        }
        else
        {
            Debug.Log("Unable to find waypoints");
        }
    }


}
