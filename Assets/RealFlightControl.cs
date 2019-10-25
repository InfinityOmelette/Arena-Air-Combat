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

    public float pitchStability;
    public float pitchStabilityZeroOffset;
    public float yawStability;
   


    public float readVelocity;
    public float readVertVelocity;
    public float pitchPlaneAoA;
    public float yawPlaneAoA;

    private Rigidbody rbRef;

    // Start is called before the first frame update
    void Start()
    {
        rbRef = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //  Velocity measurements for UI
        readVelocity = rbRef.velocity.magnitude;
        readVertVelocity = Vector3.Project(rbRef.velocity, Vector3.up).y;

    }

    private void FixedUpdate()
    {
        // =============================================  UPDATE CLASS-LEVEL PHYSICS VARIABLES
        pitchPlaneAoA = calculateAlphaOnPlane(rbRef.velocity, transform.forward, transform.right);
        yawPlaneAoA = calculateAlphaOnPlane(rbRef.velocity, transform.forward, transform.up);


        //============================================== TORQUES

        //  CONTROL TORQUES  
        Vector3 controlTorque = calculateControlTorque();

        //  STABILITY TORQUE
        Vector3 pitchStabilityTorque = calculateAxisStabilityTorque(pitchStability, pitchStabilityZeroOffset, rbRef.velocity, transform.forward, transform.right);
        Vector3 yawStabilityTorque = calculateAxisStabilityTorque(yawStability, 0.0f, rbRef.velocity, transform.forward, transform.up);

        


        //=============================================  FORCES

        //  WING LIFT
        Vector3 wingLift = calculateOnPlaneResultLiftVector(wingLiftCoefficient, 0.0f, 0.25f, // LIFT: coeff, alphaOffsetLift, highAlphaShrinkLift
            wingDragCoefficient, 0.05f, 1.9f, 0.05f,                //  DRAG: coeff, offset, amplitude, parabolicity, 
            rbRef.velocity, transform.forward, transform.right);

        //  BODY SIDE LIFT
        Vector3 sideLift = calculateOnPlaneResultLiftVector(bodySideLiftCoefficient, 0.0f, 0.25f, // LIFT: coeff, alphaOffsetLift, highAlphaShrinkLift
            bodySideDragCoefficient, 0.0f, 1.9f, 0.05f,                //  DRAG: coeff, offset, amplitude, parabolicity, 
            rbRef.velocity, transform.forward, transform.up);

        //  THRUST
        Vector3 thrustVect = transform.forward * inputNewThrust();


        //============================================ ADD RESULTS

        //  ADD RESULT VORCE
        rbRef.AddForce(wingLift + sideLift + thrustVect);

        //  ADD RESULT TORQUE
        rbRef.AddTorque(controlTorque + pitchStabilityTorque + yawStabilityTorque);
    }

    private float inputNewThrust()
    {
        currentThrust = Mathf.Clamp((MAX_THRUST_DELTA * Input.GetAxis("Throttle")) + currentThrust, THRUST_MIN, THRUST_MAX);
        currentThrustPercent = (currentThrust - THRUST_MIN) / (THRUST_MAX - THRUST_MIN) * 100f;
        return currentThrust;
    }

    private Vector3 calculateControlTorque()
    {
        //  GET PITCH INPUT

        //  BUILD INPUT TORQUE VECTOR
        Vector3 pitchTorqueVect = transform.right * pitchTorque * Input.GetAxis("Pitch") * calculateControlAxisAlphaMod(transform.right);
        Vector3 yawTorqueVect = transform.up * yawTorque * Input.GetAxis("Rudder") * calculateControlAxisAlphaMod(transform.up);
        Vector3 rollTorqueVect = -transform.forward * rollTorque * Input.GetAxis("Roll");





        return (pitchTorqueVect + yawTorqueVect + rollTorqueVect) * calculateControlAuthVelocityMod();
    }

    //  CONTROL AUTHORITY -- ALPHA -- SINGLE AXIS
    private float calculateControlAxisAlphaMod(Vector3 axis)
    {
        Vector3 velocity = Vector3.ProjectOnPlane(rbRef.velocity, axis);
        return Mathf.Cos(calculateAlphaOnPlane(rbRef.velocity, transform.forward, axis) * Mathf.Deg2Rad);
    }

    //  CONTROL AUTHORITY -- VELOCITY -- ALL AXES
    private float calculateControlAuthVelocityMod()
    {
        float aeroModifier = rbRef.velocity.magnitude * rbRef.velocity.magnitude;
        float gLimitModifier;
        return aeroModifier;
    }

    //  AXIS STABILITY TORQUE
    private Vector3 calculateAxisStabilityTorque(float stabilityCoeff, float zeroOffset, Vector3 velocity, Vector3 forward, Vector3 axis)
    {
        velocity = Vector3.ProjectOnPlane(velocity, axis);
        forward = Vector3.ProjectOnPlane(forward, axis);
        float alpha = calculateAlphaOnPlane(velocity, forward, axis) * Mathf.Deg2Rad;
        float alphaMod = Mathf.Sin(alpha - zeroOffset);
        return axis * stabilityCoeff * alphaMod * velocity.magnitude * velocity.magnitude;
    }


    


    

    // returns angle in degrees
    private float calculateAlphaOnPlane(Vector3 velocity, Vector3 forward, Vector3 planeNormal)
    {
        velocity = Vector3.ProjectOnPlane(velocity, planeNormal);
        forward = Vector3.ProjectOnPlane(forward, planeNormal);
        return Vector3.SignedAngle(forward, velocity, planeNormal); // angle in degrees
    }

    private Vector3 calculateOnPlaneResultLiftVector(
        float liftCoeff, float alphaOffsetLift, float highAlphaShrinkLift,      // lift components
        float dragCoeff, float alphaOffsetDrag, float alphaAmplitudeDrag, float alphaParabolicityDrag,  // drag components 
        Vector3 velocity, Vector3 forward, Vector3 planeNormal)            // vector info
    {
        // declare all variables
        Vector3 liftVector;     // build lift vector ON THIS PLANE
        Vector3 dragVector;     // build drag vector ON THIS PLANE
        float alpha;            // angle of attack ON THIS PLANE
        float highAlphaShrinkLiftTemp;
        float alphaModLift; // 1.0 maximum lift, sign is direction
        float alphaModDrag; // 1.0 maximum lift -- because of trig, result will always be positive

        //  Use planeNormal to remove out-of-plane components of velocity and forward vector
        velocity = Vector3.ProjectOnPlane(velocity, planeNormal);  // not necessary to get angle, but will be used for magnitude
        forward = Vector3.ProjectOnPlane(forward, planeNormal);

        //  Set initial force vector directions
        liftVector = Vector3.Cross(velocity, planeNormal).normalized;
        dragVector = -velocity.normalized;

        //  Get angle between forward and velocity -- signed to plot on trig graph, neg alpha give neg lift, and drag always positive because trig
        alpha = calculateAlphaOnPlane(velocity, forward, planeNormal) * Mathf.Deg2Rad; // RADIANS
        
        //  Only measure angle of attack on wing lift plane
        if(planeNormal == transform.right)
            pitchPlaneAoA = alpha * Mathf.Rad2Deg;

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
