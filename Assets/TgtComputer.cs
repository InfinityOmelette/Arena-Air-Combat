using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TgtComputer : MonoBehaviour
{

    public hudControl mainHud;
    public PlayerInput_Aircraft playerInput;

    public CombatFlow currentTarget;
    public bool radarLocked;

    public CombatFlow localPlayerFlow;


    public float changeTargetMaxAngle;

    public bool tgtButtonUp;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput_Aircraft>();
        mainHud = hudControl.mainHud.GetComponent<hudControl>();
        localPlayerFlow = GetComponent<CombatFlow>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tgtButtonUp)
        {
            changeTarget();
        }

        if(currentTarget != null)
        {
            
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.green, 1f);
        }
    }


    // once per frame, after physics update
    private void LateUpdate()
    {

        List<GameObject> flowArray = CombatFlow.combatUnits;

        // Loop through all combatUnits
        for(int i = 0; i < CombatFlow.combatUnits.Count; i++)
        {
            // =====================  VISIBILITY

            // Current CombatFlow to attempt to see
            CombatFlow currentFlow = flowArray[i].GetComponent<CombatFlow>();
            TgtHudIcon currentFlowHudIcon = currentFlow.myHudIconRef;

            // Various conditions will attempt to make this true
            bool isVisible = false; 

            // Show unit if this is NOT the local player
            if (!currentFlow.isLocalPlayer)
            {
                isVisible = true;
            }

            //  Send visibility result
            currentFlowHudIcon.isDetected = isVisible;



            //  =====================  DISTANCE

            // Distance between this gameobject and target
            currentFlowHudIcon.currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);



            // ======================== LINE OF SIGHT
            int terrainLayer = 1 << 10; // line only collides with terrain layer
            currentFlowHudIcon.hasLineOfSight = !Physics.Linecast(transform.position, currentFlow.transform.position, terrainLayer);


            // ========================  IFF
            if (localPlayerFlow.team == currentFlow.team)
                currentFlowHudIcon.isFriendly = true;
            else
                currentFlowHudIcon.isFriendly = false;


        }
    }



    // change target to unit closet to zero angle off of camera direction
    public CombatFlow changeTarget()
    {
        //Debug.Log("================== Searching for targets...");
        CombatFlow newTarget = null; // default point to currentTarget -- if changeTarget unsuccessful, this won't change
        List<GameObject> flowObjArray = CombatFlow.combatUnits;

        float smallestAngle = 180f; //180 is max angle Vector3.angleBetween gives

        // loop through every combatFlow object
        for(short i = 0; i < flowObjArray.Count; i++)
        {
            CombatFlow currentFlow = flowObjArray[i].GetComponent<CombatFlow>();

            float currentAngle = Vector3.Angle(playerInput.cam.camAxisHorizRef.transform.forward, currentFlow.transform.position - transform.position);

            //Debug.Log("Current target is: " + currentFlow.gameObject + ", at " + currentAngle + " degrees, smallest angle is: " + smallestAngle + " degrees.");

            // angle within max, angle smallest, and target is not on same team as localPlayer
            if (currentFlow.type == CombatFlow.Type.AIRCRAFT && // can only lock onto aircraft
                currentAngle < changeTargetMaxAngle && 
                currentAngle < smallestAngle && 
                !currentFlow.isLocalPlayer &&
                currentFlow.team != localPlayerFlow.team)
            {
                //Debug.Log("SMALLEST ANGLE: " + currentAngle + " degrees.");
                smallestAngle = currentAngle;
                newTarget = flowObjArray[i].GetComponent<CombatFlow>();
            }


        }

        if(newTarget != null)
        {
            TgtHudIcon newTargetHudIcon = newTarget.myHudIconRef;
            newTargetHudIcon.targetedState = TgtHudIcon.TargetedState.TARGETED;
        }
        

       // Debug.Log("New Target is: " + newTarget.gameObject + ", at " + smallestAngle + "degrees off nose");

        if(newTarget != currentTarget) // changing to new target
        {
            if (currentTarget != null)
            {
                // deselect process for previous target
                currentTarget.myHudIconRef.targetedState = TgtHudIcon.TargetedState.NONE;
            }
        }

        

        return currentTarget = newTarget;
    }


    void tryLockTarget()
    {

    }
}
