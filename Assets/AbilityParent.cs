using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityParent : MonoBehaviour
{
    public float reloadTimerMax = 10f;
    private float reloadTimer;

    public string abilityName = "";

    public GameObject abilityIconPrefab;


    public CombatFlow myFlow;

    public int bullshitDelayMax = 3;
    public int bullshitfuckingdelay;
    public bool didStart = false;

    public void init()
    {
        bullshitDelayMax = bullshitfuckingdelay;

        Debug.Log("Ability parent init() called");
        //reloadTimer = reloadTimerMax;

        myFlow = GetComponent<CombatFlow>();

        // if CombatFlow is null, this is likely attached to a tech object
        if(myFlow == null)
        {
            this.enabled = false; // disable if NOT attached to combatflow
        }


    }

    public void startProcess()
    {
        Debug.Log("AbilityParent startProcess called, isLocalPlayer: " + myFlow.isLocalPlayer);
        if (myFlow.isLocalPlayer)
        {
            AbilityIconManager.iconManager.linkToAircraft(this);

        }

        didStart = true;
    }

    private void Awake()
    {
        
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void delayStartProcess()
    {
        if(bullshitfuckingdelay > 0)
        {
            bullshitfuckingdelay--;
        }
        else
        {
            startProcess();
        }
    }

    public void updateProcess()
    {
        if (didStart)
        {
            // if timer complete
            if (reloadTimer < 0)
            {
                // available to activate warp
                // gather input if user pressing warp button
                // only reset timer after activating warp

                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    activate();
                }
            }
            else
            {
                reloadTimer -= Time.deltaTime;
                // update UI to show reload status
            }
        }
        else
        {
            delayStartProcess();
        }
    }

    virtual public void activate()
    {
        Debug.Log("Parent ability activate() called");
        reloadTimer = reloadTimerMax;

        //  - how should I handle if some abilities prefer to delay resetting timer?
        //  - Could I just set reload time high enough such that.. 
        //     ..it includes wait for both ability duration AND standard reload time?
    }


    virtual public void equipAbilityToAircraftObject(GameObject aircraftObj)
    {
        // How to get unity editor values to pass into script?
        // tech object's attached script can have edited values

        // so we must add the raw script initially
        // and then copy values from the tech object onto the aircraft
        clearAbility(aircraftObj);

    }

    public static void clearAbility(GameObject aircraftObj)
    {
        AbilityParent ability = aircraftObj.GetComponent<AbilityParent>();


        if (ability != null)
        {
            AbilityIconManager.iconManager.cleanup();
            GameObject.Destroy(ability);
        } 
    }

    virtual public void copyOther(AbilityParent other)
    {
        reloadTimerMax = other.reloadTimerMax;
        reloadTimer = other.reloadTimer;
        abilityName = other.abilityName;
        abilityIconPrefab = other.abilityIconPrefab;

    }

    public float readTimer()
    {
        return reloadTimer;
    }


    private void OnDestroy()
    {
        // trigger hud cleanup
        AbilityIconManager.iconManager.cleanup();
    }
}
