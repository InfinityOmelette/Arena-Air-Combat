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

    public HardpointController hardpoints;
    public Radar myRadar;
    public AI_MissileEvade aiEvade;
    public AI_GroundAttack aiGrndAttack;
    public AI_Aircraft aiAir;
    public CombatFlow myFlow;

    public CombatFlow activeTarget;


    public CombatFlow airTarget;
    public CombatFlow gndTarget;


    public float targetSelectDelay = .5f;
    private float targetSelectCounter;


    //public float gndCombatRadius = 4500f;
    public float airCombatRadius = 6500f;


    // prioritize ground unit if its distance is less than this factor of aircraft's distance
    public float aircraftSelectDistanceFactor = .5f;


    public bool inCombat = false;


    public List<BasicMissile> outgoingMissiles;


    public int outgoingMissilesDefensive = 2;

    //public List<CombatFlow> nearbyAircraft;

    

    public List<CombatFlow> enemyAircraft;

    public List<CombatFlow> friendlyAircraft;

    public float attackRunRadius = 3100f;
    public float longRangeFactor = 1.75f;

    void Awake()
    {
        aiAir = GetComponent<AI_Aircraft>();
        myFlow = GetComponent<CombatFlow>();
        outgoingMissiles = new List<BasicMissile>();
        hardpoints = GetComponent<PlayerInput_Aircraft>().hardpointController;
        myRadar = GetComponent<Radar>();
        aiEvade = GetComponent<AI_MissileEvade>();
        aiGrndAttack = GetComponent<AI_GroundAttack>();
    }

    void Start()
    {
        hardpoints.setWeaponType(0); // select the first weapon type -- by default should be AMRAAM's

        enemyAircraft = GameManager.getGM().getTeamAircraftList(myFlow.getEnemyTeam());
        friendlyAircraft = GameManager.getGM().getTeamAircraftList(myFlow.team);
        
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log("Current target: " + activeTarget);
        
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
    
    public bool hasLineOfSight(CombatFlow target)
    {
        int terrainLayer = 1 << 10; // line only collides with terrain layer
        bool lineOfSight = !Physics.Linecast(transform.position, target.transform.position, terrainLayer);
        return lineOfSight;
    }

    public bool airCombatCheck(GameObject aircraft)
    {
        return aircraft != null && Vector3.Distance(aircraft.transform.position, transform.position) < airCombatRadius;
    }

    public GameObject findClosestEnemyAircraft()
    {
        
        //int nearAircraftCount = nearAircraftCount();

        List<CombatFlow> nearbyAircraft = getNearbyEnemyAircraftList();

        int nearAircraftCount = nearbyAircraft.Count;

        bool allNearbyAttacked = allAircraftInListAttacked(nearbyAircraft);

        GameObject closestAircraft = null;

        int nearestIndex = -1;
        float nearestDist = 0f;
        bool firstSet = false;

        for (int i = 0; i < nearbyAircraft.Count; i++)
        {
            CombatFlow currAircraft = nearbyAircraft[i];

            if(currAircraft != null &&
                (!currAircraft.rwr.amraamsIncoming || (nearAircraftCount == 1 && !maxMissilesOnTarget(currAircraft))  || allNearbyAttacked))
            {
                float currDist = Vector3.Distance(transform.position, currAircraft.transform.position);

                if(!firstSet || currDist < nearestDist)
                {
                    firstSet = false;
                    nearestIndex = i;
                    nearestDist = currDist;
                }
            }

        }

        if(nearestIndex != -1)
        {
            closestAircraft = nearbyAircraft[nearestIndex].gameObject;
        }


        return closestAircraft;
    }

    private List<CombatFlow> getNearbyEnemyAircraftList()
    {
        List<CombatFlow> nearAircraft = new List<CombatFlow>();

        for(int i = 0; i < enemyAircraft.Count; i++)
        {
            CombatFlow aircraft = enemyAircraft[i];

            if(aircraft != null)
            {
                float dist = Vector3.Distance(aircraft.transform.position, transform.position);

                if (dist < airCombatRadius && hasLineOfSight(aircraft))
                {
                    nearAircraft.Add(aircraft);
                }
            }
        }

        return nearAircraft;
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

        //if (airTarget == null || maxMissilesOnTarget(airTarget) || !hasLineOfSight(airTarget) || resetTargets)
        //{
            
        //}

        airTarget = findAirTarget();

        if (gndTarget == null || maxMissilesOnTarget(gndTarget) || resetTargets)
        {
            gndTarget = findGroundTarget();
        }


        activeTarget = decideAirOrGroundTarget(airTarget, gndTarget);

        //Debug.Log("ActiveTarget: " + activeTarget + ", airTarget: " + airTarget + ", gndTarget: " + gndTarget);

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
            bool airAttacked = airTgt != null && airTgt.rwr.amraamsIncoming;
            bool gndAttacked = tooManyGroundMissiles();

            float airDistance = Vector3.Distance(airTgt.transform.position, transform.position);
            float gndDistance = Vector3.Distance(gndTarget.transform.position, transform.position);

            bool gndClose = gndDistance < airDistance * aircraftSelectDistanceFactor;

            // Attacked check
            // if air IS attacked, and ground is NOT attacked, go for ground
            // or, if ground unit is much closer than the air target, go for ground
            //  in all other cases, attack aircraft
            if (((airAttacked && !gndAttacked) || gndClose) && gndDistance < attackRunRadius * longRangeFactor)
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

    
    public CombatFlow findAirTarget()
    {
        GameObject target = findClosestEnemyAircraft();
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

            Debug.Log("======================== Launching weapon: " + launchedWeap);
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

    public int nearbyAircraftCount()
    {
        int count = 0;

        for(int i = 0; i < enemyAircraft.Count; i++)
        {
            CombatFlow currAircraft = enemyAircraft[i];

            if(currAircraft != null && hasLineOfSight(currAircraft))
            {
                float dist = Vector3.Distance(transform.position, currAircraft.transform.position);

                if (dist < airCombatRadius)
                {
                    count++;
                }

            }

        }

        return count;
    }

    public bool allAircraftInListAttacked(List<CombatFlow> aircraftList)
    {
        bool allAttacked = true;

        for(int i = 0; i < aircraftList.Count && allAttacked; i++)
        {
            CombatFlow currAircraft = enemyAircraft[i];

            if(currAircraft != null)
            {
                allAttacked = currAircraft.rwr.amraamsIncoming;
            }

        }

        return allAttacked;
    }

    private void amOffensive()
    {




        bool setOffensive = !aiGrndAttack.retreating && hardpoints.isReadyToFire() && !(tooManyGroundMissiles() && activeTarget == gndTarget);

        //aiEvade.offensive = false;

        // offensive if there is an aircraft nearby that doesn't have incoming missiles
        // or, if there is ONLY ONE aircraft nearby, and it has max missile amount inbound
        //  , otherwise, go defensive

        // don't bother processing any further if loaded weapon isn't ready to fire
        if (setOffensive)
        {

            if (activeTarget == airTarget)
            {

                int nearbyAircraft = nearbyAircraftCount();

                if (nearbyAircraft == 1)
                {
                    setOffensive = maxMissilesOnTarget(activeTarget);
                    Debug.Log("Only one nearby aircraft detected. maxMissilesOnTarget: " + setOffensive);
                }
                else if (nearbyAircraft > 1)
                {
                    setOffensive = allAircraftInListAttacked(getNearbyEnemyAircraftList() );
                    Debug.Log("Detecting multiple aircraft. AllNearbyAircraftAttacked: " + setOffensive);
                }
                else
                {
                    setOffensive = false;
                }
            }
            else // current target is ground unit
            {
                // defensive if ground target has incoming missile
                setOffensive = activeTarget.rwr.incomingMissiles.Count == 0;
            }
        }
        
        // defensive if retreating (overrides previous)
        if (aiGrndAttack.retreating)
        {
            setOffensive = false;
        }

        aiEvade.offensive = setOffensive;

    }


    // Air to air:
    //   - Use Phoenix if available
    //   - Use AMRAAM if available
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

        if (goodLock && myRadar.lockType == Radar.LockType.AIR_ONLY)
        {
            float dist = Vector3.Distance(target.transform.position, transform.position);
            goodLock = dist < myRadar.effectiveLongRange;

            Debug.Log("Goodlock, target is " + dist + " away, max range is " + myRadar.effectiveLongRange);
        }

        //Debug.Log("Goodlock: " + goodLock);

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
        //bool inEnemyKillZone = aiGrndAttack.inEnemyKillZone( gndTarget );

        bool isRetreating = aiGrndAttack.checkIsRetreating( gndTarget );


        

        Vector3 targetPosLevel = activeTarget.transform.position;
        targetPosLevel.y = transform.position.y;

        bool doAttackRun = false;

        Vector3 targetPos;

        if (isRetreating)
        {
            if (aiGrndAttack.attackDebugGroup)
            {
                targetPos = aiGrndAttack.debugRetreatLeader.transform.position;
            }
            else
            {
                targetPos = aiGrndAttack.myLane.getLeaderPos();
            }


            targetPos.y = transform.position.y;
        }
        else
        {
            float dist = Vector3.Distance(transform.position, activeTarget.transform.position);

            if (dist < attackRunRadius)
            {

                doAttackRun = activeTarget == gndTarget;

                targetPos = activeTarget.transform.position;
            }
            else
            {
                targetPos = targetPosLevel;
            }
        }

        aiAir.lowQualityActive = doAttackRun;

        return targetPos;
    }

    public bool tooManyGroundMissiles()
    {
        return countNumGroundMissiles() >= outgoingMissilesDefensive;
    }

    int countNumGroundMissiles()
    {
        int count = 0;

        for(int i = 0; i < outgoingMissiles.Count; i++)
        {
            if(outgoingMissiles[i] != null && outgoingMissiles[i].radar != null &&
                outgoingMissiles[i].radar.lockType == Radar.LockType.GROUND_ONLY)
            {
                count++;
            }
        }

        return count;
    }

}
