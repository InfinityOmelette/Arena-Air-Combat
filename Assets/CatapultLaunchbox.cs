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


    // choose closest AVAILABLE catapult to gear
    public Catapult chooseCat(CatLaunchGear gear)
    {
        float minDist = 100f; // arbitrarily large start val
        Catapult selectedCat = cats[0];

        for(int i = 0; i < cats.Count; i++)
        {
            Catapult cat = cats[i];
            float dist = Vector3.Distance(cat.launchCenter.position, gear.transform.position);
            if(cat.catAvailable() && dist < minDist)
            {
                minDist = dist;
                selectedCat = cat;
            }
        }

        return selectedCat;
    }
}
