using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class CombatFlow : MonoBehaviourPunCallbacks
{

    public static List<CombatFlow> combatUnits;

    public string abbreviation = "DMY";

    public enum Team 
    { 
        TEAM1, TEAM2, NEUTRAL 
    }

    public enum Type
    {
        AIRCRAFT, PROJECTILE , GROUND, ANTI_AIR, SAM
    }
    
    public float maxHP;
    
    [SerializeField]
    private float currentHP;

    
    public bool isLocalPlayer;
    public bool localOwned = false;


    public float detectabilityCoeff;
    public float detectabilityOffset;

    public string radarSymbol;
    
    // inefficient -- lots of non-player combat objects will have useless perspective references
    public PerspectiveManager camManager;
    public GameObject unitCam; // leave null if item won't have its own camera

    public TgtHudIcon myHudIconRef;

    public Team team;
    public Type type;
    public bool isActive;

    private bool deathCommanded = false;

    public bool doDebugDamage = false;

    public ExplodeStats explodeStats;

    public bool networkDeath;
    public bool networkDamage = true;

    public bool networkReceivedCannonImpacts = false;

    public List<int> seenBy; // photonID's of non-friendlies that can see this unit

    public RWR rwr;

    private float seenCleanWaitMax = .75f;
    private float seenCleanWaitTimer = -1f;
    //private PhotonView photonView;

    public bool isHostInstance = false;

    private LaneManager ownerLane;

    private bool firstFramePassed = false;

    public float collisionDamageFactor = 0.005f;

    public Rigidbody myRb;


    public float maxCoverageRadius = 0f;
    public float killCoverageRadius = 0f;

    public bool jamming = false;

    public bool showInWorldList = true;

    public bool aiControlled = false;

    public static Team convertNumToTeam(short num)
    {
        if (num == 0)
        {
            return Team.TEAM1;
        }
        else
        {
            return Team.TEAM2;
        }
    }

    public static short convertTeamToNum(Team team)
    {
        if (team == Team.TEAM1)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    public void setHP(float hp)
    {
        currentHP = hp;
    }

    private void Awake()
    {
        rwr = GetComponent<RWR>();

        myRb = GetComponent<Rigidbody>();

        if (CombatFlow.combatUnits == null)
            CombatFlow.combatUnits = new List<CombatFlow>();

        if (showInWorldList)
        {
            CombatFlow.combatUnits.Add(this);
        }

        seenBy = new List<int>();
        gameObject.name = abbreviation;
    }

    

    // Start is called before the first frame update
    void Start()
    {

        isHostInstance = GameManager.getGM().isHostInstance;



        if(PhotonNetwork.PlayerList.Length == 1 || (isHostInstance && GetComponent<CreepControl>() != null))
        {
            localOwned = true;
        }

        //localOwned = isHostInstance;

        explodeStats = GetComponent<ExplodeStats>();

        // spawn icon, set reference here to the TgtHudIconScript of icon spawned
        myHudIconRef = TgtIconManager.tgtIconManager.spawnIcon(this).GetComponent<TgtHudIcon>();// add my icon to hud

        if (!isLocalPlayer)
        {
            spawnRadarIcon();
        }
    }

    private void spawnRadarIcon()
    {
        hudControl.mainHud.GetComponent<hudControl>().mapManager.spawnIcon(this);
    }

    

    void FixedUpdate()
    {
        if (currentHP <= 0 && (isLocalPlayer || localOwned || !networkDeath)) // 0hp and is currently alive
            die(); // kill self
    }

    // Update is called once per frame
    private void Update()
    {
        if (!firstFramePassed)
        {
            firstFramePassed = true;
        }

        // Debug key to test damage player
        if (Input.GetKeyDown(KeyCode.H) && isLocalPlayer)
            currentHP -= 30;

        //// Debug key to test damage all NPC's with this script
        //if (Input.GetKeyDown(KeyCode.V) && !isLocalPlayer && doDebugDamage)
        //{
        //    currentHP -= 30;
        //}

        if(seenCleanWaitTimer > 0)
        {
            seenCleanWaitTimer -= Time.deltaTime;
            if(seenCleanWaitTimer <= 0)
            {
                cleanSeenBy();
            }
        }
    }
    
    public void setNetName(string name)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("rpcSetName", RpcTarget.AllBuffered, name);
        
    }

    [PunRPC]
    public void rpcSetName(string name)
    {
        gameObject.name = name;
    }

    public void setNetTeam(short teamNum)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("rpcSetTeam", RpcTarget.AllBuffered, teamNum);
    }

    [PunRPC]
    public void rpcSetTeam(short teamNum)
    {
        team = convertNumToTeam(teamNum);
    }

    public float getHP()
    {
        return currentHP;
    }

    // any client can call this
    public void dealDamage(float damage)
    {
        if (networkDamage)
        {
            photonView.RPC("rpcDealDamage", RpcTarget.All, damage);
        }
        else
        {
            dealLocalDamage(damage);
        }
    }

    public void dealLocalDamage(float damage)
    {
        currentHP -= damage;
    }

    [PunRPC]
    private void rpcDealDamage(float damage)
    {
        this.currentHP -= damage;
    }

    public void die()
    {
        if (!deathCommanded) // avoid repeated calls by outside objects
        {
            //Debug.Log("Death commanded for " + gameObject.name);
            if (networkDeath && (isLocalPlayer || localOwned))
            {

                //photonView.RPC("explode", RpcTarget.All); // don't buffer explosion
                photonView.RPC("rpcDie", RpcTarget.AllBuffered);
            }
            else
            {
                rpcDie(); // execute locally, don't propogate
            }
        }

    }

    [PunRPC]
    private void rpcDie()
    {
        Debug.LogWarning("rpcDie commanded");
        deathCommanded = true;
        isActive = false;

        if (firstFramePassed)
        {
            explode();
        }

        destroySelf();
    }

    
    
    void explode()
    {
        if (explodeStats != null)
        {

            if (isLocalPlayer || localOwned) // only deal damage if local instance owns the object
            {
                // will deal networked damage
                explodeStats.explode(transform.position);
            }
            else
            {
                // cosmetic-only explosion that all non-owner instances will see
                explodeStats.cosmeticLocalExplode(transform.position);
            }
        }
        

    }

    void destroySelf(bool doDelete = true)
    {
        Debug.LogWarning("Destroyself called");

        

        if (showInWorldList)
        {
            removeFromDatalink(); // if not present in world's list, is likely too simple to involve datalink
            CombatFlow.combatUnits.Remove(this);
        }


        if (ownerLane != null)
        {
            ownerLane.unitDeath(this);
        }

        if (isLocalPlayer)
        {
            //Debug.LogWarning("localplayer: " + gameObject.name + " destroyed. Calling showSpawnMenu");
            GameManager.getGM().showSpawnMenu();

            PlayerInput_Aircraft input = GetComponent<PlayerInput_Aircraft>();
            input.hardpointController.destroyWeapons();

        }

        if(camManager != null)
        {
            // remove this camera from perspectiveManager's list
            //  Is this a crusty way of doing this? Might be perspectiveManager's responsibility instead?
            camManager.cameras.Remove(unitCam);
        }

        //Weapon weaponRef = GetComponent<Weapon>();
        //if(weaponRef != null)
        //{
        //    Destroy(weaponRef.effectsObj);
        //}
        
        if (myHudIconRef != null)
        {
            Destroy(myHudIconRef.gameObject);
        }

        //Destroy(gameObject);

        if (doDelete)
        {

            Weapon myWeapon = GetComponent<Weapon>();

            bool missileFound = false;
            if (networkDeath && myWeapon != null && (localOwned || isLocalPlayer))
            {


                myWeapon.destroyWeapon();
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }

    void OnDestroy()
    {
        destroySelf(false);
    }

    public bool checkSeen(int viewer)
    {
        return seenBy.Count == 1 && seenBy[0] != viewer || seenBy.Count > 1;
    }

    public void tryAddSeenBy(int id)
    {
        if (!seenBy.Contains(id))
        {
            photonView.RPC("rpcAddSeenBy", RpcTarget.AllBuffered, id);
        }
    }

    [PunRPC]
    public void rpcAddSeenBy(int id)
    {
        seenBy.Add(id);
    }

    public void tryRemoveSeenBy(int id)
    {
        if (seenBy.Contains(id))
        {
            photonView.RPC("rpcRemoveSeenBy", RpcTarget.AllBuffered, id);
        }
    }

    [PunRPC]
    public void rpcRemoveSeenBy(int id)
    {
        seenBy.Remove(id);
    }


    private void removeFromDatalink()
    {
        for(int i = 0; i < CombatFlow.combatUnits.Count; i++)
        {
            CombatFlow currentFlow = CombatFlow.combatUnits[i];
            if(currentFlow != null)
            {
                //rpcRemoveSeenBy(photonView.ViewID);
                int myID = photonView.ViewID;

                // inefficient. Should have local way to do full function locally without rpc
                if (currentFlow.seenBy.Contains(myID)) 
                {
                    currentFlow.rpcRemoveSeenBy(myID);
                }
            }
            
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        //Debug.Log("OnPlayerLeftRoom called from " + gameObject.name);

        seenCleanWaitTimer = seenCleanWaitMax;
    }

    private void cleanSeenBy()
    {
        for(int i = 0; i < seenBy.Count; i++)
        {
            if(PhotonNetwork.GetPhotonView(seenBy[i]) == null)
            {
                //Debug.LogError("Removing ID from seenBy for " + gameObject.name);
                seenBy.RemoveAt(i);
                i--; // next loop iteration should examine same index after removal
            }


        }
    }

    public void returnOwnershipToHost()
    {
        if (localOwned && !isHostInstance)
        {
            photonView.RPC("rpcCheckIfHost", RpcTarget.All);
        }
    }

    [PunRPC]
    public void rpcCheckIfHost()
    {
        localOwned = isHostInstance;
    }

    public void giveOwnership(int photonID)
    {

        photonView.RPC("rpcSetOwnership", RpcTarget.All, photonID);
    }

    [PunRPC]
    public void rpcSetOwnership(int photonID)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(photonID);

        if(view != null)
        {
            CombatFlow player = view.GetComponent<CombatFlow>();
            localOwned = player.localOwned || player.isLocalPlayer;


            // This fixes bug where client launching against host-owned npc
            if (localOwned)
            {
                RealFlightControl flightCont = GetComponent<RealFlightControl>();
                if(flightCont != null)
                {
                    flightCont.enabled = true;
                }

                BasicMissile missile = GetComponent<BasicMissile>();
                if(missile != null)
                {
                    missile.setHasPassed(true);
                }
            }
            
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (localOwned || isLocalPlayer)
        {
            //GameObject otherRoot = collision.gameObject.transform.root.gameObject;
            //int id = getVictimId(otherRoot);

            //photonView.RPC("rpcContactProcess", RpcTarget.All, transform.position, id);
            float damage = collision.impulse.magnitude * collisionDamageFactor;

            if (damage > 1f)
            {
                dealDamage(damage);
            }
        }
    }


}
