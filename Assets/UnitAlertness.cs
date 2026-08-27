using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAlertness : MonoBehaviour
{

    public float wakeUpTimeMax = 8f;
    public float wakeUpTimer;


    public float lowerGuardTimeMax = 12f;
    public float lowerGuardTimer;

    public bool isAlert;

    private bool beginLoweringGuard;
    private bool beginWakingUp;


    public float lowerGuardResetTimerMax = .5f;
    public float lowerGuardResetTimer;

    public float wakeUpResetTimerMax = 10f;
    public float wakeUpResetTimer;

    public float currentCoeff = 1.0f;

    public Rigidbody alertingUnit;


    //public float minWakeTimeCoeff = .25f;
    //public float minWakeTimeRange = 2000f;
    //public float fullWakeTimeRange = 4500f;

    public float closeWakeTimeCoeff = 2f;
    public float closeWakeTimeRange = 1750;
    public float normalWakeTimeRange = 3000f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        wakeTimerProcess(Time.fixedDeltaTime);
        tryLowerGuard(Time.fixedDeltaTime);
    }

    public bool checkAlertStatus()
    {
        return isAlert;
    }

    private bool wakeTimerProcess(float deltaTime)
    {
        //bool wake = false;
        isAlert = wakeUpTimer < 0f;



        // target locked --> begin waking up
        if (beginWakingUp)
        {
            // If target is locked while alert
            if (isAlert)
            {
                resetLowerGuardTimer(deltaTime); // become fully alert
            }
            else // target locked but NOT alert
            {
                wakeUpTimer -= deltaTime * timerCoeffByDist(); // slowly become alert
            }

            wakeUpResetTimer = wakeUpResetTimerMax;

        }
        else if (!isAlert) // no target locked and not alert yet --> reset wake timer
        {
            resetWakeTimer(deltaTime);
        }


        return isAlert;
    }

    private float timerCoeffByDist()
    {
        float coeff = 1.0f;

        if(alertingUnit != null)
        {
            float dist = Vector3.Distance(transform.position, alertingUnit.transform.position);
            //coeff = minWakeTimeCoeff + 
            //    Mathf.Max((dist - minWakeTimeRange) / fullWakeTimeRange, 0.0f);
            //coeff = Mathf.Clamp(coeff, minWakeTimeCoeff, 1.0f);

            float lerpRate = Mathf.Clamp((dist - closeWakeTimeRange) / normalWakeTimeRange, 0f, 1f);
            coeff = Mathf.Lerp(closeWakeTimeCoeff, 1.0f, lerpRate);

        }

        currentCoeff = coeff;

        return coeff;
    }

    private void tryLowerGuard(float deltaTime)
    {
        bool doSleep = lowerGuardTimer < 0f;



        // no target locked --> begin lowering guard
        if (beginLoweringGuard)
        {

            // no target locked and we're sleeping --> reset wake timer
            if (doSleep)
            {
                resetWakeTimer(deltaTime);
            }
            else // no target locked but unit is awake --> slowly lower guard
            {
                lowerGuardTimer -= deltaTime;
            }

        }
        else if (!doSleep) // target locked before guard lowered --> stay alert
        {
            resetLowerGuardTimer(deltaTime);
        }


    }

    public void beginChangingAlertStatus(bool alertingUnitPresent, Rigidbody alertingUnit = null)
    {
        setLoweringGuard(!alertingUnitPresent);
        setBeginWake(alertingUnitPresent, alertingUnit);
    }

    public void setLoweringGuard(bool lowerGuard)
    {
        beginLoweringGuard = lowerGuard;
        alertingUnit = null;
    }

    public void setBeginWake(bool doWake, Rigidbody alertingUnit)
    {

        beginWakingUp = doWake;
        this.alertingUnit = alertingUnit;
    }


    private void resetWakeTimer(float deltaTime)
    {
        if(wakeUpResetTimer < 0)
        {
            wakeUpResetTimer = wakeUpResetTimerMax;
            wakeUpTimer = wakeUpTimeMax;
        }
        else
        {
            wakeUpResetTimer -= deltaTime;
        }
    }


    private void resetLowerGuardTimer(float deltaTime)
    {
        if(lowerGuardResetTimer < 0)
        {
            lowerGuardResetTimer = lowerGuardResetTimerMax;
            lowerGuardTimer = lowerGuardTimeMax;
        }
        else
        {
            lowerGuardResetTimer -= deltaTime;
        }    
    }
}
