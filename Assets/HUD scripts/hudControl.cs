using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hudControl : MonoBehaviour
{

    public static GameObject mainHud;

    // TEXT OBJECTS
    public Text speedText;
    public Text altitudeText;
    public Text throttleText;
    public Text climbText;
    public Text fuelAmtText;
    public Text burnAvailText;
    public Text airDensityText;
    public Text hpText;




    // HP BAR VARIABLES
    public GameObject hpBarCenterpointRef;
    public GameObject hpBarParentRef;
    private Vector3 hpTextOriginPos;
    public float hpTextMaxOffset;

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

    public float velocityVectorMinSpeed;

    public WeaponIndicatorManager weaponIndicatorManager;

    // REFERENCES
    public GameObject aircraftRootObj;
    public WheelsControl wheelControllerInfo;
    public GameObject rwrIconContainer;
    public MapManager mapManager;
    private Rigidbody root_rbRef;
    private RealFlightControl root_flightInfoObjRef;
    private EngineControl root_Engine;
    private CombatFlow root_combatFlow;

    public bool startVisible;

    public GameObject rwrIconPrefab;

    public GunLeadReticle reticle;
    public GunLeadReticle reticle2;

    public DropSightReticle dropReticle;

    public CNN_UI cnnUI;

    public GameObject radOffIndicator;

    public float velVectLerpRate;
    private Vector3 readVelocity;
    public float readVelLerpRate;

    public GameObject velVectAlt;

    public void setHudVisible(bool makeVisible)
    {
        Debug.LogWarning("Making hud visible: " + makeVisible);
        if (makeVisible)
        {
            transform.localScale = new Vector3(1.0f, 1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }
    private void Awake()
    {
        hudControl.mainHud = gameObject;

        // SAVE ORIGINAL POSITIONS OF UI ELEMENTS -- WILL BE MODIFIED RELATIVE TO THESE
        climbTextOriginPos = climbText.transform.localPosition;
        throttleTextOriginPos = throttleText.transform.localPosition;
        spedometerOriginPos = spedometerRef.transform.localPosition;
        altimeterOriginPos = altimeterRef.transform.localPosition;
        hpTextOriginPos = hpText.rectTransform.localPosition;


        //  MOVE TEXT TO TOP LEFT CORNER (same operation done to each)
        fuelAmtText.rectTransform.position = new Vector2(fuelAmtText.rectTransform.position.x - Screen.width / 2f, fuelAmtText.rectTransform.position.y + Screen.height / 2f);
        burnAvailText.rectTransform.position = new Vector2(burnAvailText.rectTransform.position.x - Screen.width / 2f, burnAvailText.rectTransform.position.y + Screen.height / 2f);
        airDensityText.rectTransform.position = new Vector2(airDensityText.rectTransform.position.x - Screen.width / 2f, airDensityText.rectTransform.position.y + Screen.height / 2f);


        //  MOVE HP BAR TO TOP MIDDLE
        hpBarParentRef.transform.localPosition = new Vector3(0.0f, Screen.height / 2f, 0.0f);

        // MOVE MAP DISPLAY TO LOWER RIGHT
        GameObject mapCent = mapManager.GetComponent<MapManager>().displayCenter;
        mapCent.transform.localPosition = new Vector2(mapCent.transform.localPosition.x - Screen.width / 2f,
            mapCent.transform.localPosition.y - Screen.height / 2f);


        //Debug.LogWarning("hiding initial hud");
        setHudVisible(startVisible);
    }

    // ====================================================================
    // **********************************     START     *******************
    // ====================================================================
    void Start()
    {

        


    }

    public void linkHudToAircraft(GameObject aircraftRoot)
    {
        aircraftRootObj = aircraftRoot;

        if (aircraftRoot != null)
        {

            // SET REFERENCES
            root_rbRef = aircraftRootObj.GetComponent<Rigidbody>();
            root_flightInfoObjRef = aircraftRootObj.GetComponent<RealFlightControl>();
            root_Engine = aircraftRootObj.GetComponent<EngineControl>();
            root_combatFlow = aircraftRootObj.GetComponent<CombatFlow>();
        }

        
    }



    // ====================================================================
    // *****************************     LATEUPDATE     *******************
    // ====================================================================
    void LateUpdate()
    {

        if (aircraftRootObj != null)
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
            hpText.text = Mathf.RoundToInt(root_combatFlow.getHP()).ToString() + "HP";



            processSpedometerOffset();
            processThrottleLadder();
            processClimbLadder();
            processAltimeterOffset();
            processHealthBar();



            noseAndVelIndicators();


        }

    }

    private void noseAndVelIndicators()
    {
        readVelocity = Vector3.Lerp(readVelocity, root_rbRef.velocity, readVelLerpRate * Time.deltaTime);

        // velocity vector
        if (root_rbRef.velocity.magnitude > velocityVectorMinSpeed) // only show onscreen if above minspeed
        {
            drawItemOnScreen(velocityVectorRef, Camera.main.transform.position + readVelocity.normalized, velVectLerpRate * Time.deltaTime);

            drawItemOnScreen(velVectAlt, Camera.main.transform.position + root_rbRef.velocity, 0.5f);
            //Debug.Log("Fast enough, onScreen:");
        }
        else
        {   // place behind screen if too slow
            //Debug.Log("too slow, offscreen");
            drawItemOnScreen(velocityVectorRef,
                Camera.main.transform.position - Camera.main.transform.forward, 0.5f);

            drawItemOnScreen(velVectAlt,
                Camera.main.transform.position - Camera.main.transform.forward, 0.5f);
        }

        if (cnnUI.cnnOn)
        {
            // nose indicator
            drawItemOnScreen(noseIndicatorRef, aircraftRootObj.transform.position + aircraftRootObj.transform.forward * reticle.aimPointDist, velVectLerpRate * Time.deltaTime);
        }
        else
        {
            drawItemOnScreen(noseIndicatorRef, Camera.main.transform.position + aircraftRootObj.transform.forward, velVectLerpRate * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (root_rbRef != null)
        {
            
        }
    }

    // SCALE HEALTH FILL AND OFFSET TEXT
    private void processHealthBar()
    {
        float healthScale = Mathf.Clamp(root_combatFlow.getHP() / root_combatFlow.maxHP, 0.0f, 1.0f);

        // Scale health fill to health percentage
        hpBarCenterpointRef.transform.localScale = new Vector3(healthScale, 1.0f, 1.0f);

        // Offset text as scale of max offset
        hpText.rectTransform.localPosition = hpTextOriginPos + new Vector3(healthScale * hpTextMaxOffset, 0.0f, 0.0f);

        
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
    public void drawItemOnScreen(GameObject item, Vector3 worldPosition, float lerpRate)
    {
        if (aircraftRootObj != null)
        {

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            bool onScreen = true;
            if (screenPos.z < 0) // if screenpos behind camera
                onScreen = false;

            // convert to local position on canvas -- (0,0) at center of screen
            // THIS FIXES UI STUTTERING DURING FRAME LAG
            screenPos = new Vector3(screenPos.x - Screen.width / 2,     // x
                                     screenPos.y - Screen.height / 2,   // y
                                     0.0f);                             // z

            if (onScreen)
            {
                if(item.transform.localPosition.x > Screen.width)
                {
                    lerpRate = 1.0f; // when returning onscreen, teleport instantly
                }

                // local position prevents ui stuttering
                item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, screenPos, lerpRate);
            }
            else
            {
                item.transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2);
            }
        }
            
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
