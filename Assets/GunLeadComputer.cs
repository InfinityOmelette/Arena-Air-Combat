using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TgtComputer))]
public class GunLeadComputer : MonoBehaviour
{

    private TgtComputer tgtComp;

    public GunLeadReticle reticle;
    public GunLeadReticle reticle2;

    public float bulletSpeed;
    public float maxRange;
    public float velScale;

    Vector3 aimPoint;

    private Rigidbody myRb;

    private CombatFlow myFlow;

    void Awake()
    {
        tgtComp = GetComponent<TgtComputer>();
        myFlow = GetComponent<CombatFlow>();

        myRb = GetComponent<Rigidbody>();

        // ratio of initial working values projected to new (hopefully) working value
        //  .75 velScale worked for 500 m/s bulletspeed
        //velScale = 1.0f - .25f * bulletSpeed / 500;
        //Debug.LogError("New velScale: " + velScale);
    }

    // Start is called before the first frame update
    void Start()
    {
        reticle = hudControl.mainHud.GetComponent<hudControl>().reticle;
        reticle2 = hudControl.mainHud.GetComponent<hudControl>().reticle2;

    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 targetPos = targetRb.transform.position + avgRelVel * timeToImpact;

        if (myFlow.isLocalPlayer)
        {

            if (tgtComp.currentTarget != null && tgtComp.currentTarget.type == CombatFlow.Type.AIRCRAFT)
            {

                this.aimPoint = calculateAimPoint();
                reticle.aimPointDist = rangeTo(aimPoint);

                if (reticle.aimPointDist < maxRange)
                {
                    //reticle.showReticle(true);
                    reticle.placeReticle(aimPoint);

                    reticle2.showReticle(true);
                    reticle2.placeReticle(calculateAimAngle());
                }
                else
                {
                    reticle.showReticle(false);
                    reticle2.showReticle(false);
                }

            }
            else
            {
                reticle.showReticle(false);
                reticle2.showReticle(false);
            }
        }
    }

    private float rangeTo(Vector3 to)
    {
        return Vector3.Distance(aimPoint, transform.position);
    }

    private Vector3 calculateAimPoint()
    {
        Rigidbody targetRb = tgtComp.currentTarget.GetComponent<Rigidbody>();


        Vector3 avgRelVel = (targetRb.velocity - myRb.velocity) * velScale;

        float distance = Vector3.Distance(transform.position, targetRb.transform.position);
        Vector3 targetBearingLine = targetRb.transform.position - transform.position;
        targetBearingLine = Vector3.Project(targetRb.velocity, targetBearingLine);

        float closingVel = targetBearingLine.magnitude;
        if (Vector3.Distance(transform.position, targetRb.transform.position + targetBearingLine) < distance)
        {
            closingVel *= -1f;
        }

        float timeToImpact = distance / (bulletSpeed - closingVel);

        Vector3 aimPoint = targetRb.transform.position + avgRelVel * timeToImpact;

        return aimPoint;
    }
    
    private Vector3 calculateAimAngle()
    {
        Rigidbody targetRb = tgtComp.currentTarget.GetComponent<Rigidbody>();

        Vector3 targetBearingLine =  targetRb.transform.position - transform.position;


        //==================================  LEAD ANGLE CALCULATION

        Vector3 targetRelVel = (targetRb.velocity - myRb.velocity);

        // Lead axis (cross of bearing line and target velocity)
        Vector3 leadRotationAxis = Vector3.Cross(targetBearingLine, targetRelVel);

        // Target tangential velocity --> missile will try to match its tangential velocity to this
        Vector3 targetTangentialVelocity = Vector3.Project(targetRelVel,
            Vector3.Cross(leadRotationAxis, targetBearingLine));



        //Debug.DrawRay(targetPos_now, targetTangentialVelocity, Color.blue);
        //Debug.DrawRay(targetPos_now, estimatedTargetVelocityAverage, Color.cyan);
        //Debug.Log("Estimated target average velocity: " + estimatedTargetVelocityAverage.magnitude);

        // Lead angle -- direction vector
        //  - trig from velocity magnitude to get angle where tangential velocity matches target tangential velocity
        float leadAngleDegrees = Mathf.Rad2Deg * Mathf.Asin(targetTangentialVelocity.magnitude / bulletSpeed) * velScale;

        //Debug.Log("leadAngleDegrees: " + leadAngleDegrees);

        //Debug.DrawRay(transform.position, leadRotationAxis.normalized * 10f);

        // Lead direction
        Vector3 leadDirection = Quaternion.AngleAxis(leadAngleDegrees, leadRotationAxis) * targetBearingLine.normalized;

        

        return Camera.main.transform.position + leadDirection;
    }

}
