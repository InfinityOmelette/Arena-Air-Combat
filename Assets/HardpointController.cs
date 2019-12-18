using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardpointController : MonoBehaviour
{


    public Hardpoint[] hardpoints;

    public TgtComputer tgtComputer;

    short activeHardpointIndex = 0;

    public bool launchButtonDown;

    // Commands missiles to launch

    // Start is called before the first frame update
    void Start()
    {
        fillHardpointArray();
    }

    void fillHardpointArray()
    {
        hardpoints = new Hardpoint[transform.childCount];
        for(int i = 0; i < hardpoints.Length; i++)
        {
            hardpoints[i] = transform.GetChild(i).gameObject.GetComponent<Hardpoint>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (launchButtonDown)
        {
            activeHardpointIndex++;
            if (activeHardpointIndex >= hardpoints.Length - 1)
                activeHardpointIndex = 0;


            if (tgtComputer.currentTarget == null) // if no target is locked
            {
                hardpoints[activeHardpointIndex].launchNoLock();
            }
            else // if target is locked
            {
                hardpoints[activeHardpointIndex].launchWithLock(tgtComputer.currentTarget.gameObject);
            }

            
        }
    }
}
