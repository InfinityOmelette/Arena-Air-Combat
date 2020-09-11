using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI_Aircraft))]
public class AI_GroundAttack : MonoBehaviour
{

    public float maxGroundTargetDist;

    public float closeRange; // within this dist, use nose angle

    List<CombatFlow> groundUnitsContainer;

    public GameObject debugLeaderRef;
    public GameObject debugRetreatLeader;

    public float groundCombatRadius = 4500f;

    public AI_TgtComputer aiTgtComp;

    public bool retreating = false;

    public float minRetreatDist = 500f;

    public float retreatDistScalar = 1.5f;

    void Awake()
    {
        aiTgtComp = GetComponent<AI_TgtComputer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        groundUnitsContainer = GameManager.getGM().debugGroundTgtList;
        debugLeaderRef = GameManager.getGM().debugLeader;
        debugRetreatLeader = GameManager.getGM().debugRetreatLeader;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CombatFlow findGroundTarget()
    {

        CombatFlow target = findGroundTargetByType(CombatFlow.Type.SAM);

        if(target == null)
        {
            target = findGroundTargetByType(CombatFlow.Type.ANTI_AIR);

            if(target == null)
            {
                findGroundTargetByType(CombatFlow.Type.GROUND);
            }
        }

        return target;
    }

    // try not to target an already attacked enemy
    // if all enemies are attacked, target nearest attacked enemy
    // if there are no enemies at all, return null
    public CombatFlow findGroundTargetByType(CombatFlow.Type type)
    {
        CombatFlow maybeAttackedEnemy = findGroundTargetByType(type, true);
        CombatFlow uniqueEnemy = findGroundTargetByType(type, false);

        if(uniqueEnemy == null)
        {
            return maybeAttackedEnemy;
        }
        else
        {
            return uniqueEnemy;
        }
    }

    public CombatFlow findGroundTargetByType(CombatFlow.Type type, bool includeAttackedEnemies)
    {
        // start by just finding closest ground target
        //  if any units are within closeRange, use nose angle
        //    --> select the smallest angle unit that is ALSO within closeRange
        //  otherwise, just simply select the closest unit

        CombatFlow groundUnit = null;


        bool useNoseAngle = false;

        bool firstSet = false;

        float smallestAngle = 0;
        int smallestAngleIndex = -1;

        float shortestDist = 0;
        int shortestDistIndex = -1;

        
        for(int i = 0; i < groundUnitsContainer.Count; i++)
        {
            CombatFlow currUnit = groundUnitsContainer[i];

            if (currUnit != null && currUnit.type == type && (includeAttackedEnemies || !aiTgtComp.maxMissilesOnTarget(currUnit)))
            {
                float currDist = Vector3.Distance(transform.position, currUnit.transform.position);
                float currAngle = Vector3.Angle(transform.forward, currUnit.transform.position - transform.position);

                if (!firstSet)
                {
                    smallestAngle = currAngle;
                    smallestAngleIndex = i;

                    shortestDist = currDist;
                    shortestDistIndex = i;

                    firstSet = true;
                }

                if (currDist < shortestDist)
                {
                    shortestDist = currDist;
                    shortestDistIndex = i;
                }

                if(currAngle < smallestAngle && currDist < closeRange)
                {
                    smallestAngle = currAngle;
                    smallestAngleIndex = i;

                    useNoseAngle = true;
                }
            }
        }

        if (firstSet) // return unit will remain null if none are found
        {
            if (useNoseAngle)
            {
                groundUnit = groundUnitsContainer[smallestAngleIndex];
            }
            else
            {
                groundUnit = groundUnitsContainer[shortestDistIndex];
            }
        }

        Debug.Log("Attacking ground unit: " + groundUnit + ", useNoseAngle: " + useNoseAngle);

        return groundUnit;

    }

    public Vector3 calculateAttackPos(CombatFlow groundTarget)
    {
        // Master maneuvering function that handles all piloting maneuvers for attacking a given ground unit

        // Handle running in and separating

        return groundTarget.transform.position;
    }

    public bool checkGroundCombat()
    {
        return debugLeaderRef != null && Vector3.Distance(transform.position, debugLeaderRef.transform.position) < groundCombatRadius;
    }

    public bool checkIsRetreating(CombatFlow target)
    {
        if (retreating)
        {
            // check if we can reset retreating to false
            retreating = inEnemyCoverageZone(target, target.maxCoverageRadius);
        }
        else // we are not retreating
        {
            // check if retreating needs to be set to true
            retreating = inEnemyCoverageZone(target, target.killCoverageRadius);
        }

        return retreating;
    }

    public bool inEnemyCoverageZone(CombatFlow target, float radius)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);

        radius *= retreatDistScalar;

        Debug.Log("InEnemyCoverageZone against: " + target + ", distance: " + distance + ", radius: " + radius);

        return  distance < radius || distance < minRetreatDist;
    }
}
