using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Photon.Pun;
using Photon.Realtime;


public class TeamSpawner : MonoBehaviourPunCallbacks
{

    public static GameObject localPlayerInstance;

    public CombatFlow.Team team;

    public GameObject spawnPoint_TEMP;

    public GameObject hudObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public GameObject spawnPlayer(GameObject playerPrefab)
    {
        //playerPrefab.name = PhotonNetwork.NickName;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, findEmptySpawnPoint().transform.position, Quaternion.identity, 0);
        localPlayerInstance = player;
        player.GetComponent<CombatFlow>().setNetName(PhotonNetwork.NickName);

        return player;
    }

    public void setPlayerAsControllable(GameObject playerObj)
    {
        
        PlayerInput_Aircraft inputRoot = playerObj.GetComponent<PlayerInput_Aircraft>();
        inputRoot.enabled = true;

        CombatFlow playerFlow = playerObj.GetComponent<CombatFlow>();
        playerFlow.team = team;
        playerFlow.isLocalPlayer = true;

        // Enable flight and engine control
        playerObj.GetComponent<RealFlightControl>().enabled = true;
        playerObj.GetComponent<EngineControl>().enabled = true;

        // Link hud to obj
        hudObj.SetActive(true);
        hudObj.GetComponent<hudControl>().linkHudToAircraft(playerObj);

        // Enable controllers
        inputRoot.cam.gameObject.SetActive(true);       // camera
        inputRoot.cannons.gameObject.SetActive(true); // cannon
        inputRoot.hardpointController.gameObject.SetActive(true);   // hardpoints


        // activate targeting computer and radar
        playerObj.GetComponent<Radar>().enabled = true;
        playerObj.GetComponent<TgtComputer>().enabled = true;


        inputRoot.isReady = true; // start receiving and processing input
    }


    public GameObject findEmptySpawnPoint()
    {
        //GameObject[] spawnPoints = 
        return spawnPoint_TEMP;
    }
}
