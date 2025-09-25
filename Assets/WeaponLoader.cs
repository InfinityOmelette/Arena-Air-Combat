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

    public List<Weapon> weaponsToEquip;

    public GameObject weaponDropdownOrigin;

    public TechInventory myTeamTechInventory;

    public Dropdown weaponDropdownPrefab;

    public float dropdownOffset = 30f;

    private void Awake()
    {
        validWeaponsMasterList = new List<List<Weapon>>();
        myTeamTechInventory = GetComponent<TechInventory>();
    }

    // Start is called before the first frame update
    void Start()
    {

        refreshAvailableWeapons(GameManager.getGM().selectedPlayerPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // trigger this when selected aircraft for this team changes
    public void refreshAvailableWeapons(GameObject aircraftPrefab)
    {
        activeAircraftPrefab = aircraftPrefab;
        selectedAircraftPrefabHardpointController = aircraftPrefab.GetComponent<TgtComputer>().getHardpointController();

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
        refreshUISelector();

        

    }

    public void refreshUISelector()
    {
        // Destroy all current ui weapon select dropdowns
        for (int i = 0; i < weaponDropdownOrigin.transform.childCount; i++)
        {
            GameObject dropdown = weaponDropdownOrigin.transform.GetChild(i).gameObject;
            GameObject.Destroy(dropdown);
        }

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

            newDropDown.value = validWeaponsMasterList[i].IndexOf(prefabHardpoints[i].weaponTypePrefab.GetComponent<Weapon>()); // find currently equipped weapon
            newDropDown.RefreshShownValue();
        }

        
    }

    // trigger this on aircraft spawn for this team
    public void equipLoadoutOntoSpawnedAircraft(GameObject aircraftInstance)
    {
        HardpointController hardpointControllerInstance = aircraftInstance.GetComponent<TgtComputer>().getHardpointController();
        Hardpoint[] hardpoints = hardpointControllerInstance.getHardpoints();


        // loop through each hardpoint index and find selected weapon for each
        //  > read selected index of corresponding dropdown
        //  > use dropdown index to select weapon from available types list for THAT hardpoint
        //  > load weapon onto aircraft instance
        for (int i = 0; i < prefabHardpoints.Length; i++)
        {
            Dropdown dropdown = weaponDropdownOrigin.transform.GetChild(i).GetComponent<Dropdown>();
            int selectedIndex = dropdown.value;
            Weapon newWeapon = validWeaponsMasterList[i][selectedIndex];

            
            hardpoints[i].equipNewWeapon(newWeapon);
        }

        // after all hardpoints equipped with weapon:
        // > trigger hardpointController's initialization process
        hardpointControllerInstance.initializeEquippedLoadout();
    }


    public string reportAvailableWeaponsList()
    {
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
}
