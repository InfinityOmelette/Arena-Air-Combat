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

    public Light flash;
    private float flashMaxRange = 0.0f;
    private float flashTimer = 0.0f;
    private float flashTimerMax = 0.0f;

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

    private void setOuterScale(float scaleFactor)
    {
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    private void setLightScale(float scaleFactor)
    {
        float lightScale = settingsPrefab.lightScaleRatio * scaleFactor;
        light.transform.localScale = new Vector3(lightScale, lightScale, lightScale);
    }

    private void setSmokeScale(float scaleFactor)
    {
        float smokeScale = settingsPrefab.smokeScaleRatio * scaleFactor;
        additionalSmoke.transform.localScale = new Vector3(smokeScale, smokeScale, smokeScale);
    }

    private void setEmberVel(float scaleFactor)
    {
        float emberVelFactor = settingsPrefab.ember_velocityModifierRatio * scaleFactor;
        var emberVelLifetime = embers.velocityOverLifetime;
        emberVelLifetime.speedModifierMultiplier *= emberVelFactor;
    }

    private void setEmberSize(float scaleFactor)
    {
        float emberSizeFactor = settingsPrefab.ember_startSizeRatio * scaleFactor;
        var embersMain = embers.main;
        //embersMain.startSize 
        var embersMainSize = embers.main.startSize;
        float newMax = embersMainSize.constantMax * emberSizeFactor;
        float newMin = embersMainSize.constantMin * emberSizeFactor;
        embersMain.startSize = new ParticleSystem.MinMaxCurve(newMin, newMax);
    }

    private void setShockwaveTime(float scaleFactor)
    {
        //  - lifetime .2x outer rate
        float shockTimeFactor = settingsPrefab.shock_lifetimeRatio * scaleFactor;
        var shockTimeSetter = shockwave.main;
        shockTimeSetter.startLifetime = shockwave.main.startLifetime.constant * shockTimeFactor
            + settingsPrefab.shock_lifetimeBase;
    }

    private void setShockwaveScale(float scaleFactor)
    {
        float shockSize = settingsPrefab.shock_startSizeRatio * scaleFactor;
        //var shockSizeSetter = shockwave.main.startSize.constant;
        //shockSizeSetter *= shockSize;
        shockwave.transform.localScale = new Vector3(shockSize, shockSize, shockSize);
    }

    private void setFlash(float radius)
    {
        // range
        flashMaxRange = settingsPrefab.flashRangeRadiusRatio * radius;
        applyFlashRange(flashMaxRange);

        // timer
        flashTimer = settingsPrefab.calcFlashTime(radius);
        flashTimerMax = flashTimer;
    }

    private void applyFlashRange(float range)
    {
        //flash.transform.localScale = new Vector3(scale, scale, scale);
        flash.range = range;
    }

    public void applyResize()
    {
        awake = true;

        // Ratio radius to outer scale
        float scaleFactor = calculateScaleFactor(radius);

        // outer scale
        setOuterScale(scaleFactor);

        // light
        if (light != null)
        {
            setLightScale(scaleFactor);
        }

        // Smoke
        setSmokeScale(scaleFactor);

        // embers:
        setEmberVel(scaleFactor);
        setEmberSize(scaleFactor);

        // Shockwave:
        if (checkShockRadius(radius))
        {
            setShockwaveTime(scaleFactor);
            setShockwaveScale(scaleFactor);
        }
        else
        {
            disableShock();
        }
        

        // Debris:
        //  - scale outer x and y by .5x outer rate
        //  - scale outer z by .2x outer rate
        //  - increase start size of hard debris by .25x outer rate
        //  - increase smoke trail start size by .3x outer rate

        setFlash(radius);
        
    }

    private bool checkShockRadius(float radius)
    {
        return radius >= settingsPrefab.shock_minRadius;
    }

    private void disableShock()
    {
        shockwave.gameObject.SetActive(false);
    }

    private float calculateScaleFactor(float newRadius)
    {
        return newRadius * settingsPrefab.radiusToScaleRatio;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(flash != null)
        {
            decayFlash(Time.deltaTime);
        }
        
    }

    void decayFlash(float deltaTime)
    {
        if(flashTimer > 0.0f)
        {
            float percentTimeRemain = flashTimer / flashTimerMax;
            applyFlashRange(Mathf.Lerp(0.0f, flashMaxRange, percentTimeRemain));
            flashTimer -= deltaTime;
        }
        else
        {
            flash.enabled = false;
        }
    }
}
