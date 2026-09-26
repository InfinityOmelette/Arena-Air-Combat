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

    public float internalSpeedBecauseUnityFuckingSucksSometimes = 0.0f;

    public float rudder; // value -1 to 1 --> -maxYawRate to +maxYawRate
    private float yawRate;
    public float maxYawRate;
    public float yawRateAccel;

    public float hoverTargetAlt = 10f;
    public float hoverSensitivity;

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

        steerProcess(Time.fixedDeltaTime);
    }

    private void steerProcess(float deltaTime)
    {

        if(speedSet == Speed.HALT)
        {
            yawRate = 0;
        }
        else
        {
            float targetYawRate = rudder * maxYawRate;
            float yawError = targetYawRate - yawRate;
            float absError = Mathf.Abs(yawError);

            float yawRateDelta = Mathf.Sign(yawError) * yawRateAccel * deltaTime;
            yawRateDelta = Mathf.Clamp(yawRateDelta, -absError, absError);

            float yawOut = yawRate + yawRateDelta;
            yawRate = yawOut;
        }

        // set angular velocity according to yaw rate
        myRb.angularVelocity = Vector3.up * yawRate;

        forceFlatten();
    }

    // does this allow velocity to change rotation???
    public void forceFlatten()
    {
        Vector3 eulerRot = Quaternion.ToEulerAngles(myRb.rotation) * Mathf.Rad2Deg;
        myRb.MoveRotation(Quaternion.Euler(0.0f, eulerRot.y, 0.0f));
    }

    // this does handle reverse case
    public void forwardDrive(float deltaTime)
    {
        float currSpeed = internalSpeedBecauseUnityFuckingSucksSometimes;

        float speedError = readSpeedSetting(speedSet) - currSpeed;

        float speedDelta = Mathf.Sign(speedError) * acceleration * deltaTime;

        //Debug.Log("Preclamped speedDelta: " + speedDelta);

        speedDelta = Mathf.Clamp(speedDelta, -Mathf.Abs(speedError), Mathf.Abs(speedError));

        float speedOut = currSpeed + speedDelta;

        internalSpeedBecauseUnityFuckingSucksSometimes = speedOut;


        //Debug.Log("currSpeed: " + currSpeed + ", speedError: " + speedError 
        //    + ", speedDelta: " + speedDelta + ", speedOut: "
        //    + speedOut + ", speedSet: " + speedSetValue + 
        //    ", SpeedActual: " + myRb.velocity.magnitude);

        Vector3 fwd = transform.forward;
        //fwd = new Vector3(fwd.x, 0.0f, fwd.z);
        fwd.y = 0f;

        Vector3 vert = calculateHoverYVel(); // preserve vert speed --> gravity

        myRb.velocity = fwd.normalized * internalSpeedBecauseUnityFuckingSucksSometimes + vert;
    }

    public Vector3 calculateHoverYVel()
    {
        float hoverVel = (hoverTargetAlt - transform.position.y) * hoverSensitivity;


        return new Vector3(0.0f, hoverVel, 0.0f);
    }

    public void setSpeed(Speed speed)
    {
        this.speedSet = speed;
    }

    public float readSpeedSetting(Speed speed)
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

    public void setRudder(float rudder)
    {
        this.rudder = Mathf.Clamp(rudder, -1f, 1f);
    }

    
}
