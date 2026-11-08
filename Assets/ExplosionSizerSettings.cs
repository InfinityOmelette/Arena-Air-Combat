using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSizerSettings : MonoBehaviour
{

    public float radiusToScaleRatio;

    public float smokeScaleRatio;
    public float lightScaleRatio;

    //EMBERS:
    public float ember_velocityModifierRatio;
    public float ember_startSizeRatio;

    // SHOCKWAVE:
    public float shock_minRadius;
    public float shock_lifetimeRatio;
    public float shock_lifetimeBase;
    public float shock_startSizeRatio;

    public Vector3 debrisScaleRatios;

    // FLASH
    public float flashTimeMin;
    public float flashTimeRadiusRatio;
    public float flashTimeMax;
    public float flashRangeRadiusRatio;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float calcFlashTime(float radius)
    {
        //float time = flashTimeMin + flashTimeRatio * radius;
        //return Mathf.Min(time, flashTimeMax);
        float time = flashTimeRadiusRatio * radius;
        return Mathf.Clamp(time, flashTimeMin, flashTimeMax);
    }
}
