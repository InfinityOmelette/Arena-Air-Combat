using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput_Aircraft : MonoBehaviour
{
    // =========== CONTROL INPUTS -- modifies input values in output module scripts
    // ------- NO OUTPUT PROCESSING. ONLY INPUT GATHERING, AND INPUT DELIVERY TO OUTPUT MODULE

    // axis trim

    // output module components -- manually linked in editor
    //  perhaps search children for object containing these?
    public RealFlightControl flight;
    public EngineControl engine;
    public CamManipulation cam;
    public WheelsControl wheels;
    public CannonControl cannons;
    public TgtComputer tgtComputer;
    public HardpointController hardpointController;

    public float testExplosionDistance;

    private bool tgtTimerActive;
    public float tgtLookAtHoldTime; // how long to hold until timer reaches zero
    public float currentTgtHoldTime;


    public float pitchInputLerp;
    public float rollInputLerp;
    public float rudderInputLerp;


    public float gearOffset;
    public float rocketPodOffset;
    public float bombOffset;

    private void Awake()
    {
        tgtComputer = GetComponent<TgtComputer>();
    }

    // Start is called before the first frame update
    void Start()
    {

        
        
    }







    //  BUTTON DOWN PRESSES GO HERE
    void Update()
    {
        //cam.input_camLookAtButtonDown = Input.GetButtonDown("CamLookAt");

        tgtButtonProcess();
        // if button held, activate camLookAt



        cam.input_mouseLookToggleBtnDown = Input.GetKeyDown(KeyCode.P);

        hardpointController.changeButtonDown = Input.GetButtonDown("Weapon Change");
        
        


        
        processCamOffset();
    }

    private void processCamOffset()
    {
        float camOffsetVertTemp = 0f;
        float camOffsetHorizTemp = 0f;

        if (wheels.gearIsDown)
        {
            camOffsetVertTemp = gearOffset;
        }

        // if selected weapon type needs to move camera
        Hardpoint activeHardpointRef = hardpointController.getActiveHardpoint();

        if (activeHardpointRef != null)
        {
            
            BasicBomb bombScript = activeHardpointRef.weaponTypePrefab.GetComponent<BasicBomb>();
            BasicMissile missileScript = activeHardpointRef.weaponTypePrefab.GetComponent<BasicMissile>();
            RocketPod rocketPodScript = activeHardpointRef.weaponTypePrefab.GetComponent<RocketPod>();


            if (rocketPodScript != null)
                camOffsetVertTemp = rocketPodOffset;

            if (bombScript != null)
                camOffsetVertTemp = bombOffset;

        }
        

        cam.camAxisTargetOffset_Vert = camOffsetVertTemp;
        cam.camAxisTargetOffset_Horiz = camOffsetHorizTemp;
    }

    // Press and quick release changes target
    // Press and hold looks at current target
    // Release will always disable lookAt
    private void tgtButtonProcess()
    {

        if (Input.GetButtonDown("CamLookAt")) // button pressed, start timer
        {
            tgtTimerActive = true;
            currentTgtHoldTime = tgtLookAtHoldTime;
        }


        if (tgtTimerActive)
        {
            
            if (currentTgtHoldTime >= 0f) // tick timer down
            {
                currentTgtHoldTime -= Time.deltaTime;
            }
            else  // time run out, but still active, so look at target
            {
                //Debug.Log("Looking at: " + tgtComputer.currentTarget);
                if (tgtComputer.currentTarget != null)
                {
                    cam.lookAtObj = tgtComputer.currentTarget.gameObject;
                    cam.setLookAt(true);
                }
                tgtTimerActive = false;
            }
        }
        
        

        if (Input.GetButtonUp("CamLookAt")) // button released, check time and either change target or look at target
        {
            //Debug.Log("Button released at: " + currentTgtHoldTime + " seconds remain");
            tgtTimerActive = false;
            cam.setLookAt(false);
            if(currentTgtHoldTime > 0) // timer did not reach zero, select new target
            {
                tgtComputer.tgtButtonUp = true;

            }
        }
        else // disable all when nothing is released this frame
        {
            tgtComputer.tgtButtonUp = false;

        }

        //tgtComputer.tgtButtonUp = Input.GetButtonUp("CamLookAt");
    }


    //  CONTINUOUS AXIS INPUTS GO HERE
    private void FixedUpdate()
    {
        float yaw = Input.GetAxis("Rudder");
        float throttle = Input.GetAxis("Throttle");

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // HARDPOINT CONTROLLER
        hardpointController.input_scrollWheel = Input.GetAxis("Scrollwheel");
        

        // FLIGHT
        flight.input_pitch = Mathf.Lerp(flight.input_pitch, Input.GetAxis("Pitch"), pitchInputLerp);
        flight.input_yaw = Mathf.Lerp(flight.input_yaw, yaw, rudderInputLerp);
        flight.input_roll = Mathf.Lerp(flight.input_roll, Input.GetAxis("Roll"), rollInputLerp);

        // ENGINE
        engine.input_throttleAxis = throttle;
        

        // WHEELS
        wheels.input_brakeAxis = throttle;
        wheels.input_dPadHoriz = Input.GetAxis("D-Pad Horiz");
        wheels.input_rudderAxis = Mathf.Lerp(wheels.input_rudderAxis, yaw, rudderInputLerp);

        // CAMERA
        //cam.input_freeLookHoriz = Mathf.Lerp(cam.input_freeLookHoriz, Input.GetAxis("CamLookX"), cam.freeLookLerpRate);
        //cam.input_freeLookVert = Mathf.Lerp(cam.input_freeLookVert, Input.GetAxis("CamLookY"), cam.freeLookLerpRate);
        cam.input_freeLookHoriz = Input.GetAxis("CamLookX");
        cam.input_freeLookVert = Input.GetAxis("CamLookY");
        cam.input_mouseSpeedX = mouseX;
        cam.input_mouseSpeedY = mouseY;
        

        // CANNONS
        cannons.cannonInput = Input.GetAxis("Cannon");


        launchManagement();


    }

    // CALL FROM FIXEDUPDATE -- determine precise physics update when button pressed
    void launchManagement()
    {
        // button pressed, but no launch active
        if (Input.GetButton("Weapon Launch") && !hardpointController.launchActive)
        {
            // launchprocess
            hardpointController.launchProcess();
        }

        // button NOT pressed, but launch IS active
        if(!Input.GetButton("Weapon Launch") && hardpointController.launchActive)
        {
            // end the launch
            hardpointController.launchEndProcess();
        }
    }
}
