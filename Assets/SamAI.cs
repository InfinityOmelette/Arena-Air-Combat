using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamAI : MonoBehaviour
{

    public CombatFlow currentTarget;


    public float fireRateDelay;
    private float fireRateTimer;

    private CombatFlow rootFlow;
    private Radar radar;

    public float changeCycleCounterMax;
    private float changeCycleCounter;   

    public float maxTargetRange;
    public float maxLaunchRange;

    public Transform launcherAxis;

    private SamNetworking samNet;

    public Transform missileSpawnCenter;

    public GameObject missilePrefab;

    public float acquireTimeMax;
    private float acquireTimer;

    private bool locked = false;

    public bool active = true;

    public int maxTargetSaturation_Projectile = 1;
    public int maxTargetSaturation_Aircraft = 3;

    public float closerOversaturateMargin = 3000f;

    public UnitAlertness alertness;


    public bool triggerAlertness = false;
    public bool bypassAlertness = false;


    // Start is called before the first frame update
    void Start()
    {
        rootFlow = transform.root.GetComponent<CombatFlow>();
        radar = rootFlow.GetComponent<Radar>();
        samNet = rootFlow.GetComponent<SamNetworking>();
        alertness = rootFlow.GetComponent<UnitAlertness>();

    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            // try to change to new target
            tryChangeTarget();

            if (alertness.checkAlertStatus() || bypassAlertness)
            {
                // try to perform launch
                tryLaunch();
            }

            if (triggerAlertness)
            {
                alertness.beginChangingAlertStatus(currentTarget != null, getTargetRb());
            }
            

            //tryLowerGuard(Time.deltaTime);

            if (currentTarget != null)
            {

                launcherAxis.LookAt(currentTarget.transform.position, rootFlow.transform.up);
            }
        }

        

    }

    private Rigidbody getTargetRb()
    {
        if(currentTarget == null)
        {
            return null;
        }
        return currentTarget.myRb;
    }

    private void tryLaunch()
    {
        //Debug.LogWarning("SAM reload timer: " + fireRateTimer);

        if (rootFlow.isHostInstance)
        {
            if (fireRateTimer >= 0)
            {
                fireRateTimer -= Time.deltaTime;
            }
            else // ready to fire
            {
                
                // fire as soon as target acquired
                if(currentTarget != null)
                {
                    if(locked)
                    {
                        if (Vector3.Distance(currentTarget.transform.position, transform.position) < maxLaunchRange)
                        {
                            // do fire
                            // reset timer
                            samNet.launchMissile(currentTarget, this);
                            //Debug.LogError("Firing SAM at " + currentTarget.name);
                            fireRateTimer = fireRateDelay;
                        }
                    }
                    else
                    {
                        acquireCountdown();
                    }

                }
            }
            
        }
    }

    private bool acquireCountdown()
    {

        if(acquireTimer >= 0)
        {
            acquireTimer -= Time.deltaTime;
        }
        else
        {
            locked = true;
        }

        return acquireTimer < 0;
    }

    private void tryChangeTarget()
    {
        if (rootFlow.isHostInstance)
        {

            changeCycleCounter -= Time.deltaTime;
            if (changeCycleCounter < 0)
            {
                changeCycleCounter = changeCycleCounterMax;

                CombatFlow targetFlow = findNearestTarget();

                if (targetFlow != this.currentTarget)
                {

                    ////turretNet.setTarget(targetFlow);
                    //if(currentTarget != null)
                    //{
                    //    this.currentTarget.tryEndLock(radar);

                    //    //if(currentTarget.type == CombatFlow.Type.PROJECTILE)
                    //    //{
                    //    //    Debug.LogError("Ending lock against: " + currentTarget.name + 
                    //    //        ", assigning to new target: " + targetFlow.name);
                    //    //}
                    //}

                    //if(targetFlow != null)
                    //{
                    //    targetFlow.tryBeginLock(radar);
                    //}


                    //setTarget(targetFlow);
                    samNet.setTarget(targetFlow, this);

                    //setTarget(targetFlow);

                    acquireTimer = acquireTimeMax;
                    locked = false;

                    // only target's instance will deal damage. Rest will be cosmetic-only
                    //rootFlow.giveOwnership(targetFlow.photonView.ViewID);
                }

            }
        }
    }

    private bool aircraftSaturationCheck(CombatFlow target)
    {
        return target.rwr != null && target.rwr.incomingMissiles.Count < maxTargetSaturation_Aircraft;
    }

    // returns true if target is NOT saturated with locks
    private bool projectileSaturationCheck(CombatFlow target)
    {
        //if(target == currentTarget)
        //{
        //    return true;
        //}
        return (target.rwr != null && (target.rwr.lockedBy.Count < maxTargetSaturation_Projectile
            || target.rwr.lockedBy.Contains(radar.myFlow)) || iAmClosestByWideMargin(target.rwr));
    }

    private bool iAmClosestByWideMargin(RWR targetRWR)
    {
        float myDist = Vector3.Distance(transform.position, targetRWR.transform.position);

        return myDist + closerOversaturateMargin < targetRWR.closestLocker();
    }

    private CombatFlow findNearestTarget()
    {
        CombatFlow closestTarget = null;

        // don't bother targeting someone outside of schutDistance
        float shortestDist = maxTargetRange;

        List<CombatFlow> allUnits = CombatFlow.combatUnits;

        for (int i = 0; i < allUnits.Count; i++)
        {
            CombatFlow currentFlow = allUnits[i];
            bool seeFlow = false;
            if (currentFlow != null)
            {
                if (currentFlow.team != rootFlow.team &&
                    ((currentFlow.type == CombatFlow.Type.AIRCRAFT && aircraftSaturationCheck(currentFlow)) || 
                    (radar.projectileCheck(currentFlow) && projectileSaturationCheck(currentFlow))))
                {

                    if (radar.tryDetect(currentFlow))
                    {
                        // contribute to datalink network, even if not selecting this as closest target
                        seeFlow = true;

                        

                        float currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);
                        if (currentDistance < shortestDist)
                        {
                            closestTarget = currentFlow;
                            shortestDist = currentDistance;
                        }
                    }

                    if (seeFlow)
                    {
                        currentFlow.tryAddSeenBy(rootFlow.photonView.ViewID);
                        //currentFlow.tryReceiveLock(radar);
                    }
                    else
                    {
                        currentFlow.tryRemoveSeenBy(rootFlow.photonView.ViewID);
                        //currentFlow.tryEndLock(radar);
                    }


                }
            }

            
        }



        return closestTarget;
    }


    public void setTarget(CombatFlow targetFlow)
    {

        //fuckthisbullshit();

        if (currentTarget != null)
        {
            currentTarget.tryEndLock(radar);
        }

        if (targetFlow != null)
        {
            targetFlow.tryBeginLock(radar);
        }





        currentTarget = targetFlow;
    }

    //private void fuckthisbullshit()
    //{
    //    for(int i = 0; i < CombatFlow.combatUnits.Count; i++)
    //    {
    //        CombatFlow currentFlow = CombatFlow.combatUnits[i];
    //        if(currentFlow != null)
    //        {
    //            if(currentFlow.rwr != null)
    //            {
    //                currentFlow.tryEndLock(radar);
    //            }
    //        }
    //    }
    //}

}
