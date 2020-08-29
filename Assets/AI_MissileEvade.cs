using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        CombatFlow msl = myRWR.closestMissile;

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
            if(false)
            {
                currentDir = spiralTurn(msl, currentDir);
                //currentDir = Vector3.Cross(myRb.velocity, -currentDir); // negative so that vector faces enemy msl
            }
            else if (impactTime < highPullTime)
            {
                currentDir = Vector3.up;
            }
            else
            {
                currentDir = planarDrag(msl, currentDir);
            }

            flare.flareButtonDown = impactTime < flareDropTime;
        }


        return currentDir;
    }

    private Vector3 planarDrag(CombatFlow msl, Vector3 dir)
    {
        // point from me to enemy
        Vector3 targetBearingLine = msl.transform.position - transform.position;

        float dragAngle = 45f;

        Vector3 dragDir = -targetBearingLine; // negative, so from enemy, pointing towards me

        // CRITICAL FLAW I NEED TO ADDRESS. IF TURN DIRECTION IS TOWARDS AN UNCLIMBABLE WALL, PLANE WILL GO STRAIGHT
        //  I NEED TO MAKE PLANE GIVE UP ON THAT DIRECTION, AND TURN THE OTHER WAY
        
       
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
