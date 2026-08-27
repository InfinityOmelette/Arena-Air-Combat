using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RWR : MonoBehaviourPunCallbacks
{
    private CombatFlow myFlow;

    public Transform rwrBearingAxis;

    public List<CombatFlow> lockedBy;
    public List<CombatFlow> incomingMissiles;


    public CombatFlow highestThreatMissile;
    private Rigidbody myRb;



    public float closestMissileDelay = 1f;
    private float closestMissileTimer;

    public float cleanListsDelay = 3f;
    private float cleanListsTimer;

    public float minImpactTime; // ignore missile if greater than this time away

    public bool amraamsIncoming = false;

    public float closingVelocityThreatThreshold = 50f;

    public WarningComputer warningComputer;

    //private WarningComputer warnComputer;
    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        myRb = GetComponent<Rigidbody>();
        lockedBy = new List<CombatFlow>();
        incomingMissiles = new List<CombatFlow>();

        
    }

    public void linkWarningComputer()
    {
        warningComputer = hudControl.mainHud.GetComponent<hudControl>().warningComputer;
        warningComputer.resetLists();
    }

    // Start is called before the first frame update
    void Start()
    {
        //warnComputer = hudControl.mainHud.GetComponent<hudControl>().warningComputer;
    }

    // Update is called once per frame
    void Update()
    {
        if (myFlow.aiControlled || myFlow.isLocalPlayer || myFlow.localOwned)
        {
            countDownClosestMissileTimer();
        }
        countDownCleanListTimer();
    }

    private void countDownClosestMissileTimer()
    {
        if(closestMissileTimer < 0f)
        {
            findHighestThreatMissile();
            closestMissileTimer = closestMissileDelay;
        }
        else
        {
            closestMissileTimer -= Time.deltaTime;
        }

    }

    private void findHighestThreatMissile()
    {
        highestThreatMissile = null;

        float lowestImpactTime = minImpactTime;
        int threatMissileIndex = -1;


        for(int i = 0; i < incomingMissiles.Count; i++)
        {
            CombatFlow currMissile = incomingMissiles[i];

            if(currMissile != null)
            {
                float currDist = Vector3.Distance(currMissile.transform.position, transform.position);

                float closingSpeed = calculateClosingSpeed(currMissile); // positive indicates closure, negative --> separation

                //  Debug.Log("Closing Speed:" + closingSpeed);

                float currImpactTime = currDist / closingSpeed;


                if(currImpactTime < lowestImpactTime && currImpactTime > 0f)
                {
                    lowestImpactTime = currImpactTime;
                    threatMissileIndex = i;
                }


            }
        }

        if(threatMissileIndex != -1)
        {
            highestThreatMissile = incomingMissiles[threatMissileIndex];
        }

    }

    private void countDownCleanListTimer()
    {
        if (cleanListsTimer < 0f)
        {
            amraamsIncoming = checkAmraamsIncoming();
            cleanLists();
            cleanListsTimer = cleanListsDelay;
        }
        else
        {
            cleanListsTimer -= Time.deltaTime;
        }
    }

    private void cleanLists()
    {
        //cleanLockedList();
        cleanFlowList(incomingMissiles, true);
        cleanFlowList(lockedBy);
    }

    private void cleanLockedList()
    {
        for(int i = 0; i < lockedBy.Count; i++)
        {

        }
    }

    private bool checkAmraamsIncoming()
    {
        bool amraamsIncoming = false;

        for(int i = 0; i < incomingMissiles.Count && !amraamsIncoming; i++)
        {
            if(incomingMissiles[i] != null)
            {
                amraamsIncoming = !incomingMissiles[i].myRadar.isSam;
            }
        }

        return amraamsIncoming;
    }

    private void cleanFlowList(List<CombatFlow> flowList, bool useMissileConditions = false)
    {
        if(flowList != null)
        {
            for(int i = 0; i < flowList.Count; i++)
            {
                if(flowList[i] == null || flowList[i].team == myFlow.team
                    || (useMissileConditions && !missileIsAThreat(flowList[i])))
                {
                    if(flowList[i].myRadar != null)
                    {
                        flowList[i].myRadar.rwrIcon.endLock();
                    }
                    flowList.RemoveAt(i);
                    i--; // next iteration, re-check this same index
                }
            }
        }
    }

    // must not pass in null
    // must pass in guided missile
    // must be a missile targeting this RWR
    // Returns true if missile is a threat to player
    //  --> lockedBy list includes player
    //  --> OR missile closing velocity high
    private bool missileIsAThreat(CombatFlow missile)
    {

        MissileGuidance guidance = missile.mslGuidance;
        bool missileIsLocking = guidance != null && guidance.isLocked;

        Vector3 relativeVelocity = myRb.velocity - missile.myRb.velocity;
        Vector3 bearingLine = missile.transform.position - transform.position;
        
        // POSITIVE number if closing
        float closingSpeed = 
            Mathf.Sign(Vector3.Dot(relativeVelocity, bearingLine)) * relativeVelocity.magnitude;

        return missileIsLocking || closingSpeed > closingVelocityThreatThreshold;
    }
    
    public bool isAttacked()
    {
        return incomingMissiles.Count > 0;
    }

    public void tryPing(Radar radarSource)
    {
        if (myFlow.isLocalPlayer && myFlow.team != radarSource.myFlow.team)
        {

            bool isPinging = false;
            float distance = 0.0f;
            float bearing = 0.0f;

            if (myFlow.team != radarSource.myFlow.team)
            {
                isPinging = !myFlow.jamming && radarSource.radarOn && radarSource.withinScope(transform.position);
                distance = Vector3.Distance(transform.position, radarSource.transform.position);
                bearing = calculateBearing(radarSource.transform.position);
            }
            

            IconRWR rwrIcon = radarSource.rwrIcon;

            rwrIcon.showPingResult(isPinging, distance, bearing);
        }
        
    }

    private float calculateClosingSpeed(CombatFlow msl)
    {
        Vector3 mslRelVel = msl.myRb.velocity - myRb.velocity;

        // direction, from player to missile
        Vector3 targetBearingLine = msl.transform.position - transform.position;

        Vector3 goodVel = Vector3.Project(mslRelVel, targetBearingLine);

        // positive if CLOSING --> goodVel facing towards player

        // negative if SEPARATING --> goodvel facing away from player

        float sign = 1.0f;

        // 90 degrees arbitrarily selected --> vectors are facing away from each other
        if (Vector3.Angle(targetBearingLine, goodVel) < 90f)
        {
            sign *= -1.0f;
        }

        return goodVel.magnitude * sign;
    }

    private float calculateBearing(Vector3 position)
    {
     
        position = rwrBearingAxis.InverseTransformPoint(position);
        position = new Vector3(position.x, 0f, position.z); // put onto xz plane

        float bearing = Vector3.Angle(Vector3.forward, position);
        if (position.x > 0)
        {
            bearing *= -1;
        }

        return bearing;
    }

    public void nonNetLock(Radar radarSource)
    {
        if (!lockedBy.Contains(radarSource.myFlow))
        {
            lockedBy.Add(radarSource.myFlow);
        }
    }

    public void nonNetEndLock(Radar radarSource)
    {
        while (lockedBy.Contains(radarSource.myFlow))
        {
            lockedBy.Remove(radarSource.myFlow);
            //Debug.LogError("Successful removal of locked radar source");
        }
    }

    public void netLockedBy(Radar radarSource)
    {
        Debug.Log("============= NETLOCKEDBY CALL");
        if (!lockedBy.Contains(radarSource.myFlow))
        {
            photonView.RPC("rpcLockedBy", RpcTarget.All, radarSource.photonView.ViewID);
        }
        
    }

    public void endNetLock(Radar radarSource)
    {
        Debug.Log("============  ENDNETLOCKEDBY CALL");

        if (lockedBy.Contains(radarSource.myFlow))
        {
            photonView.RPC("rpcEndLockedBy", RpcTarget.All, radarSource.photonView.ViewID);
        }
        
    }

    void tryAddListElement(CombatFlow flowAdd, List<CombatFlow> list)
    {
        if (!list.Contains(flowAdd))
        {
            list.Add(flowAdd);
        }
    }

    [PunRPC]
    public void rpcLockedBy(int sourceID)
    {
        //lockedByIDs.Add(sourceID);

        if (myFlow.isLocalPlayer || myFlow.localOwned || myFlow.aiControlled)
        {
            Debug.Log("rpc locked by");
            PhotonView view = PhotonNetwork.GetPhotonView(sourceID);
            if(view != null)
            {
                CombatFlow sourceFlow = view.GetComponent<CombatFlow>();

                if(sourceFlow.type == CombatFlow.Type.PROJECTILE)
                {
                    //incomingMissiles.Add(sourceFlow);
                    tryAddListElement(sourceFlow, incomingMissiles);
                }
                else
                {
                    //lockedBy.Add(sourceFlow);
                    tryAddListElement(sourceFlow, lockedBy);
                }

                if (myFlow.isLocalPlayer)
                {
                    Radar radarSource = view.GetComponent<Radar>();
                    radarSource.rwrIcon.beginLock();
                }
            }
        }
    }

    [PunRPC]
    public void rpcEndLockedBy(int sourceID)
    {
        
        if (myFlow.isLocalPlayer || myFlow.localOwned || myFlow.aiControlled)
        {
            Debug.Log("rpc end locked by");
            PhotonView view = PhotonNetwork.GetPhotonView(sourceID);
            if (view != null)
            {
                CombatFlow sourceFlow = view.GetComponent<CombatFlow>();

                if (sourceFlow.type == CombatFlow.Type.PROJECTILE)
                {
                    incomingMissiles.Remove(sourceFlow);

                    if(sourceFlow == highestThreatMissile)
                    {
                        highestThreatMissile = null;
                    }

                }
                else
                {
                    lockedBy.Remove(sourceFlow);
                }

                if (myFlow.isLocalPlayer)
                {
                    Radar radarSource = view.GetComponent<Radar>();
                    radarSource.rwrIcon.endLock();
                }
            }
        }
    }

    public float closestLocker()
    {
        float closestDist = 1000000f; // arbitrarily large value

        for(int i = 0; i < lockedBy.Count; i++)
        {
            if(lockedBy[i] != null)
            {
                float dist = Vector3.Distance(transform.position, lockedBy[i].transform.position);

                if(dist < closestDist)
                {
                    closestDist = dist;
                }
            }
            
        }

        return closestDist;
    }

    private void OnDestroy()
    {
        if(warningComputer != null)
        {
            warningComputer.resetLists();
        }
    }
}
