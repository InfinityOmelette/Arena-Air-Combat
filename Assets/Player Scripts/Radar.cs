using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Radar : MonoBehaviour
{
  
    // This radar's performance:
    public float scanConeAngle;
    public float maxDetectRange;
    
    public float lockAngle;
    public float detectionThreshold;

    // Global physics coefficients:
    public static float depthMod = 600; // at x meters depth, this factor is maxed
    public static float colorMod = 317; // at y velocity towards/away from me, this factor is maxed
    public static float distMod = 710;  // "a" value in desmos. Horizontal stretch
    public static float distCoeff = 2.5f; // "b" value in desmos. Vertical stretch. 2.5 average. effective distMod at 0 distance


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // call after distance section of tgtComputer
    public bool tryDetect(CombatFlow targetFlow)
    {

        bool isDetected = false;

        if (targetFlow != null)
        {

            float angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);


            isDetected = targetFlow.myHudIconRef.hasLineOfSight && // line of sight
                maxDetectRange > Vector3.Distance(targetFlow.transform.position, transform.position) && // max range
                angleOffNose < scanConeAngle && // scan cone
                calculateDetectability(targetFlow) > detectionThreshold; // detection calculation

        }

        return isDetected;
    }

  
    public bool tryLock(CombatFlow targetFlow)
    {
        float angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);
        return tryDetect(targetFlow) && angleOffNose < lockAngle;
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

    private float calculateColorMod(CombatFlow targetFlow)
    {
        float colorMod = 0.0f;

        Rigidbody targetRb = targetFlow.GetComponent<Rigidbody>();
        if(targetRb != null)
        {
            Vector3 velocity = targetRb.velocity;
            velocity = Vector3.Project(velocity, targetFlow.transform.position - transform.position);
            colorMod = Mathf.Min(velocity.magnitude / Radar.colorMod, 1.0f);
        }

        return colorMod;
    }

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
}
