using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*  
 *          
 *  aceCombatThrottleProcess()
 *      - Fundamentally unrealistic, so this will be drastically simplified
 *      - Thrust can change rapidly -- constant thrustDelta steps
 *      - MAYBE include afterburner stage? multiplies thrustDelta by x above thrust y
 *  
 *  
 *  calculateControlAuthorityByThrust() --> change to by FORWARD velocity
 *      - Set cruise thrust to be at optimal turn speed in level flight (mess with flight variables until it's at 30-50% thrust
 *      - Decreases linearly specified rate above corner velocity
 *      
 *      
 *  calculateStabilityTorque() -- REDO TO USE ANGLE OF ATTACK ALONG 2 PLANES
 *      - Slight torque to point nose towards velocity
 *      - slip axis for vertical and lateral velocity
 *          - vertical slip pitch
 *          - lateral slip yaw
 *          - lateral slip roll
 *          --> (axis velocity)^2 * axis stability ratio * normalized cross vector
 *      - Increased angle of attack will increase stability torque
 *  
 * 
 *  processSlipTorques() -- made obselete by stability torque
 * 
 */

public class RealFlightControl : MonoBehaviour
{

    public float rollTorque;
    public float pitchTorque;
    public float yawTorque;

    public float currentThrust;
    public float currentThrustPercent;
    public float MAX_THRUST_DELTA;
    public float THRUST_MIN;
    public float THRUST_MAX;

    public float wingLiftCoefficient;
    public float wingDragCoefficient;

    public float bodySideLiftCoefficient;
    public float bodySideDragCoefficient;


    public float readVelocity;
    public float readVertVelocity;
    public float readAoA;

    private Rigidbody rbRef;

    // Start is called before the first frame update
    void Start()
    {
        rbRef = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

        readVelocity = rbRef.velocity.magnitude;
        readVertVelocity = Vector3.Project(rbRef.velocity, Vector3.up).y;

        
        rbRef.AddTorque(calculateControlTorque() * calculateControlAuthorityByVelocity());



        Vector3 wingLift = calculateOnPlaneResultLiftVector(wingLiftCoefficient, 0.0f, 0.25f, // LIFT: coeff, alphaOffsetLift, highAlphaShrinkLift
            wingDragCoefficient, 0.05f, 1.9f, 0.05f,                //  DRAG: coeff, offset, amplitude, parabolicity, 
            rbRef.velocity, transform.forward, transform.right);

        Vector3 sideLift = calculateOnPlaneResultLiftVector(bodySideLiftCoefficient, 0.0f, 0.25f, // LIFT: coeff, alphaOffsetLift, highAlphaShrinkLift
            bodySideDragCoefficient, 0.0f, 1.9f, 0.05f,                //  DRAG: coeff, offset, amplitude, parabolicity, 
            rbRef.velocity, transform.forward, transform.up);

        currentThrust = getNewThrust();
        Vector3 thrustVect = transform.forward * getNewThrust();

        rbRef.AddForce(wingLift + sideLift + thrustVect);
    }

    private float getNewThrust()
    {
        currentThrust = Mathf.Clamp((MAX_THRUST_DELTA * Input.GetAxis("Throttle")) + currentThrust, THRUST_MIN, THRUST_MAX);
        currentThrustPercent = (currentThrust - THRUST_MIN) / (THRUST_MAX - THRUST_MIN) * 100f;
        return currentThrust;
    }

    private Vector3 calculateControlTorque()
    {
        //  GET PITCH INPUT
        float pitchInput = Input.GetAxis("Pitch");

        //  BUILD INPUT TORQUE VECTOR
        Vector3 torqueVect = -transform.forward * rollTorque * Input.GetAxis("Roll") +  // Roll torque
            transform.right * pitchTorque * pitchInput +                    // Pitch torque
            transform.up * yawTorque * Input.GetAxis("Rudder");                         // Yaw torque

        return torqueVect;
    }

    private float calculateControlAuthorityByVelocity()
    {
        //// cruise thrust is set to be highest turn rate
        //// above this, G limit is limiting factor to turn rate
        //// below this, aerodynamic performance is limiting factor to turn rate


        //float returnAuth = 0.0f;

        //// above cruise thrust
        //float distanceToMax = THRUST_MAX - thrust;
        //float upRange = THRUST_MAX - THRUST_CRUISE;

        //// below cruise thrust
        //float distanceToMin = thrust - THRUST_MIN;
        //float downRange = THRUST_CRUISE - THRUST_MIN;

        //float percentTowardLimit;

        //if (thrust > THRUST_CRUISE) // thrust is above cruise
        //{
        //    // simulating G-limit as limiting factor to turn rate
        //    percentTowardLimit = 1.0f - distanceToMax / upRange;
        //    returnAuth = CONTROL_AUTHORITY_CRUISE - percentTowardLimit *
        //        (CONTROL_AUTHORITY_CRUISE - CONTROL_AUTHORITY_FASTEST);
        //}
        //else // thrust is below cruise
        //{
        //    // simulate aerodynamic performance as limiting factor to turn rate
        //    percentTowardLimit = 1.0f - distanceToMin / downRange;
        //    returnAuth = CONTROL_AUTHORITY_CRUISE - percentTowardLimit *
        //        (CONTROL_AUTHORITY_CRUISE - CONTROL_AUTHORITY_SLOW);
        //}

        //currentControlAuthority = returnAuth;
        //return returnAuth;
        return 1.0f;
    }


    private Vector3 calculateOnPlaneResultLiftVector(
        float liftCoeff, float alphaOffsetLift, float highAlphaShrinkLift,      // lift components
        float dragCoeff, float alphaOffsetDrag, float alphaAmplitudeDrag, float alphaParabolicityDrag,  // drag components 
        Vector3 velocity, Vector3 forward, Vector3 planeCrossVector)            // vector info
    {
        // declare all variables
        Vector3 liftVector;     // build lift vector ON THIS PLANE
        Vector3 dragVector;     // build drag vector ON THIS PLANE
        float alpha;            // angle of attack ON THIS PLANE
        float highAlphaShrinkLiftTemp;
        float alphaModLift; // 1.0 maximum lift, sign is direction
        float alphaModDrag; // 1.0 maximum lift -- because of trig, result will always be positive

        //  Use planeCrossVector to remove out-of-plane components of velocity and forward vector
        velocity = Vector3.ProjectOnPlane(velocity, planeCrossVector);  // not necessary to get angle, but will be used for magnitude
        forward = Vector3.ProjectOnPlane(forward, planeCrossVector);

        //  Set initial force vector directions
        liftVector = Vector3.Cross(velocity, planeCrossVector).normalized;
        dragVector = -velocity.normalized;

        //  Get angle between forward and velocity -- signed to plot on trig graph, neg alpha give neg lift, and drag always positive because trig
        alpha = Vector3.SignedAngle(forward, velocity, planeCrossVector) * Mathf.Deg2Rad; // RADIANS
        if(planeCrossVector == transform.right)
            readAoA = alpha * Mathf.Rad2Deg;

        //  Reduce lift when flying backwards
        highAlphaShrinkLiftTemp = 1.0f;
        if (Mathf.Abs(alpha) > (Mathf.PI / 2))  // if alpha greater than 90 degrees
            highAlphaShrinkLiftTemp = highAlphaShrinkLift;  // reduce lift

        //  plot lift and drag vs alpha graphs on desmos to set the alphaMod values (1.0 roughly being highest expected force)
        alphaModLift = highAlphaShrinkLiftTemp * Mathf.Sin(2 * alpha) + alphaOffsetLift;  // 2 to increase period such that 90 gives max, 180 is zero, and so on
        alphaModDrag = alphaAmplitudeDrag * (-(Mathf.Cos(alpha) * Mathf.Cos(alpha)) + 1) +
            alphaParabolicityDrag * alpha * alpha + alphaOffsetDrag;

        //  Lift calculation:
        liftVector *= liftCoeff * alphaModLift * velocity.magnitude * velocity.magnitude;
        dragVector *= dragCoeff * alphaModDrag * velocity.magnitude * velocity.magnitude;

        //  Add vectors together
        return liftVector + dragVector;
    }

}
