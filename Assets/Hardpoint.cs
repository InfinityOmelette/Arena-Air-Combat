using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Hardpoint : MonoBehaviourPunCallbacks
{


    public GameObject weaponTypePrefab;

    //public GameObject testReloadWeaponTypePrefab;

    public Transform spawnCenter;

    public GameObject loadedWeaponObj;
    public GameObject activeWeaponObj;

    public float reloadTimeMax;
    public float currentReloadTimer;
    public bool readyToFire;

    // stays true from launchStart to launch
    public bool launchCommanded; // MAKE SURE THIS CHANGES BACK TO FALSE AS SOON AS LAUNCH OCCURS


    public float fireRateDelayRaw; // raw fire rate delay from weapon

    public float effectiveLaunchDelayMax; // effective launch delay, taking into account position in sequence
    private float launchDelayRemain;

    public bool dropOnLaunch = true;


    public short roundsMax;
    public short roundsRemain;

    PhotonView photonView;

    public CombatFlow rootFlow;

    public AudioSource launchSoundSource;

    public List<Weapon.Weight> acceptableWeightAtt;
    public List<Weapon.Guidance> acceptableGuidanceAtt;
    public List<Weapon.Domain> acceptableDomainAtt;

    private bool initialized = false;

    private bool stockClaimed = false;

    public HardpointController myController;
    public int stockIndex = 0;

    void Awake()
    {
        launchSoundSource = GetComponent<AudioSource>();
    }

    public void linkToController(HardpointController controller, int stockIndex)
    {
        this.myController = controller;
        this.stockIndex = stockIndex;
    }

    // Start is called before the first frame update
    void Start()
    {
        rootFlow = transform.root.GetComponent<CombatFlow>();
        photonView = PhotonView.Get(this);
        readyToFire = false;

        if (rootFlow.isLocalPlayer || rootFlow.aiControlled)
        {
            spawnWeapon();
        }

        initialized = true;
    }

    void spawnWeapon()
    {
        // instantiates prefab IN WORLD SPACE, fixed joint to player
        loadedWeaponObj = PhotonNetwork.Instantiate(weaponTypePrefab.name, spawnCenter.position, spawnCenter.rotation);

        int weaponId = loadedWeaponObj.GetComponent<PhotonView>().ViewID;
        loadedWeaponObj.GetComponent<CombatFlow>().localOwned = true;
        //Debug.Log("weaponId: " + weaponId);

        photonView.RPC("rpcInitializeWeapon", RpcTarget.AllBuffered, weaponId);
        
    }

    public void destroyWeapon()
    {
        if (rootFlow != null && (rootFlow.isLocalPlayer || rootFlow.aiControlled) && loadedWeaponObj != null)
        {
            loadedWeaponObj.GetComponent<Weapon>().destroyWeapon();
            //PhotonNetwork.Destroy(loadedWeaponObj);
        }
    }


    [PunRPC]
    void rpcInitializeWeapon(int weaponId)
    {

        PhotonView pView = PhotonNetwork.GetPhotonView(weaponId);

        if (pView != null) 
        {

            loadedWeaponObj = pView.gameObject;

            loadedWeaponObj.transform.position = spawnCenter.position;
            loadedWeaponObj.transform.rotation = spawnCenter.rotation;

            CombatFlow weaponFlow = loadedWeaponObj.GetComponent<CombatFlow>();
            Weapon weapon = loadedWeaponObj.GetComponent<Weapon>();

            weapon.myHardpoint = this;

            // locks weapon to hardpoint using fixedjoint
            weapon.linkToOwner(transform.root.gameObject);
            reloadTimeMax = weapon.reloadTimeDefault; // this really should go in linkToOwner() but I'm too lazy to repeat this in each implementation

            weapon.myTeam = transform.root.GetComponent<CombatFlow>().team;
            weaponFlow.team = weapon.myTeam;
            //Debug.LogWarning("Setting weapon to player's team: " + transform.root.GetComponent<CombatFlow>().team);



            readyToFire = true;
        }
    }

    public void launchWithLock(GameObject targetObj)
    {
        if (readyToFire)
        {
            loadedWeaponObj.GetComponent<Weapon>().myTarget = targetObj;
            launchStart();
        }
        
    }

    public void launchStart() // doesn't need lock
    {
        Debug.Log("launchStart() called");
        if (readyToFire)
        {
            activeWeaponObj = loadedWeaponObj;
            launchCommanded = true;
            launchDelayRemain = effectiveLaunchDelayMax;
        }
        else // weapon is not loaded
        {
            //Debug.Log("cannot fire weapon from hardpoint: " + gameObject.name + ", no weapon loaded");
        }
    }

    public void launch()
    {
        if (readyToFire)
        {
            //Debug.Log("launch() called");
            launchCommanded = false;
            loadedWeaponObj.GetComponent<Weapon>().launch();
            currentReloadTimer = reloadTimeMax;
        }
        else // weapon is not loaded
        {
            //Debug.Log("cannot fire weapon from hardpoint: " + gameObject.name + ", no weapon loaded");
        }
    }


    bool countDownDelayTimer()
    {
        
        bool delayComplete = false;
        if (launchDelayRemain > 0f)
        {
            launchDelayRemain -= Time.deltaTime;
        }
        else
        {
            delayComplete = true;
        }
        //Debug.Log("Time remain: " + launchDelayRemain);
        return delayComplete;
    }

    public void launchEnd()
    {
        Debug.Log("Hardpoint launchEnd called");
        // tell weapon to stop launching
        if (activeWeaponObj != null)
        {
            Debug.Log("----------------------- Successfully calling launchEnd() on weapon");
            activeWeaponObj.GetComponent<Weapon>().launchEnd();
        }
        else
        {
            Debug.Log("hardpoint launchEnd skipped, because activeWeaponObj is null");
        }



        launchCommanded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (rootFlow.isLocalPlayer || rootFlow.aiControlled)
        {

            if (Input.GetKeyDown(KeyCode.V))
            {
                //// Attempt reload with test weapon
                //destroyWeapon();
                //weaponTypePrefab = testReloadWeaponTypePrefab;
                //spawnWeapon();
            }


            if (readyToFire)
            {
                if (launchCommanded)
                {
                    if (countDownDelayTimer())
                    {
                        launch();
                    }

                }

            }
            else // not ready to fire -- try to reload, only count down reload if stock claimed
            {

                // only do this block if stock successfully claimed
                if(tryClaimStock())
                {

                    if (currentReloadTimer > 0)
                    {
                        currentReloadTimer -= Time.deltaTime;
                    }
                    else // reload timer runs out, reload
                    {

                        reloadProcess(); // if pod type, may be called repeatedly until reload complete

                    }
                }

            }
        }
    }

    // if a weapon from stock is claimed, return true
    // Otherwise, attempt to claim a weapon from stock
    private bool tryClaimStock()
    {
        //bool stockClaimed = this.stockClaimed;

        // This goes to hardpoint controller, indexes stock counter for this hardpoint's weapon type

        if(!stockClaimed &&  myController.checkStock(stockIndex))
        {
            stockClaimed = true;
            myController.claimStock(stockIndex);

        }

        return stockClaimed;
    }


    // Dropped munitions -- called once to spawn weapon onto hardpoint
    // Pod munitions -- called repeatedly to countdown weapon's own reload timer
    void reloadProcess()
    {
        if(loadedWeaponObj == null)
        {
            // the claimed stock is loaded onto aircraft, unclaim to prepare for next reload
            // called once to spawn reload
            stockClaimed = false;
            spawnWeapon();
        }
        else // weapon still present -- this must mean pod type, so call its reload process repeatedly until complete
        {
            Weapon weaponRef = loadedWeaponObj.GetComponent<Weapon>();
            weaponRef.reloadProcess();
            // upon reload complete, weapon talks back to this hardpoint to unclaim stock
            // ...not very good structure. Cumbersome, requires remembering to add such stock unclaim call per new pod type weapon
        }
    }

    public void claimStock(bool stockClaimed)
    {
        this.stockClaimed = stockClaimed;
    }

    void OnDestroy()
    {
        destroyWeapon();
    }


    public bool validateWeapon(Weapon weapPrefab)
    {
        // weapon weight and guidance and domain must be valid
        // OR any future weapon will be considered valid

        return (validateWeaponWeight(weapPrefab) &&
            validateWeaponGuidance(weapPrefab) &&
            validateWeaponDomain(weapPrefab)) 
            || weapPrefab.att_domain == Weapon.Domain.FUTURE;
    }


    public void equipNewWeapon(Weapon newWeapon)
    {
        destroyWeapon();
        weaponTypePrefab = newWeapon.gameObject;

        if (initialized)
        {
            spawnWeapon();
        }
        
    }

    // maybe unnecessary to make these functions, but i'll leave room for future implementation changes
    public bool validateWeaponWeight(Weapon weapPrefab)
    {
        return acceptableWeightAtt.Contains(weapPrefab.att_weight);
    }

    public bool validateWeaponGuidance(Weapon weapPrefab)
    {
        return acceptableGuidanceAtt.Contains(weapPrefab.att_guidance);
    }

    public bool validateWeaponDomain(Weapon weapPrefab)
    {
        return acceptableDomainAtt.Contains(weapPrefab.att_domain) || weapPrefab.att_domain == Weapon.Domain.MULTIROLE;
    }
}
