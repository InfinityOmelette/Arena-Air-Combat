using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class ShipSpawner : MonoBehaviourPunCallbacks
{
    public SpawnBank spawnBank;


    public GameObject carrierPrefab;
    public GameObject cruiserPrefab;

    public LaneAdmiral admiral;


    public float cruiserCost;
    public float carrierCost;


    //public bool debugSpawnABoat = false;
    //public bool boatSpawned = false;

    public CombatFlow myFlow;

    public Transform spawnCenter;

    public float spawnTickDelay;
    private float spawnTickTimer;

    private void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        spawnBank = GetComponent<SpawnBank>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if(debugSpawnABoat && !boatSpawned)
        //{
        //    spawnCruiser();
        //    boatSpawned = true;

        //}
    }

    // If lane has a carrier, try to spawn cruiser
    // If no carrier, prioritise carrier spawn

    private void FixedUpdate()
    {
        if(spawnTickTimer < 0f)
        {
            // try to spawn
            trySpawnProcess();
            spawnTickTimer = spawnTickDelay;
        }
        else
        {
            spawnTickTimer -= Time.fixedDeltaTime;
        }
    }

    private void trySpawnProcess()
    {
        if(admiral.isFleetReady() && admiral.laneCarrier == null)
        {
            trySpawnPrefab(carrierPrefab, carrierCost);

        }
        else
        {
            // try to spawn frontline ship (cruiser)
            trySpawnPrefab(cruiserPrefab, cruiserCost);
        }
    }

    private void trySpawnPrefab(GameObject prefab, float cost)
    {
        if(spawnBank.supplies >= cost)
        {
            spawnUnit(prefab, spawnCenter);
            spawnBank.resetSupplies();
        }
    }

    private void spawnUnit(GameObject unitPrefab, Transform spawnCent)
    {
        // instantiate
        // set position
        // set team
        // link to admiral
        GameObject newShip = PhotonNetwork.InstantiateSceneObject(unitPrefab.name, spawnCent.position,
            spawnCent.rotation);

        //CombatFlow shipFlow = newShip.GetComponent<CombatFlow>();
        ShipNavigation shipNav = newShip.GetComponent<ShipNavigation>();

        shipNav.myFlow.team = myFlow.team;

        admiral.linkShip(shipNav);

    }


}
