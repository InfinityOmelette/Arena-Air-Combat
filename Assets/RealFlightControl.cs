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
 *  calculateAxisParasiticDrag() -- use only for drag items, not lifting bodies
 *      - 
 *          
 *          
 *  aceCombatThrottleProcess()
 *      - Fundamentally unrealistic, so this will be drastically simplified
 *      - Thrust can change rapidly -- constant thrustDelta steps
 *      - MAYBE include afterburner stage? multiplies thrustDelta by x above thrust y
 *  
 *  processBaseControlTorque()
 *      - 
 *  
 *  calculateControlAuthorityByThrust() --> change to by FORWARD velocity
 *      - Set cruise thrust to be at optimal turn speed in level flight (mess with flight variables until it's at 30-50% thrust
 *      - Decreases linearly specified rate above corner velocity
 *      
 *      
 *  calculateStabilityTorque()
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


}
