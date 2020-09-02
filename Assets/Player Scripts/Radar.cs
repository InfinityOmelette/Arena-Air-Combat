using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class Radar : MonoBehaviourPun
{
    // Global physics coefficients:
    public static float depthMod = 250; // at x meters depth, this factor is maxed
    public static float colorMod = 317; // at y velocity towards/away from me, this factor is maxed
    public static float distMod = 610;  // "a" value in desmos. Horizontal stretch
    public static float distCoeff = 2.5f; // "b" value in desmos. Vertical stretch. 2.5 average. effective distMod at 0 distance

    public float ALTITUDE_ADVANTAGE_FACTOR = 1.0f; //0.7f;
    public float closingSpeedFactor = 12f;
    public float YOUR_SPEED_FACTOR = 6f;
    public float negativeSpeedFactor = 1.0f;
    public float tgtFaceAwayPerpFactor = 1.0f;
    public float myPerpendicularDragFactor;
    public float targetPerpendicularDragFactor;
    public float airDensityFactor;
    //public float avgSpeedFactor;



    // ONLY USED FOR PLAYER LOCK-ON FUNCTION, determine effective missile range.
    // only used in one function. Declared here to avoid unnecessarily recreating these ...
    // ... variables every frame
    private float heightDiffAdvantage;
    private float closingSpeedAdv;
    private float yourSpeedAdv;
    private Vector3 targetBearingLine;
    private float myPerpDragAdv;
    private float targetPerpDragAdv;
    private float avgAlt;
    private float airDensityMod;
    private float angleOffNose;
    private float dist;

    private static float RWR_PING_DELAY = .075f; // must be nonzero

    public enum LockType
    {
        AIR_ONLY,
        GROUND_ONLY,
        AIR_OR_GROUND
    }


    public LockType lockType;

    // This radar's performance:
    public float scanConeAngle;
    public float maxDetectRange;
    public float maxLockRange;
    
    public float lockAngle;
    public float detectionThreshold;

    public bool radarOn;

    public string radarID;

    private float pingWaitCurrent;

    public IconRWR rwrIcon;

    private RWR localPlayerRWR;
    private CombatFlow localPlayerFlow;

    public CombatFlow myFlow;

    private BasicMissile missile;

    public bool pingPlayer = false;

    public bool debug;

    private GameObject radOffIndicator;

    private Rigidbody myRb;

    
    public float baseLongRange;
    public float baseGoodRange;
    public float baseKillRange;

    public float effectiveLongRange;
    public float effectiveGoodRange;
    public float effectiveKillRange;


    public RangeLadder rangeLadder;

    public bool weaponLinked;

    hudControl mainHud;

    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        missile = GetComponent<BasicMissile>();
        myRb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {

        mainHud = hudControl.mainHud.GetComponent<hudControl>();
        radOffIndicator = mainHud.radOffIndicator;

        if (myFlow.isLocalPlayer)
        {
            radOffIndicator.SetActive(!radarOn);
            linkToRangeLadder();
        }

        pingWaitCurrent = RWR_PING_DELAY;

        spawnRwrIcon();

        //if (myFlow.type == CombatFlow.Type.PROJECTILE)
        //{
        //    setRadarActive(false);
        //}
        //else
        //{
        //    setRadarActive(true);
        //}
        //setRadarActive(radarOn);

        
    }


    public void copyLockData(Radar radar)
    {
        if (radar != null)
        {
            weaponLinked = true;
            maxLockRange = radar.maxLockRange;
            baseLongRange = radar.baseLongRange;
            baseGoodRange = radar.baseGoodRange;
            baseKillRange = radar.baseKillRange;
            lockAngle = radar.lockAngle;
            lockType = radar.lockType;


            closingSpeedFactor = radar.closingSpeedFactor;

            myPerpendicularDragFactor = radar.myPerpendicularDragFactor;
            targetPerpendicularDragFactor = radar.targetPerpendicularDragFactor;

            tgtFaceAwayPerpFactor = radar.tgtFaceAwayPerpFactor;
            negativeSpeedFactor = radar.negativeSpeedFactor;

            airDensityFactor = radar.airDensityFactor;

            
        }
        else
        {
            weaponLinked = false;
        }
    }

    private void linkToRangeLadder()
    {
        rangeLadder = hudControl.mainHud.GetComponent<hudControl>().rangeLadder;
        rangeLadder.linkedRadar = this;
    }

    public void toggleRadar()
    {
        setRadarActive(!radarOn);
    }

    public void setRadarActive(bool radarOn)
    {
        photonView.RPC("rpcSetRadarActive", RpcTarget.All, radarOn);
    }

    [PunRPC]
    public void rpcSetRadarActive(bool radarOn)
    {
        if (myFlow.isLocalPlayer)
        {
            radOffIndicator.SetActive(!radarOn);
        }

        //Debug.LogError("Setting radar of " + gameObject.name + " to " + radarOn);
        this.radarOn = radarOn;
    }

    private void spawnRwrIcon()
    {

        hudControl hudRef = hudControl.mainHud.GetComponent<hudControl>();
        rwrIcon = GameObject.Instantiate(hudRef.rwrIconPrefab, hudRef.rwrIconContainer.transform).GetComponent<IconRWR>();
        rwrIcon.linkToRadarSource(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayerRWR == null)
        {
            if (debug)
            {
                Debug.LogWarning("Local player rwr null, pinging set to false");
            }

            rwrIcon.showPingResult(false, 0.0f, 0.0f);

            GameObject localPlayer = GameManager.getGM().localPlayer;
            if (localPlayer != null)
            {
                
                localPlayerRWR = GameManager.getGM().localPlayer.GetComponent<RWR>();
                localPlayerFlow = localPlayerRWR.GetComponent<CombatFlow>();
                if (debug)
                {
                    Debug.LogWarning("Found local player: " + localPlayerFlow.gameObject.name);
                }
                //
            }
        }

        // don't ping yourself
        // can find localPlayerRWR
        // no friendly pings
        if (!myFlow.isLocalPlayer && localPlayerRWR != null && myFlow.team != localPlayerFlow.team)
        {
            if (debug)
            {
                Debug.LogWarning("Counting down timer");
            }

            if (pingTimer())
            {
                if (debug)
                {
                    Debug.LogWarning("Trying to ping");
                }
                tryPing();
            }
        }

        if (myFlow.isLocalPlayer)
        {
            


            if(rwrIcon != null)
            {
                GameObject.Destroy(rwrIcon.gameObject);
            }
            
        }


       
    }

    private void tryPing()
    {

        if ( localPlayerRWR != null && pingPlayer)
        {
            if (debug)
            {
                Debug.LogWarning("Local player found, ping him");
            }
            localPlayerRWR.tryPing(this);
        }
    }

    private bool pingTimer()
    {
        bool readyPing = false;
        pingWaitCurrent -= Time.deltaTime;
        if(pingWaitCurrent < 0)
        {
            readyPing = true;
            pingWaitCurrent = RWR_PING_DELAY;
        }
        return readyPing;
    }

    public bool withinScope(Vector3 position)
    {
        return lineOfSight(position) && withinCone(position);
    }

    public bool lineOfSight(Vector3 position)
    {
        int terrainLayer = 1 << 10; // line only collides with terrain layer
        bool lineOfSight = !Physics.Linecast(transform.position, position, terrainLayer);
        return lineOfSight;
    }

    public bool withinCone(Vector3 position)
    {
        float angleOffNose = Vector3.Angle(position - transform.position, transform.forward);
        return angleOffNose < scanConeAngle;
    }

    public bool tryDetect(CombatFlow targetFlow)
    {
        
        bool isDetected = false;

        if (targetFlow != null)
        {
            isDetected = !targetFlow.jamming && radarOn &&
                withinScope(targetFlow.transform.position) &&
                maxDetectRange > Vector3.Distance(targetFlow.transform.position, transform.position); //&& // max range
                //calculateDetectability(targetFlow) > detectionThreshold; // detection calculation
        }

        return isDetected;
    }
  
    public bool tryLock(CombatFlow targetFlow, bool debug = false)
    {

        if (lockableType(targetFlow) && !targetFlow.jamming)
        {
            

            heightDiffAdvantage = (transform.position.y - targetFlow.transform.position.y) * ALTITUDE_ADVANTAGE_FACTOR;
            closingSpeedAdv = calculateClosingSpeed(targetFlow.myRb) * closingSpeedFactor;
            yourSpeedAdv = myRb.velocity.magnitude * YOUR_SPEED_FACTOR;

            targetBearingLine = targetFlow.transform.position - transform.position;
            myPerpDragAdv = calculatePerpendicularVelocity(targetBearingLine, myRb.velocity) * myPerpendicularDragFactor;
            targetPerpDragAdv = calculatePerpendicularVelocity(targetBearingLine, targetFlow.myRb.velocity) * targetPerpendicularDragFactor;

            float velBearing = Vector3.Angle(targetFlow.myRb.velocity, transform.position - targetFlow.transform.position);
            // if target's velocity is facing away from player
            if(velBearing > 90f)
            {
                velBearing -= 90f; // how far past 90 degrees is velBearing
                velBearing /= 90f; // 1.0 is 90 degrees past perpendicular (straight away), 0.0 is perpendicular

                velBearing = Mathf.Lerp(1.0f, tgtFaceAwayPerpFactor, velBearing);
                velBearing = Mathf.Clamp(velBearing, 0.0f, 1.0f); // allows for 0.0 to be applied across some arc, by using negative tgtFaceAway val
                
                // I want:
                //  1.0 at 90 degrees
                //  tgtFaceAwayPerpFactor at 180

                targetPerpDragAdv *= velBearing;
                //Debug.LogWarning("====================  Target facing away. Cancelling perp factor down to: "+ targetPerpDragAdv + ", multiplier of: " + velBearing);
            }

            // average of three values, target alt twice to give more weight to the slow portion of flight
            avgAlt = (2* targetFlow.transform.position.y + transform.position.y) / 3f; 

            airDensityMod = AirEnvironmentStats.getAir().getDensityAtAltitude(avgAlt) * airDensityFactor;

            setRangeAdvantages(heightDiffAdvantage, closingSpeedAdv, yourSpeedAdv, myPerpDragAdv, targetPerpDragAdv, airDensityMod);

            angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);
            dist = Vector3.Distance(targetFlow.transform.position, transform.position);

            if(rangeLadder != null && myFlow.isLocalPlayer)
            {
                rangeLadder.tgtRange = dist;
            }

            if (debug)
            {
                //Debug.Log("radarOn: " + radarOn + ", good lock angle: " + 
               //     (angleOffNose < lockAngle) + ", goodDist: " + (dist < maxLockRange));

                //Debug.Log("angleOffNose: " + angleOffNose + ", lockAngle: " + lockAngle);
            }

            //return radarOn && angleOffNose < lockAngle && dist < (maxLockRange + heightDiffAdvantage + closingSpeedAdv) && tryDetect(targetFlow);
            return radarOn && angleOffNose < lockAngle && dist < maxLockRange && tryDetect(targetFlow);
        }
        else
        {
            if (debug)
            {
                Debug.Log("BadLockType, target is " + targetFlow.type + ", myRadar is : " + lockType);
            }

            setRangeAdvantages(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            return false;
        }
    }

    private float calculatePerpendicularVelocity(Vector3 targetBearingLine, Vector3 velocity)
    {
        Vector3 perpCross = Vector3.Cross(targetBearingLine, velocity);

        Vector3 perpVel = Vector3.Cross(perpCross, targetBearingLine);

        perpVel = Vector3.Project(velocity, perpVel);


        //if(Vector3.Angle(velocity, ))

        return perpVel.magnitude;
    }

    private void setRangeAdvantages(float heightDiffAdvantage, float closingSpeedAdv, float yourSpeedAdv,
        float myPerpendicularDragAdv, float targetPerpendicularDragAdv, float airDensityMod)
    {
        if(closingSpeedAdv < 0)
        {
            //Debug.LogWarning("******************************  Closing speed negative. Reducing closing speed effects by factor " + negativeSpeedFactor);
            closingSpeedAdv *= negativeSpeedFactor;
            
        }

        float totalAdvantage = heightDiffAdvantage + closingSpeedAdv + yourSpeedAdv
            - myPerpendicularDragAdv - targetPerpendicularDragAdv - airDensityMod;

        //Debug.LogWarning("============  airDensityMod: " + airDensityMod);

        effectiveLongRange = baseLongRange + totalAdvantage;

        float advantageScale = effectiveLongRange / baseLongRange;
        
        effectiveGoodRange = baseGoodRange * advantageScale;
        effectiveKillRange = baseKillRange * advantageScale;
        
    }

    public bool lockableType(CombatFlow flow)
    {
        return flow.type != CombatFlow.Type.PROJECTILE && (lockType == LockType.AIR_OR_GROUND
            || (lockType == LockType.AIR_ONLY && flow.type == CombatFlow.Type.AIRCRAFT)
            || (lockType == LockType.GROUND_ONLY && flow.type != CombatFlow.Type.AIRCRAFT));
    }

    private float calculateDetectability(CombatFlow targetFlow)
    {
        float distMod = calculateDistMod(targetFlow);
        //float distAddMod = 0.65f;

        return targetFlow.detectabilityCoeff * (calculateDepthMod(targetFlow) + distMod) + targetFlow.detectabilityOffset;
        //return 10f;
    }

    private float calculateDistMod(CombatFlow targetFlow)
    {
        float distance = Vector3.Distance(targetFlow.transform.position, transform.position);
        return Radar.distCoeff * Radar.distMod / (distance + Radar.distMod);
    }

    //private float calculateColorMod(CombatFlow targetFlow)
    //{
    //    float colorMod = 0.0f;

    //    Rigidbody targetRb = targetFlow.GetComponent<Rigidbody>();
    //    if(targetRb != null)
    //    {
    //        Vector3 velocity = targetRb.velocity;
    //        velocity = Vector3.Project(velocity, targetFlow.transform.position - transform.position);
    //        colorMod = Mathf.Min(velocity.magnitude / Radar.colorMod, 1.0f);
    //    }

    //    return colorMod;
    //}

    private float calculateDepthMod(CombatFlow targetFlow)
    {
        RaycastHit hit;

        float depthMod = 1.0f;

        int terrainLayer = 1 << 10; // line only collides with terrain layer
        if (Physics.Raycast(targetFlow.transform.position, targetFlow.transform.position - transform.position, out hit, Radar.depthMod, terrainLayer))
        {
            depthMod = hit.distance / Radar.depthMod;
        }


        return depthMod;
    }

    private float calculateClosingSpeed(Rigidbody targetRB)
    {
        Vector3 relVel = targetRB.velocity - myRb.velocity;

        // Vector pointing from my position to target position
        Vector3 targetBearingLine = targetRB.transform.position - transform.position;

        relVel = Vector3.Project(relVel, targetBearingLine);

        float closingSpeed = relVel.magnitude;

        

        // If relative velocity is facing away from us
        if( Vector3.Angle( relVel, targetBearingLine) < 10f)
        {
            closingSpeed *= -1f; // closing speed is negative, as target is moving away
        }

        return closingSpeed;
    }

    //public string get
}
