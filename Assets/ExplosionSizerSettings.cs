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
    public float shock_lifetimeRatio;
    public float shock_lifetimeBase;
    public float shock_startSizeRatio;

    public Vector3 debrisScaleRatios;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
