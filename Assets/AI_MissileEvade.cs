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
    public float flareDropTime = 1f;

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

            float currentDist = currentDir.magnitude;
            float impactTime = currentDist / msl.myRb.velocity.magnitude;

            currentDir.y = 0f; // don't climb or descent when fleeing

            if(impactTime < spiralTurnTime)
            {

                currentDir = Vector3.Cross(myRb.velocity, -currentDir); // negative so that vector faces enemy msl
            }

            flare.flareButtonDown = impactTime < flareDropTime;
        }


        return currentDir;
    }
}
