using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamAI : MonoBehaviour
{

    CombatFlow currentTarget;


    public float fireRateDelay;
    private float fireRateTimer;

    private CombatFlow rootFlow;
    private Radar radar;

    public float changeCycleCounterMax;
    private float changeCycleCounter;   

    public float maxTargetRange;

    public Transform launcherAxis;

    private SamNetworking samNet;

    public Transform missileSpawnCenter;

    public GameObject missilePrefab;

    public float acquireTimeMax;
    private float acquireTimer;

    // Start is called before the first frame update
    void Start()
    {
        rootFlow = transform.root.GetComponent<CombatFlow>();
        radar = rootFlow.GetComponent<Radar>();
        samNet = rootFlow.GetComponent<SamNetworking>();

    }

    // Update is called once per frame
    void Update()
    {

        // try to change to new target
        tryChangeTarget();

        // try to perform launch
        tryLaunch();


        if (currentTarget != null)
        {
            
            launcherAxis.LookAt(currentTarget.transform.position, rootFlow.transform.up);
        }

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
                    if (acquireCountdown())
                    {
                        // do fire
                        // reset timer
                        samNet.launchMissile(currentTarget);
                        //Debug.LogError("Firing SAM at " + currentTarget.name);
                        fireRateTimer = fireRateDelay;
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
                    //turretNet.setTarget(targetFlow);
                    samNet.setTarget(targetFlow);
                    acquireTimer = acquireTimeMax;

                    // only target's instance will deal damage. Rest will be cosmetic-only
                    //rootFlow.giveOwnership(targetFlow.photonView.ViewID);
                }

            }
        }
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
                if (currentFlow.team != rootFlow.team && currentFlow.type == CombatFlow.Type.AIRCRAFT)
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
                    }
                    else
                    {
                        currentFlow.tryRemoveSeenBy(rootFlow.photonView.ViewID);
                    }


                }
            }

            
        }



        return closestTarget;
    }


    public void setTarget(CombatFlow targetFlow)
    {
        currentTarget = targetFlow;
    }

}
