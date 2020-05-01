using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsControl : MonoBehaviour
{

    public WheelBehavior[] wheels;
    public Rigidbody root_RB;
    private EngineControl root_Engine;


    public float steerReductionSpeedFactor;
    public float steerReductionBeginSpeed;
    public float parkingBrakeBelowThrottlePercent; // apply parking brake below this thrust percent
    public float parkingBrakeInput;  // applied only when speed and throttle are approximately zero
    public bool gearIsDown;

    private bool gearButtonPressed = false;

    public bool brakeCurrentlyApplied = false;

    public float input_brakeAxis;
    public float input_rudderAxis;
    public float input_gear_button;

    

    // Start is called before the first frame update
    void Start()
    {
        setGearEnabled(gearIsDown);
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
        if (root_RB != null)
            readVel = root_RB.velocity.magnitude; // only access reference if not null

        // get set steering limit based on speed
        float steerInput = Mathf.Abs((steerReductionSpeedFactor) /
          (steerReductionSpeedFactor + readVel - steerReductionBeginSpeed));

        // limit extremely high 1/x values to 1
        steerInput = Mathf.Clamp(steerInput, 0.0f, 1.0f);

        // factor in rudder input.
        steerInput *= input_rudderAxis;


        // clamp and return steering input
        return steerInput; // (a / (a+x)) graph to approach 0 at increasing x, starting val 1 at x = 0

    }

    // calculate brake input
    private float brakeInputProcess()
    {
        float brakeInput = 0.0f;
        brakeCurrentlyApplied = false;

        // Check that throttle is below necessary throttle to apply brake
        if (root_RB.GetComponent<EngineControl>().currentThrottlePercent < parkingBrakeBelowThrottlePercent)
        {
            // negative so that decreasing throttle will have positive brake input
            brakeInput = -input_brakeAxis;
            brakeInput = Mathf.Clamp(brakeInput + parkingBrakeInput, 0.0f, 1.0f);
            if (brakeInput > parkingBrakeInput + 0.1f) // brake is definitely applied by player
                brakeCurrentlyApplied = true;
        }
        
        
        return brakeInput; // 1.0 is max brake, 0 is no brake
    }

    // toggle gear on gear button press
    private bool checkGearInput()
    {
        bool pressedRightNow = input_gear_button > 0.5f; // if horiz axis is definitely positive
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
