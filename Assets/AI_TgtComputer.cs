using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_TgtComputer : MonoBehaviour
{
    public int rippleFireMaxCount;

    public float rippleFireDelay;
    private float rippleFireCounter;

    private HardpointController hardpoints;
    private Radar myRadar;
    private AI_MissileEvade aiEvade;
    private AI_GroundAttack aiGrndAttack;

    public CombatFlow activeTarget;


    public CombatFlow airTarget;
    public CombatFlow gndTarget;


    public float targetSelectDelay = .5f;
    private float targetSelectCounter;


    public float gndCombatRadius = 4500f;
    public float airCombatRadius = 6500f;

    public GameObject debugLeaderRef;

    public bool inCombat = false;


    void Awake()
    {
        hardpoints = GetComponent<PlayerInput_Aircraft>().hardpointController;
        myRadar = GetComponent<Radar>();
        aiEvade = GetComponent<AI_MissileEvade>();
        aiGrndAttack = GetComponent<AI_GroundAttack>();
    }

    void Start()
    {
        hardpoints.setWeaponType(0); // select the first weapon type -- by default should be AMRAAM's
        debugLeaderRef = GameManager.getGM().debugLeader;
    }

    // Update is called once per frame
    void Update()
    {
        
        countDownTargetSelect();
        
        countDownRippleFire();
    }

    public bool checkCombatRadius()
    {
        // distance to enemy leader
        // call corresponding LaneManager's getLeader() function
        bool groundCheck = Vector3.Distance(transform.position, debugLeaderRef.transform.position) < gndCombatRadius;


        // distance to enemy player aircraft
        bool airCheck = airCombatCheck(findClosestEnemyAircraft());

        return groundCheck || airCheck;
    }

    public bool airCombatCheck(GameObject aircraft)
    {
        // DEBUG: this will always be false. I'm just checking air-to-ground capabilities
        return aircraft != null && Vector3.Distance(aircraft.transform.position, transform.position) < airCombatRadius && false;
    }

    public GameObject findClosestEnemyAircraft()
    {
        return GameManager.getGM().localPlayer;
    }

    public void countDownTargetSelect()
    {
        if(targetSelectCounter < 0f)
        {
            targetSelectCounter = targetSelectDelay;
            inCombat = checkCombatRadius();

            if (inCombat)
            {
                findTarget();
            }
        }
        else
        {
            targetSelectCounter -= Time.deltaTime;
        }
    }
    
    public CombatFlow findTarget(bool resetTargets = false)
    {
        CombatFlow prevTarget = activeTarget;

        if (airTarget == null || resetTargets)
        {
            airTarget = findAirTarget();
        }

        if (gndTarget == null || resetTargets)
        {
            gndTarget = findGroundTarget();
        }


        activeTarget = decideAirOrGroundTarget(airTarget, gndTarget);

        if(prevTarget != activeTarget)
        {
            rippleFireCounter = 0f; // don't bother waiting for ripple if selecting new target
            equipProperWeapon(activeTarget);
        }

        return activeTarget;
    }

    // placeholder, right now it just grabs ground target
    public CombatFlow decideAirOrGroundTarget(CombatFlow airTgt, CombatFlow gndTarget)
    {
        // allows for "multitasking", between fighting aircraft and attacking ground

        // Attack aircraft initially
        //  if aircraft has incoming attack, and ground target none, attack ground target
        //  if both aircraft and ground target have incoming attacks, attack aircraft
        
        return gndTarget;

    }

    // placeholder, right now it just grabs local player
    public CombatFlow findAirTarget()
    {
        GameObject target = GameManager.getGM().localPlayer;
        CombatFlow targetFlow = null;

        if(target != null)
        {
            targetFlow = target.GetComponent<CombatFlow>();
        }

        return targetFlow;
    }

    public CombatFlow findGroundTarget()
    {
        return aiGrndAttack.findGroundTarget();
    }

    public void attack(CombatFlow target)
    {

        amOffensive();

        if (readyToFireAt(target) && tryLockTarget(target) && hardpoints.isReadyToFire())
        {
            Debug.Log("AI ready to fire");
            // do attack here
            rippleFireCounter = rippleFireDelay;
            hardpoints.launchButtonDown();
        }
        else
        {
            // keep waiting until can do attack
            hardpoints.launchButtonUp();
        }

        


    }

    private void amOffensive()
    {
        aiEvade.offensive = false;

        //aiEvade.offensive = activeTarget != null && activeTarget.rwr.incomingMissiles.Count < rippleFireMaxCount;
    }


    // Air to air use amraams
    // ground: use mavericks if available
    //   - if not, use rockets if available
    //   - if not, use bombs if available
    //   - if not, return false
    private bool equipProperWeapon(CombatFlow target)
    {
        Radar.LockType reqType = Radar.LockType.AIR_OR_GROUND;

        if(target.type == CombatFlow.Type.AIRCRAFT)
        {
            reqType = Radar.LockType.AIR_ONLY;
        }
        //else if(target.type == CombatFlow.Type.SAM || target.type == CombatFlow.Type.ANTI_AIR)
        else
        {
            reqType = Radar.LockType.GROUND_ONLY;
        }

        bool typeFound = hardpoints.selectByLockType(reqType);

        // Debug.Log("LockType is: " + reqType + ", so I've selected: " + hardpoints.getActiveHardpoint().weaponTypePrefab.name);

        // aircraft and sams/AAA are the only ones that require specific lock type
        // in other cases, I'll have to make AI select a specific weapon instead of just by type

        return typeFound;
    }

    
    private bool readyToFireAt(CombatFlow target)
    {
        bool readyToFire = rippleFireCounter < 0f && countMissilesAgainstTarget(target) < rippleFireMaxCount;

        //Debug.Log("ReadydToFire: " + readyToFire);

        return readyToFire;
    }

    private void countDownRippleFire()
    {
        if(rippleFireCounter >= 0f)
        {
            rippleFireCounter -= Time.deltaTime;
        }
    }

    private int countMissilesAgainstTarget(CombatFlow target)
    {
        RWR targetRWR = target.rwr;

        int missiles = 0;

        if(targetRWR != null)
        {
            missiles = targetRWR.incomingMissiles.Count;
        }

        // use target's rwr to find missiles launched by this aircraft
        return missiles;
    }

    private bool tryLockTarget(CombatFlow target)
    {
        bool goodLock = myRadar.tryLock(target, true);

        if (goodLock)
        {
            float dist = Vector3.Distance(target.transform.position, transform.position);
            goodLock = dist < myRadar.effectiveLongRange;

            Debug.Log("Goodlock, target is " + dist + " away, max range is " + myRadar.effectiveLongRange);
        }

        Debug.Log("Goodlock: " + goodLock);

        return goodLock;
    }

    public Vector3 getAttackPos()
    {
        
        if(activeTarget.type == CombatFlow.Type.AIRCRAFT)
        {
            return activeTarget.transform.position;
        }
        else
        {
            return aiGrndAttack.calculateAttackPos(activeTarget);
        }
        
    }

}
