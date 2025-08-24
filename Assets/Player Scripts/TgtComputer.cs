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

    public CombatFlow myFlow;


    public float changeTargetMaxAngle;

    public bool tgtButtonUp;

    public float visibleRange;

    private HardpointController hardpointController;

    //public bool canShowCurrentTarget;


    public AudioSource lockTone;

    private bool playingLockTone;

    public RangeLadder rangeLadder;

    // Start is called before the first frame update
    void Start()
    {

        myRadar = GetComponent<Radar>();
        playerInput = GetComponent<PlayerInput_Aircraft>();
        mainHud = hudControl.mainHud.GetComponent<hudControl>();
        myFlow = GetComponent<CombatFlow>();
        hardpointController = playerInput.hardpointController;

        if (myFlow.isLocalPlayer)
        {
            linkToRangeLadder();
        }
    }


    private void linkToRangeLadder()
    {
        rangeLadder = hudControl.mainHud.GetComponent<hudControl>().rangeLadder;
        rangeLadder.linkedTgtComputer = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (tgtButtonUp)
        {
            changeTarget();
        }

        mainHud.showRangeLadder(myRadar.weaponLinked && myRadar.lockType == Radar.LockType.AIR_ONLY);

        if(currentTarget != null)
        {
            
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.green);

            if(currentTarget.team == myFlow.team)
            {
                Debug.Log("Deselecting target because it is on same team");
                deselectFriendly(); // should deselect if friendly targeted
            }

            if (!radarLocked && playingLockTone)
            {
                lockTone.loop = false;
                lockTone.Stop();
                playingLockTone = false;
            }

        }
        else if (playingLockTone)
        {
            lockTone.loop = false;
            lockTone.Stop();
            playingLockTone = false;
        }
    }


    // once per frame, after physics update
    private void LateUpdate()
    {

        if (myFlow.isLocalPlayer)
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

                            // ....oh this is crusty architecture. The hud controller should be handling hud display specifics
                            // oh boy

                            //  =====================  DISTANCE

                            // Distance between this gameobject and target
                            currentFlowHudIcon.currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);


                            // ======================== LINE OF SIGHT
                            int terrainLayer = 1 << 10; // line only collides with terrain layer
                            currentFlowHudIcon.hasLineOfSight = !Physics.Linecast(transform.position, currentFlow.transform.position, terrainLayer);


                            // ========================  IFF
                            currentFlowHudIcon.isFriendly = myFlow.team == currentFlow.team;

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



                                if (currentFlow.team == myFlow.team)
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
                            if (currentFlow.team != myFlow.team && currentFlow.type != CombatFlow.Type.PROJECTILE)
                            {
                                
                                if (isVisible)
                                {
                                    currentFlow.tryAddSeenBy(myFlow.photonView.ViewID);
                                }
                                else
                                {
                                    currentFlow.tryRemoveSeenBy(myFlow.photonView.ViewID);
                                }

                                currentFlowHudIcon.dataLink = currentFlow.checkSeen(myFlow.photonView.ViewID);
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



    public CombatFlow autoTargetGround(bool targetByClosest = true, bool hogMode = true)
    {
        CombatFlow newTarget = null;

        // First try to target a vulnerable SAM
        newTarget = changeTarget(CombatFlow.Type.SAM, targetByClosest, hogMode); // target by closest

        if(newTarget == null)
        {
            // If no SAM's found, try to target AAA gun
            newTarget = changeTarget(CombatFlow.Type.ANTI_AIR, targetByClosest, hogMode);

            if(newTarget == null)
            {
                // if no AAA guns found, try to target ground unit
                newTarget = changeTarget(CombatFlow.Type.GROUND, targetByClosest, hogMode);
            }
        }
        

        return newTarget;
    }

    public void deselectFriendly()
    {
        if(currentTarget != null && (currentTarget.team == myFlow.team))
        {
            // end lock on friendly
            if (radarLocked && currentTarget.rwr != null)
            {
                currentTarget.rwr.endNetLock(myRadar);
            }

            // deselect friendly
            currentTarget = null;
            prevTarget = null; // prevent auto-reselection of friendly
        }
    }

    public CombatFlow changeTarget(CombatFlow.Type desiredTargetType = (CombatFlow.Type)(-1), bool targetByClosest = false, bool hog = false)
    {
        //Debug.Log("================== Searching for targets...");
        CombatFlow newTarget = null; // default point to currentTarget -- if changeTarget unsuccessful, this won't change
        
        

        // end lock on current target regardless of who is new target
        if(currentTarget != null && radarLocked && currentTarget.rwr != null)
        {
            currentTarget.rwr.endNetLock(myRadar);
        }

        

        if (targetByClosest)
        {
            newTarget = selectByDistance(desiredTargetType, hog);
        }
        else
        {
            newTarget = selectByLookAngle(desiredTargetType, hog);
        }

        

        linkUItoNewTarget(newTarget);

        

        return currentTarget = newTarget;
    }

    // change target to unit closet to zero angle off of camera direction
    public CombatFlow selectByLookAngle(CombatFlow.Type desiredTargetType = (CombatFlow.Type)(-1), bool hog = false)
    {
        float smallestAngle = 180f; //180 is max angle Vector3.angleBetween gives

        CombatFlow newTarget = null;

        List<CombatFlow> flowObjArray = CombatFlow.combatUnits;
        // loop through every combatFlow object
        for (short i = 0; i < flowObjArray.Count; i++)
        {
            if (flowObjArray[i] != null)
            {

                CombatFlow currentFlow = flowObjArray[i];

                if (currentFlow != null && ((int)desiredTargetType == -1 || currentFlow.type == desiredTargetType))
                {

                    float currentAngle = Vector3.Angle(playerInput.cam.camRef.transform.forward, currentFlow.transform.position
                        - playerInput.cam.camRef.transform.position);

                    // If maverick is equipped, bias target selection in favor of SAM's
                    if (hardpointController.getActiveHardpoint().weaponTypePrefab.name.Equals("Maverick"))
                    {
                        if (currentFlow.type == CombatFlow.Type.SAM)
                        {
                            currentAngle *= 0.35f; // decrease the angle to make it more likely to be selected
                        }
                        else if (currentFlow.type == CombatFlow.Type.ANTI_AIR)
                        {
                            currentAngle *= 0.6f;
                        }
                    }

                    // angle within max, angle smallest, and target is not on same team as localPlayer
                    if ((currentFlow.myHudIconRef.isDetected || currentFlow.myHudIconRef.dataLink) &&
                        //!currentFlow.myHudIconRef.isFar &&
                        currentFlow.isActive &&
                        currentFlow.type != CombatFlow.Type.PROJECTILE && // cannot lock onto projectiles
                        currentAngle < changeTargetMaxAngle &&
                        currentAngle < smallestAngle &&
                        !currentFlow.isLocalPlayer &&
                        currentFlow.team != myFlow.team

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

        return newTarget;
    }

    public CombatFlow selectByDistance(CombatFlow.Type desiredTargetType = (CombatFlow.Type)(-1), bool hogMode = false)
    {
        List<CombatFlow> flowObjArray = CombatFlow.combatUnits;
        CombatFlow newTarget = null;

        float changeTargetMaxDistance = 6000f;  // MAKE THIS A PUBLIC VARIABLE VARIABLE. Or read max range from equipped weapon?
        changeTargetMaxDistance = myRadar.maxLockRange;
        float smallestDistance = changeTargetMaxDistance;
        

        // loop through every CombatFlow object
        for(short i = 0; i < flowObjArray.Count; i++)
        {
            CombatFlow currentFlow = flowObjArray[i];

            // if target is of valid type
            if (currentFlow != null && currentFlow != null && ((int)desiredTargetType == -1 || currentFlow.type == desiredTargetType))
            {

                float currentDistance = (currentFlow.transform.position - transform.position).magnitude;

                if ((currentFlow.myHudIconRef.isDetected || currentFlow.myHudIconRef.dataLink) &&
                        //!currentFlow.myHudIconRef.isFar &&
                        currentFlow.isActive &&
                        currentFlow.type != CombatFlow.Type.PROJECTILE && // cannot lock onto projectiles
                        currentDistance < changeTargetMaxDistance &&
                        currentDistance < smallestDistance &&
                        !currentFlow.isLocalPlayer &&
                        currentFlow.team != myFlow.team

                        )
                {

                    // target only valid if:
                    // Hog mode is NOT enabled
                    // Or, if hog mode IS enabled, target must not have any incoming missiles
                    if (!hogMode || 
                        (currentFlow.rwr != null &&     // target has RWR
                        currentFlow.rwr.incomingMissiles.Count == 0) // target has no incoming missiles
                        && Vector3.Angle(transform.forward, currentFlow.transform.position
                        - transform.position) < myRadar.lockAngle) // angle off nose is within max lock angle
                    {
                        // Valid target within parameters, continue checking if closer target exists
                        smallestDistance = currentDistance;
                        newTarget = flowObjArray[i].GetComponent<CombatFlow>();
                        prevTarget = newTarget;
                    }

                    
                }
            }
        }

        return newTarget;
    }

    

    void linkUItoNewTarget(CombatFlow newTarget)
    {
        if (newTarget != null)
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

        if (newTarget != currentTarget) // changing to new target
        {
            if (currentTarget != null)
            {
                // deselect process for previous target
                currentTarget.myHudIconRef.targetedState = TgtHudIcon.TargetedState.NONE;
            }
        }
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
                // do a thing on the first frame that the target switches to locked
                if(currentFlow.myHudIconRef.targetedState != TgtHudIcon.TargetedState.LOCKED)
                {
                    lockTone.loop = true;
                    lockTone.Play();
                    playingLockTone = true;

                    if(currentFlow.rwr != null)
                    {
                        currentFlow.rwr.netLockedBy(myRadar);
                    }
                }

                currentFlow.myHudIconRef.targetedState = TgtHudIcon.TargetedState.LOCKED;
                //mainHud.showRangeLadder()

            }
            else
            {
                // do a thing on the first frame that the enemy switches away from being locked
                if (currentFlow.myHudIconRef.targetedState == TgtHudIcon.TargetedState.LOCKED)
                {
                    lockTone.loop = false;
                    lockTone.Stop();
                    playingLockTone = false;

                    if (currentFlow.rwr != null)
                    {
                        currentFlow.rwr.endNetLock(myRadar);
                    }
                }
                currentFlow.myHudIconRef.targetedState = TgtHudIcon.TargetedState.TARGETED;
            }
        }
        else // confirm no targeted state if this flow is NOT the current target
        {
            // do a thing on the first frame that the enemy switches away from being locked
            if (currentFlow.myHudIconRef.targetedState == TgtHudIcon.TargetedState.LOCKED)
            {
                lockTone.loop = false;
                lockTone.Stop();
                playingLockTone = false;

                if (currentFlow.rwr != null)
                {
                    currentFlow.rwr.endNetLock(myRadar);
                }
            }
            currentFlow.myHudIconRef.targetedState = TgtHudIcon.TargetedState.NONE;
        }
    }
}
