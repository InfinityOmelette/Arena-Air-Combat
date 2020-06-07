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
    public List<CombatFlow> frontWave;
    public List<CombatFlow> myLaneUnits;
    //public List<CombatFlow> myLaneSAMs;

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

    public float leaderRadius;
    

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


    public float samTimer;
    public float rapidTimer;
    public float squadTimer;
    public float waveTimer;

    private CombatFlow myLeader;


    private Vector3 spawnAxisDir;


    private Vector3 currentSpawnPoint;
    private float currentSpawnRange;
    public int squadRemaining;

    private bool isHostInstance = false;


    private float currentRangeOffset;
    public float rangeOffsetMax;


    private bool deploySAM = false;


    void Awake()
    {

        isHostInstance = PhotonNetwork.PlayerList.Length == 1;


        waveTimer = waveDeployDelay;
        squadTimer = squadDeployDelay;
        samTimer = SAMDeployDelay;
        
        initLists();
        fillWaypointList();
        initSpawnAxis();
    }

    private void initLists()
    {
        waypoints = new List<Transform>();
        myLaneUnits = new List<CombatFlow>();
        frontWave = new List<CombatFlow>();
        //myLaneSAMs = new List<CombatFlow>();
    }

    private void initSpawnAxis()
    {
        spawnAxisDir = Vector3.Cross(Vector3.up, waypoints[0].position - transform.position).normalized;
        currentSpawnPoint = randomSpawnPoint();
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
    { // AAA not included, as a temporary bug fix
        return SAMCount == SAMPerWave && 
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


    void Update()
    {
        if(isHostInstance && !doSpawn && (PhotonNetwork.PlayerList.Length > 1 || 
            Input.GetKeyDown(KeyCode.J)))
        {
            doSpawn = true;
        }
    }

    void FixedUpdate()
    {
        

        leaderUpdateCountdown();

        if (doSpawn)
        {

            if (isHostInstance)
            {
                bool isWaveComplete = waveComplete();

                if (!isWaveComplete)
                {
                    countDownSAM();
                }



                if (squadRemaining <= 0)
                {
                    if (isWaveComplete)
                    {
                        // count down wave timer
                        countDownWave();
                    }
                    else
                    {
                        countDownSquad();
                    }
                }
                else
                {

                    countdownRapidDeploy();
                }
            }
        }
    }

    private void countDownSAM()
    {
        if(samTimer < 0)
        {
            if (SAMCount != SAMPerWave)
            {

                // deploy sam
                deploySAM = true;
                deployUnit();
                samTimer = SAMDeployDelay;
            }
        }
        else
        {
            samTimer -= Time.fixedDeltaTime;
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
        //tankCount++;
        //squadRemaining--;

        GameObject selectedPrefab = selectDeployPrefab();

        Vector3 spawnPoint = currentSpawnPoint;
        if(selectedPrefab == SAMPrefab)
        {
            spawnPoint = randomSpawnPoint();
            //Debug.LogError("SAM detected, spawning at random point");
        }


        CreepControl newCreep = PhotonNetwork.Instantiate(selectedPrefab.name, spawnPoint,
            Quaternion.LookRotation(waypoints[1].position - transform.position, Vector3.up)).GetComponent<CreepControl>();

        float range;
        



        if (selectedPrefab == SAMPrefab)
        {
            range = SAMSpacing * SAMCount + SAMSpacing;
        }
        else if(selectedPrefab == AAAPrefab)
        {
            squadRemaining--;
            range = currentSpawnRange - currentRangeOffset;
        }
        else
        {
            squadRemaining--;
            range = newCreep.effectiveRange - currentRangeOffset;
            currentSpawnRange = range;
        }

        Vector3 offset = spawnPoint - transform.position;


        int teamNum = CombatFlow.convertTeamToNum(team);

        // This shouldn't be buffered. In final product, when player joins mid-match...
        // ... should make each 
        newCreep.photonView.RPC("rpcInit", RpcTarget.AllBuffered, photonView.ViewID, offset, range, teamNum);


        // instantiate
        // place at offset
        // fill waypoints
        // set offset
        
        // RANGE:
        //  if creep, use its own range
        //  if AAA at end of squad, use previously saved range
        
    }

    private GameObject selectDeployPrefab()
    {
        if (deploySAM) // if set to deploy SAM
        {
            SAMCount++;
            deploySAM = false;
            return SAMPrefab;
        }
        else // NOT set to deploy sam
        {
            if (squadRemaining != 1 ) // all but last in squad are normal type
            {

                if (tankCount != tankPerWave)
                {
                    tankCount++;
                    return tankPrefab;
                }
                else if (rocketCount != rocketPerWave)
                {
                    rocketCount++;
                    return rocketPrefab;
                }
                else if (artilleryCount != artilleryPerWave)
                {
                    artilleryCount++;
                    return artilleryPrefab;
                }
                else
                {
                    AAACount++;
                    return AAAPrefab;
                }
            }
            else // last in squad is always AAA
            {
                AAACount++;
                return AAAPrefab;
            }
        }


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
            //myLaneSAMs.Remove(dyingFlow);
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
        generateFrontWaveList();

        return leader;
    }

    private void generateFrontWaveList()
    {
        frontWave.Clear();
        //frontWave.Add(myLeader);

        for(int i = 0; i < myLaneUnits.Count; i++)
        {
            CombatFlow currentUnit = myLaneUnits[i];

            if (currentUnit != null)
            {
                float distToLeader = Vector3.Distance(currentUnit.transform.position, myLeader.transform.position);

                if (distToLeader < leaderRadius)
                {
                    frontWave.Add(currentUnit);
                }
            }
            

        }
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
