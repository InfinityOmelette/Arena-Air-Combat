using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI_Aircraft))]
public class AI_TgtComputer : MonoBehaviour
{
    public int maxMissilesPerGroundUnit = 1;
    public int maxMissilesPerAircraft;

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


    //public float gndCombatRadius = 4500f;
    public float airCombatRadius = 6500f;

    


    public bool inCombat = false;


    public List<BasicMissile> outgoingMissiles;


    public List<CombatFlow> nearbyAircraft;



    void Awake()
    {
        nearbyAircraft = new List<CombatFlow>();
        outgoingMissiles = new List<BasicMissile>();
        hardpoints = GetComponent<PlayerInput_Aircraft>().hardpointController;
        myRadar = GetComponent<Radar>();
        aiEvade = GetComponent<AI_MissileEvade>();
        aiGrndAttack = GetComponent<AI_GroundAttack>();
    }

    void Start()
    {
        hardpoints.setWeaponType(0); // select the first weapon type -- by default should be AMRAAM's
        
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
        bool groundCheck = aiGrndAttack.checkGroundCombat();


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

            cleanWeaponList();
        }
        else
        {
            targetSelectCounter -= Time.deltaTime;
        }
    }

    private void cleanWeaponList()
    {
        for(int i = 0; i < outgoingMissiles.Count; i++)
        {
            
            if(outgoingMissiles[i] == null)
            {
                outgoingMissiles.RemoveAt(i);
                i--; // repeat this index next iteration
            }
        }
    }
    
    public CombatFlow findTarget(bool resetTargets = false)
    {
        CombatFlow prevTarget = activeTarget;

        if (airTarget == null || maxMissilesOnTarget(airTarget) || resetTargets)
        {
            airTarget = findAirTarget();
        }

        if (gndTarget == null || maxMissilesOnTarget(gndTarget) || resetTargets)
        {
            gndTarget = findGroundTarget();
        }


        activeTarget = decideAirOrGroundTarget(airTarget, gndTarget);

        Debug.Log("ActiveTarget: " + activeTarget + ", airTarget: " + airTarget + ", gndTarget: " + gndTarget);

        if(prevTarget != activeTarget)
        {
            rippleFireCounter = -1f; // don't bother waiting for ripple if selecting new target
            equipProperWeapon(activeTarget);
        }

        return activeTarget;
    }

    // placeholder, right now it just grabs ground target
    public CombatFlow decideAirOrGroundTarget(CombatFlow airTgt, CombatFlow gndTarget)
    {
        

        CombatFlow resultTarget = null;

        // null check --> target whichever is NOT null
        if (airTgt == null && gndTarget != null)
        {
            resultTarget = gndTarget;
        }
        else if (gndTarget == null && airTgt != null)
        {
            resultTarget = airTgt;
        }

        // if null check wasn't able to decide target, decide by isAttacked check
        if (resultTarget == null)
        {
            bool airAttacked = airTgt.rwr.incomingMissiles.Count > 0;
            bool gndAttacked = gndTarget.rwr.incomingMissiles.Count > 0;

            // Attacked check
            // if air IS attacked, and ground is NOT attacked, go for ground
            //  in all other cases, attack aircraft
            if (airAttacked && !gndAttacked)
            {
                resultTarget = gndTarget;
            }
            else
            {
                resultTarget = airTarget;
            }

            //Debug.Log("Attacking: " + resultTarget + ", airAttacked: " + airAttacked + ", gndAttacked: " + gndAttacked);
        }

        

        return resultTarget;

    }

    public bool maxMissilesOnTarget(CombatFlow target)
    {
        int maxMissiles = maxMissilesPerAircraft;

        if (target != null)
        {
            

            if (target.type != CombatFlow.Type.AIRCRAFT)
            {
                maxMissiles = maxMissilesPerGroundUnit;
            }
        }

        
        return target != null && target.rwr.incomingMissiles.Count >= maxMissiles;

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

        bool readyToFire = readyToFireAt(target);
        bool tryLock = tryLockTarget(target);
        bool hardpointReady = hardpoints.isReadyToFire();

        //Debug.Log("ReadyToFire: " + readyToFire + ", tryLock: " + tryLock + ", hardpointReady: " + hardpointReady);


        if (readyToFire && tryLock && hardpointReady)
        {
            Debug.Log("AI ready to fire");
            // do attack here
            rippleFireCounter = rippleFireDelay;
            GameObject launchedWeap = hardpoints.launchButtonDown();
            tryAddOutgoingWeap(launchedWeap);
            
        }
        else
        {
            // keep waiting until can do attack
            hardpoints.launchButtonUp();
        }

    }

    private void tryAddOutgoingWeap(GameObject weap)
    {
        if (weap != null)
        {

            BasicMissile msl = weap.GetComponent<BasicMissile>();
            if (msl != null)
            {
                outgoingMissiles.Add(msl);
            }
        }
        else
        {
            Debug.LogWarning("AI Aircraft tried to fire null weapon");
        }
    }

    private void amOffensive()
    {

        aiEvade.offensive = false;


        // offensive if there is an aircraft nearby that doesn't have incoming missiles
        // or, if there is ONLY ONE aircraft nearby, and it has max missile amount inbound
        //  , otherwise, go defensive

        // defensive if retreating (overrides previous)


        //aiEvade.offensive = activeTarget != null && activeTarget.rwr.incomingMissiles.Count < rippleFireMaxCount;




        if (aiGrndAttack.retreating)
        {
            aiEvade.offensive = false;
        }



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
        int tempMaxMissiles = maxMissilesPerAircraft;

        if(target.type != CombatFlow.Type.AIRCRAFT)
        {
            tempMaxMissiles = maxMissilesPerGroundUnit;
        }

        int missilesAgainstTarget = countMissilesAgainstTarget(target);

        //Debug.Log("maxMissiles: " + tempMaxMissiles + ", currentMissiles: " + missilesAgainstTarget);


        bool readyToFire = rippleFireCounter < 0f && missilesAgainstTarget < tempMaxMissiles;

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

    public bool hasMissilesAgainst(CombatFlow target)
    {
        return numMissilesAgainst(target) != 0;
    }

    public int numMissilesAgainst(CombatFlow target)
    {
        int missilesCount = 0;
        
        

        for(int i = 0; i < outgoingMissiles.Count; i++)
        {
            if(target.gameObject == outgoingMissiles[i].myTarget)
            {
                missilesCount++;
            }
        }

        return missilesCount;
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
