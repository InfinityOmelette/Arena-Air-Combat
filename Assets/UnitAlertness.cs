using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAlertness : MonoBehaviour
{

    public float wakeUpTimeMax = 8f;
    private float wakeUpTimer;


    public float lowerGuardTimeMax = 12f;
    private float lowerGuardTimer;

    public bool isAlert;

    private bool beginLoweringGuard;
    private bool beginWakingUp;


    public float lowerGuardResetTimerMax = .5f;
    private float lowerGuardResetTimer;

    public float wakeUpResetTimerMax = 3f;
    private float wakeUpResetTimer;


    //public GameObject alertingUnit;

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
                wakeUpTimer -= deltaTime; // slowly become alert
            }

        }
        else if (!isAlert) // no target locked and not alert yet --> reset wake timer
        {
            resetWakeTimer(deltaTime);
        }


        return isAlert;
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

    public void beginChangingAlertStatus(bool alertingUnitPresent)
    {
        beginLowerGuard(!alertingUnitPresent);
        beginWake(alertingUnitPresent);
    }

    public void beginLowerGuard(bool lowerGuard)
    {
        beginLoweringGuard = lowerGuard;
    }

    public void beginWake(bool doWake)
    {
        beginWakingUp = doWake;
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
