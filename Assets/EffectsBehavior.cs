using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsBehavior : MonoBehaviour
{

    public float deathCountDown;
    public bool enableDissipationTimer;


    public ParticleSystem smokeParticles;
    public Light myLight;
    public TrailRenderer lightTrail;
    public ParticleSystem sparkParticles;


    public bool playCalled = false;

    
    public float launchKnockRadius = -1;

    public void activate()
    {
        Debug.Log("Activate begin");
        //smokeTrailOld.enabled = true;
        myLight.enabled = true;
        //particleTrail.enabled = true;
        emitSmoke(true);
        emitLight(true);

        Debug.Log("Activate end");
    }

    public void deactivate()
    {
        Debug.Log("Deactivate begin");
        lightTrail.enabled = false;
        myLight.enabled = false;
        emitSmoke(false);
        emitLight(false);
        enableDissipationTimer = true;
        Debug.Log("Deactivate end");
    }

    public void beginDissipation()
    {
        emitSmoke(false);
        emitLight(false);
        enableDissipationTimer = true;
        Debug.Log("Beginning dissipation with: " + deathCountDown + " seconds remain");
    }

    public void emitSmoke(bool set)
    {
        // set particle emission
        //smokeTrailOld.emitting = set;
        //var main = particleTrail.main;
        Debug.Log("EmitSmoke called: " + set);
        if (set != smokeParticles.isPlaying)
        {
            Debug.Log("Emit smoke causing change");
            if (set)
            {
                //smokeParticles.Clear();
                fullReset(smokeParticles);
                smokeParticles.Play();
                if (sparkParticles != null) sparkParticles.Play();

                playCalled = true;
                //sparkParticles.Play();
            }
            else
            {
                smokeParticles.Stop();
                if (sparkParticles != null) sparkParticles.Stop();
            }
        }

        Debug.Log("EmitSmoke end");
        
    }

    public void emitLight(bool set)
    {
        Debug.Log("EmitLight called " + set);
        myLight.enabled = set;
        if (set) lightTrail.enabled = true;
        lightTrail.emitting = set;
    }

    public void multLightRange(float range)
    {
        myLight.range *= range;
        //lightTrail.widthCurve;
    }



    public void updatePos(Vector3 newPos)
    {
        if (transform != null)
        {
            transform.position = newPos;
        }
        
    }

    void fullReset(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);

        ps.Simulate(0f, true, true);

    }

    private void Awake()
    {
        myLight = GetComponent<Light>();
        smokeParticles = GetComponent<ParticleSystem>();
        lightTrail = GetComponent<TrailRenderer>();
        fullReset(smokeParticles);
        smokeParticles.Clear();
        //tryKnock();
    }

    bool tryKnock()
    {
        bool didKnock = false;
        if(launchKnockRadius > 0)
        {
            didKnock = PerspectiveManager.tryKnock(launchKnockRadius, transform.position);
        }
        return didKnock;
    }

    // Start is called before the first frame update
    void Start()
    {
        tryKnock();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableDissipationTimer)
        {
            if(deathCountDown > 0)
            {
                deathCountDown -= Time.deltaTime;
            }
            else
            {
                destroyProcesss();
            }
        }
    }

    void destroyProcesss()
    {
        smokeParticles.Clear();
        lightTrail.Clear();
        if (sparkParticles != null) sparkParticles.Clear();
        smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        GameObject.Destroy(gameObject);
    }

    private void OnDisable()
    {
        Debug.Log("Effects obj getting disabled");
    }

    private void OnDestroy()
    {
        Debug.Log("Effects obj getting destroyed");
        
    }


}
