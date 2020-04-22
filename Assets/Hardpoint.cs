﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Hardpoint : MonoBehaviourPunCallbacks
{


    public GameObject weaponTypePrefab;

    public Transform spawnCenter;

    public GameObject loadedWeaponObj;
    public GameObject activeWeaponObj;

    public float reloadTimeMax;
    public float currentReloadTimer;
    public bool readyToFire;

    // stays true from launchStart to launch
    public bool launchCommanded; // MAKE SURE THIS CHANGES BACK TO FALSE AS SOON AS LAUNCH OCCURS


    public float fireRateDelayRaw; // raw fire rate delay from weapon

    public float effectiveLaunchDelayMax; // effective launch delay, taking into account position in sequence
    private float launchDelayRemain;

    public bool dropOnLaunch = true;


    public short roundsMax;
    public short roundsRemain;

    PhotonView photonView;
    

    // Start is called before the first frame update
    void Start()
    {
        photonView = PhotonView.Get(this);
        readyToFire = false;

        if (transform.root.GetComponent<CombatFlow>().isLocalPlayer)
        {
            spawnWeapon();
        }
    }

    void spawnWeapon()
    {
        // instantiates prefab IN WORLD SPACE, fixed joint to player
        loadedWeaponObj = PhotonNetwork.Instantiate(weaponTypePrefab.name, spawnCenter.position, spawnCenter.rotation);

        int weaponId = loadedWeaponObj.GetComponent<PhotonView>().ViewID;
        //Debug.Log("weaponId: " + weaponId);

        photonView.RPC("rpcInitializeWeapon", RpcTarget.AllBuffered, weaponId);
        
    }

    [PunRPC]
    void rpcInitializeWeapon(int weaponId)
    {
        loadedWeaponObj = PhotonNetwork.GetPhotonView(weaponId).gameObject;

        loadedWeaponObj.transform.position = spawnCenter.position;
        loadedWeaponObj.transform.rotation = spawnCenter.rotation;

        loadedWeaponObj.GetComponent<Weapon>().myHardpoint = this;

        // locks weapon to hardpoint using fixedjoint
        loadedWeaponObj.GetComponent<Weapon>().linkToOwner(transform.root.gameObject);

        loadedWeaponObj.GetComponent<Weapon>().myTeam = transform.root.GetComponent<CombatFlow>().team;




        readyToFire = true;
    }

    public void launchWithLock(GameObject targetObj)
    {
        if (readyToFire)
        {
            loadedWeaponObj.GetComponent<Weapon>().myTarget = targetObj;
            launchStart();
        }
        
    }

    public void launchStart() // doesn't need lock
    {
        Debug.Log("launchStart() called");
        if (readyToFire)
        {
            activeWeaponObj = loadedWeaponObj;
            launchCommanded = true;
            launchDelayRemain = effectiveLaunchDelayMax;
        }
        else // weapon is not loaded
        {
            //Debug.Log("cannot fire weapon from hardpoint: " + gameObject.name + ", no weapon loaded");
        }
    }

    public void launch()
    {
        if (readyToFire)
        {
            Debug.Log("launch() called");
            launchCommanded = false;
            loadedWeaponObj.GetComponent<Weapon>().launch();
            currentReloadTimer = reloadTimeMax;
        }
        else // weapon is not loaded
        {
            //Debug.Log("cannot fire weapon from hardpoint: " + gameObject.name + ", no weapon loaded");
        }
    }


    bool countDownDelayTimer()
    {
        
        bool delayComplete = false;
        if (launchDelayRemain > 0f)
        {
            launchDelayRemain -= Time.deltaTime;
        }
        else
        {
            delayComplete = true;
        }
        //Debug.Log("Time remain: " + launchDelayRemain);
        return delayComplete;
    }

    public void launchEnd()
    {
        Debug.Log("Hardpoint launchEnd called");
        // tell weapon to stop launching
        if (activeWeaponObj != null)
        {
            Debug.Log("----------------------- Successfully calling launchEnd() on weapon");
            activeWeaponObj.GetComponent<Weapon>().launchEnd();
        }
        else
        {
            Debug.Log("hardpoint launchEnd skipped, because activeWeaponObj is null");
        }



        launchCommanded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToFire)
        {
            if (launchCommanded)
            {
                if (countDownDelayTimer())
                {
                    launch();
                }

            }

            

        }
        else // not ready to fire -- count down reload
        {
            if (currentReloadTimer > 0)
            {
                currentReloadTimer -= Time.deltaTime;
            }
            else // reload timer runs out, reload
            {

                reloadProcess(); // may be called repeatedly until reload complete

            }

        }
    }


    // called repeatedly from update until reload is complete
    void reloadProcess()
    {
        if(loadedWeaponObj == null)
        {
            spawnWeapon();
        }
        else // weapon still present -- call its reload process
        {
            Weapon weaponRef = loadedWeaponObj.GetComponent<Weapon>();
            weaponRef.reloadProcess();
        }
    }
}
