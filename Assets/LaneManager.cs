using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{

    LaneManager opponentLM;

    

    public List<Transform> waypoints;

    public List<CombatFlow> myLaneUnits;
    public List<CombatFlow> myLaneSAMs;

    public float leaderUpdateDelay;
    private float leaderUpdateTimer;

    public int squadSizeMax;

    public float SAMSpacing;

    public int artilleryPerWave;
    public int rocketPerWave;
    public int tankPerWave;
    public int AAAPerWave; // grouped into squads
    public int SAMPerWave; // placed randomly


    

    private bool doSpawn = false;

    // only counts the current wave spawning duration
    private int artilleryCount;
    private int rocketCount;
    private int tankCount;
    private int AAACount;
    private int SAMCount;

    public float laneWidth;

    public float SAMDeployDelay;
    public float rapidDeployDelay;
    public float squadDeployDelay;
    public float waveDeployDelay;


    private float rapidTimer;
    private float squadTimer;
    private float waveTimer;

    private CombatFlow myLeader;


    private Vector3 spawnAxisDir;



    void Awake()
    {
        
        initLists();
        fillWaypointList();
        initSpawnAxis();
    }

    private void initLists()
    {
        waypoints = new List<Transform>();
        myLaneUnits = new List<CombatFlow>();
        myLaneSAMs = new List<CombatFlow>();
    }

    private void initSpawnAxis()
    {
        spawnAxisDir = Vector3.Cross(Vector3.up, waypoints[0].position - transform.position).normalized;
    }

    private void fillWaypointList()
    {
        waypoints = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints.Add(transform.GetChild(i));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    void FixedUpdate()
    {
        leaderUpdateCountdown();


    }
    
    private void leaderUpdateCountdown()
    {
        if (leaderUpdateTimer < 0)
        {
            assignNewLeader();
            leaderUpdateTimer = leaderUpdateDelay;
        }
        else
        {
            leaderUpdateTimer -= Time.fixedDeltaTime;
        }
    }

    public void unitDeath(CombatFlow dyingFlow)
    {
        myLaneUnits.Remove(dyingFlow);

        if (dyingFlow == myLeader)
        {
            myLeader = assignNewLeader();
        }
        
        
    }
    
    public CombatFlow assignNewLeader()
    {
        CombatFlow leader = null;
        float farthestDist = 0;

        for(int i = 0; i < myLaneUnits.Count; i++)
        {
            CombatFlow unit = myLaneUnits[i];

            if (unit != null)
            {
                float dist = Vector3.Distance(unit.transform.position, transform.position);
                if (dist > farthestDist)
                {
                    farthestDist = dist;
                    leader = unit;
                }
            }
        }

        myLeader = leader;
        return leader;
    }

    public CombatFlow getLeader()
    {
        if(myLeader != null)
        {
            return myLeader;
        }
        else
        {
            return assignNewLeader();
        }
        
    }

    private Vector3 randomSpawnPoint()
    {
        return transform.position + spawnAxisDir * Random.Range(-laneWidth, laneWidth);
    }
}
