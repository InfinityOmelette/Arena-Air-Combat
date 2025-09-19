using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInventory : MonoBehaviour
{
    public static List<TechInventory> teamTechInventories;

    public List<TechObject> techInventory;

    public CombatFlow.Team myTeam;

    public List<AbilityParent> unlockedAbilityScripts;

    public Dropdown abilitySelectDropdown;
    public AbilityParent selectedAbility;


    private void Awake()
    {
        
        // If this is the first tech inventory to awaken, initialize the static list
        if(teamTechInventories == null)
        {
            teamTechInventories = new List<TechInventory>();

            // unknown which team "awakens" first. So default to
            // making both indexes reference this inventory.
            // when the next inventory awakens, that one will overwrite the
            // reference to its team's inventory in the list
            teamTechInventories.Add(this);
            teamTechInventories.Add(this);
        }
        else // if the list is already initialized, set reference to this in list
        {
            teamTechInventories[(int)myTeam] = this;
        }

        techInventory = new List<TechObject>();
        unlockedAbilityScripts = new List<AbilityParent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addTech(TechObject newTech)
    {
        Debug.Log("Adding tech " + newTech.gameObject.name + " to team " + myTeam);
        techInventory.Add(newTech);
        newTech.teamInventory = this;
    }

    public void addAbility(AbilityParent newAbility)
    {
        // original ability instance stored on tech object, which persists with scene

        Debug.Log("Adding ability " + newAbility.abilityName + " to techinventory of " + myTeam);

        unlockedAbilityScripts.Add(newAbility);

        // add index to dropdown list
        // Are ability's editor values preserved?

        abilitySelectDropdown.options.Add(new Dropdown.OptionData(newAbility.abilityName));
        abilitySelectDropdown.value = 0; // refresh display
        selectAbility();
        // aside note: when spawning, access inventory's selected ability and equip it onto aircraft
    }

    public void selectAbility()
    {
        // read index from dropdown
        // set selectedAbility reference
        Debug.Log("SelectAbility() called for TechInventory of " + myTeam);

        int index = abilitySelectDropdown.value;
        selectedAbility = unlockedAbilityScripts[index];
    }

    public void equipSelectedAbilityToAircraft(GameObject aircraftObj)
    {
        if(selectedAbility != null && unlockedAbilityScripts.Count > 0)
        {
            Debug.Log("Equipping ability " + selectedAbility.abilityName + " to " + aircraftObj.name);
            selectedAbility.equipAbilityToAircraftObject(aircraftObj);
        }
    }
}
