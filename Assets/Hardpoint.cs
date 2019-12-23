using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hardpoint : MonoBehaviour
{


    public GameObject weaponTypePrefab;

    public Transform spawnCenter;

    public GameObject loadedWeaponObj;
    public GameObject activeWeaponObj;

    public float reloadTime;
    public float currentTimer;
    public bool readyToFire;


    public bool dropOnLaunch = true;

    // Start is called before the first frame update
    void Start()
    {
        readyToFire = false;
        spawnWeapon();
    }


    void spawnWeapon()
    {
        // instantiates prefab IN WORLD SPACE, fixed joint to player
        loadedWeaponObj = GameObject.Instantiate(weaponTypePrefab);
        loadedWeaponObj.transform.position = spawnCenter.position;
        loadedWeaponObj.transform.rotation = spawnCenter.rotation;


        loadedWeaponObj.GetComponent<Weapon>().linkToOwner(transform.root.gameObject);
        loadedWeaponObj.GetComponent<Weapon>().myTeam = transform.root.GetComponent<CombatFlow>().team;


        loadedWeaponObj.GetComponent<Weapon>().myHardpoint = this;

        readyToFire = true;        
        
    }

    public void launchWithLock(GameObject targetObj)
    {
        if (readyToFire)
        {
            loadedWeaponObj.GetComponent<Weapon>().myTarget = targetObj;
            launch();
        }
        
    }

    public void launch() // doesn't need lock
    {
        if (readyToFire)
        {
            loadedWeaponObj.GetComponent<Weapon>().launch();
            activeWeaponObj = loadedWeaponObj;
            currentTimer = reloadTime;
        }
        else // weapon is not loaded
        {
            //Debug.Log("cannot fire weapon from hardpoint: " + gameObject.name + ", no weapon loaded");
        }
    }

    public void launchEnd()
    {
        // tell weapon to stop launching
        if (activeWeaponObj != null)
        {
            activeWeaponObj.GetComponent<Weapon>().launchEnd();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(loadedWeaponObj == null)
        {
            
            if(currentTimer > 0)
            {
                currentTimer -= Time.deltaTime;
            }
            else // reload timer runs out, reload
            {
                if(dropOnLaunch)
                    spawnWeapon();
            }

        }
    }
}
