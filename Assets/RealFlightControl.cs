using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*  
 *  calculateAxisDrag()
 *      - Three drag axes - axis drag = (axisVel)^2 * (axis drag coeff)
 *          1. Longitudinal (fwd/back)
 *          2. Lateral (left/right)
 *          3. Vertical (up/down)
 *      - standard lift
 *          - (fwd velocity)^2 times standardLiftCoefficient
 *          
 *          
 *  aceCombatThrottleProcess()
 *      - Fundamentally unrealistic, so this will be drastically simplified
 *      - Thrust can change rapidly -- constant thrustDelta steps
 *      - MAYBE include afterburner stage? multiplies thrustDelta by x above thrust y
 *  
 *  
 *  calculateControlAuthorityByThrust() --> change to by FORWARD velocity
 *      - Set optimal turn rate to be at level speed with cruise thrust
 *      - Decreases linearly specified rate above corner velocity
 *      
 *      
 *  calculateStabilityTorque()
 *      - Slight torque to point nose towards velocity
 *      - slip axis for vertical and lateral velocity
 *          - vertical slip pitch
 *          - lateral slip yaw
 *          - lateral slip roll
 *          --> axis velocity * axis stability ratio * normalized cross vector
 *      - Increased angle of attack will increase stability torque
 *  
 * 
 *  processSlipTorques() -- made obselete by stability torque
 *      - Remains mostly the same
 *      - Scale down as speed increases 
 *          (slow speed = high slip, high speed = less slip)
 *      >>> "STALLING" WILL OCCUR WHEN SPEED IS SO SLOW THAT SLIP TORQUE
 *          IS STRONGER THAN CONTROL AUTHORITY
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
