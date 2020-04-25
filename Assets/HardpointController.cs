using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HardpointController : MonoBehaviourPunCallbacks
{


    public Hardpoint[] hardpoints;

    public List<List<Hardpoint>> weaponTypeHardpointLists; // two dimensional list -- hardpoint array for each weapon type
    short[] activeHardpointIndexes;   // use weaponTypeArrays[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]]

    public short activeTypeIndex;
    public List<bool> groupThisType_List;


    public WeaponIndicatorManager weaponIndicatorManager;

    public TgtComputer tgtComputer;

    public float input_scrollWheel;

    public float input_changeWeaponAxis;
    private bool changeButtonHeld;

    public bool launchActive;

    private CombatFlow rootFlow;

    
    // Commands missiles to launch

    private void Awake()
    {
        weaponTypeHardpointLists = new List<List<Hardpoint>>();
        groupThisType_List = new List<bool>();
        rootFlow = transform.root.GetComponent<CombatFlow>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (rootFlow.isLocalPlayer)
        {
            // putting this in Start to guarantee that hudControl's awake() has run first so that static mainHud property is set
            weaponIndicatorManager = hudControl.mainHud.GetComponent<hudControl>().weaponIndicatorManager;
            if (weaponIndicatorManager == null)
                Debug.Log("HARDPOINT CONTROLLER UNABLE TO FIND WEAPON INDICATOR MANAGER");

            fillHardpointArray();
        }
    }

    public void destroyWeapons()
    {
        CombatFlow rootFlow = transform.root.gameObject.GetComponent<CombatFlow>();
        if (rootFlow.isLocalPlayer)
        {
            for(int i = 0; i < hardpoints.Length; i++)
            {
                hardpoints[i].destroyWeapon();
            }
        }
    }

    void fillHardpointArray()
    {
        weaponIndicatorManager.deleteAll();

        // raw array of hardpoints themselves
        hardpoints = new Hardpoint[transform.childCount];

        // loop through each hardpoint -- sort them into lists
        for(int i = 0; i < hardpoints.Length; i++)
        {
            
            hardpoints[i] = transform.GetChild(i).gameObject.GetComponent<Hardpoint>();

            // if type cannot be found, start a new first level list
            // if type is found, add to existing list

            short typeIndex = findTypeIndex(hardpoints[i].weaponTypePrefab);

            // if new type found
            if (typeIndex < 0)
            { 
                weaponTypeHardpointLists.Add(new List<Hardpoint>()); // add new list
                typeIndex = (short)(weaponTypeHardpointLists.Count - 1); // should never be < 0, with list being added this block

                // Read from this prefab if this hardpoint type should be launched together. Add this bool to the list
                groupThisType_List.Add(hardpoints[i].weaponTypePrefab.GetComponent<Weapon>().groupHardpointsTogether);

                // tell weapon indicator manager to spawn new container
                weaponIndicatorManager.spawnNewContainer(hardpoints[i].weaponTypePrefab);

            }
            weaponTypeHardpointLists[typeIndex].Add(hardpoints[i]); // add item to existing list

            Debug.Log("Size of hardpoint list: " + weaponTypeHardpointLists[typeIndex].Count);

            // tell weapon indicator manager to spawn new indicator inside typeIndex container
            weaponIndicatorManager.spawnNewIndicator(typeIndex, 
                (short)(weaponTypeHardpointLists[typeIndex].Count - 1), // hardpoint order position will be size of type's list - 1
                hardpoints[i]); // hardpoint that the new indicator will be linked to

        }

        // all types are now known, have short designating active for each
        activeHardpointIndexes = new short[weaponTypeHardpointLists.Count];


        weaponIndicatorManager.showActiveWeaponType(0); // hud will assume the first weapon type is selected

        Debug.Log("Reached end of hardpoint function");
        
    }

    // search through each list to find existing weapon prefab type
    //  compare argument to first item in each list
    short findTypeIndex(GameObject prefabLink)
    {
        short typeIndex = -1; // -1 if cannot be found

        for(short i = 0; i < weaponTypeHardpointLists.Count && typeIndex == -1; i++)
        {
            if(prefabLink == weaponTypeHardpointLists[i][0].weaponTypePrefab)
            {
                typeIndex = i;
            }
        }

        return typeIndex;
    }


    public Hardpoint getActiveHardpoint()
    {
        return weaponTypeHardpointLists[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]];
    }

    // Update is called once per frame
    void Update()
    {
        if (rootFlow.isLocalPlayer)
        {

            if ((Mathf.Abs(input_changeWeaponAxis) > .3f)) // if definitely pressed, either direction
            {
                if (!changeButtonHeld)
                {
                    changeButtonHeld = true;
                    advanceWeaponType((short)Mathf.RoundToInt(input_changeWeaponAxis));
                }
            }
            else
            {
                changeButtonHeld = false;
                short scrollAdvance = (short)Mathf.RoundToInt(-input_scrollWheel);
                Debug.Log("ScrollAdvance: " + scrollAdvance + "with raw: " + input_scrollWheel);
                advanceWeaponType(scrollAdvance);
            }
        }
    }

    public void launchEndProcess()
    {

        // if fire these together, call launchEnd on whole group

        // otherwise, launchEnd on just active one

        launchActive = false;


        if (groupThisType_List[activeTypeIndex])
        {
            Debug.Log("Grouped Launch End Process");

            // loop through all hardpoints of this type, launchEnd on all
            for (short i = 0; i < weaponTypeHardpointLists[activeTypeIndex].Count; i++)
            {
                weaponTypeHardpointLists[activeTypeIndex][i].launchEnd();
            }
        }
        else // if NOT grouped, launch end on just the active hardpoint
        {
            Debug.Log("Single Launch End Process");
            weaponTypeHardpointLists[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]].launchEnd();
        }
    }

    public void launchProcess()
    {

        launchActive = true;

        if (groupThisType_List[activeTypeIndex]) // if this type is to be launched all together
        {
            Debug.Log("Grouped LaunchProcess");

            // call launch on all of this type
            // do NOT move to next hardpoint. Hardpoints will reload when they can

            // loop through all hardpoints of this type, fire each
            Debug.Log("Amount in group: " + weaponTypeHardpointLists[activeTypeIndex].Count);

            // raw fire rate divided by count in this active type
            float launchDelayPerSequencePos = weaponTypeHardpointLists[activeTypeIndex][0].fireRateDelayRaw / 
                                                (weaponTypeHardpointLists[activeTypeIndex].Count);

            // select starting index
            short index = selectGroupStartIndex(); // write a function to select hardpoint whose weapon has the most shots remaining of those ready to fire

            // loop through each
            //  i is used just to step forward correct number of times. Actual active index is 'index' variable
            for (short i = 0; i < weaponTypeHardpointLists[activeTypeIndex].Count; i++)
            {
                Debug.Log("========= FIRING WEAPON IN GROUP: " + weaponTypeHardpointLists[activeTypeIndex][index].loadedWeaponObj.name);


                // set delay based on number of times i has stepped forward * launchDelayPerSequencePos

                weaponTypeHardpointLists[activeTypeIndex][index].effectiveLaunchDelayMax =
                    launchDelayPerSequencePos * i;

                launchHardpoint(weaponTypeHardpointLists[activeTypeIndex][index]);


                if(++index > (weaponTypeHardpointLists[activeTypeIndex].Count - 1)) // if past end of array
                {
                    index = 0;
                }
            }

        }
        else // fire one hardpoint at a time
        {
            Debug.Log("Single LaunchProcess for hardpoint " + activeHardpointIndexes[activeTypeIndex]);

            nextAvailableHardpointIndex(activeTypeIndex);
            Hardpoint currentActiveHardpoint = weaponTypeHardpointLists[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]];
            launchHardpoint(currentActiveHardpoint);
            
        }
        
    }


    // 0 default
    // of the hardpoints readyToFire, select one with most roundsRemaining
    short selectGroupStartIndex()
    {
        short mostRoundsRemainIndex = 0;
        short mostRoundsRemain = 0;

        for(short i = 0; i < weaponTypeHardpointLists[activeTypeIndex].Count; i++)
        {
            Hardpoint currentHardpoint = weaponTypeHardpointLists[activeTypeIndex][i];
            if (currentHardpoint.readyToFire)
            {
                Weapon loadedWeap = currentHardpoint.loadedWeaponObj.GetComponent<Weapon>();
                if (loadedWeap != null)
                {
                    if(loadedWeap.roundsRemain > mostRoundsRemain)
                    {
                        mostRoundsRemain = loadedWeap.roundsRemain;
                        mostRoundsRemainIndex = i;
                    }
                }
            }

        }


        return mostRoundsRemainIndex;
    }

    // try to launch with lock
    void launchHardpoint(Hardpoint hardpoint)
    {
        if (tgtComputer.currentTarget == null || !tgtComputer.radarLocked) // if no target is locked
        {
            hardpoint.launchStart();
        }
        else // if target is locked
        {
            hardpoint.launchWithLock(tgtComputer.currentTarget.gameObject);
        }
    }

    short nextAvailableHardpointIndex(short typeIndex)
    {
        float smallestReloadTime = 0;

        Debug.Log("NextavailableHardpointIndex called");

        // check if current is null -- no need to search if this index is already good
        if (!weaponTypeHardpointLists[typeIndex][activeHardpointIndexes[typeIndex]].readyToFire)
        {

            activeHardpointIndexes[typeIndex]++; // increment index for this type

            // if we need to search for a different hardpoint
            if (activeHardpointIndexes[typeIndex] > weaponTypeHardpointLists[typeIndex].Count - 1 ||        // check if index is past range for this type
                !weaponTypeHardpointLists[typeIndex][activeHardpointIndexes[typeIndex]].readyToFire)        // check if this next index is good as well
            {
                // loop through all in weaponTypeList, select first available
                // if that fails, select the one with least reload time remaining

                bool availableFound = false;
                float smallestTimeRemainVal = -1f; // -1 to signify first value hasn't been selected
                short smallestTimeRemainIndex = 0; // default to first index

                // loop through all the hardpoints for this type
                for (short i = 0; i < weaponTypeHardpointLists[typeIndex].Count && !availableFound; i++)
                {

                    Hardpoint currentHardpoint = weaponTypeHardpointLists[typeIndex][i];
                    //Debug.Log("Reloadtime at " + i + ": " + currentHardpoint.currentTimer);

                    // check if current hardpoint is ready to fire -- (controller depends on hardpoints to accurately report this)
                    if (currentHardpoint.readyToFire)
                    {
                        availableFound = true;
                        activeHardpointIndexes[typeIndex] = i;
                        Debug.Log("Available found at index: " + activeHardpointIndexes[typeIndex]);
                    }

                    // save smallest timer value and index
                    if (currentHardpoint.currentReloadTimer < smallestTimeRemainVal || smallestTimeRemainVal < 0)
                    {
                        Debug.Log("New small time found: " + currentHardpoint.currentReloadTimer);
                        smallestTimeRemainVal = currentHardpoint.currentReloadTimer;
                        smallestReloadTime = smallestTimeRemainVal;
                        smallestTimeRemainIndex = i;
                    }

                }


                // looped through all, couldn't find available. Select least reload time remaining
                if (!availableFound)
                {
                    activeHardpointIndexes[typeIndex] = smallestTimeRemainIndex;
                    Debug.Log("None available found. Defaulting to least time remaining at: " + smallestTimeRemainIndex);
                }
            }
        }
        //Debug.Log("============== RESULT: hardpoint " + activeHardpointIndexes[typeIndex] + " selected, with reload time remaining: ");

        return activeHardpointIndexes[typeIndex];
    }


    public void setWeaponType(short index)
    {
        // check that index is within the range of weapon types
        if(index >= 0 && index < weaponTypeHardpointLists.Count)
        {
            launchEndProcess();         // end currently selected weapon
            activeTypeIndex = index;    // change type

            // indicator shows change -- this is only UI update
            weaponIndicatorManager.showActiveWeaponType(activeTypeIndex);
        }
    }


    void advanceWeaponType(short advance)
    {
        if (advance != 0)
        {
            // end previous launch when any change made
            launchEndProcess();

            // advance
            activeTypeIndex += advance;

            // looping
            if (activeTypeIndex > weaponTypeHardpointLists.Count - 1)
                activeTypeIndex = 0;
            else if (activeTypeIndex < 0)
                activeTypeIndex = (short)(weaponTypeHardpointLists.Count - 1);


            // indicator shows change -- this is only UI update
            weaponIndicatorManager.showActiveWeaponType(activeTypeIndex);
        }
    }
}
