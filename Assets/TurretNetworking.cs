using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class TurretNetworking : MonoBehaviourPunCallbacks
{

    public AI_TurretMG turret;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setTarget(CombatFlow target)
    {
        

        int id = -1;
        if(target!= null)
        {
            //Debug.LogWarning("Setting Target: " + target.gameObject.name);
            id = target.photonView.ViewID;
        }

        photonView.RPC("rpcSetTurretTarget", RpcTarget.All, id);
    }

    [PunRPC]
    public void rpcSetTurretTarget(int viewID)
    {
        if (viewID != -1)
        {
            try
            {
                PhotonView view = PhotonNetwork.GetPhotonView(viewID);
                turret.setTarget(view.gameObject);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Failed to find view number " + viewID);
            }
        }
        else
        {
            turret.setTarget(null);
        }

       
    }

}
