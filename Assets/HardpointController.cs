using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardpointController : MonoBehaviour
{


    public Hardpoint[] hardpoints;

    public List<List<Hardpoint>> weaponTypeLists; // two dimensional list -- hardpoint array for each weapon type
    short[] activeHardpointIndexes;   // use weaponTypeArrays[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]]

    public short activeTypeIndex;

    

    public TgtComputer tgtComputer;


    public bool launchButtonDown;
    public bool changeButtonDown;

    // Commands missiles to launch

    private void Awake()
    {
        weaponTypeLists = new List<List<Hardpoint>>();
        
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
                weaponTypeLists.Add(new List<Hardpoint>()); // add new list
                typeIndex = (short)(weaponTypeLists.Count - 1); // should never be < 0, with list being added this block
            }
            weaponTypeLists[typeIndex].Add(hardpoints[i]); // add item to existing list

        }

        // all types are now known, have short designating active for each
        activeHardpointIndexes = new short[weaponTypeLists.Count];
    }

    // search through each list to find existing weapon prefab type
    //  compare argument to first item in each list
    short findTypeIndex(GameObject prefabLink)
    {
        short typeIndex = -1; // -1 if cannot be found

        for(short i = 0; i < weaponTypeLists.Count && typeIndex == -1; i++)
        {
            if(prefabLink == weaponTypeLists[i][0].weaponTypePrefab)
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

        if (changeButtonDown)
        {
            changeWeaponType();
        }
    }

    void launchProcess()
    {
        nextAvailableHardpointIndex(activeTypeIndex);
        Hardpoint currentActiveHardpoint = weaponTypeLists[activeTypeIndex][activeHardpointIndexes[activeTypeIndex]];

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
        activeHardpointIndexes[typeIndex]++; // increment index for this type

        if(activeHardpointIndexes[typeIndex] > weaponTypeLists[typeIndex].Count - 1)
        {
            // loop through all in weaponTypeList, select first available
            // if that fails, select the one with least reload time remaining

            bool availableFound = false;
            float smallestTimeRemainVal = -1f; 
            short smallestTimeRemainIndex = -1; // -1 to signify first value hasn't been selected

            for (short i = 0; i < weaponTypeLists[typeIndex].Count - 1 && !availableFound; i++)
            {

                Hardpoint currentHardpoint = weaponTypeLists[typeIndex][i];
                
                // check if current hardpoint has loaded weapon
                if(currentHardpoint.loadedWeaponObj != null)
                {
                    availableFound = true;
                    activeHardpointIndexes[typeIndex] = i;
                }

                // save smallest timer value and index
                if(currentHardpoint.currentTimer < smallestTimeRemainVal || smallestTimeRemainIndex < 0)
                {
                    smallestTimeRemainVal = currentHardpoint.currentTimer;
                    smallestTimeRemainIndex = i;
                }

            }


            // looped through all, couldn't find available. Select least reload time remaining
            if (!availableFound)
            {
                activeHardpointIndexes[typeIndex] = smallestTimeRemainIndex;
            }
        }


        return activeHardpointIndexes[typeIndex];
    }


    void changeWeaponType()
    {
        activeTypeIndex++;
        if (activeTypeIndex > weaponTypeLists.Count - 1)
            activeTypeIndex = 0;
    }
}
