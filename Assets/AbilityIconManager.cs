using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityIconManager : MonoBehaviour
{
    public static AbilityIconManager iconManager;

    public AbilityParent linkedAircraftAbility;


    public FlareIcon reloadStatusIcon;

    // Is there any reason for this manager to exist, instead of aircraft directly talking to icon?
    // Leaves room for multiple ability icons at least?

    private void Awake()
    {
        Debug.Log("AbilityIconManager's Awake() called");

        if(iconManager == null)
        {
            AbilityIconManager.iconManager = this; // allow other scripts to easily access

        }

        // place on lower left corner
        Vector3 initPos = transform.localPosition;
        transform.localPosition = new Vector3(-Screen.width / 2f + initPos.x, -Screen.height / 2f + initPos.y, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(linkedAircraftAbility != null)
        {
            bool isReady = linkedAircraftAbility.readTimer() < 0f;
            reloadStatusIcon.setReloadStatus(isReady, linkedAircraftAbility.readTimer(), linkedAircraftAbility.reloadTimerMax);
        }
        else if(reloadStatusIcon != null)
        {
            cleanup();
        }


    }

    public void linkToAircraft(AbilityParent aircraftAbility)
    {
        // should i validate that we're actually accessing an aircraft type of player?
        // fuck it, we ball

        Debug.Log("Linking ability icon manager to " + aircraftAbility.gameObject.name);

        linkedAircraftAbility = aircraftAbility;

        // read hud icon prefab from aircraft
        reloadStatusIcon = GameObject.Instantiate(linkedAircraftAbility.abilityIconPrefab, transform).GetComponent<FlareIcon>();

        // Spawn Ability UI prefab

    }

    public void cleanup()
    {
        GameObject.Destroy(reloadStatusIcon.gameObject);
        linkedAircraftAbility = null;
    }
}
