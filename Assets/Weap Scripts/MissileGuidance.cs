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
    private Radar myRadar;

    public float maxCorrectionErrorAngle; // at this angle error, torque is max

    public Weapon weaponRef;

    private BasicMissile missileRef;

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

    private CombatFlow targetFlow = null;

    private Vector3 targetBearingLine;

    public float scanConeFactor = 1f;

    // (a / (t + a)) graph in desmos -- longer time remain is, 
    // the lower the velocity average estimate will be
    public float timeRemainDragEstFactor = 10f;

    //private Vector3 previousBearingLine;

    public bool isLocked;

    public float maxLoftDegreesKM = 18f;
    public float loftChangeSlope = 6.7f; // degrees per kilometer past minRange
    public float loftMinRangeKM = 3.3f; // kilometers



    private void Awake()
    {
        missileRef = GetComponent<BasicMissile>();
        weaponRef = GetComponent<Weapon>();
        rocketMotor = GetComponent<RocketMotor>();
        myFlightControl = GetComponent<RealFlightControl>();
        myRB = GetComponent<Rigidbody>();
        myRadar = GetComponent<Radar>();
        
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
        if(myTarget != null && targetRB == null)
            targetRB = weaponRef.myTarget.GetComponent<Rigidbody>();

        if (weaponRef.launched)
        {

            if (weaponRef.myTarget != null)
            {
                //Debug.LogError("Weapon has target: " + weaponRef.myTarget.name);

                // ======================== LINE OF SIGHT
                bool lineOfSight = false;
                int terrainLayer = 1 << 10; // line only collides with terrain layer
                lineOfSight = !Physics.Linecast(transform.position, weaponRef.myTarget.transform.position, terrainLayer);


                // If target successfully tracked by missile, notify target and update target position data
                if (lineOfSight)
                {
                    if (weaponRef.launched && targetFlow == null) // outer control layer already checkks that weaponRef.myTarget != null
                    {
                        targetFlow = weaponRef.myTarget.GetComponent<CombatFlow>();
                    }


                    bool tryLock = !targetFlow.jamming && myRadar.tryDetect(targetFlow);

                    if(tryLock != isLocked && targetFlow.rwr != null)
                    {
                        if (tryLock) // begin lock
                        {
                            targetFlow.rwr.netLockedBy(myRadar);
                        }
                        else // end lock
                        {
                            targetFlow.rwr.endNetLock(myRadar);
                        }
                    }

                    isLocked = tryLock;

                    //myRadar.tryDetect(targetFlow)
                    if (tryLock)
                    {
                        //guidanceProcess();
                        updateTargetData();
                    }
                }

                // If missile loses track of target, continue intercepting previous known course
                if (targetRB != null)
                {
                    guidanceProcess();
                }

            }
            else // weaponRef.myTarget IS null
            {
                // if target is deleted after guided launch, turn off radar. Missile's trashed
                if (missileRef != null && missileRef.guidedLaunch && myRadar.radarOn)
                {
                    myRadar.radarOn = false;
                }
            }
        }
    }

    private void updateTargetData()
    {
        // UPDATE TARGET POSITION AND VELOCITY
        targetPos_now = targetRB.position + projectForwardByTime(targetPosForwardProjectionTime); // aim slightly ahead
        targetVel_now = targetRB.velocity;



        // UPDATE TARGET ACCELERATION
        targetAccel = Vector3.up * assumedGravityAccel; // compensate for gravity acceleration -- keep velocity from sagging downwards
        if (targetVel_prev != null)
            targetAccel += (targetVel_now - targetVel_prev) * Time.fixedDeltaTime; // target acceleration by looking at change in velocity

        targetBearingLine = targetPos_now - transform.position;
    }

    private void guidanceProcess()
    {
        


        if (weaponRef.launched)
        {
            myFlightControl.enabled = true;

            // Target bearing line


            // ============================  ESTIMATIONS

            float deltaVBoost = Mathf.Min(Mathf.Abs(estimatedTimeToImpact), rocketMotor.burnTime) * rocketMotor.thrustForce;
            float deltaVCruise = Mathf.Min(Mathf.Abs(estimatedTimeToImpact - rocketMotor.burnTime), rocketMotor.cruiseTime) * rocketMotor.cruiseThrust;
            if(estimatedTimeToImpact < rocketMotor.burnTime)
            {
                deltaVCruise = 0;
            }

            //Debug.LogWarning("DeltaVBoost" + deltaVBoost + "DeltaVCruise: " + deltaVCruise);

            // estimate average missile speed based on distance, thrust, remaining burn time, altitude difference
            estimatedMissileVelocityAverage = myRB.velocity +
                transform.forward.normalized * deltaVBoost +
                transform.forward.normalized * deltaVCruise -
                 Vector3.up * (targetPos_now.y - transform.position.y) * assumedGravityAccel;
            //estimatedMissileVelocityAverage = myRB.velocity;

            // estimate closing speed -- positive for closing, negative for separating
            Vector3 closingVector = Vector3.Project(estimatedTargetVelocityAverage -
                estimatedMissileVelocityAverage, targetBearingLine);
            float closingSpeed = closingVector.magnitude;

            // if closing vect not in same direction as bearing line, we are separating. Change sign to negative
            if (!Mathf.Approximately(Vector3.Angle(-closingVector, targetBearingLine), 0.0f))
                closingSpeed *= -1;

            // estimate time to intercept
            estimatedTimeToImpact = Vector3.Distance(targetPos_now, transform.position) / closingSpeed;



            float burnTime = rocketMotor.burnTime + rocketMotor.cruiseTime;

            float glideTime = Mathf.Max(0.0f, estimatedTimeToImpact - burnTime);

            estimatedMissileVelocityAverage *= timeRemainDragEstFactor / (glideTime + timeRemainDragEstFactor);

            // estimate target average velocity
            //estimatedTargetVelocityAverage = targetRB.velocity + (targetAccel * estimatedTimeToImpact / 2);
            estimatedTargetVelocityAverage = targetVel_now;

           // Debug.Log("Estimations ------------ estimated missile average velocity: " + estimatedMissileVelocityAverage.magnitude +
          //      " estimated TARGET average velocity: " + estimatedTargetVelocityAverage.magnitude + " estimated time to impact: " +
           //     estimatedTimeToImpact + " seconds, closing speed: " + closingSpeed);

          //  Debug.DrawRay(transform.position, estimatedMissileVelocityAverage, Color.magenta); // show estimated missilve velocity average
           // Debug.DrawRay(targetPos_now, estimatedTargetVelocityAverage, Color.magenta); // estimated target average velocity
           // Debug.DrawRay(transform.position, closingVector, Color.white);

            //==================================  LEAD ANGLE CALCULATION

            // Lead axis (cross of bearing line and target velocity)
            Vector3 leadRotationAxis = Vector3.Cross(targetBearingLine, estimatedTargetVelocityAverage);

            // Target tangential velocity --> missile will try to match its tangential velocity to this
            Vector3 targetTangentialVelocity = Vector3.Project(estimatedTargetVelocityAverage,
                Vector3.Cross(leadRotationAxis, targetBearingLine));



            //Debug.DrawRay(targetPos_now, targetTangentialVelocity, Color.blue);
            //Debug.DrawRay(targetPos_now, estimatedTargetVelocityAverage, Color.cyan);
            //Debug.Log("Estimated target average velocity: " + estimatedTargetVelocityAverage.magnitude);

            // Lead angle -- direction vector
            //  - trig from velocity magnitude to get angle where tangential velocity matches target tangential velocity
            float leadAngleDegrees = Mathf.Rad2Deg * Mathf.Asin(targetTangentialVelocity.magnitude / estimatedMissileVelocityAverage.magnitude);

            leadAngleDegrees = Mathf.Min(leadAngleDegrees, myRadar.scanConeAngle * scanConeFactor);

            //Debug.Log("leadAngleDegrees: " + leadAngleDegrees);

            //Debug.DrawRay(transform.position, leadRotationAxis.normalized * 10f);

            // Lead direction
            Vector3 leadDirection = Quaternion.AngleAxis(leadAngleDegrees, leadRotationAxis) * targetBearingLine.normalized;


            // Loft formula based entirely off of distance
            leadDirection = loftAdjustment(leadDirection, targetBearingLine);

            // Show lead direction
            //Debug.DrawRay(transform.position, leadDirection * Vector3.Distance(targetRB.position, transform.position), Color.green);

            // Target bearing line -- uses drawRay to confirm line vector
            //Debug.DrawRay(transform.position, targetBearingLine.normalized * 
            //  Vector3.Distance(targetPos_now, transform.position), Color.red);

            //  Corrected velocity -- CYAN
           // Debug.DrawRay(transform.position, leadDirection.normalized * myRB.velocity.magnitude, Color.cyan);

           // // Target's current velocity -- YELLOW
           // Debug.DrawRay(targetPos_now, targetRB.velocity, Color.yellow);

            // Target average velocity
            // Debug.DrawRay(targetPos_now, estimatedTargetVelocityAverage, Color.white);


            // Parallel target bearing line, placed at end of corrected velocity
            //Debug.DrawRay(transform.position + leadDirection.normalized * myRB.velocity.magnitude,
            //  targetBearingLine.normalized * 1000f, Color.magenta);


            //  ==========================  CORRECTIVE TORQUE CALCULATION

            // correctiveTorqueVector -- axis to torque around to move velocity towards lead direction
            //  - cross of velocity and lead direction vector
            //  - magnitude -- angleBetween velocity and lead direction, ratio of angle vs maxAngle (max 1.0)
            //  - projected onto xy plane to nullify any roll requirement
            float currentVelocityErrorAngle = Vector3.Angle(estimatedMissileVelocityAverage, leadDirection); // degrees

            Vector3 correctiveTorqueVect =
                Vector3.ProjectOnPlane(Vector3.Cross(myRB.velocity, leadDirection), transform.forward).normalized * // torque direction
                (Mathf.Min(currentVelocityErrorAngle / maxCorrectionErrorAngle, 1.0f));  // torque magnitude




           // Debug.DrawRay(transform.position, correctiveTorqueVect * 30f, Color.blue);
            // Debug.DrawRay(transform.position, correctiveTorqueVect.normalized * 30f, Color.green);
           // Debug.DrawRay(transform.position, myRB.velocity, Color.yellow);
            //Debug.DrawRay(transform.position, transform.forward * 100f, Color.white);



            // Convert to yaw/pitch inputs, -1.0 to 1.0
            myFlightControl.input_pitch = transform.InverseTransformDirection(correctiveTorqueVect).x;
            myFlightControl.input_yaw = transform.InverseTransformDirection(correctiveTorqueVect).y;

           // Debug.Log("Missile pitch input: " + myFlightControl.input_pitch);



        }

        targetPos_prev = targetPos_now;
        targetVel_prev = targetVel_now;
    }


    // See desmos chart "Missile loft angle vs distance"
    private Vector3 loftAdjustment(Vector3 interceptLine, Vector3 targetBearingLine)
    {

        float distance = targetBearingLine.magnitude / 1000f; // kilometers
        

        if(distance < loftMinRangeKM)
        {
            return interceptLine; // no adjustment, because target within minimum loft range
        }
        else
        {
            float loftUpAngle = Mathf.Min(loftChangeSlope * (distance - loftMinRangeKM), maxLoftDegreesKM);

            Vector3 angleUpAxis = Vector3.Cross(interceptLine, Vector3.up).normalized;
            return (Quaternion.AngleAxis(loftUpAngle, angleUpAxis) * interceptLine).normalized;
        }

        

    }
    


    private Vector3 projectForwardByTime(float projectionTime)
    {
        return targetRB.velocity * projectionTime;
    }


    void OnDestroy()
    {
        if(targetFlow != null && isLocked && targetFlow.rwr != null)
        {
            targetFlow.rwr.rpcEndLockedBy(myRadar.photonView.ViewID);
        }
    }

}
