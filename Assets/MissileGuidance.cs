using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 *  REALISTIC MISSILE PERFORMANCE + GUIDANCE
  - Reasonable missile physics values -- ~1.5km range
  - Predicted lead angle (seek constant-bearing-decreasing-range) aka proportional guidance
    > Target bearing line 
	> Target velocity --> target tangential velocity
	> Projected lead axis/plane -- coplanar with target bearing line and target velocity (cross of the two)
	> Projectile velocity
	> Projected lead -- trig function to get lead angle -- ANGLE OFF OF BEARING LINE IN-PLANE WITH TARGET VELOCITY
	  >>> Direction vectors? Target quaternions?
	  >>> tangential component of projectile velocity matches tangential component of target velocity
	> torque vector is cross of projectile velocity and target direction vector?
	  >>> angle between belocity and target direction vector 
	  >>> torque vector separated into PITCH and YAW input components
	> pitch and yaw components fed into realFlight script

  - Predicted average velocity over remaining flight time
  - Lofting stage (function of distance)
 * 
 * 
 * 
 * 
 * */
public class MissileGuidance : MonoBehaviour
{

    public Rigidbody targetRB;

    public RocketMotor rocketMotor;
    public RealFlightControl myFlightControl;
    public Rigidbody myRB;

    public float maxCorrectionErrorAngle; // at this angle error, torque is max

    public Weapon weaponRef;


    public float assumedGravityAccel;
    public float targetPosForwardProjectionTime; // give time for warhead to explode fully in front of target

    // estimations
    private Vector3 estimatedMissileVelocityAverage;
    private Vector3 estimatedTargetVelocityAverage;
    private float estimatedTimeToImpact;

    // target position
    private Vector3 targetPos_now;
    private Vector3 targetPos_prev;

    // target velocity
    private Vector3 targetVel_now;
    private Vector3 targetVel_prev;

    private Vector3 targetAccel;

    

    private void Awake()
    {
        weaponRef = GetComponent<Weapon>();
        rocketMotor = GetComponent<RocketMotor>();
        myFlightControl = GetComponent<RealFlightControl>();
        myRB = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        myFlightControl.enabled = false;

        targetAccel = new Vector3();
        targetVel_now = new Vector3();
        targetVel_prev = new Vector3();

        targetPos_now = new Vector3();
        targetPos_prev = new Vector3();
    }



    private void FixedUpdate()
    {
        GameObject myTarget = weaponRef.myTarget;
        if(myTarget != null)
            targetRB = weaponRef.myTarget.GetComponent<Rigidbody>();

        if (weaponRef.myTarget != null)
        {

            // UPDATE TARGET POSITION AND VELOCITY
            targetPos_now = targetRB.position + projectForwardByTime(targetPosForwardProjectionTime); // aim slightly ahead
            targetVel_now = targetRB.velocity;

            // UPDATE TARGET ACCELERATION
            targetAccel = Vector3.up * assumedGravityAccel; // compensate for gravity acceleration -- keep velocity from sagging downwards
            if (targetVel_prev != null)
                targetAccel += (targetVel_now - targetVel_prev) * Time.deltaTime; // target acceleration by looking at change in velocity


            if (weaponRef.launched)
            {
                myFlightControl.enabled = true;

                // Target bearing line
                Vector3 targetBearingLine = targetPos_now - transform.position;


                // ============================  ESTIMATIONS

                // estimate average missile speed based on distance, thrust, remaining burn time, altitude difference
                estimatedMissileVelocityAverage = myRB.velocity + transform.forward.normalized * Mathf.Min(estimatedTimeToImpact, rocketMotor.burnTime) * 
                   rocketMotor.thrustForce - Vector3.up * (targetRB.position.y - transform.position.y) * assumedGravityAccel;
                //estimatedMissileVelocityAverage = myRB.velocity;

                // estimate closing speed -- positive for closing, negative for separating
                Vector3 closingVector = Vector3.Project(estimatedTargetVelocityAverage -
                    estimatedMissileVelocityAverage, targetBearingLine);
                float closingSpeed = closingVector.magnitude;

                // if closing vect not in same direction as bearing line, we are separating. Change sign to negative
                if (!Mathf.Approximately(Vector3.Angle(closingVector, targetBearingLine), 0.0f))
                    closingSpeed *= -1;

                // estimate time to intercept
                estimatedTimeToImpact = closingSpeed * Vector3.Distance(targetRB.position, transform.position);

                // estimate target average velocity
                //estimatedTargetVelocityAverage = targetRB.velocity + (targetAccel * estimatedTimeToImpact / 2);
                estimatedTargetVelocityAverage = targetRB.velocity;


                //==================================  LEAD ANGLE CALCULATION

                // Lead axis (cross of bearing line and target velocity)
                Vector3 leadRotationAxis = Vector3.Cross(targetBearingLine, estimatedTargetVelocityAverage);

                // Target tangential velocity --> missile will try to match its tangential velocity to this
                Vector3 targetTangentialVelocity = Vector3.Project(estimatedTargetVelocityAverage,
                    Vector3.Cross(leadRotationAxis, targetBearingLine));



                Debug.DrawRay(targetPos_now, targetTangentialVelocity, Color.blue);
                Debug.DrawRay(targetPos_now, estimatedTargetVelocityAverage, Color.cyan);
                Debug.Log("Estimated target average velocity: " + estimatedTargetVelocityAverage.magnitude);

                // Lead angle -- direction vector
                //  - trig from velocity magnitude to get angle where tangential velocity matches target tangential velocity
                float leadAngleDegrees = Mathf.Rad2Deg * Mathf.Asin(targetTangentialVelocity.magnitude / estimatedMissileVelocityAverage.magnitude);

                

                // Lead direction
                Vector3 leadDirection = Quaternion.AngleAxis(leadAngleDegrees, leadRotationAxis) * targetBearingLine.normalized;

                // Show lead direction
                Debug.DrawRay(transform.position, leadDirection * Vector3.Distance(targetRB.position, transform.position), Color.green);

                // Target bearing line
                Debug.DrawRay(transform.position, targetBearingLine.normalized * 
                    Vector3.Distance(targetPos_now, transform.position), Color.red);

                //  Corrected velocity
                Debug.DrawRay(transform.position, leadDirection.normalized * myRB.velocity.magnitude, Color.yellow);

                // Target's current velocity
                //Debug.DrawRay(targetPos_now, targetRB.velocity, Color.yellow);

                // Target average velocity
               // Debug.DrawRay(targetPos_now, estimatedTargetVelocityAverage, Color.white);


                // Parallel target bearing line, placed at end of corrected velocity
                Debug.DrawRay(transform.position + leadDirection.normalized * myRB.velocity.magnitude,
                    targetBearingLine.normalized * 1000f, Color.magenta);


                //  ==========================  CORRECTIVE TORQUE CALCULATION

                // correctiveTorqueVector -- axis to torque around to move velocity towards lead direction
                //  - cross of velocity and lead direction vector
                //  - magnitude -- angleBetween velocity and lead direction, ratio of angle vs maxAngle (max 1.0)
                //  - projected onto xy plane to nullify any roll requirement
                Vector3 correctiveTorqueVect = 
                    Vector3.ProjectOnPlane(Vector3.Cross(myRB.velocity, leadDirection), transform.forward).normalized *
                    (Mathf.Min(leadAngleDegrees / maxCorrectionErrorAngle, 1.0f));


                //Debug.DrawRay(transform.position, correctiveTorqueVect * 30f, Color.blue);
               // Debug.DrawRay(transform.position, correctiveTorqueVect.normalized * 30f, Color.green);
                //Debug.DrawRay(transform.position, myRB.velocity, Color.yellow);
                //Debug.DrawRay(transform.position, transform.forward * 100f, Color.white);

                

                // Convert to yaw/pitch inputs, -1.0 to 1.0
                myFlightControl.input_pitch = transform.InverseTransformDirection(correctiveTorqueVect).x;
                myFlightControl.input_yaw = transform.InverseTransformDirection(correctiveTorqueVect).y;

                Debug.Log("Missile pitch input: " + myFlightControl.input_pitch);

                

            }

            targetPos_prev = targetPos_now;
            targetVel_prev = targetVel_now;
        }
    }

    private void proportionalNavigation()
    {

    }

    private void myNavigation()
    {
        // Target bearing line


        // Lead axis (cross of bearing line and target velocity)

        // Target tangential velocity --> missile will try to match its tangential velocity to this

        // Lead angle -- direction vector
        //  - trig from velocity to get angle where tangential velocity matches target tangential velocity

        // correctiveTorqueVector -- axis to torque around to move velocity towards lead direction
        //  - cross of velocity and lead direction vector
        //  - magnitude -- angleBetween velocity and lead direction, ratio of angle vs maxAngle (max 1.0)

        // Convert to yaw/pitch inputs, -1.0 to 1.0
    }

    private Vector3 projectForwardByTime(float projectionTime)
    {
        return targetRB.velocity * projectionTime;
    }

}
