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

    public AirEnvironmentStats air;
    Rigidbody rbRef;

    // ================================ START
    void Start()
    {
        checkAirStatsRefError();
        rbRef = GetComponent<Rigidbody>();
        currentFuelMass = Mathf.Clamp(currentFuelMass, 0.0f, maxFuelMass);
    }

    private void checkAirStatsRefError()
    {
        if (air == null)
            Debug.Log("Error: " + gameObject.ToString() + " unable to find air ref");
    }

    // =============================== FIXEDUPDATE
    void FixedUpdate()
    {
        //  THRUST BASE
        currentThrottlePercent = inputThrottle();       // set throttle
        currentBaseThrust = stepBaseThrustToTarget(currentThrottlePercent); // step thrust value
        currentBaseThrustPercent = (currentBaseThrust - THRUST_MIN) / (THRUST_MAX - THRUST_MIN) * 100f; // update current thrust

        //  BURN RATE MODIFICATION
        float fuelBurnMod = calculateFuelBurnMod(transform.position.y, burnRateAltitudeResiliency); // calculate fuel burn mod
        currentFuelMass -= fuelBurnMod * seaLevelMaxBurnRate * (currentBaseThrustPercent / 100f);       // burn fuel according to burn rate and target thrust
        float currentTrueThrust = currentBaseThrust * fuelBurnMod; // create thrust according to burn rate


        // ADD FORCE
        rbRef.AddForce(transform.forward * currentTrueThrust);



        
    }


    // SET THROTTLE
    private float inputThrottle()
    {
        float controllerInput = Input.GetAxis("Throttle");

        // RESET currentThrottleDelta TO ZERO IF:
        //  - controller input sign differs from currentThrottleDelta sign
        //  - controller input approximately zero
        if (Mathf.Sign(currentThrottleDelta) != Mathf.Sign(controllerInput) || Mathf.Approximately(controllerInput, 0.0f))
            currentThrottleDelta = 0.0f; // reset

        // Step currentThrottleDelta towards target delta
        currentThrottleDelta = Mathf.MoveTowards(currentThrottleDelta,
            MAX_THROTTLE_DELTA * controllerInput, throttleAccel);

        // step currentThrottlePercent by delta
        return currentThrottlePercent = Mathf.Clamp(currentThrottlePercent + currentThrottleDelta, 0.0f, 100f);
    }

    private float stepBaseThrustToTarget(float targetThrottlePercent)
    {
        // target thrust is percentage along value range
        float targetThrust = (targetThrottlePercent / 100.0f) * (THRUST_MAX - THRUST_MIN) + THRUST_MIN;

        // step towards target thrust
        return Mathf.MoveTowards(currentBaseThrust, targetThrust, MAX_THRUST_DELTA);


    }


    


    // Give modifier for fuel burn rate -- 1.0 is full burn (multiplied by throttle to get thrust and true burn rate)
    private float calculateFuelBurnMod(float altitude, float altitudeResiliency)
    {
        float resultMod = 0.0f;
        currentAirDensity = air.getDensityAtAltitude(altitude);
        resultMod = Mathf.Pow(currentAirDensity, altitudeResiliency);
        currentBurnMod = resultMod;

        return resultMod;
    }



}
