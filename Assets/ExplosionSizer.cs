using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSizer : MonoBehaviour
{
    public ExplosionSizerSettings settingsPrefab;
    public float radius;

    
    public ParticleSystem outer;
    public ParticleSystem additionalSmoke;
    public ParticleSystem embers;
    public ParticleSystem shockwave;

    // large entities
    public ParticleSystem light;
    public ParticleSystem debris;


    public bool awake = false;

    private void OnValidate()
    {
        Debug.Log("ExplosionSizer Validate");
        if (awake)
        {
            applyResize();
        }
    }

    public void applyResize(float newRadius)
    {
        
        radius = newRadius;
        applyResize();
    }

    public void applyResize()
    {
        awake = true;

        // Ratio radius to outer scale
        float scaleFactor = radius * settingsPrefab.radiusToScaleRatio;

        // Scale up outer prefab
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        // Scale up light
        if (light != null)
        {
            float lightScale = settingsPrefab.lightScaleRatio * scaleFactor;
            light.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }


        // Scale up additional smoke
        float smokeScale = settingsPrefab.smokeScaleRatio * scaleFactor;
        additionalSmoke.transform.localScale = new Vector3(smokeScale, smokeScale, smokeScale);

        // embers:
        //  - inner velocity 1x outer rate
        float emberVelFactor = settingsPrefab.ember_velocityModifierRatio * scaleFactor;
        var emberVelLifetime = embers.velocityOverLifetime;
        emberVelLifetime.speedModifier = emberVelLifetime.speedModifier.constant * emberVelFactor;

        //  - start size 4x outer rate
        float emberSizeFactor = settingsPrefab.ember_startSizeRatio * scaleFactor;
        var embersMainSize = embers.main.startSize;
        embersMainSize.constantMax *= emberSizeFactor;
        embersMainSize.constantMin *= emberSizeFactor;


        // Shockwave:
        //  - lifetime .2x outer rate
        float shockTimeFactor = settingsPrefab.shock_lifetimeRatio * scaleFactor;
        var shockTimeSetter = shockwave.main;
        shockTimeSetter.startLifetime = shockwave.main.startLifetime.constant * shockTimeFactor 
            + settingsPrefab.shock_lifetimeBase;

        //  - start size 1.2x outer rate
        float shockSize = settingsPrefab.shock_startSizeRatio * scaleFactor;
        //var shockSizeSetter = shockwave.main.startSize.constant;
        //shockSizeSetter *= shockSize;
        shockwave.transform.localScale = new Vector3(shockSize, shockSize, shockSize);

        // Debris:
        //  - scale outer x and y by .5x outer rate
        //  - scale outer z by .2x outer rate
        //  - increase start size of hard debris by .25x outer rate
        //  - increase smoke trail start size by .3x outer rate
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
