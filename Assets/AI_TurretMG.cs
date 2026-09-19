using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AI_TurretMG : MonoBehaviour
{

    public Rigidbody targetRb;
    private float leadAngle = 0;


    private bool gunsOn = false;

    public float schutDistance = 1200f;

    private CombatFlow rootFlow;

    public ParticleSystem gun;

    public float booleetSpeed;


    private TurretNetworking turretNet;

    public float changeCycleCounterMax;
    private float changeCycleCounter;

    public float targetVelMultiplier;

    public AudioSource gunfireSound;
    public AudioSource gunfireSoundEnd;

    public Rigidbody myRb;

    public bool active = true;

    public bool debug = false;

    public bool isStatic = false;
    public bool onlyTargetAbove = true;

    private int turretIndex = -1;

    public float estimatedTimeToImpact = 0.0f;

    private Vector3 estimatedImpactPoint;

    //public float rotationSpeed;

    //private bool isJef = false;

    public List<CombatFlow.Type> targetTypes;

    public TankTurret tankTurret;

    public float targetClosingTrim = 1.0f;

    public UnitAlertness alertness;
    public bool bypassAlertness = false;
    public bool triggerAlertness = false;

    public bool bypassLineOfSight = false;

    public bool prioritizeNavalTargets = false;

    //public bool debugGunFuckery = false;

    public void setIndex(int index)
    {
        turretIndex = index;
    }

    private void Awake()
    {
        if(myRb == null)
        {
            myRb = transform.root.GetComponent<Rigidbody>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        changeCycleCounter = changeCycleCounterMax;

        turretNet = GetComponent<TurretNetworking>();

        if(turretNet == null)
        {
            turretNet = transform.root.GetComponent<TurretNetworking>();
        }

        alertness = transform.root.GetComponent<UnitAlertness>();

        rootFlow = transform.root.GetComponent<CombatFlow>();

        if(gun != null)
        {
            booleetSpeed = gun.startSpeed;

            schutDistance = booleetSpeed * gun.startLifetime;
        }
        

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
        if (targetRb != null && active)
        {
            canShoot = targetInParams();
            
            if (canShoot)
            {
                if (isStatic)
                {
                    transform.rotation = AI_TurretMG.calculateBulletLeadRot(transform.position, 
                        targetRb, booleetSpeed, targetVelMultiplier, this, targetClosingTrim);
                }
                else
                {
                    transform.rotation = AI_TurretMG.calculateBulletLeadRot(myRb, targetRb, 
                        booleetSpeed, targetVelMultiplier, this, targetClosingTrim);
                }
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

        if (triggerAlertness)
        {
            alertness.beginChangingAlertStatus(targetRb != null, targetRb );
        }

        setGunState(canShoot && (bypassAlertness || alertness.checkAlertStatus()));

    }

    private bool targetIsAbove(Rigidbody targetRb)
    {
        return targetRb != null && targetRb.transform.position.y > transform.position.y;
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

                if (targetFlow != null && targetFlow.myRb != targetRb 
                    && (targetIsAbove(targetFlow.myRb) || !onlyTargetAbove))
                {

                    //if (debugGunFuckery)
                    //{
                    //    Debug.Log("Setting target: " + targetFlow.report());
                    //}

                    //Debug.LogWarning
                    //Debug.LogWarning("new target name: " + targetFlow.name);
                    if (targetRb != null && targetFlow != null)
                    {
                        //Debug.Log("AAA found new target. Old: " + targetRb.gameObject.name + ", new: " + targetFlow.gameObject.name);
                    }

                    turretNet.setTarget(targetFlow, turretIndex);

                    // only target's instance will deal damage. Rest will be cosmetic-only
                    rootFlow.giveOwnership(targetFlow.photonView.ViewID);
                }

                // really dumb to check this twice but hey I don't see any cops around
                if(targetFlow == null)
                {
                    turretNet.setTarget(null, turretIndex);
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

        if (debug)
            Debug.Log("FindNearestTarget called");

        CombatFlow shipFound = null;

        for(int i = 0; i < allUnits.Count; i++)
        {
            CombatFlow currentFlow = allUnits[i];

            //if (currentFlow.isLocalPlayer)
            //{
            //    //Debug.Log("Found local player");
            //}

            

            if (currentFlow != null)
            {

                //if (debugGunFuckery)
                //{
                //    Debug.Log("Outer Check: " + allUnits[i].report());
                //}

                if (currentFlow.team != rootFlow.team && targetTypes.Contains(currentFlow.type) && 
                    !currentFlow.isSuppressedStrategic() && shipCheck(currentFlow, shipFound))
                {
                    //if (debugGunFuckery)
                    //{
                    //    Debug.Log("Is proper type: " + currentFlow.report());
                    //}
                    
                    
                    float currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);

                    if (currentDistance < shortestDist && 
                        (shipFound == null || isNavalWithinRange(currentFlow, currentDistance)))
                    {
                        //if (debugGunFuckery)
                        //{
                        //    Debug.Log("Is within range: " + currentFlow.report());
                        //}
                        int terrainLayer = 1 << 10; // line only collides with terrain layer
                        bool checkLOS = bypassLineOfSight 
                            || currentFlow.type == CombatFlow.Type.NAVAL
                            || !Physics.Linecast(transform.position, currentFlow.transform.position, terrainLayer);
                        if (checkLOS)
                        {
                            closestTarget = currentFlow;
                            shortestDist = currentDistance;

                            //if (debugGunFuckery)
                            //{
                            //    Debug.Log("Los good on: " + currentFlow.report());
                            //}

                            if (closestTarget.type == CombatFlow.Type.NAVAL)
                            {
                                shipFound = closestTarget;
                                //if (debugGunFuckery)
                                //{
                                //    Debug.Log("ShipFound!!!");
                                //}
                                
                            }
                        }
                    }
                }

                
            }
        }

        //if (debug && closestTarget != null && closestTarget.isLocalPlayer)
        //{
        //    Debug.Log("Found local player");
        //}

        //if (debugGunFuckery)
        //{
        //    if(closestTarget == null)
        //    {
        //        Debug.Log("Closest target: null");
        //    }
        //    else
        //    {
        //        Debug.Log("Closest target: " + closestTarget.report());
        //    }
        //}

        return closestTarget;
    }

    // valid target if not prioritizing naval, or no ship has been found this tick, or target is naval anyways
    private bool shipCheck(CombatFlow targetFlow, CombatFlow shipFound)
    {
        //if (debugGunFuckery)
        //{
        //    Debug.Log("shipCheck on: " + targetFlow.report());
        //}
        
        return !prioritizeNavalTargets 
            || shipFound == null || targetFlow.type == CombatFlow.Type.NAVAL;
    }

    private bool isNavalWithinRange(CombatFlow targetFlow, float distance)
    {
        //if (debugGunFuckery)
        //{
        //    Debug.Log("isNavalWithinRange on: " + targetFlow.report());
        //}
        
        return prioritizeNavalTargets && targetFlow.type == CombatFlow.Type.NAVAL && distance < schutDistance;
    }

    private void setGunState(bool gunSet)
    {
        if (gunSet != gunsOn && gun != null)
        {
            gunsOn = gunSet;
            if (gunSet)
            {
                gunfireSound.loop = true;
                gunfireSound.Play();

                if(gun != null)
                {
                    gun.Play();
                }
                
            }
            else
            {
                gunfireSound.loop = false;
                gunfireSound.Play();

                gunfireSoundEnd.Play();

                gun.Stop();
            }
        }

        if(tankTurret != null)
        {
            tankTurret.fireMission = gunSet;
            if (gunSet)
            {
                tankTurret.target = targetRb.gameObject;
            }
            else
            {
                tankTurret.target = null;
            }

        }
    }

    public static Quaternion calculateBulletLeadRot(Vector3 myPos, Vector3 targetPosition,
        Vector3 relativeVelocity, float bulletSpeed, float targetVelMultiplier = 1.0f,
        AI_TurretMG turret = null, float targetClosingTrim = 1.0f)
    {
        float distance = Vector3.Distance(myPos, targetPosition);
        Vector3 targetBearingLine = targetPosition - myPos;


        targetBearingLine = Vector3.Project(relativeVelocity, targetBearingLine);

        float closingVel = targetBearingLine.magnitude * targetClosingTrim;
        if (Vector3.Distance(myPos, targetPosition + targetBearingLine) < distance)
        {
            closingVel *= -1f;
        }

        float timeToImpact = distance / (bulletSpeed - closingVel);

        

        Vector3 targetPos = targetPosition + relativeVelocity * timeToImpact * targetVelMultiplier;

        if (turret != null)
        {
            turret.estimatedTimeToImpact = timeToImpact;
            turret.estimatedImpactPoint = targetPos;

        }

        return Quaternion.LookRotation(targetPos - myPos, Vector3.up);
    }

    public static Quaternion calculateBulletLeadRot(Vector3 myPos, Rigidbody targetBody, 
        float bulletSpeed, float targVelMultiplier, AI_TurretMG turret = null, 
        float targetClosingTrim = 1.0f)
    {
        Vector3 relativeVelocity = targetBody.velocity;

        return calculateBulletLeadRot(myPos, targetBody.transform.position, relativeVelocity, 
            bulletSpeed, targVelMultiplier, turret, targetClosingTrim);
    }

    public static Quaternion calculateBulletLeadRot(Rigidbody origBody, Rigidbody targetBody,
        float bulletSpeed, float targVelMultiplier = 1.0f, AI_TurretMG turret = null,
        float targetClosingTrim = 1.0f)
    {
        //Debug.Log("Calculatebullet lead for " + origBody.gameObject.name);
        // Velocity of target with origBody as the moving reference frame
        Vector3 relativeVelocity = targetBody.velocity - origBody.velocity;

        Vector3 myPos = origBody.transform.position;
        if(turret != null)
        {
            myPos = turret.transform.position;
        }

        return calculateBulletLeadRot(myPos, targetBody.transform.position, 
            relativeVelocity, bulletSpeed, targVelMultiplier, turret, targetClosingTrim);

    }

    private bool targetInParams()
    {

        float distance = Vector3.Distance(transform.position, targetRb.transform.position);
        int terrainLayer = 1 << 10; // line only collides with terrain layer
        bool hasLineOfSight = bypassLineOfSight || !Physics.Linecast(transform.position, targetRb.transform.position, terrainLayer);
        return distance < schutDistance && hasLineOfSight && (targetIsAbove(targetRb) || !onlyTargetAbove) ;
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

    public float getFuzeTime()
    {
        return estimatedTimeToImpact;
    }

    public Vector3 getFuzePos()
    {
        return estimatedImpactPoint;
    }

}
