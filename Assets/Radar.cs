using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public float scanConeAngle;
    public float maxDetectRange;
    public float lockAngle;

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

        float angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);
        
      
        isDetected = targetFlow.myHudIconRef.hasLineOfSight &&
            angleOffNose < scanConeAngle && 
            maxDetectRange > Vector3.Distance(targetFlow.transform.position, transform.position);

        return isDetected;
    }

  
    public bool tryLock(CombatFlow targetFlow)
    {
        float angleOffNose = Vector3.Angle(targetFlow.transform.position - transform.position, transform.forward);
        return tryDetect(targetFlow) && angleOffNose < lockAngle;
    }
}
