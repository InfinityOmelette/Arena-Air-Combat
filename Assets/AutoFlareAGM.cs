using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFlareAGM : AutoFlare
{
    MissileGuidance guidance;

    public float flareSpamDistanceToTarget;

    private void Awake()
    {
        setRefs();
        guidance = GetComponent<MissileGuidance>();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        autoFlareProcess();
        targetProxFlareProcess();
    }

    private void targetProxFlareProcess()
    {
        CombatFlow target = guidance.targetFlowPersistent;

        if(target != null)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if(dist < flareSpamDistanceToTarget)
            {
                flareEmitter.flareButtonDown = true;
            }
        }
    }

}
