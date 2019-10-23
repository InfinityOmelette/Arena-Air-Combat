using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*  
 *  create general lift function:
 *  calculateResultLiftVector(float AoA, float dragCoeff, float liftCoeff, float alphaToLiftOffset, float alphaToDragOffset, Vector3 velocity, Transform referenceTransform)
 *      - LIFT IS PERPENDICULAR TO VELOCITY, NOT FORWARD
 *      - Allows for more consistent thrust/drag ratio
 *      - Magnitude scaled up by alpha (angle of attack), from 0 to 90 (positive or negative)
 *          -   LIFT SCALED LINEARLY WITH ALPHA
 *          -   DRAG SCALED EXPONENTIALLY WITH ALPHA
 *          -   0 ALPHA WILL CREATE SOME LIFT/DRAG, DRAG WILL NEVER BE ZERO --> OFFSET ALPHA 
 *      - To get Alpha: 
 *          // *flip sign(s) if necessary*
 *          var localVelocity = transform.InverseTransformDirection(rb.velocity);
 *          var angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z);
 *      - Two force vectors
 *          - Lift = ((liftCoeff * alpha) + alphaToLiftOffset) * (velocity^2) * Cross(transform.right, velocity).normalized
 *          - Drag = ((dragCoeff * alpha)^2 + alphaToDragOffset) * (velocity^2) * (-velocity.normalized)
 *  
 *  
 *  calculateLiftsAndDrags() -- compensates for both induced and parasitic drag
 *          WEIRD FUCKIN IDEA -- TWO "LIFT" VECTORS CALCULATED THE SAME WAY? (Up lift and side lift)
 *              AXES RELATIVE TO VELOCITY
 *              - WING LIFT
 *              - "SIDE" LIFT
 *  
 *  
 *          
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }


    /*  create general lift function:
    *  calculateResultLiftVector(float AoA, float dragCoeff, float liftCoeff, float alphaToLiftOffset, float alphaToDragOffset, Vector3 velocity, Transform referenceTransform)
    *      - LIFT IS PERPENDICULAR TO VELOCITY, NOT FORWARD
    *      - Allows for more consistent thrust/drag ratio
    *      - Magnitude scaled up by alpha(angle of attack), from 0 to 90 (positive or negative)
    *          -   LIFT SCALED LINEARLY WITH ALPHA
    *          -   DRAG SCALED EXPONENTIALLY WITH ALPHA
    *          -   0 ALPHA WILL CREATE SOME LIFT/DRAG, DRAG WILL NEVER BE ZERO --> OFFSET ALPHA 
    *      - To get Alpha: 
    *          // *flip sign(s) if necessary*
    *          var localVelocity = transform.InverseTransformDirection(rb.velocity);
    *          var angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z);
    *      - Two force vectors
    *          - Lift = ((liftCoeff* alpha) + alphaToLiftOffset) * (velocity^2) * Cross(transform.right, velocity).normalized
    *          - Drag = (dragCoeff * (alpha^2) + alphaToDragOffset) * (velocity^2) * (-velocity.normalized)
    */


    private Vector3 calculateOnPlaneResultLiftVector(
        float liftCoeff, float alphaOffsetLift, float highAlphaShrinkLift,      // lift components
        float dragCoeff, float alphaOffsetDrag, float alphaAmplitudeDrag, float alphaParabolicityDrag,  // drag components 
        Vector3 velocity, Vector3 forward, Vector3 planeCrossVector)            // vector info
    {


        //  Use planeCrossVector to remove out-of-plane components of velocity and forward vector
        velocity = Vector3.ProjectOnPlane(velocity, planeCrossVector);  // not necessary to get angle, but will be used for magnitude
        forward = Vector3.ProjectOnPlane(forward, planeCrossVector);

        //  Set initial force vector directions
        Vector3 liftVector = Vector3.Cross(velocity, planeCrossVector).normalized;
        Vector3 dragVector = -velocity.normalized;

        //  Get angle between forward and velocity -- signed to plot on trig graph, neg alpha give neg lift, and drag always positive because trig
        float alpha = Vector3.SignedAngle(forward, velocity, planeCrossVector) * Mathf.Deg2Rad; // RADIANS

        //  Reduce lift when flying backwards
        float highAlphaShrinkLiftTemp = 1.0f;
        if (Mathf.Abs(alpha) > (Mathf.PI / 2))  // if alpha greater than 90 degrees
            highAlphaShrinkLiftTemp = highAlphaShrinkLift;  // reduce lift

        //  plot lift and drag vs alpha graphs on desmos to set the alphaMod values (1.0 roughly being highest expected force)
        float alphaModLift = highAlphaShrinkLiftTemp * Mathf.Sin(2 * alpha);  // 2 to increase period such that 90 gives max, 180 is zero, and so on
        float alphaModDrag = alphaAmplitudeDrag * (-(Mathf.Cos(alpha) * Mathf.Cos(alpha)) + 1) +
            alphaParabolicityDrag * alpha * alpha + alphaOffsetDrag;

        //  Lift calculation:
        liftVector *= liftCoeff * alphaModLift * velocity.magnitude * velocity.magnitude;
        dragVector *= dragCoeff * alphaModDrag * velocity.magnitude * velocity.magnitude;

        //  Add vectors together
        return liftVector + dragVector;
    }

}
