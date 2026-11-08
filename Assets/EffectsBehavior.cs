using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsBehavior : MonoBehaviour
{

    public float deathCountDown;
    public bool enableDissipationTimer;


    public ParticleSystem particleTrail;
    public Light myLight;
    public TrailRenderer smokeTrailOld;


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
        smokeTrailOld.enabled = false;
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
            particleTrail.Play();
        }
        else
        {
            particleTrail.Stop();
        }
        
    }

    public void emitLight(bool set)
    {
        myLight.enabled = set;
    }

    public void multLightRange(float range)
    {
        myLight.range *= range;
    }

    

    public void updatePos(Vector3 newPos)
    {
        transform.position = newPos;
    }

    private void Awake()
    {
        myLight = GetComponent<Light>();
        particleTrail = GetComponent<ParticleSystem>();
        smokeTrailOld = GetComponent<TrailRenderer>();
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
