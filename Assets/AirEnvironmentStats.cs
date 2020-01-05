using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirEnvironmentStats : MonoBehaviour
{

    public float airDensityAltitudeMod; // increase to have higher densities at higher altitudes
    public float effectiveSeaLevel; // below this, altitude will be effectively 0. Density constant in this region

    public float contrailAltitude;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float getDensityAtAltitude(float altitude)
    {
        altitude = Mathf.Max(0.0f, altitude - effectiveSeaLevel); // below sea level, density constant.
        return airDensityAltitudeMod / (altitude + airDensityAltitudeMod); // 1/x graph shifted left so that 0 alt gives 1.0 density
    }
}
