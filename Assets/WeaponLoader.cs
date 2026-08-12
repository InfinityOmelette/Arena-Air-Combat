using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WeaponLoader : MonoBehaviour
{

    public GameObject activeAircraftPrefab;
    public HardpointController selectedAircraftPrefabHardpointController;

    public Hardpoint[] prefabHardpoints;

    // outer list indexes hardpoints, inner list indexes valid weapons for corresponding hardpoint
    public List<List<Weapon>> validWeaponsMasterList;

    //public List<Weapon> weaponsToEquip;

    public GameObject weaponDropdownOrigin;

    public TechInventory myTeamTechInventory;

    public Dropdown weaponDropdownPrefab;

    public float dropdownOffset = 30f;


    public Dropdown loadoutPresetDropdown;

    public GameObject stockSliderOrigin;

    public StockSliderReferences stockSliderPrefab;

    public float stockSliderOffset = 30f;

    public Text weightLabel;

    public bool acceptableWeight = false;

    // loadoutpreset dropdown options must get built when aircraft selected
    //  - available weapons refreshed
    //  - loadout preset dropdown built -- defaults to 0 (may not need to trigger update if all hardpoint dropdowns default to 0 anyways?)

    // when loadout preset selected
    //  - 

    int ignoreModifyCallbacks = 0;

    private static List<LoadoutStorage> refreshedStorages;

   // private bool ignoreReselection = false;

    private void Awake()
    {
        validWeaponsMasterList = new List<List<Weapon>>();
        myTeamTechInventory = GetComponent<TechInventory>();
    }

    // because loadoutstorage in prefab data persists on disk
    // ...we must refresh the storages whenever accessed for the first time
    public static List<LoadoutStorage> getRefreshedStorages()
    {
        if(refreshedStorages == null)
        {
            refreshedStorages = new List<LoadoutStorage>();
        }
        return refreshedStorages;
    }

    // Start is called before the first frame update
    void Start()
    {

        refreshAvailableWeapons(GameManager.getGM().selectedPlayerPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log(getStorage().reportAllLoadouts());
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log(reportPrefabHardpointWeapons());
        }



        if (Input.GetKeyDown(KeyCode.F7))
        {
            modifyLoadoutFromDropdowns();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            debugRefreshDropdowns();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            //buildWeaponSelectorDropdowns();
            readLoadoutOntoDropdowns();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.LogError(getStorage().reportLoadout(ref getCurrentLoadoutRef()));
        }
    }

    

    public void debugRefreshDropdowns()
    {
        Debug.Log("Debug refreshing weapon dropdowns");
        for(int i = 0; i < prefabHardpoints.Length; i++)
        {
            GameObject dropdown = weaponDropdownOrigin.transform.GetChild(i).gameObject;
            dropdown.GetComponent<Dropdown>().RefreshShownValue();
        }
    }

    public string reportPrefabHardpointWeapons()
    {
        string report = "";
        report += "*************** Prefab weapons: \n";

        for(int i = 0; i < prefabHardpoints.Length; i++)
        {
            report += "Weapon " + i + ": " + prefabHardpoints[i].weaponTypePrefab.name + "\n";
        }
        return report;
    }

    public void refreshAvailableWeapons()
    {
        refreshAvailableWeapons(GameManager.getGM().selectedPlayerPrefab);
    }

    // trigger this when selected aircraft for this team changes
    public void refreshAvailableWeapons(GameObject aircraftPrefab)
    {
        Debug.Log("Refreshing available weapons for " + aircraftPrefab.gameObject.name);

        activeAircraftPrefab = aircraftPrefab;
        selectedAircraftPrefabHardpointController = aircraftPrefab.GetComponent<TgtComputer>().getHardpointController();

        selectedAircraftPrefabHardpointController.getStorage().refreshIfNotFresh();

        validWeaponsMasterList = new List<List<Weapon>>(); // I assume garbage collector will handle clearing data from old list?

        prefabHardpoints = selectedAircraftPrefabHardpointController.getHardpoints();

        // go through each hardpoint and build each one a list of valid weapons
        for(int i = 0; i < prefabHardpoints.Length; i++)
        {
            // build available weapons list for this hardpoint
            validWeaponsMasterList.Add(new List<Weapon>());

            // loop through all weapons in team inventory list, mark which ones are valid
            for(int j = 0; j < myTeamTechInventory.teamWeaponInventory.Count; j++)
            {
                Weapon newWeaponPrefab = myTeamTechInventory.teamWeaponInventory[j];

                if (prefabHardpoints[i].validateWeapon(newWeaponPrefab))
                {
                    validWeaponsMasterList[i].Add(newWeaponPrefab);
                }
            }
        }

        // at this point, weapon availability lists should be valid. Now we just send it to UI dropdowns
        Debug.Log(reportAvailableWeaponsList());

        // update UI dropdowns with available weapon data
        buildWeaponSelectorDropdowns();

        refreshLoadoutPresetDropdown();

        

        

    }

    public void deleteAllChildren(GameObject obj)
    {
        Debug.Log("Beginning child deletion");
        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            Debug.Log(" Deleting child ");
            GameObject.Destroy(obj.transform.GetChild(i).gameObject);
        }

    }

    // updates hardpoint dropdowns according to currently selected aircraft
    // defaulting to preset 0 -- Default loadout
    public void buildWeaponSelectorDropdowns()
    {
        Debug.Log("Build weapon selector dropdowns");

        //// Destroy all current ui weapon select dropdowns
        //for (int i = 0; i < weaponDropdownOrigin.transform.childCount; i++)
        //{
        //    GameObject dropdown = weaponDropdownOrigin.transform.GetChild(i).gameObject;
        //    GameObject.Destroy(dropdown); 
        //    // ...if this index deleted, why does this work if incrementing and removing this index????
        //}

        //if(weaponDropdownOrigin.transform.childCount > 0)
        //{
        //    Debug.LogError("Some weapon dropdowns failed to delete: " + weaponDropdownOrigin.transform.childCount);
        //}

        deleteAllChildren(weaponDropdownOrigin);

        // add a new dropdown list for each hardpoint
        for (int i = 0; i < validWeaponsMasterList.Count; i++)
        {
            // create a ui dropdown
            Dropdown newDropDown = GameObject.Instantiate(weaponDropdownPrefab, weaponDropdownOrigin.transform).GetComponent<Dropdown>();

            // offset dropdown position
            newDropDown.transform.localPosition = new Vector3(0f, -dropdownOffset * i, 0f);
            newDropDown.options.Clear();
            
            // set dropdown elements
            for (int j = 0; j < validWeaponsMasterList[i].Count; j++)
            {
                Weapon validWeapon = validWeaponsMasterList[i][j];
                newDropDown.options.Add(new Dropdown.OptionData(validWeapon.gameObject.name));
            }

            int weaponIndex = validWeaponsMasterList[i].IndexOf(prefabHardpoints[i].weaponTypePrefab.GetComponent<Weapon>());
            Debug.Log("Building hardpoint " + i + ", loading weapon " + weaponIndex);

            // find currently equipped weapon (to prefab aircraft)
            newDropDown.value = weaponIndex;

            newDropDown.onValueChanged.AddListener(delegate
            {
                modifyLoadoutFromDropdowns();
            });

            newDropDown.RefreshShownValue();
        }

        generateStockSliders();

        //readLoadoutOntoDropdowns();
        
    }

    // PRE-REQUISITES FOR CALLING:
    //  - stock and weapon type arrays set for loadout
    public void generateStockSliders()
    {

        HardpointController controller = selectedAircraftPrefabHardpointController;



        deleteAllStockSliders();

        ref LoadoutStorage.LoadoutPreset activeLoadoutRef = ref getCurrentLoadoutRef();

        // loop through weapon types
        for (int i = 0; i < activeLoadoutRef.weaponTypes.Count; i++)
        {
            //  - instantiate UI slider, place inside corresponding team origin, offset by index
            //  - calculate max stock for weapon, set this to max value on slider
            Weapon weap = activeLoadoutRef.weaponTypes[i];
            int stockOfWeap = activeLoadoutRef.stock[i];
            int maxStockForWeap = controller.getStorage().maxIndividualStock(weap); // ignoring individual max for now
                                                                                    //  - set slider value according to stock amt
                                                                                    //  - attach onValueChanged listener?

            StockSliderReferences newStockSlider = GameObject.Instantiate(stockSliderPrefab, 
                stockSliderOrigin.transform).GetComponent<StockSliderReferences>();

            newStockSlider.transform.localPosition = new Vector3(0f, -stockSliderOffset * i, 0f);

            newStockSlider.initializeSlider(weap, stockOfWeap);

            // attach callback to slider
            newStockSlider.stockSlider.onValueChanged.AddListener(delegate
            {
                onStockSliderValueChange();
            });



        }

    }

    public void deleteAllStockSliders()
    {
        ////transform.get

        //for (int i = 0; i < stockSliderOrigin.transform.childCount; i++)
        //{
        //    GameObject sliderObj = stockSliderOrigin.transform.GetChild(i).gameObject;
        //    GameObject.Destroy(sliderObj);
        //    // somehow this works in weapon dropdown case?
        //    // even though we're incrementing after deleting an index?????
        //    // this doesn't leave undeleted indexes????
        //}


        //if (stockSliderOrigin.transform.childCount > 0)
        //{
        //    Debug.LogError("DeleteAllStockSliders skipped some child indexes: " + stockSliderOrigin.transform.childCount);
        //}

        deleteAllChildren(stockSliderOrigin);
    }

    // propagate slider value to corresponding loadout weapon stock
    // ignore if slider being generated for first time?
    //  -> initial slider creation value set shouldn't cause problem?
    public void onStockSliderValueChange()
    {
        ref LoadoutStorage.LoadoutPreset loadoutRef = ref getCurrentLoadoutRef();
        
        // loop through all sliders
        for(int i = 0; i < stockSliderOrigin.transform.childCount; i++)
        {
            // each slider index corresponds to weapon type
            StockSliderReferences sliderObj = stockSliderOrigin.transform.GetChild(i).GetComponent<StockSliderReferences>();

            int stockAmt = (int)sliderObj.stockSlider.value;

            loadoutRef.stock[i] = stockAmt;

            sliderObj.quantityReadoutText.text = stockAmt.ToString();
        }

        validateWeight();
        updateWeightLabel();


    }

    public bool validateWeight()
    {
        return acceptableWeight = getStorage().checkWeight(ref getCurrentLoadoutRef());
    }

    // called whenever selected aircraft changed
    public void refreshLoadoutPresetDropdown()
    {
        Debug.Log("Refresh loadout presetdropdown()");
        loadoutPresetDropdown.options.Clear();

        LoadoutStorage storage = selectedAircraftPrefabHardpointController.getStorage();

        string[] loadoutNames = storage.reportNamesArrayByTeam(myTeamTechInventory.myTeam);

        for(int i = 0; i < loadoutNames.Length; i++)
        {
            loadoutPresetDropdown.options.Add(new Dropdown.OptionData(loadoutNames[i]));
        }

        loadoutPresetDropdown.value = 0; // 0 is default loadout

        loadoutPresetDropdown.RefreshShownValue();


        // re-building to fix intermittent bug affecting display values
        // ...when switching from a high-g custom to multirole default
        // Kind of hacky fix. Sometimes builds multiple times. 
        Debug.Log("************ debug build called");
        buildWeaponSelectorDropdowns();

    }

    // Called whenever one of the hardpoint dropdowns changes value from user input
    //  !!!! If attempting to modify a default loadout, the current values only modify the CUSTOM loadout
    //   and then custom loadout is programmatically selected
    public void modifyLoadoutFromDropdowns()
    {
        Debug.Log("ModifyLoadoutFromDropdowns()");

        if (ignoreModifyCallbacks == 0)
        {


            // This actually should select the custom loadout
            ref LoadoutStorage.LoadoutPreset customLoadout = ref getCustomLoadoutRef();

            Debug.Log("Test 6");

            // loop through all dropdowns
            //  - get weapon ref from index and available weapons list
            //  - set that weapon ref to corresponding index in loadout
            for (int i = 0; i < prefabHardpoints.Length; i++)
            {
                Debug.Log("Test loop 1  " + i);
                Dropdown dropdown = weaponDropdownOrigin.transform.GetChild(i).GetComponent<Dropdown>();
                Debug.Log("Test loop 2 " + i);
                int selectedIndex = dropdown.value;
                Debug.Log("Test loop 3 " + i + ", selectedIndex: " + selectedIndex + ", validWeaponsMasterList[i].count: " + validWeaponsMasterList[i].Count);
                Weapon newWeapon = validWeaponsMasterList[i][selectedIndex];
                Debug.Log("Test loop 4 " + i);


                customLoadout.loadout[i] = newWeapon;
                Debug.Log("Test loop 5 " + i);
            }

            // after loadout modifications complete, must reinstantiate loadout's stock
            //LoadoutStorage storage = selectedAircraftPrefabHardpointController.getStorage();
            LoadoutStorage.instantiateStockList(ref customLoadout, true);

            Debug.Log("Test 7");
            // programmatically select custom loadout at dropdown
            //  - should be valid regardless of if dropdown refresh occurs from selecting custom loadout preset
            // But, ignore reselection if we're changing from custom TO a standard loadout
            //if (!ignoreReselection)
            //{
            //    int customIndex = getCustomIndex();

            //    loadoutPresetDropdown.value = getCustomIndex();
            //    loadoutPresetDropdown.RefreshShownValue();
            //}
            loadoutPresetDropdown.value = getCustomIndex();
            loadoutPresetDropdown.RefreshShownValue();

            generateStockSliders();

        }
        else
        {
            ignoreModifyCallbacks--;
            Debug.Log("Ignoring modify callback. " + ignoreModifyCallbacks + " callbacks remaining");
        }

        
        
    }



    // Called whenever the loadout preset dropdown changes value from user input
    public void readLoadoutOntoDropdowns()
    {
        Debug.Log("readLoadoutOntoDropdowns()");
        // current loadout should be changed by changed preset dropdown index
        ref LoadoutStorage.LoadoutPreset currentLoadoutRef = ref getCurrentLoadoutRef();

        // prevent weapon dropdowns from re-setting loadout preset dropdown if...
        // ...setting preset to a non-custom loadout
        //ignoreReselection = loadoutPresetDropdown.value <= getCustomIndex();

        // changing value of weapon dropdowns programmatically will still trigger onchanged callback
        // ...so, we will ignore all callbacks triggered by just changing the preset
        // because we do NOT want this to change loadout data, only display current preset
        //ignoreModifyCallbacks = validWeaponsMasterList.Count;

        Debug.Log(reportAvailableWeaponsList());
        Debug.Log(reportPrefabHardpointWeapons());

        for (int i = 0; i < validWeaponsMasterList.Count; i++)
        {
            Dropdown weapDropdown = getWeaponDropdown(i);

            Weapon newWeapon = currentLoadoutRef.loadout[i];

            int newIndex = validWeaponsMasterList[i].IndexOf(newWeapon);

            Debug.Log("Reading dropdown " + i + ", loading weapon " + newWeapon.gameObject.name + " at index: " + newIndex);

            // we don't want to trigger loadout writes, only display the selected preset at the dropdowns
            if(newIndex != weapDropdown.value)
            {
                ignoreModifyCallbacks++;
            }

            weapDropdown.value = newIndex;

            weapDropdown.RefreshShownValue();
        }

    }

    public void updateWeightLabel()
    {
        int weight = LoadoutStorage.weightTally(ref getCurrentLoadoutRef());
        int maxWeight = getStorage().getController().maxWeight;


        string label = "Weight: " + weight + " / " + maxWeight;
        

        if(weight > maxWeight)
        {
            label += " Overweight!!";
        }

        weightLabel.text = label;
    }
    

    // trigger this on aircraft spawn for this team
    // TODO: Make this fetch proper selected loadout via preset index AND team
    //  ....actually, maybe i don't need to change anything? If correct values stored in dropdowns?
    //  --> not strictly necessary, but for cleanliness, I should refactor this to use the loadout struct
    public void equipLoadoutOntoSpawnedAircraft(GameObject aircraftInstance)
    {
        Debug.Log("equipLoadoutOntoSpawnedAircraft()");
        HardpointController hardpointControllerInstance = aircraftInstance.GetComponent<TgtComputer>().getHardpointController();
        //Hardpoint[] hardpoints = hardpointControllerInstance.getHardpoints();


        //// loop through each hardpoint index and find selected weapon for each
        ////  > read selected index of corresponding dropdown
        ////  > use dropdown index to select weapon from available types list for THAT particular hardpoint
        ////  > load weapon onto aircraft instance
        //for (int i = 0; i < prefabHardpoints.Length; i++)
        //{
        //    Dropdown dropdown = weaponDropdownOrigin.transform.GetChild(i).GetComponent<Dropdown>();
        //    int selectedIndex = dropdown.value;
        //    Weapon newWeapon = validWeaponsMasterList[i][selectedIndex];

            
        //    hardpoints[i].equipNewWeapon(newWeapon);
        //}

        // after all hardpoints equipped with weapon:
        // > trigger hardpointController's initialization process
        //hardpointControllerInstance.initializeEquippedLoadout();
        //int dropdownIndex = loadoutPresetDropdown.value;


        hardpointControllerInstance.equipLoadoutPreset(ref getCurrentLoadoutRef());
    }

    //// makes all selected weapon dropdowns to propagate their values to selected aircraft prefab's loadout
    //// loadoutIndex selects which loadout of the aircraft prefab will be modified
    //public void updateAircraftPrefabLoadout(GameObject aircraftPrefab, int loadoutIndex)
    //{
    //    Debug.Log("updateAircraftPrefabLoadout()");
    //    activeAircraftPrefab = aircraftPrefab;
    //    selectedAircraftPrefabHardpointController = aircraftPrefab.GetComponent<TgtComputer>().getHardpointController();
    //    prefabHardpoints = selectedAircraftPrefabHardpointController.getHardpoints();

    //    LoadoutStorage loadStorage = selectedAircraftPrefabHardpointController.getStorage();

    //    for(int i = 0; i < prefabHardpoints.Length; i++)
    //    {
    //        Dropdown dropdown = weaponDropdownOrigin.transform.GetChild(i).GetComponent<Dropdown>();
    //        int selectedIndex = dropdown.value;
    //        Weapon newWeapon = validWeaponsMasterList[i][selectedIndex];

    //        loadStorage.standardLoadouts[loadoutIndex].loadout[i] = newWeapon;
    //    }

    //    //loadStorage.instantiateStockList(ref loadStorage.standardLoadouts[loadoutIndex], true);
    //}


    public string reportAvailableWeaponsList()
    {
        Debug.Log("reportAvailableWeaponsList()");
        string report = "**************** Available weapons for " + activeAircraftPrefab + " ******************************\ntest\ntest2\n ";

        report += "validWeaponsMasterList.Count = " + validWeaponsMasterList.Count + "\n";

        for(int i = 0; i < validWeaponsMasterList.Count; i++)
        {
            report += "Available weapons for hardpoint " + i + ": ";

            for(int j = 0; j < validWeaponsMasterList[i].Count; j++)
            {
                report += validWeaponsMasterList[i][j].gameObject.name;
            }

            report += "\n";
        }


        return report;
    }

    // current loadout is based on loadout preset selection, and this loader's team
    public ref LoadoutStorage.LoadoutPreset getCurrentLoadoutRef()
    {
        Debug.Log("WeaponLoader's getCurrentLoadoutRef()");
        return ref selectedAircraftPrefabHardpointController.getStorage().getLoadoutRef(loadoutPresetDropdown.value, myTeamTechInventory.myTeam);
    }

    public ref LoadoutStorage.LoadoutPreset getCustomLoadoutRef()
    {
        Debug.Log("WeaponLoader's getCustomLoadoutRef()");
        return ref selectedAircraftPrefabHardpointController.getStorage().getCustomLoadout(myTeamTechInventory.myTeam);
    }

    public int getCustomIndex()
    {
        Debug.Log("WeaponLoader's getCustomIndex()");
        return selectedAircraftPrefabHardpointController.getStorage().getCustomIndex();
    }

    public Dropdown getWeaponDropdown(int index)
    {
        Debug.Log("WeaponLoader's getWeaponDropdown()");
        return weaponDropdownOrigin.transform.GetChild(index).GetComponent<Dropdown>();
    }

    public LoadoutStorage getStorage()
    {
        return selectedAircraftPrefabHardpointController.getStorage();
    }
}
