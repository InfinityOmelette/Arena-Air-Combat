using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

public class AI_Spawner : MonoBehaviourPunCallbacks
{

    public InputField txtNumAI;

    public Toggle chkLockToBot;

    private GameManager gm;


    public float spawnDelay = 2f;
    public float spawnCounter;


    public List<AI_GroundAttack> myAI;
    public List<float> aiRespawnTimers;


    //private TeamSpawner spawner;

    public int maxAI = 8;

    int totalSpawnCount = 0;

    public bool lockToBot = false;

    public PhotonView ph;

    private AirSpawnController spawnControl;

    void Awake()
    {
        ph = GetComponent<PhotonView>();
        //spawner = GetComponent<TeamSpawner>();
        myAI = new List<AI_GroundAttack>();
        aiRespawnTimers = new List<float>();

        spawnControl = GetComponent<AirSpawnController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.getGM();


        txtNumAI.readOnly = !gm.isHostInstance;
        chkLockToBot.interactable = gm.isHostInstance;

        chkLockToBot.isOn = lockToBot;

    }

    // Update is called once per frame
    void Update()
    {

        if (gm.isHostInstance) 
        {
            txtNumAI.enabled = true;

            // spawnCounter is used purely for having interval between concurrent AI player spawns
            // So they don't spawn on top of each other
            if (spawnCounter > 0)
            {
                spawnCounter -= Time.deltaTime;
            }
            else
            {
                if (trySpawn(myAI, spawnControl.getTeam()))
                {
                    spawnCounter = spawnDelay;
                }
            }

            tryIncrementAiSpawnTimers(Time.deltaTime);
        }
        else
        {
            txtNumAI.enabled = false;
        }


    }

    // This defaults to first spawner of team. In future, will need to decide on spawner
    // based on match context
    public bool canSpawnIndex(int index)
    {
        return aiRespawnTimers[index] > spawnControl.getSpawner().respawnTimeEffective;
    }

    public void tryIncrementAiSpawnTimers(float deltaTime)
    {
        for(int i = 0; i < myAI.Count; i++)
        {
            if(myAI[i] == null)
            {
                aiRespawnTimers[i] += deltaTime;
            }
        }
    }

    

    private bool trySpawn(List<AI_GroundAttack> listAI, CombatFlow.Team team)
    {
        bool didSpawn = false;

        for(int i = 0; i < listAI.Count && !didSpawn; i++)
        {
            // defaults to trying to spawn in first spawner
            // in future, will need to decide spawner from match context
            if(listAI[i] == null && canSpawnIndex(i))
            {
                didSpawn = true;
                listAI[i] = doSpawn();
                aiRespawnTimers[i] = 0.0f;
            }
        }

        return didSpawn;
    }
    
    private AI_GroundAttack doSpawn()
    {
        totalSpawnCount++;

        CombatFlow newAircraft = 
            gm.spawnPlayer(CombatFlow.convertTeamToNum(spawnControl.getTeam()), true);
        AI_GroundAttack newAirGndAtk = newAircraft.GetComponent<AI_GroundAttack>();



        newAirGndAtk.assignToLane(decideLane(myAI));

        newAircraft.setNetName("JeffBot" + totalSpawnCount);

        return newAirGndAtk;
    }

    public void onTextChange()
    {

        if (gm.isHostInstance)
        {

            float numRaw;

            bool noErrorNum = float.TryParse(txtNumAI.text, out numRaw);

            int numSet;



            if (noErrorNum)
            {
                numSet = Mathf.RoundToInt(numRaw);


                if (numSet > maxAI)
                {
                    numSet = maxAI;
                }
                else if (numSet < 0)
                {
                    numSet = 0;
                }


                changeContainerSize(myAI, numSet);
            }
            else
            {
                numSet = myAI.Count;
            }


            //txtNumAI.text = numSet.ToString();
            ph.RPC("rpcShowCountAI", RpcTarget.AllBuffered, numSet);
        }
    }


    private void changeContainerSize(List<AI_GroundAttack> listAI, int newSize)
    {
        if (gm.isHostInstance)
        {

            while (listAI.Count != newSize)
            {

                if (listAI.Count < newSize)
                {
                    // add a slot -- note that this is NOT spawning the aircraft here
                    //  spawning done one at a time via timer in update
                    listAI.Add(null);
                    aiRespawnTimers.Add(spawnControl.getSpawner().respawnTimeEffective);
                }
                else if (listAI.Count > newSize)
                {
                    int lastIndex = listAI.Count - 1;

                    if (listAI[lastIndex] != null)
                    {
                        listAI[lastIndex].myFlow.die(); // networked
                    }
                    listAI.RemoveAt(lastIndex);
                    aiRespawnTimers.RemoveAt(lastIndex);

                }

            }
        }

    }

    private int decideLane(List<AI_GroundAttack> listAI)
    {
        int lane = 0; // default top

        if (lockToBot)
        {
            lane = 1;
        }
        else {

            int numInTopLane = countNumInLane(listAI, 0); // 0 is top lane
            int numInBotLane = countNumInLane(listAI, 1); // 1 is bot lane

            // if there are fewer bot lane AI in the air than top lane AI in the air
            if (numInBotLane < numInTopLane)
            {
                lane = 1; // spawn a bot lane ai
            }
        }

        return lane;
    }

    private int countNumInLane(List<AI_GroundAttack> listAI, int laneId)
    {
        int count = 0;

        for (int i = 0; i < listAI.Count; i++)
        {
            if (listAI[i] != null && listAI[i].laneIndex == laneId)
            {
                count++; // laneIndex is 0 if top lane, 1 if bot
            }
        }

        return count;
    }


    public void onLockToBotCheck()
    {
        Debug.Log("LockToBot: " + lockToBot + ", checkValue: " + chkLockToBot.isOn);

        if (gm.isHostInstance && lockToBot != chkLockToBot.isOn)
        {
            lockToBot = chkLockToBot.isOn;

            ph.RPC("rpcShowLockToBot", RpcTarget.AllBuffered, lockToBot);




            pulseLockBot(lockToBot);
        }
    }

    public void pulseLockBot(bool lockBot)
    {
        if (lockBot)
        {
            updateAllToLane(1);
        }
        else
        {
            reassignLanes();
        }

    }


    public void reassignLanes()
    {
        Debug.Log("Reassigning lanes");

        for(int i = 0; i < myAI.Count; i++)
        {
            if(myAI[i] != null)
            {
                myAI[i].laneIndex = -1; // reset all to bad value so we can reassign fresh values
            }
        }


        for(int i = 0; i < myAI.Count; i++)
        {
            if(myAI[i] != null)
            {
                myAI[i].assignToLane(decideLane(myAI));
            }
        }
    }

    public void updateAllToLane(int laneId)
    {
        for(int i = 0; i < myAI.Count; i++)
        {
            if(myAI[i] != null)
            {
                myAI[i].assignToLane(laneId);
            }

        }
    }


    [PunRPC]
    public void rpcShowCountAI(int count)
    {
        txtNumAI.text = count.ToString();
    }


    [PunRPC]
    public void rpcShowLockToBot(bool lockToBot)
    {
        chkLockToBot.isOn = lockToBot;
    }




}
