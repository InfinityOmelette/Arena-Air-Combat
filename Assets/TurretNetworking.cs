using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class TurretNetworking : MonoBehaviourPunCallbacks
{

    public AI_TurretMG turret;


    public List<AI_TurretMG> myTurrets;

    // Start is called before the first frame update
    void Start()
    {
        if(myTurrets != null)
        {
            for(int i = 0; i < myTurrets.Count; i++)
            {
                myTurrets[i].setIndex(i);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setTarget(CombatFlow target, int turretIndex = 0)
    {
        

        int id = -1;
        if(target!= null)
        {
            Debug.LogWarning("Setting Target: " + target.gameObject.name);
            id = target.photonView.ViewID;
        }

        photonView.RPC("rpcSetTurretTarget", RpcTarget.All, id, turretIndex);
    }

    
    [PunRPC]
    public void rpcSetTurretTarget(int viewID, int turretIndex = 0)
    {
        //Debug.LogWarning("rpcSetTurretTarget called");

        AI_TurretMG turretRef = turret;
        if(turretIndex != -1)
        {
            turretRef = myTurrets[turretIndex];
        }


        if (viewID != -1)
        {
            try
            {
                PhotonView view = PhotonNetwork.GetPhotonView(viewID);
                turretRef.setTarget(view.gameObject);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Failed to find view number " + viewID);
            }
        }
        else
        {
            myTurrets[turretIndex].setTarget(null);
        }

       
    }

}
