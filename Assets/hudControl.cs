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
    public Text fuelAmtText;
    public Text burnAvailText;
    public Text airDensityText;

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
    public GameObject aircraftRootObj;
    public WheelsControl wheelControllerInfo;
    private Rigidbody root_rbRef;
    private RealFlightControl root_flightInfoObjRef;
    private EngineControl root_Engine;
    private Camera cam;


    // Start is called before the first frame update
    void Start()
    {
        // SET REFERENCES
        root_rbRef = aircraftRootObj.GetComponent<Rigidbody>();
        root_flightInfoObjRef = aircraftRootObj.GetComponent<RealFlightControl>();
        root_Engine = aircraftRootObj.GetComponent<EngineControl>();
        cam = Camera.main;


        // SAVE ORIGINAL POSITIONS OF UI ELEMENTS -- WILL BE MODIFIED RELATIVE TO THESE
        climbTextOriginPos = climbText.transform.localPosition;
        throttleTextOriginPos = throttleText.transform.localPosition;
        spedometerOriginPos = spedometerRef.transform.localPosition;
        altimeterOriginPos = altimeterRef.transform.localPosition;


        //  MOVE TEXT TO TOP LEFT CORNER (same operation done to each)
        fuelAmtText.rectTransform.position = new Vector2(fuelAmtText.rectTransform.position.x - Screen.width/2f, fuelAmtText.rectTransform.position.y + Screen.height/2f);
        burnAvailText.rectTransform.position = new Vector2(burnAvailText.rectTransform.position.x - Screen.width / 2f, burnAvailText.rectTransform.position.y + Screen.height / 2f);
        airDensityText.rectTransform.position = new Vector2(airDensityText.rectTransform.position.x - Screen.width / 2f, airDensityText.rectTransform.position.y + Screen.height / 2f);

    }

    

    
    void LateUpdate()
    {

        // Set readout values
        float mpsVelToKPH = 3.6f;
        throttleText.text = writeThrottleText();
        speedText.text = Mathf.RoundToInt(root_rbRef.velocity.magnitude * mpsVelToKPH).ToString() + "kph";
        altitudeText.text = Mathf.RoundToInt(aircraftRootObj.transform.position.y).ToString() + "m";
        climbText.text = Mathf.RoundToInt(root_flightInfoObjRef.readVertVelocity).ToString() + "m/s >";
        airDensityText.text = "AIR DENSITY: " + Mathf.RoundToInt(root_Engine.currentAirDensity * 100f).ToString() + "%";
        fuelAmtText.text = "FUEL: " + Mathf.RoundToInt(root_Engine.currentFuelMass) + "kg";
        burnAvailText.text = "BURN AVAIL: " + Mathf.RoundToInt(root_Engine.currentBurnMod * 100f).ToString() + "%";
        

        
        processSpedometerOffset();
        processThrottleLadder();
        processClimbLadder();
        processAltimeterOffset();

        // nose indicator
        drawItemOnScreen(noseIndicatorRef, cam.transform.position + aircraftRootObj.transform.forward, 0.5f);

        // velocity vector
        drawItemOnScreen(velocityVectorRef, cam.transform.position + root_rbRef.velocity.normalized, 0.5f);

    }

    // Either show throttle percentage or if brakes applied
    private string writeThrottleText()
    {
        string returnString;
        // set throttle text readout based on wheelcontroller info
        if (wheelControllerInfo != null && wheelControllerInfo.brakeCurrentlyApplied) // if brake applied
            returnString = "< BRK";
        else // if brake is not applied
            returnString = "< " + Mathf.RoundToInt(root_Engine.currentThrottlePercent).ToString() + "%";

        return returnString;
    }
    




    // Place item onto screen from world point
    private void drawItemOnScreen(GameObject item, Vector3 worldPosition, float lerpRate)
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
        item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, screenPos, lerpRate);
    }


   

    // PROCESS SPEDOMETER OFFSET
    //  - Raises speed indicator from bottom of slider as speed increases
    //  - Stops at top, at or above maximum speed
    private void processSpedometerOffset()
    {
        float speedScale = Mathf.Clamp(root_rbRef.velocity.magnitude / spedometerMaxSpeed, 0.0f, 1.0f);
        spedometerRef.transform.localPosition = spedometerOriginPos + new Vector3(0.0f, speedScale * spedometerMaxOffset, 0.0f);

    }


    //  PROCESS ALTIMETER OFFSET
    //   - Raises altimeter from bottom of slider as alt increases
    private void processAltimeterOffset()
    {
        float altScale = Mathf.Clamp(aircraftRootObj.transform.position.y / altimeterMaxAlt, 0.0f, 1.0f);
        altimeterRef.transform.localPosition = altimeterOriginPos + new Vector3(0.0f, altScale * altimeterMaxOffset, 0.0f);
    }
    

    // PROCESS THROTTLE LADDER
    //  - scales throttle fill from 0 to 1
    //  - positions text to fill position
    private void processThrottleLadder()
    {
        // get throttle and thrust decimal
        float thrustScale = root_Engine.currentBaseThrustPercent / 100f;      // give thrust decimal from 0 to 1
        float throttleScale = root_Engine.currentThrottlePercent / 100f;  // give throttle decimal from 0 to 1

        // scale Thrust ladder
        throttleLadderCenterpointRef.transform.localScale = new Vector3(throttleLadderCenterpointRef.transform.localScale.x,
            thrustScale,
            throttleLadderCenterpointRef.transform.localScale.y);

        // Offset Throttle text
        throttleText.transform.localPosition = throttleTextOriginPos + new Vector3(0.0f, throttleScale * throttleTextMaxOffset, 0.0f);

    }


    // PROCESS CLIMB LADDER
    //  - scales climb fill from -1 to 1
    //  - positions text to fill position
    private void processClimbLadder()
    {

        float climbScale = Mathf.Clamp(root_flightInfoObjRef.readVertVelocity / climbLadderMax, -1f, 1f);

        // Scale climb ladder -- only changing Y scale but unity wants whole new vector cause it's bad
        climbLadderCenterpointRef.transform.localScale = new Vector3(climbLadderCenterpointRef.transform.localScale.x,  // x scale constant
            climbScale,       // change y scale
            climbLadderCenterpointRef.transform.localScale.z);  // z scale constant

        // Offset Climb text
        climbText.transform.localPosition = climbTextOriginPos + new Vector3(0.0f, climbScale * climbTextMaxOffset, 0.0f);
    }


}
