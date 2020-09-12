using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI_Aircraft))]
public class AI_GroundAttack : MonoBehaviour
{

    public float maxGroundTargetDist;

    public float closeRange; // within this dist, use nose angle

    List<CombatFlow> enemyGroundUnitsContainer;

    //public GameObject debugLeaderRef;
    //public GameObject debugRetreatLeader;

    public float groundCombatRadius = 4500f;

    public AI_TgtComputer aiTgtComp;

    private CombatFlow myFlow;

    public bool retreating = false;

    public float minRetreatDist = 500f;

    public float retreatDistScalar = 1.5f;

    public int laneIndex = 0;

    public LaneManager enemyLane;
    public LaneManager myLane;

    void Awake()
    {
        aiTgtComp = GetComponent<AI_TgtComputer>();
        myFlow = GetComponent<CombatFlow>();
    }

    // Start is called before the first frame update
    void Start()
    {
        assignToLane(laneIndex);
        //enemyGroundUnitsContainer = GameManager.getGM().debugGroundTgtList;
        //debugLeaderRef = GameManager.getGM().debugLeader;
        //debugRetreatLeader = GameManager.getGM().debugRetreatLeader;
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

        
        for(int i = 0; i < enemyGroundUnitsContainer.Count; i++)
        {
            CombatFlow currUnit = enemyGroundUnitsContainer[i];

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
                groundUnit = enemyGroundUnitsContainer[smallestAngleIndex];
            }
            else
            {
                groundUnit = enemyGroundUnitsContainer[shortestDistIndex];
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
        return enemyLane.getLeader() != null && Vector3.Distance(transform.position, enemyLane.getLeader().transform.position) < groundCombatRadius;
    }

    public bool checkIsRetreating(CombatFlow target)
    {

        //retreating =  !aiTgtComp.hardpoints.isReadyToFire();

        if (target != null)
        {

            if (retreating)
            {
                // check if we can reset retreating to false
                retreating = inEnemyCoverageZone(target, target.maxCoverageRadius) || !aiTgtComp.hardpoints.isReadyToFire();
            }
            else // we are not retreating
            {
                // check if retreating needs to be set to true
                //  retreat if heardpoint is not ready to fire
                //  retreat if we're inside kill radius
                retreating = !aiTgtComp.hardpoints.isReadyToFire() ||
                    inEnemyCoverageZone(target, target.killCoverageRadius);
            }
        }

        return retreating;
    }

    public bool inEnemyCoverageZone(CombatFlow target, float radius)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);

        radius *= retreatDistScalar;

        //Debug.Log("InEnemyCoverageZone against: " + target + ", distance: " + distance + ", radius: " + radius);

        return  distance < radius || distance < minRetreatDist;
    }

    // 0 always top lane
    // 1 always bottom lane
    public void assignToLane(int laneID)
    {
        GameManager gm = GameManager.getGM();

        myLane = gm.getTeamLanes(myFlow.team)[laneID];
        enemyLane = gm.getTeamLanes(myFlow.getEnemyTeam())[laneID];

        enemyGroundUnitsContainer = enemyLane.myLaneUnits;

        //GameManager.getGM()
    }
}
