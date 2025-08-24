using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiCreepGunController : MonoBehaviour
{

    Rigidbody myRb;
    CombatFlow myFlow;
    StrategicTarget myStrat;


    public float retargetTimeMax = 2f;
    private float retargetTimeCurrent;

    public float gunMaxRange;

    void Awake()
    {
        myRb = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
        myStrat = GetComponent<StrategicTarget>();
    }

    void FixedUpdate()
    {
        if(retargetTimeCurrent < 0)
        {
            // Retargeting process
            retargetProcess();

            retargetTimeCurrent = retargetTimeMax;
        }
        else
        {
            retargetTimeCurrent -= Time.fixedDeltaTime;
        }
        

    }

    void retargetProcess()
    {

        List<CombatFlow> possibleTargets = new List<CombatFlow>();

        for(int i = 0; i < myStrat.enemyLane.frontWave.Count; i++)
        {
            CombatFlow currentCreep = myStrat.enemyLane.frontWave[i];

            if(currentCreep != null && Vector3.Distance(transform.position, currentCreep.transform.position) < gunMaxRange)
            {
                possibleTargets.Add(currentCreep);
            }
        }


    }
}
