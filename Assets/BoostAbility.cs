using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostAbility : AbilityParent
{
    //public float reloadTimerMax = 10f;
    //private float reloadTimer;

    public float boostVelocity;

    Rigidbody myRb;


    private void Awake()
    {
        //reloadTimer = reloadTimerMax;
        base.init();
        //base.abilityName = "Boost"; // try to set name from inspector instead?
        myRb = GetComponent<Rigidbody>();
        Debug.Log("Ability child Awake() called");

        // spawn and activate UI element
        //  - load picture onto UI element
    }

    // Start is called before the first frame update
    void Start()
    {
        base.startProcess();
    }

    private void Update()
    {
        base.updateProcess();
    }


    // Update is called once per frame
    //void Update()
    //{
    //    // if timer complete
    //    if (reloadTimer < 0)
    //    {
    //        // available to activate warp
    //        // gather input if user pressing warp button
    //        // only reset timer after activating warp

    //        if (Input.GetKeyDown(KeyCode.LeftControl))
    //        {
    //            activate();
    //        }
    //    }
    //    else
    //    {
    //        reloadTimer -= Time.deltaTime;
    //        // update UI to show reload status
    //    }
    //}

    override
    public void activate()
    {
        //reloadTimer = reloadTimerMax;
        base.activate();
        myRb.velocity += transform.forward * boostVelocity;

        // activate any effects

    }

    override
    public void copyOther(AbilityParent other)
    {
        base.copyOther(other);

        BoostAbility otherBoost = (BoostAbility)other;
        boostVelocity = otherBoost.boostVelocity;
    }

    override
    public void equipAbilityToAircraftObject(GameObject aircraftObj)
    {
        // How to get unity editor values to pass into script?
        // tech object's attached script can have edited values

        // so we must add the raw script initially
        // and then copy values from the tech object onto the aircraft

        BoostAbility equippedBoost = aircraftObj.AddComponent<BoostAbility>();
        equippedBoost.copyOther(this); // this should pass editor-set values onto equipped boost script
    }
}
