using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoadoutStorage : MonoBehaviour
{
    public HardpointController hardpointController;

    
    public LoadoutPreset activeLoadout;

    // why the fuck isn't this a list..?
    // Maybe not an issue since it's set at inspector, not runtime?
    public LoadoutPreset[] standardLoadouts; 

    public LoadoutPreset[] customLoadouts;

    // One list per team -- which list is selected by team, then indexed to loadout within team
    public LoadoutPreset[][] allCustomLoadouts;


    public int defaultCustomListSize = 3;

    // this makes loadouts editable from inspector
    // ....but also makes programmatic changes to loadout prefab go to disk
    // ....that should be okay for the build, but editor version will need to refresh each run
    [Serializable]
    public struct LoadoutPreset
    {
        public string name;
        public List<Weapon> loadout;

        // a stock list for each loadout preset. UI sets stock levels. Then, upon spawn, 
        // these stock list values are copied onto the instantiated aircraft
        // Three main layers, INSTANTIATION, MODIFICATION and APPLICATION;
        //  - instantiation: reads weapons in loadout, sets initial list size for stock
        //    -> Instantiation should remember previous weapon values for custom weapon changes
        //    -> Stock values per weapon should ONLY auto-set when default-generate is selected
        //    -> Otherwise, previous values carry over, even if overweight. User will modify weight accordingly
        //      >>>> weapon loadout changes can change corresponding weapon indexes
        //      >>>> therefore, loadout must remember weapon types per index
        //  - Modification:  auto generation or UI modifies stock values for each weapon type
        //    -> UI object: Text label, and text input box per weapon stock type
        //    -> Generate a set of these, at designated origin, with designated spacing
        //      --> generation occurs whenever a weapon loadout change occurs. Previous values may carryover
        //    -> UI Object: Total weight tally. Whenever stock value changes, total stock weight tallied
        //      --> checks maximum allowable weight, reads from prefab hardpointcontroller
        //    -> Only allows spawn if weight is less than max allowed
        //      --> only blocks spawn button?
        //  - Application:  stored stock values of this loadout are copied onto instantiated aircraft

        public List<int> stock; // each index = unique weapon type. Value = how many weapons of that type to store in onboard stock
        public bool generateDefaultStock;
        public List<Weapon> weaponTypes; // used to identify weapon type for stock values
    }

    //LoadoutPreset testLoadoutPleaseIgnore;

    // Start is called before the first frame update
    void Start()
    {
        //GameObject.Destroy(this); // no reason to store loadouts on instantiated aircraft
        destroyExtraLoadouts();
    }

    void destroyExtraLoadouts()
    {
        standardLoadouts = null;
        customLoadouts = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // count how many weapon types there are
    // call this when weapon list is changed then validated
    // the stock list needs to be reinstantiated every time the weapon list changes
    //  --> ONLY change when internal weapon list changes -- not on dropdown changes such as changing aircraft or slot
    // then, externally a different function will be used to modify the actual stock values
    // ....how to make the fucking AI take default loadout without disturbing stored loadouts
    //   --> AI spawn will instantiate and generate a new loadoutpreset that doesn't get stored anywhere,
    //      ...except onto newly instantiated AI aircraft
    public void instantiateStockList(ref LoadoutPreset loadout, bool carryoverPrevStock = false)
    {

        if(loadout.stock == null)
        {
            loadout.stock = new List<int>();
        }
        if(loadout.weaponTypes == null)
        {
            loadout.weaponTypes = new List<Weapon>();
        }

        //loadout.stock.Clear();

        // new set of stock lists, to compare old against new and carryover values
        List<Weapon> newWeapTypes = new List<Weapon>();
        List<int> newStock = new List<int>();

        // loop through loadout weapons to build weapon type list
        for (int i = 0; i < loadout.loadout.Count; i++)
        {
            Weapon weap = loadout.loadout[i];

            // if new type found
            if (!newWeapTypes.Contains(weap))
            {
                newWeapTypes.Add(weap); // keep track of this type
                newStock.Add(0); // add blank index to stock. This index will correspond to above weap type

            }
        }

        // only regenerate fresh stock values if auto-generate stock checked
        if (carryoverPrevStock && !loadout.generateDefaultStock)
        {

            carryOverPrevStock(ref loadout, newWeapTypes, newStock);

        }
        else // auto generate stock levels
        {
            // overwrite old lists since we don't care about carryover here
            loadout.stock = newStock;
            loadout.weaponTypes = newWeapTypes;
            generateDefaultStock(ref loadout);
        }
        

        
        
        
    }

    public void carryOverPrevStock(ref LoadoutPreset loadoutRef, List<Weapon> newWeapTypes, List<int> newStock)
    {
        // loop through all pre-existing weapon types
        for (int i = 0; i < loadoutRef.weaponTypes.Count; i++)
        {
            Weapon oldWeapType = loadoutRef.weaponTypes[i];
            //Debug.LogError("Grabbing oldType " + i);

            // loop through all newly found weapon types
            for (int j = 0; j < newWeapTypes.Count; j++)
            {
                //Debug.LogError("Grabbing new type: " + j);
                Weapon newWeapType = newWeapTypes[j];

                if (oldWeapType == newWeapType)
                {
                    // same weapon type found, save old stock onto new
                    newStock[j] = loadoutRef.stock[i];
                }
            }
        }

        loadoutRef.stock = newStock;
        loadoutRef.weaponTypes = newWeapTypes;
    }

    // ensure stock and weapontype lengths are properly initialized before calling
    public void generateDefaultStock(ref LoadoutPreset loadoutRef)
    {
        bool weaponAdded = false;
        int maxWeight = getController().maxWeight;
        int newWeight = 0;

        zeroStock(ref loadoutRef);

        // continue looping until no weapons are added
        do
        {
            weaponAdded = false;

            // loop through each weapon in loadout
            for (int i = 0; i < loadoutRef.loadout.Count; i++)
            {
                Weapon weap = loadoutRef.loadout[i];

                int testWeight = newWeight + weap.stockWeight;

                if (testWeight <= maxWeight)
                {
                    // add weapon to stock
                    int weapIndex = loadoutRef.weaponTypes.IndexOf(weap);
                    loadoutRef.stock[weapIndex]++;
                    newWeight = testWeight;
                    weaponAdded = true;
                }

            }


        } while (weaponAdded);

        loadoutRef.generateDefaultStock = false; // subsequent stock calls won't regenerate defaults
    }

    private void zeroStock(ref LoadoutPreset loadout)
    {

        for(int i = 0; i < loadout.stock.Count; i++)
        {
            loadout.stock[i] = 0;
        }
    }

    public bool checkWeight(ref LoadoutPreset loadout)
    {
        //int weightTally = 0;

        //for(int i = 0; i < loadout.weaponTypes.Count; i++)
        //{
        //    Weapon weap = loadout.weaponTypes[i];
        //    int stockOfWeap = loadout.stock[i];

        //    weightTally += weap.stockWeight * stockOfWeap;
        //}

        return weightTally(ref loadout) <= hardpointController.maxWeight;
    }

    public static int weightTally(ref LoadoutPreset loadout)
    {
        int weightTally = 0;

        for (int i = 0; i < loadout.weaponTypes.Count; i++)
        {
            Weapon weap = loadout.weaponTypes[i];
            int stockOfWeap = loadout.stock[i];

            weightTally += weap.stockWeight * stockOfWeap;
        }
        return weightTally;
    }

    public int maxIndividualStock(Weapon weap)
    {
        // integer division will cut off decimal
        // so result will be maximum allowable individual stock
        return getController().maxWeight / weap.stockWeight;
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

    public ref LoadoutPreset getLoadoutRef(int loadoutDropdownIndex, CombatFlow.Team team = CombatFlow.Team.NEUTRAL)
    {
        Debug.Log("LoadoutStorage.getLoadoutRef()");
        // establishing scope of loadoutRef, though requires instantiation to arbitrary value
        ref LoadoutPreset loadoutRef = ref standardLoadouts[0]; 

        // loadout 0 index from selector is always a default

        // If we attempt to index a non-default loadout
        if(loadoutDropdownIndex >= getCustomBeginIndex())
        {
            checkCustomStorageInitialization();
            // we must be trying to read a CUSTOM loadout
            loadoutRef = ref allCustomLoadouts[(int)team][customConvertIndex(loadoutDropdownIndex, team)];
        }
        else // if we are accessing a standard loadout
        {
            loadoutRef = ref standardLoadouts[loadoutDropdownIndex];
        }

        // validate and, if need be, construct loadout as default
        validateLoadoutConstruction(ref loadoutRef);

        return ref loadoutRef;
    }

    private int customConvertIndex(int dropdownIndex, CombatFlow.Team team)
    {
        int customIndex = dropdownIndex - getCustomBeginIndex();

        return validateCustomIndex(customIndex, team);
    }

    public int getCustomListSize(CombatFlow.Team team)
    {
        return (allCustomLoadouts[(int)team]).Length;
    }

    public int validateCustomIndex(int customIndex, CombatFlow.Team team)
    {
        return Mathf.Clamp(customIndex, 0, getCustomListSize(team) - 1);
    }


    public ref LoadoutPreset getCustomLoadout(CombatFlow.Team team, int customIndex = 0)
    {
        Debug.Log("LoadoutStorage.getCustomLoadout()");

        

        checkCustomStorageInitialization();


        return ref getLoadoutRef(getCustomBeginIndex() + customIndex, team);
    }

    private void checkCustomStorageInitialization()
    {
        if (allCustomLoadouts == null || allCustomLoadouts.Length == 0)
        {
            allCustomLoadouts = new LoadoutPreset[2][]; // 2 for number of teams

            initializeCustomArray(ref allCustomLoadouts[0]);
            initializeCustomArray(ref allCustomLoadouts[1]);

        }
    }

    private void initializeCustomArray(ref LoadoutPreset[] loadouts)
    {
        loadouts = new LoadoutPreset[defaultCustomListSize];

        for (int i = 0; i < defaultCustomListSize; i++)
        {
            ref LoadoutPreset loadoutRef = ref loadouts[i];
            loadoutRef.name = "Custom " + (1 + i);

            //validateLoadoutConstruction(ref loadoutRef);



        }
    }


    public void tryInitializeLoadout(ref LoadoutPreset loadoutRef)
    {

        bool doConstruct = false;

        if (loadoutRef.loadout == null)
        {
            loadoutRef.loadout = new List<Weapon>();

            //for (int i = 0; i < getController().getHardpoints().Length; i++)
            //{
            //    loadoutRef.loadout.Add(null);
            //}

            //doConstruct = true;

        }

        if (loadoutRef.weaponTypes == null)
        {
            loadoutRef.weaponTypes = new List<Weapon>();
            doConstruct = true;
        }

        if (loadoutRef.stock == null)
        {
            loadoutRef.stock = new List<int>();
            doConstruct = true;
        }

        //if (doConstruct)
        //{
        //    constructLoadoutAsDefault(ref loadoutRef);
        //}

    }

    public void validateLoadoutConstruction(ref LoadoutPreset loadoutRef)
    {
        Debug.Log("LoadoutStorage.validateLoadoutConstruction()");
        Hardpoint[] hardpoints = getController().getHardpoints(); // throws array out of bounds exception somewhere after this

        Debug.Log("Test 1");


        if(getController().getRootFlow().team == CombatFlow.Team.TEAM1 &&
            string.Equals(loadoutRef.name, "Anti Ship"))
        {
            Debug.Log("Test case found");
        }

        tryInitializeLoadout(ref loadoutRef);

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
        //for(int i = 0; i < standardLoadouts.Length; i++)
        //{
        //    standardLoadouts[i].loadout.Clear();
        //}

        for(int i = 0; i < customLoadouts.Length; i++)
        {
            customLoadouts[i].loadout.Clear();
        }
    }

    // Only called once per aircraft prefab to initialize the default loadout based on
    // prefab hardpoint weapon settings
    private void constructLoadoutAsDefault(ref LoadoutPreset loadoutRef)
    {
        Debug.Log("LoadoutStorage.constructLoadoutAsDefault()");

        // Fill the default loadout with hardpoint weapons set from aircraft prefab
        Hardpoint[] hardpoints = getController().getHardpoints();

        //if (loadoutRef.name.Equals("Custom1"))
        //{
        //    Debug.LogError("Custom1 being constructed");
        //}

        for(int i = 0; i < hardpoints.Length; i++)
        {
            loadoutRef.loadout[i] = hardpoints[i].weaponTypePrefab.GetComponent<Weapon>();
        }

        // instantiate loadout stock. Will generate default values
        instantiateStockList(ref loadoutRef);

    }

    // index of custom loadout as seen by preset dropdown selector
    public int getCustomBeginIndex()
    {
        Debug.Log("LoadoutStorage.getCustomIndex()");
        return standardLoadouts.Length;
        //return 1; // structure changed so that 0 index is default, custom is 1
    }

    // this is also used to index selected loadouts from UI
    //  the returned string[] array corresponds to selectable loadouts per aircraft
    public string[] reportNamesArrayByTeam(CombatFlow.Team team)
    {
        Debug.Log("LoadoutStorage.reportNamesArrayByTeam()");

        //int stringCount = standardLoadouts.Length + 1; // 1 custom loadout will be included
        //string[] reportArray = new string[stringCount];

        //for (int i = 0; i < standardLoadouts.Length; i++)
        //{
        //    reportArray[i] = standardLoadouts[i].name;
        //}
        //reportArray[standardLoadouts.Length] = customLoadouts[(int)team].name;

        checkCustomStorageInitialization();

        int standardLength = standardLoadouts.Length;
        int customLength = allCustomLoadouts[(int)team].Length;

        string[] reportArray = new string[standardLength + customLength]; // one default, one custom loadout

        for(int i = 0; i < standardLoadouts.Length; i++)
        {
            reportArray[i] = standardLoadouts[i].name;
        }

        for(int i = 0; i < customLength; i++)
        {
            reportArray[standardLength + i] = allCustomLoadouts[(int)team][i].name;
        }

        //reportArray[0] = standardLoadouts[(int)team].name;
        //reportArray[1] = customLoadouts[(int)team].name;

        return reportArray;

    }

    public ref LoadoutPreset getDefaultLoadout(CombatFlow.Team team)
    {
        return ref standardLoadouts[0];
    }

    public LoadoutPreset getDefaultLoadoutCopy(CombatFlow.Team team)
    {
        validateLoadoutConstruction(ref standardLoadouts[0]);
        return standardLoadouts[0];
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

        for(int i = 0; i < allCustomLoadouts.Length; i++)
        {
            report += "\nNew Custom Loadouts Team" + i + ": \n";
            for(int j = 0; j < allCustomLoadouts[i].Length; j++)
            {
                report += reportLoadout(ref allCustomLoadouts[i][j]);
            }
        }

        return report;
    }
    

    public string reportLoadout(ref LoadoutPreset loadout)
    {
        string report = "";

        
        
        report += "Loadout name: " + loadout.name + "\n";

        tryInitializeLoadout(ref loadout);

        for (int j = 0; j < loadout.loadout.Count; j++)
        {
            Weapon weap = loadout.loadout[j];
            string name = "null";
            if(weap != null)
            {
                name = weap.gameObject.name;
            }
            report += "Weapon " + j + ": " + name + "\n";
        }

        report += "\nStock Levels: \n  ";

        for(int i = 0; i < loadout.stock.Count; i++)
        {
            report += loadout.weaponTypes[i].name + ": " + loadout.stock[i] + "\n";
        }
        

        return report;
    }
    
}
