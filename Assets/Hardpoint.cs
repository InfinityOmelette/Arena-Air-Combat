using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hardpoint : MonoBehaviour
{


    public GameObject weaponTypePrefab;

    public Transform spawnCenter;

    public GameObject currentWeaponObj;




    // Start is called before the first frame update
    void Start()
    {
        spawnWeapon();
    }


    void spawnWeapon()
    {
        // instantiates prefab IN WORLD SPACE, fixed joint to player
        currentWeaponObj = GameObject.Instantiate(weaponTypePrefab);
        currentWeaponObj.transform.position = spawnCenter.position;
        currentWeaponObj.transform.rotation = spawnCenter.rotation;

        currentWeaponObj.GetComponent<Weapon>().linkToOwner(transform.root.gameObject);


        
        
    }

    public void launchWithLock(GameObject targetObj)
    {
        
        currentWeaponObj.GetComponent<Weapon>().myTarget = targetObj;
        launchNoLock();
    }

    public void launchNoLock()
    {
        currentWeaponObj.GetComponent<Weapon>().launch();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
