using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelZoom : MonoBehaviour
{
    Rigidbody rootRb;
    Transform root;

    public Vector3 localAccel;

    Vector3 prevVel;

    Vector3 trueAccel;
    

    // Positive --> FOV increase when accelerating forward
    // Negative --> FOV decrease when accelerating forward
    public float zoomRate = .15f;

    public float maxModAbs = 15f;
    public float minAccel = 5f;

    public float currentZoomMod;

    public float zoomLerpRate = 1f;

    public float accelDiscard = 10f;

    public bool debug = false;

    public float accelMagnitude;

    public float accelModRate;
    public float accelModLerpRate = 2f;
    public float maxAccelModRadius = 20f;
    Vector3 accelModSmoothed;

    private void Awake()
    {
        root = transform.root;
        rootRb = root.GetComponent<Rigidbody>();
        prevVel = new Vector3();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 clamDirVect(Vector3 dir, float maxRadius)
    {
        float magnitude = dir.magnitude;
        if(magnitude > maxRadius)
        {
            dir = (dir.normalized) * maxRadius;
        }
        return dir;
    }

    public Vector3 accelOffset(float deltaTime)
    {
        Vector3 targetMod = -trueAccel * accelModRate;
        targetMod = clamDirVect(targetMod, this.maxAccelModRadius);

        accelModSmoothed = Vector3.Lerp(accelModSmoothed, targetMod, deltaTime * accelModLerpRate);

        return accelModSmoothed;
    }

    public float zoomMod(float deltaTime)
    {
        //float fwdAccel = localAccel.z;
        float fwdAccel = this.accelMagnitude;
        float targetMod = 0.0f;

        if (Mathf.Abs(fwdAccel) > minAccel)
        {

            targetMod = Mathf.Clamp(fwdAccel * zoomRate, -maxModAbs, maxModAbs);
        }

        this.currentZoomMod = Mathf.Lerp(currentZoomMod, targetMod, zoomLerpRate * deltaTime);

        return currentZoomMod;
    }

    private void FixedUpdate()
    {
        updateAccel();
    }

    void updateAccel()
    {
        Vector3 currentVel = rootRb.velocity;
        Vector3 trueAccel = (currentVel - prevVel) / Time.fixedDeltaTime;
        float accelMagnitude = (currentVel.magnitude - prevVel.magnitude) / Time.fixedDeltaTime;

        
        prevVel = currentVel;

        Debug.DrawRay(root.position, trueAccel, Color.cyan);

        
        

        //if (trueAccel.magnitude < accelDiscard || debug)
        if(accelMagnitude < accelDiscard || debug)
        {


            this.accelMagnitude = accelMagnitude;
            this.trueAccel = trueAccel;


            localAccel = root.InverseTransformDirection(trueAccel);
        }



    }


}
