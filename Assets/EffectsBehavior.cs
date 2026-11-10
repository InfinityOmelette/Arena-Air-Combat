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

    public void activate()
    {
        //smokeTrailOld.enabled = true;
        myLight.enabled = true;
        //particleTrail.enabled = true;
        emitSmoke(true);
        emitLight(true);
    }

    public void deactivate()
    {
        lightTrail.enabled = false;
        myLight.enabled = false;
        emitSmoke(false);
        emitLight(false);
        enableDissipationTimer = true;
    }

    public void beginDissipation()
    {
        emitSmoke(false);
        emitLight(false);
        enableDissipationTimer = true;
    }

    public void emitSmoke(bool set)
    {
        // set particle emission
        //smokeTrailOld.emitting = set;
        //var main = particleTrail.main;
        if (set)
        {
            smokeParticles.Play();
            if (sparkParticles != null) sparkParticles.Play();
            //sparkParticles.Play();
        }
        else
        {
            smokeParticles.Stop();
            if (sparkParticles != null) sparkParticles.Stop();
        }
        
    }

    public void emitLight(bool set)
    {
        myLight.enabled = set;
        lightTrail.enabled = set;
        lightTrail.emitting = set;
    }

    public void multLightRange(float range)
    {
        myLight.range *= range;
        //lightTrail.widthCurve;
    }

    

    public void updatePos(Vector3 newPos)
    {
        transform.position = newPos;
    }

    private void Awake()
    {
        myLight = GetComponent<Light>();
        smokeParticles = GetComponent<ParticleSystem>();
        lightTrail = GetComponent<TrailRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
                GameObject.Destroy(gameObject);
            }
        }
    }

    
}
