using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RealFlightControl : MonoBehaviour
{

    public bool debug;

    public float input_pitch;
    public float input_roll;
    public float input_yaw;

    public float rollTorque;
    public float pitchTorque;
    public float yawTorque;
    public float pitchTrim;

    public float downPitchMultiplier;

    //public float currentThrust;
    //public float currentThrustPercent;
    //public float MAX_THRUST_DELTA;
    //public float THRUST_MIN;
    //public float THRUST_MAX;
    //public float currentThrottlePercent; // 0-100 to stay consistent with current thrust percent
    
    //public float MAX_THROTTLE_DELTA;
    //public float throttleAccel;
    //public float currentThrottleDelta;
    

    public float wingLiftCoefficient;
    public float wingDragCoefficient;
    public float wingDragOffset;

    public float bodySideLiftCoefficient;
    public float bodySideDragCoefficient;
    public float bodySideDragOffset;

    public float totalParasiticDrag;

    public float pitchStability;
    public float pitchStabilityZeroOffset;
    public float yawStability;


    public float readCurrentG;
    public float maxControlAuthority;
    public float aeroPerformVelSensitivity;
    public float gLimitVelSensitivity;
    public float readCurrentAuthMod;

    public float readVelocity;
    public float readVertVelocity;
    public float pitchPlaneAoA;
    public float yawPlaneAoA;


    private AirEnvironmentStats air;
    private Rigidbody rbRef;

    // Start is called before the first frame update
    void Start()
    {
        rbRef = GetComponent<Rigidbody>();
        air = findAirStats("AirEnvironmentProperties");

    }

    private AirEnvironmentStats findAirStats(string statsObjName)
    {
        AirEnvironmentStats airRef = GameObject.Find(statsObjName).GetComponent<AirEnvironmentStats>();
        if (airRef == null)
            Debug.Log("No AirEnvironmentStats object found.");
        return airRef;
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

        // Angle of attack on pitch and yaw planes
        pitchPlaneAoA = calculateAlphaOnPlane(rbRef.velocity, transform.forward, transform.right);
        yawPlaneAoA = calculateAlphaOnPlane(rbRef.velocity, transform.forward, transform.up);



        //=============================================  FORCES

        //  WING LIFT
        Vector3 wingLift = calculateOnPlaneResultLiftVector(wingLiftCoefficient, 0.0f, 0.25f, // LIFT: coeff, alphaOffsetLift, highAlphaShrinkLift
            wingDragCoefficient, wingDragOffset + totalParasiticDrag, 1.9f, 0.05f,                //  DRAG: coeff, offset, amplitude, parabolicity, 
            rbRef.velocity, transform.forward, transform.right);
        readCurrentG = wingLift.magnitude / rbRef.mass; // read current G accel

        //  BODY SIDE LIFT
        Vector3 sideLift = calculateOnPlaneResultLiftVector(bodySideLiftCoefficient, 0.0f, 0.25f, // LIFT: coeff, alphaOffsetLift, highAlphaShrinkLift
            bodySideDragCoefficient, bodySideDragOffset + totalParasiticDrag, 1.9f, 0.05f,                //  DRAG: coeff, offset, amplitude, parabolicity, 
            rbRef.velocity, transform.forward, transform.up);
        

        //============================================== TORQUES

        //  CONTROL TORQUE MODIFIERS
        //Vector3 controlTorque = calculateControlTorque();
        float authMod = calculateControlAuthVelocityMod(aeroPerformVelSensitivity, gLimitVelSensitivity, maxControlAuthority, rbRef.velocity);
        readCurrentAuthMod = authMod;

        
        // CONTROL TORQUE VECTORS
        Vector3 pitchTorqueVect = transform.right * pitchTorque * authMod * processPitchInput(pitchTrim) * calculateControlAxisAlphaMod(transform.right); 
        Vector3 yawTorqueVect = transform.up * yawTorque * authMod * input_yaw * calculateControlAxisAlphaMod(transform.up);
        Vector3 rollTorqueVect = -rbRef.velocity.normalized * rollTorque * authMod * input_roll; // roll around velocity axis

        if (debug)
        {
            Debug.Log("Pitch magnitude: " + pitchTorqueVect.magnitude + ", yaw magnitude: " + yawTorqueVect.magnitude + "pitchInputProcess: " + 
                processPitchInput(pitchTrim) + ", pitch controlAxisAlphaMod: " + calculateControlAxisAlphaMod(transform.right));
            Debug.DrawRay(transform.position, pitchTorqueVect.normalized * 30f, Color.magenta);
            Debug.DrawRay(transform.position, yawTorqueVect.normalized * 30f, Color.cyan);
        }


        //  STABILITY TORQUE
        Vector3 pitchStabilityTorque = calculateAxisStabilityTorque(pitchStability, pitchStabilityZeroOffset, rbRef.velocity, transform.forward, transform.right);
        Vector3 yawStabilityTorque = calculateAxisStabilityTorque(yawStability, 0.0f, rbRef.velocity, transform.forward, transform.up);

        


        //============================================ ADD RESULT VECTORS

        float airDensity = 1.0f; // default value if no air object found
        if (air != null)
            airDensity = air.getDensityAtAltitude(transform.position.y);

        //  ADD RESULT VORCE
        rbRef.AddForce((wingLift + sideLift) * airDensity); // removed thrust
        

        //  ADD RESULT TORQUE
        rbRef.AddTorque((pitchTorqueVect + rollTorqueVect + yawTorqueVect + pitchStabilityTorque + yawStabilityTorque) * airDensity);
        
        

    }

    //  PITCH INPUT
    private float processPitchInput(float pitchTrim)
    {
        //  remember, negative Pitch axis is positive pitch
        // compress same side of trim pitch range
        // stretch opposite side of trim pitch range


        float pitchPosRange = 1.0f * downPitchMultiplier - pitchTrim; // distance from trim to positive (pitch down) gimbal limit
        float pitchNegRange = Mathf.Abs(-1.0f - pitchTrim); // distance from trim to negative (pitch up) gimbal limit
        float pitchInput = input_pitch;
       // readRawPitch = pitchInput;

        if (pitchInput > 0) // if positive (pitch down)
            pitchInput *= pitchPosRange;
        else        // if negative pitch (pitch up)
            pitchInput *= pitchNegRange;

        //readMultipliedPitch = pitchInput;

        pitchInput += pitchTrim;

        //readResultPitchInput = pitchInput;

        return pitchInput;    // trim will never change maximum input

    }





    //  CONTROL AUTHORITY -- ALPHA -- SINGLE AXIS
    private float calculateControlAxisAlphaMod(Vector3 axis)
    {
        Vector3 velocity = Vector3.ProjectOnPlane(rbRef.velocity, axis);
        return Mathf.Cos(calculateAlphaOnPlane(rbRef.velocity, transform.forward, axis) * Mathf.Deg2Rad);
    }

    //  CONTROL AUTHORITY -- VELOCITY -- ALL AXES -- FACTORS G LIMIT AND AERO PERFORMANCE
    private float calculateControlAuthVelocityMod(float aeroPerformLimitScalar, float gLimitScalar, float maxAuth, Vector3 velocity)
    {
        //  Aero performance -- increases by velocity squared
        float aeroPerformLimit = aeroPerformLimitScalar * velocity.magnitude * velocity.magnitude;

        //  G limit -- inverse of velocity
        float gLimit = gLimitScalar / velocity.magnitude;

        // return the limiting factor -- maxAuth, aero performance, or g limit
        return Mathf.Min(aeroPerformLimit, gLimit, maxAuth);
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

    // calculate lift and drag on specified plane
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
        highAlphaShrinkLiftTemp = 1.0f; // multiplier starts at 1.0 to not affect at all until changed
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
