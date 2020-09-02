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

    public CombatFlow target;

    

    
    void Awake()
    {
        hardpoints = GetComponent<PlayerInput_Aircraft>().hardpointController;
        myRadar = GetComponent<Radar>();
        aiEvade = GetComponent<AI_MissileEvade>();
    }

    void Start()
    {
        hardpoints.setWeaponType(0); // select the first weapon type -- by default should be AMRAAM's
    }

    // Update is called once per frame
    void Update()
    {
        countDownRippleFire();
    }

    // placeholder, right now it just grabs local player
    public CombatFlow findTarget()
    {
        GameObject target = GameManager.getGM().localPlayer;
        CombatFlow targetFlow = null;

        if(target != null)
        {
            rippleFireCounter = 0f;
            targetFlow = target.GetComponent<CombatFlow>();

            checkTypeAgainst(targetFlow);
        }

        this.target = targetFlow;

        return targetFlow;
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
        aiEvade.offensive = target != null && target.rwr.incomingMissiles.Count < rippleFireMaxCount;
    }


    private bool checkTypeAgainst(CombatFlow target)
    {
        Radar.LockType reqType = Radar.LockType.AIR_OR_GROUND;

        if(target.type == CombatFlow.Type.AIRCRAFT)
        {
            reqType = Radar.LockType.AIR_ONLY;
        }
        else if(target.type == CombatFlow.Type.SAM || target.type == CombatFlow.Type.ANTI_AIR)
        {
            reqType = Radar.LockType.GROUND_ONLY;
        }

        bool typeFound = hardpoints.selectByLockType(reqType);

        Debug.Log("LockType is: " + reqType + ", so I've selected: " + hardpoints.getActiveHardpoint().weaponTypePrefab.name);

        // aircraft and sams/AAA are the only ones that require specific lock type
        // in other cases, I'll have to make AI select a specific weapon instead of just by type

        return typeFound;
    }

    
    private bool readyToFireAt(CombatFlow target)
    {
        bool readyToFire = rippleFireCounter < 0f && countMissilesAgainstTarget(target) < rippleFireMaxCount;

        Debug.Log("ReadydToFire: " + readyToFire);

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
        // use target's rwr to find missiles launched by this aircraft
        return target.rwr.incomingMissiles.Count;
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


}
