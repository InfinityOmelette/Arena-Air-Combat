using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSightComputer : MonoBehaviour
{
    
    public float maxRange;
    public bool active;
    public float initForwardVel;

    private DropSightReticle reticle;
    private Rigidbody rb;
    private CombatFlow myFlow;

    //Vector3 dropPoint;

    private float g;

    private Vector3 dropDir;

    // Start is called before the first frame update
    void Start()
    {
        reticle = hudControl.mainHud.GetComponent<hudControl>().dropReticle;
        rb = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
        g = Physics.gravity.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        if (myFlow.isLocalPlayer && reticle != null)
        {
            //Vector3 dropPoint = calculateDropPoint();
            float dropRange = calculateDropRange();
            //Debug.Log("Drop range: " + dropRange);
            //bool doShow = active && dropRange < maxRange;
            bool doShow = active;
            if (doShow)
            {
                Vector3 dropPoint = calculateDropPoint(dropRange);
                
                reticle.placeReticle(dropPoint);
                
            }
            reticle.showReticle(doShow);
        }
    }

    public void setComputer(bool active, float initForwardVel, float maxRange)
    {
        this.active = active;
        this.initForwardVel = initForwardVel;
        this.maxRange = maxRange;
    }

    private float calculateDropRange()
    {
        Vector3 initVel = rb.velocity + transform.forward * initForwardVel;
        dropDir = initVel;
        float elevRad = getElevation(initVel) * Mathf.Deg2Rad;
        float velMagnitude = initVel.magnitude;

       // Debug.Log("My vel magnitude: " + rb.velocity.magnitude + " , proj magnitude " 
        //    + velMagnitude + " elevation: " + elevRad * Mathf.Rad2Deg + " degrees");

        

        return velMagnitude * Mathf.Cos(elevRad) * 
            (velMagnitude * Mathf.Sin(elevRad) + Mathf.Sqrt(Mathf.Pow(velMagnitude * Mathf.Sin(elevRad), 2)
            + 2 * g * transform.position.y)) / g;
    }

    private float getElevation(Vector3 dir)
    {
        Vector3 horizontal = new Vector3(dir.x, 0.0f, dir.z);
        float elev = Vector3.Angle(dir, horizontal);
        elev *= Mathf.Sign(dir.y);
        return elev;
    }

    private Vector3 calculateDropPoint(float range)
    {
        Vector3 horizDir = new Vector3(dropDir.x, 0.0f, dropDir.z).normalized * range;
        return new Vector3(transform.position.x, 0.0f, transform.position.z) + horizDir;
    }
}
