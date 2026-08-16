using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    CatLaunchGear linkedGear;
    public Transform launchCenter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void linkToGear(CatLaunchGear gear)
    {
        linkedGear = gear;
    }

    public void release(CatLaunchGear gear)
    {
        linkedGear = null;
    }

    public bool catAvailable()
    {
        return linkedGear == null;
    }
}
