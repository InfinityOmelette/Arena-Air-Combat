using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TgtComputer : MonoBehaviour
{

    public hudControl mainHud;
    public PlayerInput_Aircraft playerInput;

    public Radar myRadar;

    public CombatFlow currentTarget;
    private CombatFlow prevTarget;


    public bool radarLocked;

    public CombatFlow localPlayerFlow;


    public float changeTargetMaxAngle;

    public bool tgtButtonUp;

    public float visibleRange;

    private HardpointController hardpointController;

    //public bool canShowCurrentTarget;




    // Start is called before the first frame update
    void Start()
    {

        myRadar = GetComponent<Radar>();
        playerInput = GetComponent<PlayerInput_Aircraft>();
        mainHud = hudControl.mainHud.GetComponent<hudControl>();
        localPlayerFlow = GetComponent<CombatFlow>();
        hardpointController = playerInput.hardpointController;
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
            
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.green);
        }
    }


    // once per frame, after physics update
    private void LateUpdate()
    {

        if (localPlayerFlow.isLocalPlayer)
        {

            List<CombatFlow> flowArray = CombatFlow.combatUnits;

            // Loop through all combatUnits
            for (int i = 0; i < CombatFlow.combatUnits.Count; i++)
            {

                if (flowArray[i] != null)
                {

                    // Current CombatFlow to attempt to see
                    CombatFlow currentFlow = flowArray[i];


                    if (currentFlow != null)
                    {

                        TgtHudIcon currentFlowHudIcon = currentFlow.myHudIconRef;

                        if (currentFlowHudIcon != null)
                        {

                            //  =====================  DISTANCE

                            // Distance between this gameobject and target
                            currentFlowHudIcon.currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);

                            // ======================== LINE OF SIGHT
                            int terrainLayer = 1 << 10; // line only collides with terrain layer
                            currentFlowHudIcon.hasLineOfSight = !Physics.Linecast(transform.position, currentFlow.transform.position, terrainLayer);


                            // ========================  IFF
                            currentFlowHudIcon.isFriendly = localPlayerFlow.team == currentFlow.team;

                            //{ // debug block

                            //    Weapon currentWeap = currentFlow.GetComponent<Weapon>();
                            //    if(currentWeap != null && currentFlow.localOwned)
                            //    {
                            //        Debug.LogWarning(currentFlow.gameObject.name + "'s team is: " + currentFlow.team + ", local player's is : "
                            //            + localPlayerFlow.team);
                            //    }

                            //}


                            // =====================  VISIBILITY

                            // Various conditions will attempt to make this true
                            bool isVisible = false;



                            // Show unit if this is NOT the local player
                            if (!currentFlow.isLocalPlayer && currentFlow.isActive)
                            {



                                if (currentFlow.team == localPlayerFlow.team)
                                {
                                    isVisible = true;
                                }
                                else
                                {
                                    isVisible = Vector3.Distance(currentFlow.transform.position, transform.position) < visibleRange
                                        && currentFlow.myHudIconRef.hasLineOfSight;
                                    if (!isVisible)
                                    {
                                        isVisible = myRadar.tryDetect(currentFlow);

                                    }
                                }
                            }

                            // if nonfriendly
                            if (currentFlow.team != localPlayerFlow.team && currentFlow.type != CombatFlow.Type.PROJECTILE)
                            {
                                
                                if (isVisible)
                                {
                                    currentFlow.tryAddSeenBy(localPlayerFlow.photonView.ViewID);
                                }
                                else
                                {
                                    currentFlow.tryRemoveSeenBy(localPlayerFlow.photonView.ViewID);
                                }

                                currentFlowHudIcon.dataLink = currentFlow.checkSeen(localPlayerFlow.photonView.ViewID);
                            }

                            //  Send visibility result
                            currentFlowHudIcon.isDetected = isVisible;

                            if(isVisible && prevTarget != null && currentFlow == prevTarget && currentTarget == null)
                            {
                                currentTarget = currentFlow;
                                TgtHudIcon newTargetHudIcon = currentTarget.myHudIconRef;
                                newTargetHudIcon.targetedState = TgtHudIcon.TargetedState.TARGETED;
                                hudControl.mainHud.GetComponent<hudControl>().mapManager.target = currentTarget.transform;
                            }

                            //canShowCurrentTarget = isVisible;

                            if (!isVisible && !currentFlowHudIcon.dataLink && currentTarget == currentFlow)
                            {
                                currentTarget.myHudIconRef.targetedState = TgtHudIcon.TargetedState.NONE;
                                currentTarget = null;
                                //canShowCurrentTarget = false;
                                hudControl.mainHud.GetComponent<hudControl>().mapManager.target = null;
                                playerInput.cam.lookAtObj = null;

                            }

                            // ========= TRY TO LOCK
                            tryLockTarget(currentFlow);
                        }

                    }
                }

            }
        }
    }



    // change target to unit closet to zero angle off of camera direction
    public CombatFlow changeTarget()
    {
        //Debug.Log("================== Searching for targets...");
        CombatFlow newTarget = null; // default point to currentTarget -- if changeTarget unsuccessful, this won't change
        List<CombatFlow> flowObjArray = CombatFlow.combatUnits;

        float smallestAngle = 180f; //180 is max angle Vector3.angleBetween gives

        // loop through every combatFlow object
        for(short i = 0; i < flowObjArray.Count; i++)
        {
            if (flowObjArray[i] != null)
            {

                CombatFlow currentFlow = flowObjArray[i];

                if (currentFlow != null)
                {

                    float currentAngle = Vector3.Angle(playerInput.cam.camRef.transform.forward, currentFlow.transform.position 
                        - playerInput.cam.camRef.transform.position);
                    //if (currentFlow.type == CombatFlow.Type.AIRCRAFT)
                    //{
                    //    Debug.LogWarning(currentFlow + "'s angle is: " + currentAngle + " off camera center");
                    //}
                    //Debug.Log("Current target is: " + currentFlow.gameObject + ", at " + currentAngle + " degrees, smallest angle is: " + smallestAngle + " degrees.");

                    // angle within max, angle smallest, and target is not on same team as localPlayer
                    if ((currentFlow.myHudIconRef.isDetected || currentFlow.myHudIconRef.dataLink) &&
                        //!currentFlow.myHudIconRef.isFar &&
                        currentFlow.isActive &&
                        currentFlow.type != CombatFlow.Type.PROJECTILE && // cannot lock onto projectiles
                        currentAngle < changeTargetMaxAngle &&
                        currentAngle < smallestAngle &&
                        !currentFlow.isLocalPlayer &&
                        currentFlow.team != localPlayerFlow.team
                        
                        )
                    {
                        //Debug.Log("SMALLEST ANGLE: " + currentAngle + " degrees.");
                        smallestAngle = currentAngle;
                        newTarget = flowObjArray[i].GetComponent<CombatFlow>();
                        prevTarget = newTarget;
                    }
                }
            }


        }

        if(newTarget != null)
        {
            TgtHudIcon newTargetHudIcon = newTarget.myHudIconRef;
            newTargetHudIcon.targetedState = TgtHudIcon.TargetedState.TARGETED;
            hudControl.mainHud.GetComponent<hudControl>().mapManager.target = newTarget.transform;
        }
        else
        {
            hudControl.mainHud.GetComponent<hudControl>().mapManager.target = null;
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

    void tryLockTarget(CombatFlow currentFlow)
    {
        if(currentFlow == currentTarget)
        {
            float weaponScanZone = 0.0f;

            Radar currentWeaponRadar = hardpointController.getActiveHardpoint().weaponTypePrefab.GetComponent<Radar>();

            if(currentWeaponRadar != null)
            {
                weaponScanZone = currentWeaponRadar.scanConeAngle;
                myRadar.lockAngle = weaponScanZone;
                myRadar.maxLockRange = currentWeaponRadar.maxLockRange;
                myRadar.lockType = currentWeaponRadar.lockType;
                radarLocked = myRadar.tryLock(currentFlow);
            }
            else
            {
                radarLocked = false;
            }
            
            if (radarLocked)
            {
                currentFlow.myHudIconRef.targetedState = TgtHudIcon.TargetedState.LOCKED;
            }
            else
            {
                currentFlow.myHudIconRef.targetedState = TgtHudIcon.TargetedState.TARGETED;
            }
        }
        else // confirm no targeted state if this flow is NOT the current target
        {
            currentFlow.myHudIconRef.targetedState = TgtHudIcon.TargetedState.NONE;
        }
    }
}
