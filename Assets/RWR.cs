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

    //private WarningComputer warnComputer;
    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        myRb = GetComponent<Rigidbody>();
        lockedBy = new List<CombatFlow>();
        incomingMissiles = new List<CombatFlow>();
    }


    // Start is called before the first frame update
    void Start()
    {
        //warnComputer = hudControl.mainHud.GetComponent<hudControl>().warningComputer;
    }

    // Update is called once per frame
    void Update()
    {
        if (myFlow.aiControlled)
        {
            countDownCleanListTimer();
            countDownClosestMissileTimer();
        }
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

                Debug.Log("Closing Speed:" + closingSpeed);

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
        cleanFlowList(incomingMissiles);
        cleanFlowList(lockedBy);
    }

    private void cleanFlowList(List<CombatFlow> flowList)
    {
        if(flowList != null)
        {
            for(int i = 0; i < flowList.Count; i++)
            {
                if(flowList[i] == null)
                {
                    flowList.RemoveAt(i);
                    i--; // next iteration, re-check this same index
                }
            }
        }
    }
    


    public void tryPing(Radar radarSource)
    {
        if (myFlow.team != radarSource.myFlow.team)
        {

            bool isPinging = !myFlow.jamming && radarSource.radarOn && radarSource.withinScope(transform.position);
            float distance = Vector3.Distance(transform.position, radarSource.transform.position);
            float bearing = calculateBearing(radarSource.transform.position);

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

        //Debug.LogWarning("rwr bearing angle: " + bearing);

        return bearing;
    }

    public void netLockedBy(Radar radarSource)
    {
        Debug.Log("============= NETLOCKEDBY CALL");
        photonView.RPC("rpcLockedBy", RpcTarget.All, radarSource.photonView.ViewID);
    }

    public void endNetLock(Radar radarSource)
    {
        Debug.Log("============  ENDNETLOCKEDBY CALL");
        photonView.RPC("rpcEndLockedBy", RpcTarget.All, radarSource.photonView.ViewID);
    }

    [PunRPC]
    public void rpcLockedBy(int sourceID)
    {
        //lockedByIDs.Add(sourceID);

        if (myFlow.isLocalPlayer || myFlow.localOwned)
        {
            Debug.Log("rpc locked by");
            PhotonView view = PhotonNetwork.GetPhotonView(sourceID);
            if(view != null)
            {
                CombatFlow sourceFlow = view.GetComponent<CombatFlow>();

                if(sourceFlow.type == CombatFlow.Type.PROJECTILE)
                {
                    incomingMissiles.Add(sourceFlow);
                }
                else
                {
                    lockedBy.Add(sourceFlow);
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
        

        if (myFlow.isLocalPlayer || myFlow.localOwned)
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
}
