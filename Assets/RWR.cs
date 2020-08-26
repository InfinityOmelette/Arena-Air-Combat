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

    //private WarningComputer warnComputer;
    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
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
        photonView.RPC("rpcLockedBy", RpcTarget.Others, radarSource.photonView.ViewID);
    }

    public void endNetLock(Radar radarSource)
    {
        photonView.RPC("rpcEndLockedBy", RpcTarget.Others, radarSource.photonView.ViewID);
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
