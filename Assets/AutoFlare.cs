using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFlare : MonoBehaviour
{
    protected RWR rwr;

    protected FlareEmitter flareEmitter;

    public float flareDropTime;

    protected Rigidbody myRb;

    public float flareMissileVelocityMin;

    public bool enableAutoFlare;
    public bool debug = false;

    private void Awake()
    {
        setRefs();
    }

    protected void setRefs()
    {
        rwr = GetComponent<RWR>();
        flareEmitter = GetComponent<FlareEmitter>();
        myRb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        autoFlareProcess();
        
    }

    public void autoFlareProcess()
    {
        CombatFlow msl = rwr.highestThreatMissile;
        flareEmitter.flareButtonDown = false;
        if (msl != null)
        {
            float impactTime = calculateImpactTime(rwr.highestThreatMissile);
            flareEmitter.flareButtonDown = flareConditions(impactTime, msl);

            //if (debug)
            //{
            //    if (flareEmitter.flareButtonDown)
            //    {
            //        Debug.Log(gameObject.name + " ATTEMPTING TO FLARE!!!!!------------------");
            //    }

            //    Debug.Log(gameObject.name + " DETECTED INCOMING MISSILE WITH IMPACT TIME: " + impactTime + "s");

            //}
        }
    }

    //public bool flareProcess

    public bool flareConditions(float impactTime, CombatFlow msl)
    {
        return impactTime < flareDropTime
                && msl.myRb.velocity.magnitude > flareMissileVelocityMin && enableAutoFlare;
    }

    public float calculateImpactTime(CombatFlow msl)
    {
        

        Vector3 mslDir = transform.position - msl.transform.position;
        Vector3 mslRelativeVel = msl.myRb.velocity - myRb.velocity;

        return (mslDir.magnitude) / (mslRelativeVel.magnitude);
    }
}
