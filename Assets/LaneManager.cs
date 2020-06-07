using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LaneManager : MonoBehaviourPunCallbacks
{

    public CombatFlow.Team team;

    public LaneManager opponentLM;

    public GameObject tankPrefab;
    public GameObject rocketPrefab;
    public GameObject artilleryPrefab;
    public GameObject AAAPrefab;
    public GameObject SAMPrefab;

    public List<Transform> waypoints;

    public List<CombatFlow> myLaneUnits;
    public List<CombatFlow> myLaneSAMs;

    public float leaderUpdateDelay;
    private float leaderUpdateTimer;

    public int squadSizeMax;
    public int squadSizeMin;

    public float SAMSpacing;

    public int artilleryPerWave;
    public int rocketPerWave;
    public int tankPerWave;
    public int AAAPerWave; // grouped into squads
    public int SAMPerWave; // placed randomly


    

    private bool doSpawn = false;

    // only counts the current wave spawning duration
    public int artilleryCount;
    public int rocketCount;
    public int tankCount;
    public int AAACount;
    public int SAMCount;

    public float laneWidth;

    public float SAMDeployDelay;
    public float rapidDeployDelay;
    public float squadDeployDelay;
    public float waveDeployDelay;


    public float rapidTimer;
    public float squadTimer;
    public float waveTimer;

    private CombatFlow myLeader;


    private Vector3 spawnAxisDir;


    private Vector3 currentSpawnPoint;
    private float currentSpawnRange;
    public int squadRemaining;

    private bool isHostInstance = false;


    void Awake()
    {

        isHostInstance = PhotonNetwork.PlayerList.Length == 1;


        waveTimer = waveDeployDelay;
        squadTimer = squadDeployDelay;
        
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

    private bool waveComplete()
    {
        return AAACount == AAAPerWave && SAMCount == SAMPerWave && 
            tankCount == tankPerWave && artilleryCount == artilleryPerWave && 
            rocketCount == rocketPerWave;
    }

    private void resetDeployCounts()
    {
        AAACount = 0;
        SAMCount = 0;
        tankCount = 0;
        artilleryCount = 0;
        rocketCount = 0;
    }

    void FixedUpdate()
    {
        leaderUpdateCountdown();

        if (isHostInstance)
        {

            if (squadRemaining == 0)
            {
                if (!waveComplete())
                {
                    countDownSquad();
                }
                else
                {
                    // count down wave timer
                    countDownWave();
                }
            }
            else
            {

                countdownRapidDeploy();
            }
        }
    }

    private void countDownWave()
    {
        if (waveTimer < 0)
        {
            // action
            resetDeployCounts();
            waveTimer = waveDeployDelay;
        }
        else
        {
            waveTimer -= Time.fixedDeltaTime;
        }

    }

    private void countDownSquad()
    {
        if (squadTimer < 0)
        {
            // action
            initiateSquadSpawn();
            squadTimer = squadDeployDelay;
        }
        else
        {
            squadTimer -= Time.fixedDeltaTime;
        }
    }

    private void countdownRapidDeploy()
    {
        if(rapidTimer < 0)
        {
            deployUnit();
            rapidTimer = rapidDeployDelay;
        }
        else
        {
            rapidTimer -= Time.fixedDeltaTime;
        }
    }

    // inefficient as fuck, but it isn't called very often, so I'll leave it
    //  (would be better to keep a running tally, instead of summing every time)
    private int unitsRemain()
    {
        int total = tankPerWave - tankCount;
        total += AAAPerWave - AAACount;
        
        total += artilleryPerWave - artilleryCount;
        total += rocketPerWave - rocketCount;

        //total += SAMPerWave - SAMCount; -- SAM not included, because it spawns on its own independent cycle
        //  sams are NOT "grouped" into squad spawns

        return total;
    }

    private void initiateSquadSpawn()
    {
        // set squad size
        squadRemaining = Mathf.Min( Random.Range(squadSizeMin, squadSizeMax), unitsRemain());

        currentSpawnPoint = randomSpawnPoint();
    }

    private void deployUnit()
    {
        tankCount++;
        squadRemaining--;

        CreepControl newCreep = PhotonNetwork.Instantiate(tankPrefab.name, currentSpawnPoint,
            Quaternion.LookRotation(waypoints[1].position - transform.position, Vector3.up)).GetComponent<CreepControl>();

        float range = newCreep.effectiveRange;
        Vector3 offset = currentSpawnPoint - transform.position;

        int teamNum = CombatFlow.convertTeamToNum(team);

        newCreep.photonView.RPC("rpcInit", RpcTarget.All, photonView.ViewID, offset, range, teamNum);


        // instantiate
        // place at offset
        // fill waypoints
        // set offset
        
        // RANGE:
        //  if creep, use its own range
        //  if AAA
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

        if(dyingFlow.type == CombatFlow.Type.SAM)
        {
            myLaneSAMs.Remove(dyingFlow);
        }

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
