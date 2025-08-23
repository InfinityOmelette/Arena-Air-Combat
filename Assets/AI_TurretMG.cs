using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AI_TurretMG : MonoBehaviour
{

    Rigidbody targetRb;
    private float leadAngle = 0;


    private bool gunsOn = false;

    private float schutDistance = 1200f;

    private CombatFlow rootFlow;

    public ParticleSystem gun;

    private float booleetSpeed;

    private TurretNetworking turretNet;

    public float changeCycleCounterMax;
    private float changeCycleCounter;

    public float targetVelMultiplier;

    public AudioSource gunfireSound;
    public AudioSource gunfireSoundEnd;

    public Rigidbody myRb;

    //public float rotationSpeed;
    
    //private bool isJef = false;

    // Start is called before the first frame update
    void Start()
    {
        changeCycleCounter = changeCycleCounterMax;

        turretNet = transform.root.GetComponent<TurretNetworking>();

        rootFlow = transform.root.GetComponent<CombatFlow>();
        booleetSpeed = gun.startSpeed;

        schutDistance = booleetSpeed * gun.startLifetime;

        //isJef = rootObj.name.Equals("JefTrok");

        //if (isJef)
        //{
        //    Debug.LogError("Jef found");
        //}
    }

    // Update is called once per frame
    void Update()
    {

        tryChangeTarget();
        bool canShoot = false;
        if (targetRb != null)
        {
            canShoot = targetInParams();
            
            if (canShoot)
            {
                transform.rotation = AI_TurretMG.calculateBulletLeadRot(myRb, targetRb, booleetSpeed, targetVelMultiplier);
            }
            else
            {
                turretNet.setTarget(null);
                rootFlow.returnOwnershipToHost();
            }
        }
        else
        {
            rootFlow.returnOwnershipToHost();
        }

        setGunState(canShoot);




    }

    private void tryChangeTarget()
    {
        if (rootFlow.isHostInstance)
        {

            changeCycleCounter -= Time.deltaTime;
            if (changeCycleCounter < 0)
            {
                changeCycleCounter = changeCycleCounterMax;

                CombatFlow targetFlow = findNearestTarget();
                if (targetFlow != null && targetFlow.GetComponent<Rigidbody>() != targetRb)
                {
                    //Debug.LogWarning
                    //Debug.LogWarning("new target name: " + targetFlow.name);
                    if (targetRb != null && targetFlow != null)
                    {
                        Debug.LogWarning("AAA found new target. Old: " + targetRb.gameObject.name + ", new: " + targetFlow.gameObject.name);
                    }

                    turretNet.setTarget(targetFlow);

                    // only target's instance will deal damage. Rest will be cosmetic-only
                    rootFlow.giveOwnership(targetFlow.photonView.ViewID);
                }
                
            }
        }
    }

    public void setTarget(GameObject obj)
    {
        if (obj != null)
        {
            Rigidbody newRb = obj.GetComponent<Rigidbody>();
            //targetRb = obj.GetComponent<Rigidbody>();
            if (newRb != targetRb)
            {
                targetRb = newRb;
                //Debug.LogWarning("Setting target: " + targetRb.gameObject.name);
                //Debug.LogWarning("Setting acquire timer to " + acquireTimer);
            }
        }
        else
        {
            targetRb = null;
        }
    }

    private CombatFlow findNearestTarget()
    {
        CombatFlow closestTarget = null;

        // don't bother targeting someone outside of schutDistance
        float shortestDist = schutDistance;

        List<CombatFlow> allUnits = CombatFlow.combatUnits;

        for(int i = 0; i < allUnits.Count; i++)
        {
            CombatFlow currentFlow = allUnits[i];

            if (currentFlow != null)
            {
                if (currentFlow.team != rootFlow.team && currentFlow.type == CombatFlow.Type.AIRCRAFT)
                {
                    
                    float currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);

                    if (currentDistance < shortestDist)
                    {
                        int terrainLayer = 1 << 10; // line only collides with terrain layer
                        bool hasLineOfSight = !Physics.Linecast(transform.position, currentFlow.transform.position, terrainLayer);
                        if (hasLineOfSight)
                        {
                            closestTarget = currentFlow;
                            shortestDist = currentDistance;
                        }
                    }
                }
            }
        }



        return closestTarget;
    }


    private void setGunState(bool gunSet)
    {
        if (gunSet != gunsOn)
        {
            gunsOn = gunSet;
            if (gunSet)
            {
                gunfireSound.loop = true;
                gunfireSound.Play();

                gun.Play();
            }
            else
            {
                gunfireSound.loop = false;
                gunfireSound.Play();

                gunfireSoundEnd.Play();

                gun.Stop();
            }
        }
    }

    public static Quaternion calculateBulletLeadRot(Vector3 myPos, Vector3 targetPosition, Vector3 relativeVelocity, float bulletSpeed, float targetVelMultiplier = 1.0f)
    {
        float distance = Vector3.Distance(myPos, targetPosition);
        Vector3 targetBearingLine = targetPosition - myPos;


        targetBearingLine = Vector3.Project(relativeVelocity, targetBearingLine);

        float closingVel = targetBearingLine.magnitude;
        if (Vector3.Distance(myPos, targetPosition + targetBearingLine) < distance)
        {
            closingVel *= -1f;
        }

        float timeToImpact = distance / (bulletSpeed - closingVel);

        Vector3 targetPos = targetPosition + relativeVelocity * timeToImpact * targetVelMultiplier;

        return Quaternion.LookRotation(targetPos - myPos, Vector3.up);
    }

    public static Quaternion calculateBulletLeadRot(Rigidbody origBody, Rigidbody targetBody, float bulletSpeed, float targVelMultiplier = 1.0f)
    {
        //Debug.Log("Calculatebullet lead for " + origBody.gameObject.name);
        // Velocity of target with origBody as the moving reference frame
        Vector3 relativeVelocity = targetBody.velocity - origBody.velocity;


        return calculateBulletLeadRot(origBody.transform.position, targetBody.transform.position, relativeVelocity, bulletSpeed, targVelMultiplier);

    }

    private bool targetInParams()
    {

        float distance = Vector3.Distance(transform.position, targetRb.transform.position);
        int terrainLayer = 1 << 10; // line only collides with terrain layer
        bool hasLineOfSight = !Physics.Linecast(transform.position, targetRb.transform.position, terrainLayer);
        return distance < schutDistance && hasLineOfSight;
    }

    //private void setLeadAngle()
    //{
    //    Vector3 targetBearingLine = targetRb.position - rootObj.transform.position;
    //    Vector3 leadAxis = Vector3.Cross(targetBearingLine, targetRb.velocity).normalized;

    //    // Target tangential velocity --> missile will try to match its tangential velocity to this
    //    Vector3 targetTangentialVelocity = Vector3.Project(targetRb.velocity,
    //        Vector3.Cross(leadAxis, targetBearingLine));

    //    float leadAngleDegrees = Mathf.Rad2Deg * Mathf.Asin(targetTangentialVelocity.magnitude / targetRb.velocity.magnitude);


    //    //Vector3 leadVect = targetBearingLine * Quaternion.AngleAxis(leadAngleDegrees, leadAxis);
    //    transform.LookAt(targetRb.transform);
    //    Quaternion newRot = transform.rotation * Quaternion.AngleAxis(leadAngleDegrees, leadAxis);
    //    transform.rotation = newRot;
    //}


}
