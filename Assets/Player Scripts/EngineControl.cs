using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineControl : MonoBehaviour
{
    

    public float currentBaseThrust;
    public float currentBaseThrustPercent;
    public float MAX_THRUST_DELTA;
    public float THRUST_MIN;
    public float THRUST_MAX;
    public float currentThrottlePercent; // 0-100 to stay consistent with current thrust percent

    public float MAX_THROTTLE_DELTA;
    public float throttleAccel;
    public float currentThrottleDelta;

    public float currentAirDensity;
    public float currentBurnMod;

    public float maxFuelMass;
    public float currentFuelMass;
    public float seaLevelMaxBurnRate;
    public float burnRateAltitudeResiliency; // 0 burns same as air density, 1.0 burns constant unchanged by air density

    public float massUpdateGap;
    private float previousFuelMassUpdate = 0;

    public float input_throttleAxis;
    public float input_scrollWheelAxis;
    public float scrollWheelMultiplier;

    public float minAB_thrust;
    public float minAB_Scale;
    public float maxAB_Scale;

    public GameObject afterburnerGraphic;

    public AirEnvironmentStats air;
    public Contrail contrailRef;


    private CombatFlow myFlow;

    Rigidbody rbRef;

    public AudioSource jetEngine;
    public AudioSource afterburner;
    public AudioSource engineFar;

    public float abVolumeOffset;

    private float initAfterburnVolume;
    private float initEngineVolume;

    public float enginePitchMin;
    public float engineVolumeMin;


    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        rbRef = GetComponent<Rigidbody>();
        initAfterburnVolume = afterburner.volume;
        initEngineVolume = jetEngine.volume;
    }

    // ================================ START
    void Start()
    {
        if (myFlow.isLocalPlayer)
        {
            checkAirStatsRefError();

            currentFuelMass = Mathf.Clamp(currentFuelMass, 0.0f, maxFuelMass);
        }


        jetEngine.loop = true;
        afterburner.loop = true;

        jetEngine.Play();
        afterburner.Play();


        //jetEngine.volume = 0.0f;
        //afterburner.volume = 0.0f;


        if (!myFlow.isLocalPlayer)
        {
            engineFar.loop = true;
            engineFar.Play();
        }
    }

    private void checkAirStatsRefError()
    {
        if (air == null)
            air = AirEnvironmentStats.getAir();
    }


    // ================================ LATEUPDATE
    void LateUpdate()
    {
        if (myFlow.isLocalPlayer)
        {

            updateFuelMass(); // root rigidbody will change mass depending on fuel level
            processAfterburnerGraphic();

            if (myFlow.isLocalPlayer)
            {
                afterBurnerVolume();
                jetEngine.volume = initEngineVolume;

                enginePitchAndVolume();
            }

            contrailRef.engineOn = currentFuelMass > 0f;
        }
    }

    private void enginePitchAndVolume()
    {
        float minABThrust = minAB_thrust * THRUST_MAX / 100f;

        float thrustPercent = Mathf.Clamp((currentBaseThrust) / (minABThrust), 
            0.0f, 1.0f);

        float pitch = thrustPercent * (1.0f - enginePitchMin) + enginePitchMin;

        jetEngine.pitch = pitch;


        float volume = thrustPercent * (initEngineVolume - engineVolumeMin) + engineVolumeMin;
        jetEngine.volume = volume;
    }

    private void afterBurnerVolume()
    {
        float minABThrust = minAB_thrust * THRUST_MAX / 100f;
        float abPercent = Mathf.Max((currentBaseThrust - minABThrust)
            / (THRUST_MAX - minABThrust), 0.0f);

        float abOffset = 0f;

        if (abPercent > 0.1)
        {
            abOffset = abVolumeOffset;
        }

        afterburner.volume = initAfterburnVolume * abPercent + abOffset;

        //Debug.Log("Current thrust base: " + currentBaseThrust + "with " + abPercent + "abPercent");
    }

    private void Update()
    {
        if (myFlow.isLocalPlayer)
        {
            currentThrottlePercent = inputThrottleFromMouse();

            //  THRUST BASE
            currentThrottlePercent = inputThrottleFromJoypad();       // set throttle
        }
    }




    private float inputThrottleFromMouse()
    {
        float newThrottle = currentThrottlePercent;
        newThrottle += input_scrollWheelAxis * scrollWheelMultiplier;
        return Mathf.Clamp(newThrottle, 0f, 100f);
    }

    private void processAfterburnerGraphic()
    {
        // min 3 max 10 scale

        if(currentBaseThrustPercent > minAB_thrust)
        {
            //afterburnerGraphic.GetComponent<Renderer>().enabled = true;
            afterburnerGraphic.SetActive(true);

            // linearly scale from 0 to 1 as thrust increases from minAB_thrust to 100% thrust
            float thrustRangeDecimal = (currentBaseThrustPercent - minAB_thrust) / 100f;

            // linearly scale new Z scale from min to max as thrust decimal increases from 0 to 1
            float newScaleZ = thrustRangeDecimal * (maxAB_Scale - minAB_Scale) + minAB_Scale;


            Vector3 originalLocalScale = afterburnerGraphic.transform.localScale;

            afterburnerGraphic.transform.localScale = 
                new Vector3(originalLocalScale.x, originalLocalScale.y, newScaleZ);
        }
        else
        {
            //afterburnerGraphic.GetComponent<Renderer>().enabled = false;
            afterburnerGraphic.SetActive(false);
        }
    }

    private void updateFuelMass()
    {
        // Only update after value changes by certain amount
        //  THIS MIGHT NOT AFFECT EFFICIENCY MUCH AT ALL -- depends on how heavy it is to update mass
        if (Mathf.Abs(previousFuelMassUpdate - currentFuelMass) > massUpdateGap) 
        {
            rbRef.mass -= previousFuelMassUpdate;   //  Return rigidbody to original mass
            rbRef.mass += currentFuelMass;          //  Add updated amount of fuel to it
            previousFuelMassUpdate = currentFuelMass; // record previous update
        }
    }


    // =============================== FIXEDUPDATE
    void FixedUpdate()
    {
        if (myFlow.isLocalPlayer)
        {

            //  THRUST BASE

            currentBaseThrust = stepBaseThrustToTarget(currentThrottlePercent); // step thrust value
            currentBaseThrustPercent = (currentBaseThrust - THRUST_MIN) / (THRUST_MAX - THRUST_MIN) * 100f; // update current thrust

            //  BURN RATE MODIFICATION
            float fuelBurnMod = calculateFuelBurnMod(transform.position.y, burnRateAltitudeResiliency); // calculate fuel burn mod

            // burn fuel according to burn rate and target thrust (moving toward zero to avoid bouncing fuel level at 0)
            currentFuelMass = Mathf.MoveTowards(currentFuelMass, 0.0f,
                fuelBurnMod * seaLevelMaxBurnRate * (currentBaseThrustPercent / 100f) * Time.fixedDeltaTime);

            currentFuelMass = Mathf.Clamp(currentFuelMass, 0.0f, maxFuelMass);
            float currentTrueThrust = currentBaseThrust * fuelBurnMod; // create thrust according to burn rate


            // ADD FORCE
            rbRef.AddForce(transform.forward * currentTrueThrust);

        }
        
    }


    // SET THROTTLE
    private float inputThrottleFromJoypad()
    {

        float controllerInput = input_throttleAxis;

        // RESET currentThrottleDelta TO ZERO IF:
        //  - controller input sign differs from currentThrottleDelta sign
        //  - controller input approximately zero
        if (Mathf.Sign(currentThrottleDelta) != Mathf.Sign(controllerInput) || Mathf.Approximately(controllerInput, 0.0f))
            currentThrottleDelta = 0.0f; // reset

        // Step currentThrottleDelta towards target delta
        currentThrottleDelta = Mathf.MoveTowards(currentThrottleDelta,
            MAX_THROTTLE_DELTA * controllerInput, throttleAccel * Time.deltaTime);

        // step currentThrottlePercent by delta
        return currentThrottlePercent = Mathf.Clamp(currentThrottlePercent + (currentThrottleDelta * Time.deltaTime), 0.0f, 100f);
    }

    private float stepBaseThrustToTarget(float targetThrottlePercent)
    {
        // target thrust is percentage along value range
        float targetThrust = (targetThrottlePercent / 100.0f) * (THRUST_MAX - THRUST_MIN) + THRUST_MIN;

        // FUEL CHECK
        if (Mathf.Approximately(currentFuelMass, 0.0f))
            targetThrust = 0.0f;

        // step towards target thrust
        return Mathf.MoveTowards(currentBaseThrust, targetThrust, MAX_THRUST_DELTA * Time.deltaTime);


    }


    


    // Give modifier for fuel burn rate -- 1.0 is full burn (multiplied by throttle to get thrust and true burn rate)
    private float calculateFuelBurnMod(float altitude, float altitudeResiliency)
    {
        float resultMod = 0.0f;
        currentAirDensity = air.getDensityAtAltitude(altitude);
        float resil = Mathf.Clamp(1.0f - altitudeResiliency, 0.0f, 1.0f);

        resultMod = Mathf.Pow(currentAirDensity, resil);
        currentBurnMod = resultMod;

        return resultMod;
    }

    


}
