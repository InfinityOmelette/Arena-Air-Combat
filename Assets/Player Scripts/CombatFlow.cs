using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class CombatFlow : MonoBehaviourPunCallbacks
{

    public static List<GameObject> combatUnits;


    public enum Team 
    { 
        TEAM1, TEAM2, NEUTRAL 
    }

    public enum Type
    {
        AIRCRAFT, PROJECTILE , GROUND
    }
    
    public float maxHP;
    public float currentHP;

    
    public bool isLocalPlayer;
    public bool localOwned = false;


    public float detectabilityCoeff;


    
    // inefficient -- lots of non-player combat objects will have useless perspective references
    public PerspectiveManager camManager;
    public GameObject unitCam; // leave null if item won't have its own camera

    public TgtHudIcon myHudIconRef;

    public Team team;
    public Type type;
    public bool isActive = true;

    private bool deathCommanded = false;

    public bool doDebugDamage = false;

    public ExplodeStats explodeStats;

    public bool networkDeath;

    //private PhotonView photonView;

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


    private void Awake()
    {
        if (CombatFlow.combatUnits == null)
            CombatFlow.combatUnits = new List<GameObject>();

        CombatFlow.combatUnits.Add(gameObject);


    }

    // Start is called before the first frame update
    void Start()
    {

        explodeStats = GetComponent<ExplodeStats>();

        // spawn icon, set reference here to the TgtHudIconScript of icon spawned
        myHudIconRef = TgtIconManager.tgtIconManager.spawnIcon(this).GetComponent<TgtHudIcon>();// add my icon to hud
    }

    

    void FixedUpdate()
    {
        if (currentHP <= 0 && (isLocalPlayer || localOwned || !networkDeath)) // 0hp and is currently alive
            die(); // kill self
    }

    // Update is called once per frame
    private void Update()
    {
        // Debug key to test damage player
        if (Input.GetKeyDown(KeyCode.C) && isLocalPlayer)
            currentHP -= 3;

        // Debug key to test damage all NPC's with this script
        if (Input.GetKeyDown(KeyCode.V) && !isLocalPlayer && doDebugDamage)
        {
            currentHP -= 30;
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

    public void die()
    {
        if (!deathCommanded) // avoid repeated calls by outside objects
        {
            //Debug.Log("Death commanded for " + gameObject.name);
            if (networkDeath && (isLocalPlayer || localOwned))
            {
                photonView.RPC("rpcDie", RpcTarget.All);
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
        deathCommanded = true;
        isActive = false;
        explode();
        destroySelf();
    }

    void explode()
    {
        // blast radius deals damage
        // explosion render
        //Debug.Log("----------------------------- Combat flow explode() called by " + gameObject.name);
        //Debug.Log("Explode called for " + gameObject.name);


        explodeStats.explode(transform.position);
    }

    void destroySelf()
    {
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
        CombatFlow.combatUnits.Remove(gameObject);
        Destroy(myHudIconRef.gameObject);
        Destroy(gameObject);
    }


    



}
