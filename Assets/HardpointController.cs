using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardpointController : MonoBehaviour
{


    public Hardpoint[] hardpoints;

    public List<List<Hardpoint>> weaponTypeHardpointLists; // two dimensional list -- hardpoint array for each weapon type
    short[] activeHardpointIndexes;   // use weaponTypeArrays[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]]

    public short activeTypeIndex;

    

    public TgtComputer tgtComputer;


    public bool launchButtonDown;
    public bool launchButtonUp;
    public bool changeButtonDown;

    // Commands missiles to launch

    private void Awake()
    {
        weaponTypeHardpointLists = new List<List<Hardpoint>>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        fillHardpointArray();
    }

    void fillHardpointArray()
    {
        // raw array of hardpoints themselves
        hardpoints = new Hardpoint[transform.childCount];

        // loop through each hardpoint -- sort them into lists
        for(int i = 0; i < hardpoints.Length; i++)
        {
            
            hardpoints[i] = transform.GetChild(i).gameObject.GetComponent<Hardpoint>();

            // if type cannot be found, start a new first level list
            // if type is found, add to existing list

            short typeIndex = findTypeIndex(hardpoints[i].weaponTypePrefab);
            if(typeIndex < 0)
            {
                weaponTypeHardpointLists.Add(new List<Hardpoint>()); // add new list
                typeIndex = (short)(weaponTypeHardpointLists.Count - 1); // should never be < 0, with list being added this block
            }
            weaponTypeHardpointLists[typeIndex].Add(hardpoints[i]); // add item to existing list

        }

        // all types are now known, have short designating active for each
        activeHardpointIndexes = new short[weaponTypeHardpointLists.Count];
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




    // Update is called once per frame
    void Update()
    {
        if (launchButtonDown)
        {
            launchProcess();
        }

        if (launchButtonUp)
        {
            weaponTypeHardpointLists[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]].launchEnd();
        }

        if (changeButtonDown)
        {
            changeWeaponType();
        }
    }

    void launchProcess()
    {
        nextAvailableHardpointIndex(activeTypeIndex);
        Hardpoint currentActiveHardpoint = weaponTypeHardpointLists[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]];

        if (tgtComputer.currentTarget == null) // if no target is locked
        {
            currentActiveHardpoint.launch();
        }
        else // if target is locked
        {
            currentActiveHardpoint.launchWithLock(tgtComputer.currentTarget.gameObject);
        }
    }

    short nextAvailableHardpointIndex(short typeIndex)
    {
        float smallestReloadTime = 0;

        // check if current is null -- no need to search if this index is already good
        if (weaponTypeHardpointLists[typeIndex][activeHardpointIndexes[typeIndex]].loadedWeaponObj == null)
        {

            activeHardpointIndexes[typeIndex]++; // increment index for this type


            if (activeHardpointIndexes[typeIndex] > weaponTypeHardpointLists[typeIndex].Count - 1 ||         // check if index is past range for this type
                weaponTypeHardpointLists[typeIndex][activeHardpointIndexes[typeIndex]].loadedWeaponObj == null) // check if this next index is good as well
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

                    // check if current hardpoint has loaded weapon
                    if (currentHardpoint.loadedWeaponObj != null)
                    {
                        availableFound = true;
                        activeHardpointIndexes[typeIndex] = i;
                        //Debug.Log("Available found at index: " + activeHardpointIndexes[typeIndex]);
                    }

                    // save smallest timer value and index
                    if (currentHardpoint.currentTimer < smallestTimeRemainVal || smallestTimeRemainVal < 0)
                    {
                        //Debug.Log("New small time found: " + currentHardpoint.currentTimer);
                        smallestTimeRemainVal = currentHardpoint.currentTimer;
                        smallestReloadTime = smallestTimeRemainVal;
                        smallestTimeRemainIndex = i;
                    }

                }


                // looped through all, couldn't find available. Select least reload time remaining
                if (!availableFound)
                {
                    activeHardpointIndexes[typeIndex] = smallestTimeRemainIndex;
                    //Debug.Log("None available found. Defaulting to least time remaining at: " + smallestTimeRemainIndex);
                }
            }
        }
        //Debug.Log("============== RESULT: hardpoint " + activeHardpointIndexes[typeIndex] + " selected, with reload time remaining: ");

        return activeHardpointIndexes[typeIndex];
    }


    void changeWeaponType()
    {
        activeTypeIndex++;
        if (activeTypeIndex > weaponTypeHardpointLists.Count - 1)
            activeTypeIndex = 0;
    }
}
