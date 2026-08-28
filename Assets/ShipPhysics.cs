using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShipPhysics : MonoBehaviour
{
    public enum Speed
    {
        REVERSE,
        HALT,
        SLOW,
        CRUISE,
        FLANK
    }

    public float reverseSpeed;
    public float slowSpeed;
    public float cruiseSpeed;
    public float flankSpeed;


    public Rigidbody myRb;

    public Speed speedSet;

    public float acceleration;


    public float maxRotSpeed;

    public float internalSpeedBecauseUnityFuckingSucksSometimes = 0.0f;


    private void Awake()
    {
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
        forwardDrive(Time.fixedDeltaTime);
    }

    // this doesn't handle reverse speed case
    public void forwardDrive(float deltaTime)
    {
        float currSpeed = internalSpeedBecauseUnityFuckingSucksSometimes;

        float speedError = speedSetting(speedSet) - currSpeed;

        float speedDelta = Mathf.Sign(speedError) * acceleration * deltaTime;

        //Debug.Log("Preclamped speedDelta: " + speedDelta);

        speedDelta = Mathf.Clamp(speedDelta, -Mathf.Abs(speedError), Mathf.Abs(speedError));

        float speedOut = currSpeed + speedDelta;

        internalSpeedBecauseUnityFuckingSucksSometimes = speedOut;


        //Debug.Log("currSpeed: " + currSpeed + ", speedError: " + speedError 
        //    + ", speedDelta: " + speedDelta + ", speedOut: "
        //    + speedOut + ", speedSet: " + speedSetValue + 
        //    ", SpeedActual: " + myRb.velocity.magnitude);

        myRb.velocity = transform.forward * internalSpeedBecauseUnityFuckingSucksSometimes;
    }



    public float speedSetting(Speed speed)
    {
        float speedOut = 0;
        switch (speed)
        {
            case Speed.REVERSE:
                speedOut = reverseSpeed;
                break;

            case Speed.SLOW:
                speedOut = slowSpeed;
                break;

            case Speed.CRUISE:
                speedOut = cruiseSpeed;
                break;

            case Speed.FLANK:
                speedOut = flankSpeed;
                break;
        }

        return speedOut;
    }
}
