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

    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        missile = GetComponent<BasicMissile>();
    }

    // Start is called before the first frame update
    void Start()
    {

        radOffIndicator = hudControl.mainHud.GetComponent<hudControl>().radOffIndicator;

        if (myFlow.isLocalPlayer)
        {
            radOffIndicator.SetActive(!radarOn);
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

        if (myFlow.isLocalPlayer && rwrIcon != null)
        {
            GameObject.Destroy(rwrIcon.gameObject);
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
            isDetected = radarOn &&
                withinScope(targetFlow.transform.position) &&
                maxDetectRange > Vector3.Distance(targetFlow.transform.position, transform.position) && // max range
                calculateDetectability(targetFlow) > detectionThreshold; // detection calculation
        }

        return isDetected;
    }
  
    public bool tryLock(CombatFlow targetFlow)
    {
        if (lockableType(targetFlow))
        {
            float angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);
            float dist = Vector3.Distance(targetFlow.transform.position, transform.position);
            return radarOn && angleOffNose < lockAngle && dist < maxLockRange && tryDetect(targetFlow);
        }
        else
        {
            return false;
        }
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

    //public string get
}
