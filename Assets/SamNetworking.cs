using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class SamNetworking : MonoBehaviourPunCallbacks
{

    public SamAI sam;

    public List<SamAI> sams;

    private CombatFlow myFlow;

    private Radar myRadar;

    private StrategicTarget myStrat;

    private void Awake()
    {
        if(sam != null)
        {
            if(sams == null)
            {
                sams = new List<SamAI>();
                sams.Add(sam);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        myFlow = GetComponent<CombatFlow>();
        myRadar = GetComponent<Radar>();
        myStrat = GetComponent<StrategicTarget>();
    }

    // Update is called once per frame
    void Update()
    {

        if(myStrat != null && myStrat.isSuppressed)
        {
            //Debug.Log("Sam's disabled, ending lock");
            myRadar.rwrIcon.endLock();
            myRadar.rwrIcon.isPinging = false;
            myRadar.rwrIcon.showPingResult(false, 0, 0);
        }

        if(myStrat != null)
        {
            sam.active = !myStrat.isSuppressed;
        }

    }

    public void setTarget(CombatFlow target, SamAI activeSam)
    {

        int id = -1;
        if (target != null)
        {
            //Debug.LogWarning("Setting Target: " + target.gameObject.name);
            id = target.photonView.ViewID;
        }

        int samIndex = sams.IndexOf(activeSam);

        photonView.RPC("rpcSetSamTarget", RpcTarget.All, id, samIndex);
    }

    [PunRPC]
    public void rpcSetSamTarget(int viewID, int samIndex)
    {
        if (viewID != -1)
        {
            
            PhotonView view = PhotonNetwork.GetPhotonView(viewID);

            if (view != null)
            {
                CombatFlow targetFlow = view.GetComponent<CombatFlow>();

                sams[samIndex].setTarget(targetFlow);

                Radar myRadar = GetComponent<Radar>();

                if (targetFlow.gameObject == GameManager.getGM().localPlayer)
                {
                    myRadar.rwrIcon.beginLock();
                }
                else
                {
                    myRadar.rwrIcon.endLock();
                }

            }


        }
        else
        {
            
            if(sams[samIndex].currentTarget.gameObject == GameManager.getGM().localPlayer)
            {
                Radar myRadar = GetComponent<Radar>();
                myRadar.rwrIcon.endLock();
            }

            sams[samIndex].setTarget(null);
        }


    }

    // only local owner should call this
    public void launchMissile(CombatFlow target, SamAI launchingSAM)
    {
        if (myFlow.localOwned && target != null)
        {
            Debug.LogWarning("Launching at " + target.name);

            GameObject missileObj = PhotonNetwork.Instantiate(launchingSAM.missilePrefab.name,
                launchingSAM.missileSpawnCenter.position, launchingSAM.missileSpawnCenter.rotation);

            BasicMissile missile = missileObj.GetComponent<BasicMissile>();
            CombatFlow missileFlow = missileObj.GetComponent<CombatFlow>();
            
            // this instance will network its position
            missileFlow.localOwned = true;
            missileFlow.isActive = true;
            missile.myTarget = target.gameObject;
            

            missile.myTeam = transform.root.GetComponent<CombatFlow>().team;
            missileFlow.team = missile.myTeam;

            

            missile.launch();
            missile.radar.setRadarActive(true);
            //missile.radar.radarOn = true;
            photonView.RPC("rpcMissileInit", RpcTarget.AllBuffered, missile.photonView.ViewID);
            
        }
        
    }

    [PunRPC]
    private void rpcMissileInit(int missileID)
    {
        BasicMissile missile = PhotonNetwork.GetPhotonView(missileID).GetComponent<BasicMissile>();
        CombatFlow missileFlow = missile.GetComponent<CombatFlow>();

        missile.myTeam = transform.root.GetComponent<CombatFlow>().team;
        missileFlow.team = missile.myTeam;
    }

}
