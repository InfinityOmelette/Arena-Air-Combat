using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategicTarget : MonoBehaviour
{
    public CombatFlow myFlow;

    public float suppressionHealthMax;
    public float suppressionHealthCurrent;

    public bool isSuppressed = false;
    public float suppressionRepairRate;

    public Lane lane;

    public enum Lane
    {
        TOP,
        BOTTOM,
        BASE
    }

    // Aircraft suppress, ground forces capture

    // When suppressed:
    //  - Creeps no longer stopped & no longer engage
    //  - This structure no longer engages in any activity
    //   > guns stop shooting, stops spawning air defenses
    //   > stops any logistical support (doesn't spawn creeps anymore, doesn't

    // How suppression health is lost
    //  - Creeps shoot structure and do some suppression damage
    //  - Aircraft shoot structure and do lots of suppression damage
    //  - Creep - creep damage will be reduced significantly so that aircraft damage naturally greater

    // How suppression health is regained
    //  - Structure regains suppression health over time slowly

    // How suppression state changes
    //  - When suppression health reaches 0, structure enters "Suppressed" state
    //  - As suppression health is repaired, structure remains "suppressed" until suppression health maxes out.
    //  - Can continue to take suppression damage while being repaired -- structure can be permanently suppressed if takes repeated damage

    // Capture mechanics
    //  - When suppressed, creeps ignore structure and proceed as if structure wasn't there
    //  - When a creep passes by a trigger (thin line collider across width of lane), WHILE the structure is suppressed, structure is captured

    // When unsuppressed, creeps will stop and shoot at structure when within range


    public static List<StrategicTarget> AllStrategicTargets;

    private void Awake()
    {
        // Explicitly using static naming for readability
        if(StrategicTarget.AllStrategicTargets == null)
        {
            StrategicTarget.AllStrategicTargets = new List<StrategicTarget>();
        }

        StrategicTarget.AllStrategicTargets.Add(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        myFlow = GetComponent<CombatFlow>();
        myFlow.isActive = true;
        suppressionHealthCurrent = suppressionHealthMax;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        repairSuppression(Time.fixedDeltaTime);

        if(suppressionHealthCurrent < 0.0f)
        {
            activateSuppressedState();
        }
    }

    public void capture(CombatFlow.Team capturingTeam)
    {
        myFlow.setNetTeam(0);
    }

    public void dealSuppression(float damage)
    {
        
        suppressionHealthCurrent -= damage;
        Debug.Log("Applying " + damage + " suppression damage to " + gameObject.name + ", remaining SuppHP: " + suppressionHealthCurrent);
    }

    private void repairSuppression(float time)
    {
        suppressionHealthCurrent += suppressionRepairRate * time;

        if(suppressionHealthCurrent > suppressionHealthMax)
        {
            deactivateSuppressedState();
        }

        suppressionHealthCurrent = Mathf.Min(suppressionHealthCurrent, suppressionHealthMax);
        //Debug.Log("Healing applied to " + gameObject.name);
    }

    private void activateSuppressedState()
    {
        
        suppressionHealthCurrent = 0f;
        isSuppressed = true;
        
    }

    private void deactivateSuppressedState()
    {
        isSuppressed = false;
    }
}
