using System.Collections;
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


    // SPEDOMETER OFFSET
    public GameObject spedometerRef;
    private Vector3 spedometerOriginPos;
    public float spedometerMaxSpeed;
    public float spedometerMaxOffset;


    //  ALTIMETER OFFSET
    public GameObject altimeterRef;
    private Vector3 altimeterOriginPos;
    public float altimeterMaxOffset;
    public float altimeterMaxAlt;


    // NOSE INDICATOR
    public GameObject noseIndicatorRef;

    // VELOCITY VECTOR
    public GameObject velocityVectorRef;



    // REFERENCES
    private Rigidbody rbRef;
    private RealFlightControl flightInfoObjRef;
    private Camera cam;


    // Start is called before the first frame update
    void Start()
    {
        // SET REFERENCES
        rbRef = GetComponent<Rigidbody>();
        flightInfoObjRef = GetComponent<RealFlightControl>();
        cam = Camera.main;


        // SAVE ORIGINAL POSITIONS OF UI ELEMENTS -- WILL BE MODIFIED RELATIVE TO THESE
        climbTextOriginPos = climbText.transform.localPosition;
        throttleTextOriginPos = throttleText.transform.localPosition;
        spedometerOriginPos = spedometerRef.transform.localPosition;
        altimeterOriginPos = altimeterRef.transform.localPosition;
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

        processSpedometerOffset();
        processThrottleLadder();
        processClimbLadder();
        processAltimeterOffset();

        // nose indicator
        drawItemOnScreen(noseIndicatorRef, cam.transform.position + transform.forward);

        // velocity vector
        drawItemOnScreen(velocityVectorRef, cam.transform.position + rbRef.velocity.normalized);

    }


    




    // Place item onto screen from world point
    private void drawItemOnScreen(GameObject item, Vector3 worldPosition)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);
        if (screenPos.z < 0) // if screenpos behind camera
            item.SetActive(false);
        else    // if in front of camera
            item.SetActive(true);

        // convert to local position on canvas -- (0,0) at center of screen
        // THIS FIXES UI STUTTERING DURING FRAME LAG
        screenPos = new Vector3(screenPos.x - Screen.width / 2,     // x
                                 screenPos.y - Screen.height / 2,   // y
                                 0.0f);                             // z

        // local position prevents ui stuttering
        item.transform.localPosition = screenPos;
    }


   

    // PROCESS SPEDOMETER OFFSET
    //  - Raises speed indicator from bottom of slider as speed increases
    //  - Stops at top, at or above maximum speed
    private void processSpedometerOffset()
    {
        float speedScale = Mathf.Clamp(rbRef.velocity.magnitude / spedometerMaxSpeed, 0.0f, 1.0f);
        spedometerRef.transform.localPosition = spedometerOriginPos + new Vector3(0.0f, speedScale * spedometerMaxOffset, 0.0f);

    }


    //  PROCESS ALTIMETER OFFSET
    //   - Raises altimeter from bottom of slider as alt increases
    private void processAltimeterOffset()
    {
        float altScale = Mathf.Clamp(transform.position.y / altimeterMaxAlt, 0.0f, 1.0f);
        altimeterRef.transform.localPosition = altimeterOriginPos + new Vector3(0.0f, altScale * altimeterMaxOffset, 0.0f);
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
