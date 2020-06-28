using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{

    public float garbageCollectDelay;
    private float gcTimer;


    // Start is called before the first frame update
    void Start()
    {
        gcTimer = garbageCollectDelay;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(gcTimer <= 0)
        {
            System.GC.Collect();
            gcTimer = garbageCollectDelay;
            Debug.LogWarning("=================  GARBAGE COLLECT  =============");
        }
        else
        {
            gcTimer -= Time.fixedDeltaTime;
        }
    }
}
