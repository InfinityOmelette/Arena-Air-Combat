using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class HostMigration : MonoBehaviourPunCallbacks
{

    public bool isMasterClient = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isMasterClient = PhotonNetwork.IsMasterClient;
    }


    
}
