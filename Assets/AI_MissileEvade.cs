using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI_Aircraft))]
public class AI_MissileEvade : MonoBehaviour
{

    RWR myRWR;
    Rigidbody myRb;
    AI_Aircraft airAI;
    FlareEmitter flare;

    //public float spiralTurnRange = 750f;
    public float spiralTurnTime = 3f;

    //public float flareDropRange = 250f;
    public float flareDropTime = .85f;

    public float minDiveAlt;
    public float minSpiralSpeed;

    public int dragDirSign = 1;

    public float dragSignSwitchAngle;

    public float highPullTime;
    public float minHighPullSpeed;

    public bool offensive = true;

    public float dragOffsetAngle = 45f;

    public float flareMissileVelocityMin;
    

    void Awake()
    {
        myRWR = GetComponent<RWR>();
        myRb = GetComponent<Rigidbody>();
        airAI = GetComponent<AI_Aircraft>();
        flare = GetComponent<FlareEmitter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public Vector3 tryMissileEvade(Vector3 currentDir)
    {
        CombatFlow msl = myRWR.highestThreatMissile;

        flare.flareButtonDown = false;

        if(msl != null)
        {
            // move in opposite direction from incoming missile.
            currentDir = (transform.position - msl.transform.position);

            Vector3 mslRelVel = msl.myRb.velocity - myRb.velocity;


            float currentDist = currentDir.magnitude;
            float impactTime = currentDist / mslRelVel.magnitude;

            currentDir.y = 0f; // don't climb or descent when fleeing

            float speedKPH = myRb.velocity.magnitude * airAI.MS_2_KPH;

            //if (impactTime < spiralTurnTime && speedKPH > minSpiralSpeed)
            //if(false)
            //{
            //    currentDir = spiralTurn(msl, currentDir);
            //    //currentDir = Vector3.Cross(myRb.velocity, -currentDir); // negative so that vector faces enemy msl
            //}
            //else 
            if (impactTime < highPullTime)
            {
                // I should make this pull in other directions

                // if I have the energy to go up
                if (myRb.velocity.magnitude * airAI.MS_2_KPH > minHighPullSpeed)
                {
                    currentDir = Vector3.up;
                    Debug.Log("AVOIDING UP");

                }
                else
                {
                    Debug.Log("AVOIDING BY SAG");
                   

                    // Vector pointing from my position to incoming missile's
                    Vector3 targetBearingLine = msl.transform.position - transform.position;
                    Vector3 missileSagDir = Vector3.ProjectOnPlane(mslRelVel, targetBearingLine);       // remove any closing relative velocity
                    Vector3 correctionDir = Vector3.ProjectOnPlane(-missileSagDir, transform.forward);  // remove any forward/backward component. We want a turn 
                    
                    currentDir = correctionDir;
                }

            }
            else
            {
                currentDir = planarDrag(msl, currentDir);
            }

            flare.flareButtonDown = impactTime < flareDropTime && msl.myRb.velocity.magnitude > flareMissileVelocityMin;
        }


        return currentDir;
    }

    

    private Vector3 planarDrag(CombatFlow msl, Vector3 dir)
    {
        // point from me to enemy missile
        Vector3 mslBearingLine = msl.transform.position - transform.position;
        //mslBearingLine.y = 0f;

        float dragAngle = dragOffsetAngle;

        Vector3 dragDir = -dir; // try to just head in desired directions. Offsets will be based on this

        // Offensive --> try to fly towards target direction. Defensive, maneuver away from missile
        if (!offensive)
        {
            dragDir = -mslBearingLine; // negative, so from enemy, pointing towards me
        }



        //if (airAI.leftIntersect || airAI.rightIntersect)
        //{
        //    dragDirSign = airAI.wallAvoidDirectionNum;
        //}


        dragDir = airAI.yawOffset(dragDir, dragAngle * dragDirSign);

        
        float angle = Vector3.Angle(myRb.velocity, dragDir);

        if(angle < dragSignSwitchAngle)
        {
            Debug.Log("SWITCHING DRAG SIDE");
            dragDirSign *= -1;
            dragDir = airAI.yawOffset(dragDir, dragAngle * dragDirSign * 2);
            // multiplied by 2 to compensate for pointing the wrong way. x1 to get to center, x2 to get to opposite side
        }

        dir = dragDir;

        return dir;
    }

    private Vector3 spiralTurn(CombatFlow msl, Vector3 dir)
    {
        

        Vector3 targetBearingLine = msl.transform.position - transform.position;

        Vector3 spiralDir = Vector3.Cross(myRb.velocity, targetBearingLine);

        //if (spiralDir.y < 0)
        //{
            dir = spiralDir;
            
            float alt = transform.position.y;

            // if too close to the ground, go upwards
            if (spiralDir.y < 0f && alt < minDiveAlt)
            {
                spiralDir.y *= -1f;
            }
        //}

        
        return dir;
        
    }
}
