using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShell : MonoBehaviour
{

    public bool localOwned;


    private Rigidbody rb;
    public GameObject effectsCenter;
    private ExplodeStats explodeStats;

    public GameObject effectsInit;

    public GameObject effectsObj;
    public Light effectsLight;
    public TrailRenderer trail;

    public float smokeEmitTime;
    public float lightEmitTime;

    //private bool readyToEmit;

    //private bool startTrailOn;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        explodeStats = GetComponent<ExplodeStats>();
    }

    void Start()
    {
        effectsObj = GameObject.Instantiate(effectsInit);
        trail = effectsObj.GetComponent<TrailRenderer>();
        effectsLight = effectsObj.GetComponent<Light>();
        effectsObj.transform.position = effectsCenter.transform.position;
        GameObject.Destroy(effectsInit);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void FixedUpdate()
    {

        if (effectsObj != null)
        {
            effectsObj.transform.position = effectsCenter.transform.position;

            //trail.emitting = readyToEmit;

            if (trail.emitting)
            {
                if (smokeEmitTime > 0)
                {
                    smokeEmitTime -= Time.fixedDeltaTime;
                }
                else
                {
                    trail.emitting = false;
                }
            }

            if (effectsLight.enabled)
            {
                if (lightEmitTime > 0)
                {
                    lightEmitTime -= Time.fixedDeltaTime;
                }
                else
                {
                    effectsLight.enabled = false;
                }
            }


        }

        transform.rotation = Quaternion.LookRotation(rb.velocity, transform.up);


        if (transform.position.y < 0)
        {
            explodeStats.explode(transform.position);
            GameObject.Destroy(gameObject);
        }
    }


    public void readyEmit()
    {
        //readyToEmit = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(effectsLight != null)
        {
            effectsLight.enabled = false;
        }

        explodeStats.explode(transform.position);
        GameObject.Destroy(gameObject);
    }

}
