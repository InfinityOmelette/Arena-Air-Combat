using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmiralStrategy : MonoBehaviour
{
    public float bombardStandoffOnAxis = 6000f;

    public LaneAdmiral admiral;


    public float standoffHaltBuffer = 500f;

    private void Awake()
    {
        admiral = GetComponent<LaneAdmiral>();    
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public ShipNavigation.NavMode fleetStrategy()
    {
        ShipNavigation.NavMode navMode = ShipNavigation.NavMode.ADVANCE;

        float leaderAxisPos = admiral.laneAxisPos(admiral.getLeader());

        // whichever is lower --> enemy fleet, or frontline enemy structure
        

        // Check enemy fleet. If within range, standoff from them and engage
        float enemyFleetAxisPos = 
            admiral.laneAxisPos(admiral.opponentAdmiral.getLeader());


        // If enemy fleet far, Check enemy structures. Standoff from them and engage
        StrategicTarget frontlineEnemyStruct = findFrontlineEnemyStructure();
        float structAxisPos = admiral.laneAxisPos(frontlineEnemyStruct.gameObject);

        float desireAxisPos = Mathf.Min(enemyFleetAxisPos, structAxisPos);

        float leaderDeltaToDesirePos = desireAxisPos - leaderAxisPos;

        navMode = navToPosByDelta(leaderDeltaToDesirePos);


        return navMode;
    }

    public ShipNavigation.NavMode navToPosByDelta(float deltaToDesirePos)
    {
        ShipNavigation.NavMode navMode = ShipNavigation.NavMode.ADVANCE;

        if (Mathf.Abs(deltaToDesirePos) < standoffHaltBuffer)
        {
            navMode = ShipNavigation.NavMode.STOP;
        }
        else if (deltaToDesirePos > 0f)
        {
            navMode = ShipNavigation.NavMode.ADVANCE;
        }
        else
        {
            navMode = ShipNavigation.NavMode.RETREAT;
        }

        return navMode;
    }

    public StrategicTarget findFrontlineEnemyStructure()
    {
        List<StrategicTarget> allStructs = StrategicTarget.AllStrategicTargets;

        float smallestAxisPos = 80000f; // arbitrarily large start dist

        StrategicTarget closestTarget = null;

        for(int i = 0; i < allStructs.Count; i++)
        {
            StrategicTarget currentStruct = allStructs[i];

            if(currentStruct.myFlow.team != admiral.team
                && currentStruct.lane == admiral.lane)
            {
                float currentAxisPos = admiral.laneAxisPos(currentStruct.gameObject);

                if(currentAxisPos < smallestAxisPos)
                {
                    closestTarget = currentStruct;
                    smallestAxisPos = currentAxisPos;
                }

            }
        }

        return closestTarget;
    }
}
