using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hardpoint : MonoBehaviour
{


    public GameObject weaponTypePrefab;

    public Transform spawnCenter;

    public GameObject loadedWeaponObj;

    public float reloadTime;
    public float currentTimer;




    // Start is called before the first frame update
    void Start()
    {
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


        
        
    }

    public void launchWithLock(GameObject targetObj)
    {
        if (loadedWeaponObj != null)
        {
            loadedWeaponObj.GetComponent<Weapon>().myTarget = targetObj;
            launch();
        }
        
    }

    public void launch() // doesn't need lock
    {
        if (loadedWeaponObj != null)
        {
            loadedWeaponObj.GetComponent<Weapon>().launch();
            loadedWeaponObj = null;
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
                spawnWeapon();
            }

        }
    }
}
