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
        GameObject emptySpawn = findEmptySpawnPoint();

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, emptySpawn.transform.position, Quaternion.identity, 0);
        localPlayerInstance = player;

        player.transform.rotation = emptySpawn.transform.rotation;

        CombatFlow playerFlow = player.GetComponent<CombatFlow>();
        playerFlow.setNetName(PhotonNetwork.NickName);
        playerFlow.setNetTeam(CombatFlow.convertTeamToNum(team));

        return player;
    }

    // moves all of nested children to layer, too
    void MoveToLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
            MoveToLayer(child, layer);
    }

    public void setPlayerAsAI(GameObject aircraftObj)
    {
        Debug.Log("Setting player as AI");

        // layer 8 is localPlayer layer
        MoveToLayer(aircraftObj.transform, 8);

        PlayerInput_Aircraft inputRoot = aircraftObj.GetComponent<PlayerInput_Aircraft>();
        inputRoot.enabled = false;

        aircraftObj.GetComponent<AI_Aircraft>().enabled = true;

        const int localPlayerLayer = 8;
        inputRoot.cannons.setIgnoreLayer(localPlayerLayer);

        CombatFlow playerFlow = aircraftObj.GetComponent<CombatFlow>();
        playerFlow.team = team;
        playerFlow.localOwned = true;
        playerFlow.aiControlled = true;

        // Enable flight and engine control
        aircraftObj.GetComponent<RealFlightControl>().enabled = true;
        aircraftObj.GetComponent<EngineControl>().enabled = true;

        // Link hud to obj
        hudControl hud = hudObj.GetComponent<hudControl>();
        //hud.setHudVisible(true);
        //hud.linkHudToAircraft(aircraftObj);

        // Enable controllers
        //inputRoot.cam.gameObject.SetActive(true);       // camera
        inputRoot.cannons.gameObject.SetActive(true); // cannon
        inputRoot.hardpointController.gameObject.SetActive(true);   // hardpoints

        // Save this player's camera
        //PerspectiveManager.getPManager().mainCam = inputRoot.cam.camRef.GetComponent<Camera>();

        // activate targeting computer and radar
        aircraftObj.GetComponent<Radar>().enabled = true;
        //aircraftObj.GetComponent<TgtComputer>().enabled = true;


        //inputRoot.isReady = true; // start receiving and processing input
    }

    public void setPlayerAsControllable(GameObject playerObj)
    {
        // layer 8 is localPlayer layer
        MoveToLayer(playerObj.transform, 8);

        PlayerInput_Aircraft inputRoot = playerObj.GetComponent<PlayerInput_Aircraft>();
        inputRoot.enabled = true;

        const int localPlayerLayer = 8;
        inputRoot.cannons.setIgnoreLayer(localPlayerLayer);

        CombatFlow playerFlow = playerObj.GetComponent<CombatFlow>();
        playerFlow.team = team;
        playerFlow.isLocalPlayer = true;

        // Enable flight and engine control
        playerObj.GetComponent<RealFlightControl>().enabled = true;
        playerObj.GetComponent<EngineControl>().enabled = true;

        // Link hud to obj
        hudControl hud = hudObj.GetComponent<hudControl>();
        hud.setHudVisible(true);
        hud.linkHudToAircraft(playerObj);

        // Enable controllers
        inputRoot.cam.gameObject.SetActive(true);       // camera
        inputRoot.cannons.gameObject.SetActive(true); // cannon
        inputRoot.hardpointController.gameObject.SetActive(true);   // hardpoints

        // Save this player's camera
        PerspectiveManager.getPManager().mainCam = inputRoot.cam.camRef.GetComponent<Camera>();

        // activate targeting computer and radar
        playerObj.GetComponent<Radar>().enabled = true;
        playerObj.GetComponent<TgtComputer>().enabled = true;


        inputRoot.isReady = true; // start receiving and processing input
    }


    public GameObject findEmptySpawnPoint()
    {
        GameObject[] children = new GameObject[transform.childCount];
        //GameObject defaultObj = childrenTransforms[0].gameObject; // default to first

        int selectIndex = -1;

        for(int i = 0; i < children.Length && selectIndex == -1; i++)
        {
            children[i] = transform.GetChild(i).gameObject;
            GayAssColliderScript collScript = children[i].GetComponent<GayAssColliderScript>();

            if (!collScript.isTriggered)
            {
                selectIndex = i;
            }

        }

        if(selectIndex < 0)
        {
            selectIndex = 0;
        }


        return children[selectIndex];
    }



}
