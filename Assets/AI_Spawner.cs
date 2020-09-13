using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

public class AI_Spawner : MonoBehaviourPun
{

    public InputField txtNumAI;

    public Toggle chkLockToBot;

    private GameManager gm;


    public float spawnDelay = 2f;
    public float spawnCounter;


    public List<AI_GroundAttack> myAI;

    private TeamSpawner spawner;

    public int maxAI = 8;

    int totalSpawnCount = 0;

    public bool lockToBot = false;

    public PhotonView ph;

    void Awake()
    {
        ph = GetComponent<PhotonView>();

        spawner = GetComponent<TeamSpawner>();
        myAI = new List<AI_GroundAttack>();
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

            if (spawnCounter > 0)
            {
                spawnCounter -= Time.deltaTime;
            }
            else
            {
                if (checkSpawnContainer(myAI, spawner.team))
                {
                    spawnCounter = spawnDelay;
                }
            }
        }
        else
        {
            txtNumAI.enabled = false;
        }
    }

    

    private bool checkSpawnContainer(List<AI_GroundAttack> listAI, CombatFlow.Team team)
    {
        bool didSpawn = false;

        for(int i = 0; i < listAI.Count && !didSpawn; i++)
        {
            if(listAI[i] == null)
            {
                didSpawn = true;
                listAI[i] = doSpawn();
            }
        }

        return didSpawn;
    }
    
    private AI_GroundAttack doSpawn()
    {
        totalSpawnCount++;

        CombatFlow newAircraft = gm.spawnPlayer(CombatFlow.convertTeamToNum(spawner.team), true);
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
                }
                else if (listAI.Count > newSize)
                {
                    int lastIndex = listAI.Count - 1;

                    if (listAI[lastIndex] != null)
                    {
                        listAI[lastIndex].myFlow.die(); // networked
                    }
                    listAI.RemoveAt(lastIndex);

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

            int numBotLanes = countBotLanes(listAI);

            // if there are fewer bot lane AI in the air than top lane AI in the air
            if (numBotLanes < (listAI.Count - numBotLanes))
            {
                lane = 1; // spawn a bot lane ai
            }
        }

        return lane;
    }

    private int countBotLanes(List<AI_GroundAttack> listAI)
    {
        int count = 0;

        for(int i = 0; i < listAI.Count; i++)
        {
            if(listAI[i] != null)
            {
                count += listAI[i].laneIndex; // laneIndex is 0 if top lane, 1 if bot
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
