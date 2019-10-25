using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    public float MAX_POSITIVE_G;
    public float POS_OVER_G_SLOPE;
    public float MAX_NEGATIVE_G;
    public float NEG_OVER_G_SLOPE;
    public float readCurrentG;

    public float readVelocity;
    public float readVertVelocity;
    public float pitchPlaneAoA;
    public float yawPlaneAoA;

    public float weightReduction;

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


        //============================================== TORQUES

        //  CONTROL TORQUES  
        //Vector3 controlTorque = calculateControlTorque();
        float velocityMod = calculateControlAuthVelocityMod();  // control authority
        Vector3 pitchTorqueVect = transform.right * pitchTorque * velocityMod * Input.GetAxis("Pitch") * calculateControlAxisAlphaMod(transform.right); 
        Vector3 rollTorqueVect = transform.up * yawTorque * velocityMod * Input.GetAxis("Rudder") * calculateControlAxisAlphaMod(transform.up);
        Vector3 yawTorqueVect = -transform.forward * velocityMod * rollTorque * Input.GetAxis("Roll");


        // if maximum lift reached, set maximum torque to zero
        float currentG = wingLift.magnitude / rbRef.mass;
        readCurrentG = currentG;
  
        pitchTorqueVect *= calculateGLimitMod(wingLift, transform.up, rbRef.mass, MAX_POSITIVE_G, POS_OVER_G_SLOPE,
            MAX_NEGATIVE_G, NEG_OVER_G_SLOPE);

        //float yawGLimitScale = 0.001f; // decrease maximum, increase slope
        //yawTorqueVect *= calculateGLimitMod(sideLift, transform.right, rbRef.mass, MAX_POSITIVE_G * yawGLimitScale, POS_OVER_G_SLOPE / yawGLimitScale, 
        //    MAX_POSITIVE_G * yawGLimitScale, POS_OVER_G_SLOPE / 2f);    // both positive and negative use the same sign, because side G limit symmetrical



        //  STABILITY TORQUE
        Vector3 pitchStabilityTorque = calculateAxisStabilityTorque(pitchStability, pitchStabilityZeroOffset, rbRef.velocity, transform.forward, transform.right);
        Vector3 yawStabilityTorque = calculateAxisStabilityTorque(yawStability, 0.0f, rbRef.velocity, transform.forward, transform.up);


        //============================================ ADD RESULTS

        //  ADD RESULT VORCE
        rbRef.AddForce(wingLift + sideLift + thrustVect);

        //  ADD RESULT TORQUE
        rbRef.AddTorque(pitchTorqueVect + rollTorqueVect + yawTorqueVect + pitchStabilityTorque + yawStabilityTorque);
    }

    private float calculateGLimitMod(Vector3 lift, Vector3 upOrient, float mass, 
        float MAX_POSITIVE_G, float pos_gLimitSlope, 
        float MAX_NEGATIVE_G, float neg_gLimitSlope)
    {
        float gLimitMod = 1.0f;
        float currentG = lift.magnitude / mass; // F/m = a

        // set sign of G
        if (Vector3.Angle(lift, upOrient) > 90f) // Determine if vectors are in the same hemisphere -- if not, lift is "negative"
            currentG *= -1; // set direction

        // increase gLimitMod the higher currentG is over max g
        if (currentG > MAX_POSITIVE_G)
            gLimitMod -= pos_gLimitSlope * (currentG - MAX_POSITIVE_G); // over G gap is squared
        else if (currentG < -MAX_NEGATIVE_G)
            gLimitMod -= neg_gLimitSlope *  (-MAX_NEGATIVE_G - currentG); // over G gap is squared

        return Mathf.Max(0.0f, gLimitMod);  // modifier will never below zero -- negative control authority

    }

    private float inputNewThrust()
    {
        currentThrust = Mathf.Clamp((MAX_THRUST_DELTA * Input.GetAxis("Throttle")) + currentThrust, THRUST_MIN, THRUST_MAX);
        currentThrustPercent = (currentThrust - THRUST_MIN) / (THRUST_MAX - THRUST_MIN) * 100f;
        return currentThrust;
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
        return rbRef.velocity.magnitude * rbRef.velocity.magnitude; ;
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
