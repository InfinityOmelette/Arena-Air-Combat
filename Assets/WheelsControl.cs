using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsControl : MonoBehaviour
{

    public WheelBehavior[] wheels;
    public Rigidbody aircraftRootRB;


    public float steerReductionSpeedFactor;
    public float steerReductionBeginSpeed;
    public float parkingBrakeBelowThrottlePercent; // apply parking brake below this thrust percent
    public float parkingBrakeInput;  // applied only when speed and throttle are approximately zero
    public bool gearIsDown;

    private bool gearButtonPressed = false;
    

    // Start is called before the first frame update
    void Start()
    {
        //setGearEnabled(gearIsDown);
    }

    

    private void FixedUpdate()
    {
        processAllWheels(); // inputs that are read every frame
        checkGearInput();   // inputs that are toggle
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
        float readVel = 0.0f; // default value if unable to access
        if (aircraftRootRB != null)
            readVel = aircraftRootRB.velocity.magnitude; // only access reference if not null

        // get set steering limit based on speed
        float steerInput = (steerReductionSpeedFactor) /
          (steerReductionSpeedFactor + readVel - steerReductionBeginSpeed);

        // get rudder input.
        steerInput = Mathf.Abs(steerInput) * Input.GetAxis("Rudder");


        // clamp and return steering input
        return Mathf.Clamp(steerInput, -1.0f, 1.0f); ; // (a / (a+x)) graph to approach 0 at increasing x, starting val 1 at x = 0

    }

    // calculate brake input
    private float brakeInputProcess()
    {
        float brakeInput = 0.0f;

        // Check that throttle is below necessary throttle to apply brake
        if(aircraftRootRB.GetComponent<RealFlightControl>().currentThrottlePercent < parkingBrakeBelowThrottlePercent)
        {
            // negative so that decreasing throttle will have positive brake input
            brakeInput = -Input.GetAxis("Throttle");
            brakeInput = Mathf.Clamp(brakeInput + parkingBrakeInput, 0.0f, 1.0f);
        }
        
        return brakeInput; // 1.0 is max brake, 0 is no brake
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
