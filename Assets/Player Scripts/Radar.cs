﻿using System.Collections;
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

    // This radar's performance:
    public float scanConeAngle;
    public float maxDetectRange;
    
    public float lockAngle;
    public float detectionThreshold;

    public bool radarOn;

    public string radarID;

    private float pingWaitCurrent;

    public IconRWR rwrIcon;

    private RWR localPlayerRWR;
    private CombatFlow localPlayerFlow;

    private CombatFlow myFlow;

    // Start is called before the first frame update
    void Start()
    {
        myFlow = GetComponent<CombatFlow>();
        pingWaitCurrent = RWR_PING_DELAY;

        spawnRwrIcon();

        if (myFlow.type == CombatFlow.Type.PROJECTILE)
        {
            setRadarActive(false);
        }
        else
        {
            setRadarActive(true);
        }
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
            GameObject localPlayer = GameManager.getGM().localPlayer;
            if (localPlayer != null)
            {
                localPlayerRWR = GameManager.getGM().localPlayer.GetComponent<RWR>();
                localPlayerFlow = localPlayerRWR.GetComponent<CombatFlow>();
            }
        }

        // no friendly pings
        if (!myFlow.isLocalPlayer && localPlayerRWR != null && myFlow.team != localPlayerFlow.team)
        {
            if (pingTimer())
            {
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

        if ( localPlayerRWR != null)
        {
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
        float angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);
        return  radarOn && tryDetect(targetFlow) && angleOffNose < lockAngle;
    }

    private float calculateDetectability(CombatFlow targetFlow)
    {
        float distMod = calculateDistMod(targetFlow);
        //float distAddMod = 0.65f;

        return targetFlow.detectabilityCoeff * (calculateDepthMod(targetFlow) + distMod);
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
