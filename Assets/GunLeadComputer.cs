using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TgtComputer))]
public class GunLeadComputer : MonoBehaviour
{

    private TgtComputer tgtComp;

    public GunLeadReticle reticle;

    public float bulletSpeed;
    public float maxRange;
    public float velScale;

    Vector3 aimPoint;

    private Rigidbody myRb;


    void Awake()
    {
        tgtComp = GetComponent<TgtComputer>();


        myRb = GetComponent<Rigidbody>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        reticle = hudControl.mainHud.GetComponent<hudControl>().reticle;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 targetPos = targetRb.transform.position + avgRelVel * timeToImpact;

        if(tgtComp.currentTarget != null)
        {

            this.aimPoint = calculateAimPoint();

            if(rangeTo(aimPoint) < maxRange)
            {
                reticle.showReticle(true);
                reticle.placeReticle(aimPoint);
            }
            else
            {
                reticle.showReticle(false);
            }

        }
        else
        {
            reticle.showReticle(false);
        }


    }

    private float rangeTo(Vector3 to)
    {
        return Vector3.Distance(aimPoint, transform.position);
    }

    private Vector3 calculateAimPoint()
    {
        Rigidbody targetRb = tgtComp.currentTarget.GetComponent<Rigidbody>();


        Vector3 avgRelVel = targetRb.velocity - myRb.velocity;

        float distance = Vector3.Distance(transform.position, targetRb.transform.position);
        Vector3 targetBearingLine = targetRb.transform.position - transform.position;
        targetBearingLine = Vector3.Project(targetRb.velocity, targetBearingLine);

        float closingVel = targetBearingLine.magnitude;
        if (Vector3.Distance(transform.position, targetRb.transform.position + targetBearingLine) < distance)
        {
            closingVel *= -1f;
        }

        float timeToImpact = distance / (bulletSpeed - closingVel);

        Vector3 aimPoint = targetRb.transform.position + avgRelVel * timeToImpact * velScale;

        return aimPoint;
    }
    
}
