﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hudControl : MonoBehaviour
{

    // TEXT OBJECTS
    public Text speedText;
    public Text altitudeText;
    public Text throttleText;
    public Text climbText;

    // CLIMB LADDER VARIABLES
    public GameObject climbLadderCenterpointRef;
    private Vector3 climbTextOriginPos; // offset from original position
    public float climbTextMaxOffset; // position text to stay on ladder scale
    public float climbLadderMax; // max velocity ladder will show


    // THROTTLE LADDER VARIABLES
    public GameObject throttleLadderCenterpointRef;
    private Vector3 throttleTextOriginPos;
    public float throttleTextMaxOffset;



    // REFERENCES
    private Rigidbody rbRef;
    private RealFlightControl flightInfoObjRef;


    // Start is called before the first frame update
    void Start()
    {
        rbRef = GetComponent<Rigidbody>();
        flightInfoObjRef = GetComponent<RealFlightControl>();
        climbTextOriginPos = climbText.transform.localPosition;
        throttleTextOriginPos = throttleText.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        // Set readout values
        float mpsVelToKPH = 3.6f;
        speedText.text = Mathf.RoundToInt(rbRef.velocity.magnitude * mpsVelToKPH).ToString() + "kph";
        throttleText.text = "< " + Mathf.RoundToInt(flightInfoObjRef.currentThrustPercent).ToString() + "%";
        altitudeText.text = Mathf.RoundToInt(transform.position.y).ToString() + "m";
        climbText.text = Mathf.RoundToInt(flightInfoObjRef.readVertVelocity).ToString() + "m/s >";


        processClimbLadder();

        processThrottleLadder();

    }

    

    // PROCESS THROTTLE LADDER
    //  - scales throttle fill from 0 to 1
    //  - positions text to fill position
    private void processThrottleLadder()
    {
        // get throttle decimal
        float throttleScale = flightInfoObjRef.currentThrustPercent / 100f; // give decimal from 0 to 1

        // scale throttle ladder
        throttleLadderCenterpointRef.transform.localScale = new Vector3(throttleLadderCenterpointRef.transform.localScale.x,
            throttleScale,
            throttleLadderCenterpointRef.transform.localScale.y);

        // Offset Throttle text
        throttleText.transform.localPosition = throttleTextOriginPos + new Vector3(0.0f, throttleScale * throttleTextMaxOffset, 0.0f);

    }


    // PROCESS CLIMB LADDER
    //  - scales climb fill from -1 to 1
    //  - positions text to fill position
    private void processClimbLadder()
    {

        float climbScale = Mathf.Clamp(flightInfoObjRef.readVertVelocity / climbLadderMax, -1f, 1f);

        // Scale climb ladder -- only changing Y scale but unity wants whole new vector cause it's bad
        climbLadderCenterpointRef.transform.localScale = new Vector3(climbLadderCenterpointRef.transform.localScale.x,  // x scale constant
            climbScale,       // change y scale
            climbLadderCenterpointRef.transform.localScale.z);  // z scale constant

        // Offset Climb text
        climbText.transform.localPosition = climbTextOriginPos + new Vector3(0.0f, climbScale * climbTextMaxOffset, 0.0f);
    }


}
