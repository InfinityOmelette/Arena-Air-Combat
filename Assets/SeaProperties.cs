using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaProperties : MonoBehaviour
{

    private static float seaLevel = 0.0f;
    //private static PhysicsProperties phys;
    private static SeaProperties sea;


    public float seaLevelOverride;

    public static SeaProperties getSea()
    {
        if(sea == null)
        {
            sea = GameObject.Find("EnvironmentProperties").GetComponent<SeaProperties>();
        }
        return sea;
    }

    public static float getSeaLevel()
    {
        return seaLevel;
    }

    private void Awake()
    {
        seaLevel = seaLevelOverride;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        seaLevel = seaLevelOverride;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
