using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoadoutStorage : MonoBehaviour
{
    public HardpointController hardpointController;


    public LoadoutPreset[] standardLoadouts;
    public LoadoutPreset[] customLoadouts;

    // this makes loadouts editable from inspector
    // ....but also makes programmatic changes to loadout prefab go to disk
    // ....that should be okay for the build, but editor version will need to refresh each run
    [Serializable]
    public struct LoadoutPreset
    {
        public string name;
        public List<Weapon> loadout;
    }

    //LoadoutPreset testLoadoutPleaseIgnore;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Destroy(this); // no reason to store loadouts on instantiated aircraft
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    


    public HardpointController getController()
    {
        Debug.Log("LoadoutStorage.getController()");
        if(hardpointController == null)
        {
            hardpointController = GetComponent<HardpointController>();
        }
        return hardpointController;
    }

    public ref LoadoutPreset getLoadoutRef(int loadoutIndex, CombatFlow.Team team = CombatFlow.Team.NEUTRAL)
    {
        Debug.Log("LoadoutStorage.getLoadoutRef()");
        // establishing scope of loadoutRef, though requires instantiation to arbitrary value
        ref LoadoutPreset loadoutRef = ref standardLoadouts[0]; 

        // If we attempt to index a loadout past those in standard loadout list...
        if(loadoutIndex >= standardLoadouts.Length)
        {
            // we must be trying to read a CUSTOM loadout
            loadoutRef = ref customLoadouts[(int)team];
        }
        else // if we are accessing a standard loadout
        {
            loadoutRef = ref standardLoadouts[loadoutIndex];
        }

        // validate and, if need be, construct loadout as default
        validateLoadoutConstruction(ref loadoutRef);

        return ref loadoutRef;
    }

    public ref LoadoutPreset getCustomLoadout(CombatFlow.Team team)
    {
        Debug.Log("LoadoutStorage.getCustomLoadout()");
        return ref getLoadoutRef(standardLoadouts.Length, team);
    }

    public void validateLoadoutConstruction(ref LoadoutPreset loadoutRef)
    {
        Debug.Log("LoadoutStorage.validateLoadoutConstruction()");
        Hardpoint[] hardpoints = getController().getHardpoints(); // throws array out of bounds exception somewhere after this

        Debug.Log("Test 1");

        if (loadoutRef.loadout.Count != hardpoints.Length)
        {
            Debug.Log("Test 2");
            loadoutRef.loadout.Clear();

            // build list to correct size
            for(int i = 0; i < hardpoints.Length; i++)
            {
                loadoutRef.loadout.Add(null);
            }
            
        }

        Debug.Log("Test 3");

        // if any values of loadout are null, reconstruct loadout as default
        if (loadoutRef.loadout.Contains(null))
        {
            Debug.Log("Test 4");
            constructLoadoutAsDefault(ref loadoutRef);
        }

        Debug.Log("Test 5");
    }

    // clear any values that got saved to disk
    public void refreshIfNotFresh()
    {
        //Debug.LogError("Attempting loadout refresh");

        // weapon loader statically tracks which loadoutstorages have been refreshed
        //  - if this loadoutstorage is not on the list, refresh it
        if (!WeaponLoader.getRefreshedStorages().Contains(this))
        {
            WeaponLoader.getRefreshedStorages().Add(this);
            clearAllPresetLoadouts();
        }
    }

    public void clearAllPresetLoadouts()
    {
        //Debug.LogError("Refreshing loadouts");
        for(int i = 0; i < standardLoadouts.Length; i++)
        {
            standardLoadouts[i].loadout.Clear();
        }

        for(int i = 0; i < customLoadouts.Length; i++)
        {
            customLoadouts[i].loadout.Clear();
        }
    }


    private void constructLoadoutAsDefault(ref LoadoutPreset loadoutRef)
    {
        Debug.Log("LoadoutStorage.constructLoadoutAsDefault()");
        Hardpoint[] hardpoints = getController().getHardpoints();

        //if (loadoutRef.name.Equals("Custom1"))
        //{
        //    Debug.LogError("Custom1 being constructed");
        //}

        for(int i = 0; i < hardpoints.Length; i++)
        {
            loadoutRef.loadout[i] = hardpoints[i].weaponTypePrefab.GetComponent<Weapon>();
        }
    }

    public int getCustomIndex()
    {
        Debug.Log("LoadoutStorage.getCustomIndex()");
        return standardLoadouts.Length;
    }

    public string[] reportNamesArrayByTeam(CombatFlow.Team team)
    {
        Debug.Log("LoadoutStorage.reportNamesArrayByTeam()");
        int stringCount = standardLoadouts.Length + 1; // 1 custom loadout will be included
        string[] reportArray = new string[stringCount];

        for (int i = 0; i < standardLoadouts.Length; i++)
        {
            reportArray[i] = standardLoadouts[i].name;
        }
        reportArray[standardLoadouts.Length] = customLoadouts[(int)team].name;

        return reportArray;

    }


    public string reportAllLoadouts()
    {
        string report = "Loadout storage report***************************\n";
        report += " Standard loadouts:\n";

        for(int i = 0; i < standardLoadouts.Length; i++)
        {
            report += reportLoadout(ref standardLoadouts[i]);
        }

        report += " \n\n Custom loadouts: \n";

        for(int i = 0; i < customLoadouts.Length; i++)
        {
            report += reportLoadout(ref customLoadouts[i]);
        }

        return report;
    }
    

    public string reportLoadout(ref LoadoutPreset loadout)
    {
        string report = "";

        
        
        report += "Loadout name: " + loadout.name + "\n";

        for (int j = 0; j < loadout.loadout.Count; j++)
        {
            report += "Weapon " + j + ": " + loadout.loadout[j].gameObject.name + "\n";
        }
        

        return report;
    }
    
}
