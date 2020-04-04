using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatalinkHub : MonoBehaviour
{

    List<CombatFlow> targetList;

    // Start is called before the first frame update
    void Start()
    {
        targetList = new List<CombatFlow>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool tryAddTarget(CombatFlow target)
    {
        bool doAdd = false;

        // is this an expensive process to do for every target?
        // maybe limit number of calls to this function
        if (!targetList.Contains(target)) 
        {
            doAdd = true;
            targetList.Add(target);
        }

        return doAdd;
    }
}
