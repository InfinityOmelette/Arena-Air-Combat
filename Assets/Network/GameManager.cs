using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;



public class GameManager : MonoBehaviourPunCallbacks
{

    public GameObject playerPrefab;

    public GameObject hudObj;
    public hudControl hudControl;

    public GameObject spawnUIPanel;

    // set these references in editor
    public GameObject[] teamSpawnerCollections;

    public CombatFlow.Team localTeam; // set this at runtime when player selects team.

    public PerspectiveManager pm;

    private static GameManager gm;

    public GameObject localPlayer;


    public int targetFrameRate;


    public bool isHostInstance;

    public NetPositionHub netPosHub;

    // this isn't used currently, but seems like a good thing to have  ¯\_(ツ)_/¯
    public string userID;



    public UnityEvent playerSpawnEvent;


    public List<CombatFlow> debugGroundTgtList;
    public GameObject debugLeader;
    public GameObject debugRetreatLeader;

    
    public List<List<CombatFlow>> teamAircraftLists;


    public List<CombatFlow> debugViewTeam1Aircraft;
    public List<CombatFlow> debugViewTeam2Aircraft;


    public List<LaneManager> team1Lanes;
    public List<LaneManager> team2Lanes;


    public const int TOP_LANE_INDEX = 0;
    public const int BOT_LANE_INDEX = 1;


    public GameObject aiSpawnControlObj;
    

    public static GameManager getGM()
    {
        if(gm == null)
        {
            gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        return gm;
    }

    void Awake()
    {
        teamAircraftLists = new List<List<CombatFlow>>();

        teamAircraftLists.Add(new List<CombatFlow>());
        teamAircraftLists.Add(new List<CombatFlow>());

        debugViewTeam1Aircraft = teamAircraftLists[0];
        debugViewTeam2Aircraft = teamAircraftLists[1];


        playerSpawnEvent = new UnityEvent();

        netPosHub = GetComponent<NetPositionHub>();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;

        isHostInstance = PhotonNetwork.PlayerList.Length == 1;
    }


    public List<CombatFlow> getTeamAircraftList(CombatFlow.Team team)
    {
        return getTeamAircraftList(CombatFlow.convertTeamToNum(team));
    }

    public List<CombatFlow> getTeamAircraftList(int teamNum)
    {
        return teamAircraftLists[teamNum];
    }

    public List<LaneManager> getTeamLanes(CombatFlow.Team team)
    {
        if(team == CombatFlow.Team.TEAM1)
        {
            return team1Lanes;
        }
        else
        {
            return team2Lanes;
        }

    }

    public List<LaneManager> getTeamLanes(int teamNum)
    {
        return getTeamLanes(CombatFlow.convertNumToTeam((short)teamNum));
    }

    void Start()
    {
        //spawnPlayer();
        pm = PerspectiveManager.getPManager();

        userID = PhotonNetwork.LocalPlayer.UserId;

        //PhotonNetwork.play
    }

    void Update()
    {
        

        //if (isHostInstance && Input.GetKeyDown(KeyCode.B))
        //{
        //    Debug.Log("=========== SPAWNING AI");
        //    spawnPlayer(CombatFlow.convertTeamToNum( 0), true);
        //}
    }

    
    public void spawnPlayerNoReturn(int teamNum)
    {
        spawnPlayer(teamNum, false);
    }


    public CombatFlow spawnPlayer(int teamNum)
    {
        return spawnPlayer(teamNum, false);
    }

    public CombatFlow spawnPlayer(int teamNum, bool isAI = false)
    {

        CombatFlow newSpawn = null;

        if (!isAI)
        {
            localTeam = CombatFlow.convertNumToTeam((short)teamNum);
            spawnUIPanel.SetActive(false);
        }

        Debug.Log("Spawning player on team: " + teamNum + ", isAI: " + isAI);

        // call almost all of this at runtime after player selects team
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate

            Debug.Log("LocalPlayerInstance: " + TeamSpawner.localPlayerInstance);

            if (TeamSpawner.localPlayerInstance == null || isAI)
            {



                //PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint.position, Quaternion.identity, 0);
                TeamSpawner spawner = getSpawnerByNum(teamNum);
                GameObject playerObj;

                if (isAI)
                {
                    playerObj = spawner.spawnPlayer(playerPrefab, "Jeff", false);
                    Debug.Log("Spawning AI");
                    spawner.setPlayerAsAI(playerObj);
                    //playerObj.name = "Jeff";
                }
                else
                {
                    playerObj = spawner.spawnPlayer(playerPrefab, PhotonNetwork.NickName);
                    spawner.setPlayerAsControllable(playerObj);
                    localPlayer = playerObj;
                    playerObj.name = PhotonNetwork.NickName;
                    playerSpawnEvent.Invoke();
                }

                newSpawn = playerObj.GetComponent<CombatFlow>();
                //spawner.setPlayerAsControllable(playerObj);
                //localPlayer = playerObj;

            }
        }

        return newSpawn;
    }

    public void showSpawnMenu()
    {
        hudControl.setHudVisible(false);
        hudControl.linkHudToAircraft(null);
        pm.switchToType(PerspectiveProperties.CamType.SPECTATOR);
        pm.setMouseLock(false);
        spawnUIPanel.SetActive(true);
    }


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion


    #region Private Methods

    private TeamSpawner getSpawnerByTeam(CombatFlow.Team team)
    {
        TeamSpawner returnObj = null;
        for(int i = 0; i < teamSpawnerCollections.Length; i++)
        {
            TeamSpawner current = teamSpawnerCollections[i].GetComponent<TeamSpawner>();
            if (current.team == team)
            {
                returnObj = current;
            }
        }
        return returnObj;
    }

    private TeamSpawner getSpawnerByNum(int teamNum)
    {
        return getSpawnerByTeam(CombatFlow.convertNumToTeam((short)teamNum));
    }


    void LoadArena()
    {
       

        //if (!PhotonNetwork.IsMasterClient)
        //{
        //    Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        //}
        //Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        //PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    #endregion


    #region Photon Callbacks



    public override void OnPlayerEnteredRoom(Player player)
    {
        
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isHostInstance = true;
        }
    }

    #endregion
}

