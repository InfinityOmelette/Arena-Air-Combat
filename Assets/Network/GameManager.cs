using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static GameManager getGM()
    {
        if(gm == null)
        {
            gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        return gm;
    }

    void Start()
    {
        //spawnPlayer();
        pm = PerspectiveManager.getPManager();
        
    }

    public void spawnPlayer(int teamNum)
    {
        localTeam = CombatFlow.convertNumToTeam((short)teamNum);
        spawnUIPanel.SetActive(false);

        // call almost all of this at runtime after player selects team
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            if (TeamSpawner.localPlayerInstance == null)
            {
                //PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint.position, Quaternion.identity, 0);
                TeamSpawner spawner = getSpawnerByTeam(localTeam);
                GameObject playerObj = spawner.spawnPlayer(playerPrefab);
                spawner.setPlayerAsControllable(playerObj);
                localPlayer = playerObj;
                //playerObj.name = PhotonNetwork.NickName;

            }
        }
    }

    public void showSpawnMenu()
    {
        hudControl.setHudVisible(false);
        hudControl.linkHudToAircraft(null);
        pm.switchToType(PerspectiveProperties.CamType.SPECTATOR);
        pm.setMouseLock(false);
        spawnUIPanel.SetActive(true);
    }


    #region Photon Callbacks




    #endregion


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

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


        //if (PhotonNetwork.IsMasterClient)
        //{
        //    Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


        //    LoadArena();
        //}
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        //if (PhotonNetwork.IsMasterClient)
        //{
        //    Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


        //    LoadArena();
        //}
    }

    #endregion
}

