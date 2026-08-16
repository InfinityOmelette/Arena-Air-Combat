using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultLaunchbox : MonoBehaviour
{

    public List<Catapult> cats;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public Catapult chooseCat(CatLaunchGear gear)
    {
        return cats[0];
    }
}
