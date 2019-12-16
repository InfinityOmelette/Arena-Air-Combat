using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatFlow : MonoBehaviour
{

    public static List<GameObject> combatUnits;


    public enum Team 
    { 
        TEAM1, TEAM2, NEUTRAL 
    }

    public enum Type
    {
        AIRCRAFT, PROJECTILE
    }
    
    public float maxHP;
    public float currentHP;
    public bool isLocalPlayer;


    public float explosionRadius; // damage falls off linearly from maximum at center, to zero at radius
    public float explosionMaxDamage;
    
    // inefficient -- lots of non-player combat objects will have useless perspective references
    public PerspectiveManager camManager;
    public GameObject unitCam; // leave null if item won't have its own camera

    public GameObject myHudIconRef;

    public Team team;
    public Type type;
    public bool isAlive = true;


    public bool doDebugDamage = false;

    private ExplodeStats explodeStats;


    private void Awake()
    {
        if (CombatFlow.combatUnits == null)
            CombatFlow.combatUnits = new List<GameObject>();

        CombatFlow.combatUnits.Add(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(isLocalPlayer)
            camChange(PerspectiveProperties.CamType.PLAYER);

        explodeStats = GetComponent<ExplodeStats>();

        myHudIconRef = TgtIconManager.tgtIconManager.spawnIcon(this);// add my icon to hud
    }

    

    void FixedUpdate()
    {
        if (currentHP <= 0 && isAlive) // 0hp and is currently alive
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


    void die()
    {
        isAlive = false; // he ded now
        explode();
        if (isLocalPlayer)
            camChange(PerspectiveProperties.CamType.SPECTATOR);
        CombatFlow.combatUnits.Remove(gameObject);
        // remove my icon from hud
        destroySelf();

    }

    void camChange(PerspectiveProperties.CamType camType)
    {
        if(camManager != null)
            camManager.switchToType(camType);
    }

    void explode()
    {
        // blast radius deals damage
        // explosion render
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
        Destroy(myHudIconRef);
        Destroy(gameObject);
    }


    



}
