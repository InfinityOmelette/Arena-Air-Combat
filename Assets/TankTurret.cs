using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : MonoBehaviour
{
    public GameObject projectileSpawn;

    // copy this for every shot
    public GameObject shellSettings;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            fire();
        }
    }

    private void setAim()
    {

    }


    private void fire()
    {
        // copy variable data over to determine what kind of shell
        GameObject shell = GameObject.Instantiate(shellSettings);
        shell.transform.position = projectileSpawn.transform.position;
        shell.transform.rotation = projectileSpawn.transform.rotation;
        shell.SetActive(true);

        //shell.GetComponent<TankShell>().readyEmit();
    }
}
