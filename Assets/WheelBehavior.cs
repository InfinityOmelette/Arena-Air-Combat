using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelBehavior : MonoBehaviour
{
    public WheelCollider wheelCollider;

    private Vector3 wheelCCenter;
    private RaycastHit hit;

    
    public float maxSteerAngle;
    public float steerRate; // per physics update
    public float brakeTorque;
    

    void Start()
    {

    }

    void Update()
    { 
        // align mesh with collider
        alignExtension();
        alignRotation();

        wheelCollider.motorTorque = 0.0001f; // to escape "park brake" mode
    }

    // called externally
    // 1.0 arg gives max steer right, -1.0 max steer left (sign in this comment might be off)
    public bool doSteer(float steerInput)
    {
        bool didSteer = false;
        if (!Mathf.Approximately(maxSteerAngle, 0.0f)) // don't bother processing steer if max steer angle nearly zero
        {
            // step steering angle towards input * maxSteerAngle, step size of steer rate
            float steerTarget = Mathf.MoveTowards(wheelCollider.steerAngle, steerInput * maxSteerAngle, steerRate);

            // clamp steering angle to steering limit
            wheelCollider.steerAngle = Mathf.Clamp(steerTarget, -maxSteerAngle, maxSteerAngle);

            didSteer = true; // wheel did steer
        }

        return didSteer; // return whether wheel did steer
    }

    // called externally
    // 1.0 arg gives max brake
    public bool doBrake(float brakeInput)
    {
        bool didBrake = false;
        if (!Mathf.Approximately(brakeTorque, 0.0f))// don't bother processing brake if brake torque nearly zero
        {
            didBrake = true;
            wheelCollider.brakeTorque = Mathf.Clamp(brakeInput * brakeTorque, 0.0f, brakeTorque);
        }
        return didBrake; // return whether wheel did brake
    }


    


    
    private void alignExtension()
    {
        wheelCCenter = wheelCollider.transform.position + wheelCollider.center;

        // cast a ray from wheel center, in the downwards direction, the length of suspension distance plus radius
        // check if this ray collides with anything
        //  save the collision point into hit
        if (Physics.Raycast(wheelCCenter, -wheelCollider.transform.up, out hit, wheelCollider.suspensionDistance + wheelCollider.radius))
        {
            // if ray collided, move wheel to position where its edge contacts point
            transform.position = hit.point + (wheelCollider.transform.up * wheelCollider.radius);
        }
        else
        {
            // if ray didn't collide, move wheel to full suspension extension
            transform.position = wheelCCenter - (wheelCollider.transform.up * wheelCollider.suspensionDistance);
        }
    }

    private void alignRotation()
    {
        
         // y rotation for steering
         // z rotation so that cylinder is sideways like wheel
         transform.localEulerAngles = new Vector3(0.0f, wheelCollider.steerAngle, 90f);
        
    }


    
}
