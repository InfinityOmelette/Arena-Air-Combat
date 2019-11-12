﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsControl : MonoBehaviour
{

    public WheelBehavior[] wheels;
    public Rigidbody aircraftRootRB;


    public float steerReductionSpeedFactor;
    public bool gearIsDown;

    private bool gearButtonPressed = false;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    private void FixedUpdate()
    {
        processAllWheels();
        checkGearInput();
    }

    // steering and braking for all wheels
    private void processAllWheels()
    {
        if (gearIsDown) // don't bother looping through wheels if they're not down
        {
            // Loop through all wheels
            for (int i = 0; i < wheels.Length; i++)
            {
                //  STEER PROCESS
                wheels[i].doSteer(steerInputProcess());

                //  BRAKE PROCESS
                wheels[i].doBrake(brakeInputProcess());

                //  gear check not included for each wheel every frame to reduce cpu load
            }
        }
    }

    // calculate steering input, factoring in speed limiting
    private float steerInputProcess()
    {
        // get velocity from root parent
        float readVel = 0.0f;
        if (aircraftRootRB != null)
            readVel = aircraftRootRB.velocity.magnitude; // only access reference if not null

        // set steering
        return (steerReductionSpeedFactor * Input.GetAxis("Rudder")) /
          (steerReductionSpeedFactor + readVel); // (a / (a+x)) graph to approach 0 at increasing x, starting val 1 at x = 0

    }

    // calculate brake input
    private float brakeInputProcess()
    {
        // negative so that decreasing throttle will have positive brake input
        return -Input.GetAxis("Throttle");
    }

    // toggle gear on gear button press
    private bool checkGearInput()
    {
        bool pressedRightNow = Input.GetAxis("D-Pad Horiz") > 0.5f; // if horiz axis is definitely positive
        if(pressedRightNow != gearButtonPressed && pressedRightNow) // value changed on this step, and is pressed
        {
            gearIsDown = setGearEnabled(!gearIsDown); // toggle gear down
        }
        

        return gearButtonPressed = pressedRightNow;
    }

    // command all wheels to raise or lower
    public bool setGearEnabled(bool enabled)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].setWheelLowered(enabled);
        }
        

        return enabled;
    }



}
