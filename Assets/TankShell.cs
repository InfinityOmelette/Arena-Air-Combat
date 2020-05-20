using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShell : MonoBehaviour
{

    public bool localOwned;

    public float initSpeed;

    private Rigidbody rb;
    public GameObject effectsCenter;
    private ExplodeStats explodeStats;

    public GameObject effectsInit;

    public GameObject effectsObj;
    public Light effectsLight;
    public TrailRenderer trail;

    //private bool readyToEmit;

    private float emitDelay;

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

        //trail.emitting = false;
        rb.velocity = transform.forward * initSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if(effectsObj != null)
        {
            effectsObj.transform.position = effectsCenter.transform.position;

            //trail.emitting = readyToEmit;

        }

        transform.rotation = Quaternion.LookRotation(rb.velocity, transform.up);
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
