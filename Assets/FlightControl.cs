using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 *  To Do:  - increase pitch drag
 *          - Make pitch and nose drop slip independent of roll (decrease when fully inverted, slightly increase 90 degrees bank)
 *          - Test Edit
 *          - Blah blah blah
 *          - mega blah
 */





public class FlightControl : MonoBehaviour
{



    public float triggerValue;


    public float thrust = 20f;
    public float thrustDelta;
    public float thrustTarget;
    private float THRUST_CRUISE = 100f;
    public float CRUISE_PERCENT = 35f;
    public float THRUST_MAX = 150f;
    public float THRUST_MIN = 60f;
    public float throttleDeadzone = 0.12f;

    public float lift = 1f;                 // not currently implemented
    public float artificialWeight = 5f;     // not currently implemented


    public float pitchTorque = 1f;
    public float rollTorque = 3.5f;
    public float yawTorque = 0.3f;
    public float pitchDragRate = 0.1f;


    //public float controlAuthorityRateAero = 0.3f;       //  0.01
    //public float controlAuthorityOffsetAero = 0.1f;     //  -0.2
    //public float controlAuthorityRateGLimit = 0.3f;     //  -0.005
    //public float controlAuthorityOffsetGLimit = 0.1f;   //  1.35
    public float CONTROL_AUTHORITY_CRUISE = 1.0f;
    public float CONTROL_AUTHORITY_SLOW;        //  limited by aerodynamic performance
    public float CONTROL_AUTHORITY_FASTEST;     //  limited by G limit
    public float currentControlAuthority;
    
    

    public float yawSlipRate = 0.5f;
    public float pitchSlipRate = 0.5f;      //  .0051
    public float rollSlipRate = 0.5f;       //  .0025
    public float noseDropSlipRate = 0.5f;   //  .003525

    


    public float readZRoll;
    public float alternateReadZRoll;
    public float readTempThrustDelta;
    public float readVelocity;

   // public float torque;



    private Rigidbody rbRef;

	// Use this for initialization
	void Start ()
    {
        rbRef = gameObject.GetComponent<Rigidbody>();

        THRUST_CRUISE = (THRUST_MAX - THRUST_MIN) * CRUISE_PERCENT/100f + THRUST_MIN;
	}
	
	// Update is called once per frame
	void Update ()
    {
        triggerValue = Input.GetAxis("Throttle");


        readVelocity = rbRef.velocity.magnitude;
        //readZRoll = transform.rotation.z;
        
    }


    void FixedUpdate()
    {
        //  =============  PITCH, ROLL, YAW TORQUES  ================================================================

        //  GET PITCH INPUT
        float pitchInput = Input.GetAxis("Pitch");

        //  BUILD INPUT TORQUE VECTOR
        Vector3 torqueVect = -transform.forward * rollTorque * Input.GetAxis("Roll") +  // Roll torque
            transform.right * pitchTorque * pitchInput +                    // Pitch torque
            transform.up * yawTorque * Input.GetAxis("Rudder");                         // Yaw torque

        //  CONTROL AUTHORITY
        torqueVect *= calculateControlAuthorityByThrust();  // control authority (higher speed -> more authority)
        
        // ROLL, PITCH, NOSE SLIP (induced by roll)
        torqueVect += processSlipTorques();

        // if using transform.rotation.z -- 0.285
        // if using roll -- 0.005
        // Apply resulting torque vector
        rbRef.AddTorque(torqueVect);


        aceCombatThrottleProcess();
        

        // COMBINE AND APPLY FORCES
        Vector3 forceVect = transform.forward * thrust; // THRUST
        forceVect += transform.up * lift;               // LIFT
        forceVect += Vector3.down * artificialWeight;   // WEIGHT
        forceVect += -rbRef.velocity * absVal(pitchInput) * pitchDragRate;   // PITCH DRAG
        rbRef.AddForce(forceVect);  // APPLY TOTAL FORCE VECTOR



    }

    //private float calculateControlAuthorityByVelocity()
    //{
    //    float returnAuth = 0.0f;
    //    float authorityAeroLimit = rbRef.velocity.magnitude * controlAuthorityRateAero + controlAuthorityOffsetAero;      // Increases with speed
    //    float authorityGLimit = rbRef.velocity.magnitude * controlAuthorityRateGLimit + controlAuthorityOffsetGLimit;           // Decreases with speed

    //    //  SET AUTHORITY TO LOWEST OF THE TWO -- THE LIMITING FACTOR
    //    if (authorityAeroLimit < authorityGLimit)
    //        returnAuth = authorityAeroLimit;    // Airframe can structurally sustain higher G load, but aerodynamic performance won't reach it
    //    else
    //        returnAuth = authorityGLimit;       // Aerodynamic performance can apply higher G load, but airframe won't structurally support it

    //    readCurrentControlAuthority = returnAuth;
    //    return returnAuth;
    //}



    private float calculateControlAuthorityByThrust()
    {
        // cruise thrust is set to be highest turn rate
        // above this, G limit is limiting factor to turn rate
        // below this, aerodynamic performance is limiting factor to turn rate


        float returnAuth = 0.0f;

        // above cruise thrust
        float distanceToMax = THRUST_MAX - thrust;
        float upRange = THRUST_MAX - THRUST_CRUISE;

        // below cruise thrust
        float distanceToMin = thrust - THRUST_MIN;
        float downRange = THRUST_CRUISE - THRUST_MIN;

        float percentTowardLimit;

        if (thrust > THRUST_CRUISE) // thrust is above cruise
        {
            // simulating G-limit as limiting factor to turn rate
            percentTowardLimit = 1.0f - distanceToMax / upRange;
            returnAuth = CONTROL_AUTHORITY_CRUISE - percentTowardLimit * 
                ( CONTROL_AUTHORITY_CRUISE - CONTROL_AUTHORITY_FASTEST);
        }
        else // thrust is below cruise
        {
            // simulate aerodynamic performance as limiting factor to turn rate
            percentTowardLimit = 1.0f - distanceToMin / downRange;
            returnAuth = CONTROL_AUTHORITY_CRUISE - percentTowardLimit * 
                (CONTROL_AUTHORITY_CRUISE - CONTROL_AUTHORITY_SLOW);
        }

        currentControlAuthority = returnAuth;
        return returnAuth;
    }


    private void aceCombatThrottleProcess()
    {


        //  THROTTLE INPUT -- SET TARGET THRUST
        float throttleInput = Input.GetAxis("Throttle");
        thrustTarget = THRUST_CRUISE + throttleInput * (THRUST_CRUISE - THRUST_MIN);



        // THROTTLE CHANGE -- MOVE THRUST TOWARD TARGET THRUST
        //float thrustTolerance = thrustDelta + 0.1f;
        float distanceToLimit = 0.0f;
        float totalRange = THRUST_MAX - THRUST_MIN;
        float tempThrustDelta = thrustDelta;


        
        //  FIND DISTANCE TO LIMIT
        if (thrust > thrustTarget)  //  DECREASING THROTTLE
        {
            distanceToLimit = thrust - THRUST_MIN; 
        }
        else if (thrust < thrustTarget) // INCREASING THROTTLE
        {
            distanceToLimit = THRUST_MAX - thrust;
        }
        else
            tempThrustDelta = 0;

        tempThrustDelta *= distanceToLimit / totalRange;   // thrustDelta approaches zero as distance to limit approaches zero

        readTempThrustDelta = tempThrustDelta;
        thrust = Mathf.MoveTowards(thrust, thrustTarget, tempThrustDelta);  // step towards target, max step as tempThrustDelta
        
    }


    private Vector3 processSlipTorques()
    {
        Vector3 returnTorque = new Vector3(0f, 0f, 0f);


        //  ROLL IS NO LONGER A FACTOR FOR PITCH AND NOSE DROP SLIP
        //  multiply original by roughly 120 to get equivalent slip rate at 120 degrees applied to all degrees

        //  ROLL IS ONLY USED FOR ROLL SLIP

        

        // ===================================  FOR PITCH SLIP -- ROLL MEASURED WITH 180 AS MAXIMUM
        Vector3 rollPlaneVect = transform.up;
        Vector3 horizPlaneFwdVect = transform.forward;
        horizPlaneFwdVect.y = 0;  // transform forward vector onto horizontal plane
        horizPlaneFwdVect = Vector3.Project(transform.up, horizPlaneFwdVect);   
        rollPlaneVect -= horizPlaneFwdVect;                                     // TRANSPOSE LIFT VECTOR ONTO ROLL PLANE
        float rollAlternateMethod = Vector3.Angle(Vector3.up, rollPlaneVect);
        alternateReadZRoll = rollAlternateMethod;

        returnTorque += -transform.right * currentControlAuthority * pitchSlipRate; // PITCH SLIP


        // =============================  NOSE DROP -- ROLL MEASURED WITH 180 AS MAXIMUM
        Vector3 noseDropAxis = Vector3.Cross(Vector3.up, transform.forward).normalized; // Perpendicular vector between up and forward
        returnTorque += noseDropAxis * currentControlAuthority * noseDropSlipRate; // NOSE DROP SLIP



        // ===========================   FOR YAW AND ROLL SLIP -- ROLL MEASURED WITH 90 AS MAXIMUM
        var fwd = transform.forward;
        fwd.y = 0;
        fwd *= Mathf.Sign(transform.up.y);
        var right = Vector3.Cross(Vector3.up, fwd).normalized;
        float roll = Vector3.Angle(right, transform.right) * Mathf.Sign(transform.right.y);
        readZRoll = roll;

        //returnTorque += -transform.up * roll * yawSlipRate;                               // YAW SLIP
        returnTorque += transform.forward * roll * 
            rollSlipRate * currentControlAuthority;  // ROLL SLIP


        return returnTorque;
    }

    private float absVal(float val)
    {
        float returnVal = val;
        if (val < 0)
            returnVal *= -1;
        return returnVal;
    }

    private float clampFloat(float val, float min, float max)
    {
        float returnVal = val;
        if (val < min)
            returnVal = min;
        else if (val > max)
            returnVal = max;
        return returnVal;
    }
}
