﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class BasicBomb : Weapon
{

    public GameObject ownerObj;

    public CombatFlow myCombatFlow;

    public Rigidbody rbRef;

    public float impactDamage;


    private GameObject oneImpactVictim;
    
    void Awake()
    {
        myCombatFlow = GetComponent<CombatFlow>();
        rbRef = GetComponent<Rigidbody>();
        setColliders(false);

        myCombatFlow.isActive = false;
    }

    override
    public void linkToOwner(GameObject newOwner)
    {
        ownerObj = newOwner;
        GetComponent<FixedJoint>().connectedBody = ownerObj.GetComponent<Rigidbody>();

        myHardpoint.roundsMax = 1;
        myHardpoint.roundsRemain = 1;
    }

    // Update is called once per frame
    void Update()
    {
        tryArm();

        if (armed)
        {
            transform.rotation = Quaternion.LookRotation(rbRef.velocity, transform.up);
        }
    }


    private void FixedUpdate()
    {
        checkLinecastCollision();
    }


    override
    public void launch()
    {
        myCombatFlow.localOwned = true;

        photonView.RPC("rpcLaunch", RpcTarget.All);

    }

    [PunRPC]
    private void rpcLaunch()
    {
        myHardpoint.readyToFire = false;
        myHardpoint.loadedWeaponObj = null; // no longer loaded -- remove reference

        Destroy(GetComponent<FixedJoint>());

        rbRef.velocity = ownerObj.GetComponent<Rigidbody>().velocity;

        myHardpoint.roundsRemain = 0;

        launched = true;
        armTimeRemaining = armingTime;
        myCombatFlow.isActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (myCombatFlow.localOwned)
        {
            GameObject otherRoot = other.gameObject.transform.root.gameObject;
            int id = getVictimId(otherRoot);

            photonView.RPC("rpcContactProcess", RpcTarget.All, transform.position, id);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (myCombatFlow.localOwned)
        {
            GameObject otherRoot = collision.gameObject.transform.root.gameObject;
            int id = getVictimId(otherRoot);

            photonView.RPC("rpcContactProcess", RpcTarget.All, transform.position, id);
        }
    }

    [PunRPC]
    override
    public void rpcContactProcess(Vector3 position, int otherId)
    {
        if (oneImpactVictim == null)
        {
            transform.position = position;

            GameObject other = null;
            CombatFlow otherFlow = null;

            if (otherId != -1)
            {
                other = PhotonNetwork.GetPhotonView(otherId).gameObject;
                otherFlow = other.gameObject.GetComponent<CombatFlow>();
            }

            if (other != null)
            {
                oneImpactVictim = other.transform.root.gameObject;
            }

            Debug.Log("Bomb collided");
            myCombatFlow.currentHP -= myCombatFlow.currentHP; // die immediately on collision
                                                              //CombatFlow otherFlow = other.gameObject.GetComponent<CombatFlow>();
            if (otherFlow != null)
            {
                otherFlow.currentHP -= impactDamage;
            }
            
        }
    }

    

}