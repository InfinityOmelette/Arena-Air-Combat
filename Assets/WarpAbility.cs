using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpAbility : AbilityParent
{

    public float warpDistance;

    Rigidbody myRb;


    // UI reference

    private void Awake()
    {
        base.init();
        //base.abilityName = "Warp";
        myRb = GetComponent<Rigidbody>();

        // spawn and activate UI element
        //  - load picture onto UI element
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        base.updateProcess();

    }

    override
    public void activate()
    {
        base.activate();

        myRb.position += transform.forward * warpDistance;
        // also need to check if we hit the fuckin ground lol

        // activate any effects

    }

    override
    public void copyOther(AbilityParent other)
    {
        base.copyOther(other);

        WarpAbility otherWarp = (WarpAbility)other;
        warpDistance = otherWarp.warpDistance;
    }

    override
    public void equipAbilityToAircraftObject(GameObject aircraftObj)
    {
        // How to get unity editor values to pass into script?
        // tech object's attached script can have edited values

        // so we must add the raw script initially
        // and then copy values from the tech object onto the aircraft

        WarpAbility equippedWarp = aircraftObj.AddComponent<WarpAbility>(); // adds raw script instance
        equippedWarp.copyOther(this); // this should pass editor-set values onto equipped warp script
    }

}
