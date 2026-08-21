using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class CreepSpawnPoint : MonoBehaviourPunCallbacks
{

    public StrategicTarget myStrat;


    // Lane manager handles timing and spawn triggering as usual
    // but propogates spawn copies to each factory's position in lane

    public LaneManager parentLane;

    public SpawnBank myBank;

    private int rosterSAMs;
    private int rosterTanks;
    private int rosterArtilleries;
    private int rosterRocket;

    //private int rosterSquadCount;

    public int maxSquadSize;
    private int squadMemberCounter;


    public bool doSpawn;

    public float samPercent;
    public float tankPercent;
    public float artilleryPercent;
    public float rocketPercent;


    public float SAMDeployDelay; // time between SAM spawns
    public float rapidDeployDelay; // time between squad member spawns
    public float squadDeployDelay; // time between squad spawns

    public float laneWidth;

    private float samTimer;
    private float squadTimer;
    private float rapidTimer;

    private Vector3 squadSpawnPoint;
    public float minSpawnOffset;
    public Transform spawnCenter;

    private float prevRange;

    public float initSAMDelay;
    public float frontSAMStandoff;
    public float SAMSpacing;

    private int SAMCount;

    private void Awake()
    {
        myStrat = GetComponent<StrategicTarget>();
        myBank = GetComponent<SpawnBank>();
    }

    // Start is called before the first frame update
    void Start()
    {
        parentLane = myStrat.myLane;
    }



    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            beginWave(); // normally this called by command center
        }
    }

    void FixedUpdate()
    {
        updateLaneRef();

        // only performs spawn if ordered to by command
        //  doSpawn remains active until spawn wave complete
        if (doSpawn)
        {
            spawnSchedule();
        }
    }


    private void spawnSchedule()
    {
        if (waveComplete())
        {
            endWave();
        }
        else // spawn tickets remaining in roster, so try to spawn those
        {
            // independent SAM spawn timer
            if (rosterSAMs > 0)
            {
                samTimerProcess();
            }

            // independent SQUAD spawn timer
            if (squadUnitsAvailable())
            {
                squadTimerProcess();
            }
        }
    }

    private void endWave()
    {
        doSpawn = false; // register that wave is complete, stop timer process
        SAMCount = 0;
    }

    private bool squadUnitsAvailable()
    {
        return rosterRocket + rosterArtilleries + rosterTanks > 0;
    }

    private void samTimerProcess()
    {
        
       if (samTimer < 0)
       {
           spawnSAM();
           samTimer = SAMDeployDelay;
       }
       else
       {
           // count down timer
           samTimer -= Time.fixedDeltaTime;
       }
        
    }


    
    private void spawnSAM()
    {
        deployUnit(parentLane.SAMPrefab, randomSpawnPoint());
        SAMCount++;
        rosterSAMs--;
    }

    private void squadTimerProcess()
    {
        if(squadTimer < 0)
        {
            // work on spawning new squad, member by member

            if(squadMemberCounter > 0)
            {
                rapidTimerProcess();
                
            }
            else
            {
                squadMemberCounter = maxSquadSize;
                squadTimer = squadDeployDelay;
                setRandomSquadSpawnPoint();
            }

            // only reset timer once squad is fully spawned
        }
        else
        {
            squadTimer -= Time.fixedDeltaTime;
        }
    }

    private Vector3 getSpawnAxisDir()
    {
        return spawnCenter.right;
    }

    private Vector3 randomSpawnOffset()
    {
        float randSign = Mathf.Sign(Random.Range(-1f, 1f));
        return getSpawnAxisDir() * Random.Range(minSpawnOffset, laneWidth) * randSign;
    }

    private Vector3 randomSpawnPoint()
    {
        return spawnCenter.position + randomSpawnOffset();
    }

    private void setRandomSquadSpawnPoint()
    {
        squadSpawnPoint =  randomSpawnPoint();
    }

    private void rapidTimerProcess()
    {
        if(rapidTimer < 0)
        {
            GameObject selectedPrefab = selectSquadDeployPrefab();

            deployUnit(selectedPrefab, squadSpawnPoint);
            squadMemberCounter--;
            rapidTimer = rapidDeployDelay;

        }
        else
        {
            rapidTimer -= Time.fixedDeltaTime;
        }
    }

    private void deployUnit(GameObject unitPrefab, Vector3 spawnPos)
    {
        // instantiate
        // set 

        Vector3 offset = spawnPos - spawnCenter.position;

        CreepControl newCreep = PhotonNetwork.InstantiateSceneObject(unitPrefab.name, spawnPos,
            Quaternion.LookRotation(spawnCenter.forward, Vector3.up)).GetComponent<CreepControl>();

        int teamNum = (int)parentLane.team;

        float range = calculateRange(unitPrefab, newCreep);


        newCreep.photonView.RPC("rpcInit", RpcTarget.AllBuffered, parentLane.photonView.ViewID, 
            offset, range, teamNum);

    }

    // creep's "effective range" is range from enemy leader at which it stops moving
    private float calculateRange(GameObject unitPrefab, CreepControl newCreep)
    {
        float range;
        // remember, "effective range" just determines when creep stops moving when approached by enemy creep leader
        if (unitPrefab == parentLane.SAMPrefab)
        {
            range = SAMSpacing * (SAMCount + 1) + frontSAMStandoff;
        }
        else if (unitPrefab == parentLane.AAAPrefab)
        {
            //squadRemaining--;
            range = prevRange;
        }
        else
        {
            //squadRemaining--;
            range = newCreep.effectiveRange;
            prevRange = range;
        }

        return range;
    }

    private GameObject selectSquadDeployPrefab()
    {
        // spawn creep, either tank, artillery or rocket
        // if squadcounter

        GameObject selectedPrefab = null;

        if (squadMemberCounter == 1) // last unit in squad will be AAA
        {
            // spawn AAA
            selectedPrefab = parentLane.AAAPrefab;
        }
        else if (rosterTanks > 0)
        {
            // spawn tank
            rosterTanks--;
            selectedPrefab = parentLane.tankPrefab;
        }
        else if (rosterArtilleries > 0)
        {

            // spawn artillery
            rosterArtilleries--;
            selectedPrefab = parentLane.artilleryPrefab;
        }
        else // not checking rocket roster because this function only gets entered if non-sam in roster
        {
            // spawn rocket
            rosterRocket--;
            selectedPrefab = parentLane.rocketPrefab;
        }

        //squadMemberCounter--;
        return selectedPrefab;
    }

    // counts remaining roster spawn tickets, complete if all spent
    private bool waveComplete()
    {
        return rosterSAMs + rosterTanks + rosterArtilleries + rosterRocket <= 0;
    }

    private void updateLaneRef()
    {
        if (parentLane != myStrat.myLane)
        {
            if(parentLane != null)
            {
                parentLane.spawnFOBs.Remove(this);
            }
            

            parentLane = myStrat.myLane;
            parentLane.spawnFOBs.Add(this);
        }
    }

    public void beginWave()
    {
        doSpawn = true;
        SAMCount = 0;
        samTimer = initSAMDelay;
        generateSpawnRoster();
        resetSquadCount();
        setRandomSquadSpawnPoint();
    }

    private void resetSquadCount()
    {
        squadMemberCounter = maxSquadSize;
    }

    public void generateSpawnRoster()
    {
        calculateSpawnRoster(myBank.supplies);
        myBank.resetSupplies();
    }

    private void calculateSpawnRoster(int resources)
    {
        rosterSAMs = Mathf.RoundToInt(samPercent * resources);
        rosterArtilleries = Mathf.RoundToInt(artilleryPercent * resources);
        rosterTanks = Mathf.RoundToInt(tankPercent * resources);
        rosterRocket = Mathf.RoundToInt(rocketPercent * resources);
    }
}
