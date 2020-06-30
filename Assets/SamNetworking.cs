using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class SamNetworking : MonoBehaviourPunCallbacks
{

    public SamAI sam;

    private CombatFlow myFlow;

    // Start is called before the first frame update
    void Start()
    {
        myFlow = GetComponent<CombatFlow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setTarget(CombatFlow target)
    {

        int id = -1;
        if (target != null)
        {
            //Debug.LogWarning("Setting Target: " + target.gameObject.name);
            id = target.photonView.ViewID;
        }

        photonView.RPC("rpcSetSamTarget", RpcTarget.All, id);
    }

    [PunRPC]
    public void rpcSetSamTarget(int viewID)
    {
        if (viewID != -1)
        {
            
            PhotonView view = PhotonNetwork.GetPhotonView(viewID);

            if (view != null)
            {
                CombatFlow targetFlow = view.GetComponent<CombatFlow>();
                sam.setTarget(targetFlow);

                if(targetFlow.gameObject == GameManager.getGM().localPlayer)
                {
                    Radar myRadar = GetComponent<Radar>();
                    myRadar.rwrIcon.beginLock();
                }

            }


        }
        else
        {
            
            if(sam.currentTarget.gameObject == GameManager.getGM().localPlayer)
            {
                Radar myRadar = GetComponent<Radar>();
                myRadar.rwrIcon.endLock();
            }

            sam.setTarget(null);
        }


    }

    // only local owner should call this
    public void launchMissile(CombatFlow target)
    {
        if (myFlow.localOwned && target != null)
        {
            Debug.LogWarning("Launching at " + target.name);

            GameObject missileObj = PhotonNetwork.Instantiate(sam.missilePrefab.name,
                sam.missileSpawnCenter.position, sam.missileSpawnCenter.rotation);

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
