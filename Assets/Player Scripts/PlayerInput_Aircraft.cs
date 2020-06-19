using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput_Aircraft : MonoBehaviourPunCallbacks
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
    public Radar myRadar;

    private CombatFlow myFlow;

    public float testExplosionDistance;

    private bool tgtTimerActive;
    public float tgtLookAtHoldTime; // how long to hold until timer reaches zero
    public float currentTgtHoldTime;


    public float pitchInputLerp;
    public float rollInputLerp;
    public float rudderInputLerp;


    public float gearOffset;
    public float maverickOffset;
    public float rocketPodOffset;
    public float bombOffset;

    public bool isReady = false;

    private void Awake()
    {
        tgtComputer = GetComponent<TgtComputer>();
        myFlow = GetComponent<CombatFlow>();
        myRadar = GetComponent<Radar>();
    }

    // Start is called before the first frame update
    void Start()
    {

        
        
    }

    //  BUTTON DOWN PRESSES GO HERE
    void Update()
    {
        if (isReady && myFlow.isLocalPlayer)
        {

            float throttle = Input.GetAxis("Throttle");

            // ENGINE
            engine.input_throttleAxis = throttle;
            engine.input_scrollWheelAxis = Input.GetAxis("Scrollwheel");

            // WHEEL BRAKES
            wheels.input_brakeAxis = throttle;

            //cam.input_camLookAtButtonDown = Input.GetButtonDown("CamLookAt");

            tgtButtonProcess();
            // if button held, activate camLookAt

            bool pBtnDown = Input.GetKeyDown(KeyCode.P);

            cam.input_mouseLookToggleBtnDown = pBtnDown;
            hardpointController.input_mouseLookToggleBtnDown = pBtnDown;

            hardpointController.input_changeWeaponAxis = Input.GetAxis("Weapon Change");
            getWeaponSelectNumber();

            processCamOffset();


            if(Input.GetButtonDown("Radar Toggle"))
            {
                Debug.LogWarning("Toggling radar");
                myRadar.toggleRadar();
            }

        }
    }

    private void processCamOffset()
    {
        if (isReady && myFlow.isLocalPlayer)
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

                if (activeHardpointRef.weaponTypePrefab.name.Equals("Maverick"))
                    camOffsetVertTemp = maverickOffset;

            }


            cam.camAxisTargetOffset_Vert = camOffsetVertTemp;
            cam.camAxisTargetOffset_Horiz = camOffsetHorizTemp;
        }
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
        if (myFlow.isLocalPlayer)
        {
            float yaw = Input.GetAxis("Rudder");
            

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");




            // FLIGHT
            flight.input_pitch = Mathf.Lerp(flight.input_pitch, Input.GetAxis("Pitch"), pitchInputLerp);
            flight.input_yaw = Mathf.Lerp(flight.input_yaw, yaw, rudderInputLerp);
            flight.input_roll = Mathf.Lerp(flight.input_roll, Input.GetAxis("Roll"), rollInputLerp);


            // WHEELS
            wheels.input_gear_button = Input.GetAxis("Gear");
            wheels.input_rudderAxis = Mathf.Lerp(wheels.input_rudderAxis, yaw, rudderInputLerp);

            // CAMERA
            //cam.input_freeLookHoriz = Mathf.Lerp(cam.input_freeLookHoriz, Input.GetAxis("CamLookX"), cam.freeLookLerpRate);
            //cam.input_freeLookVert = Mathf.Lerp(cam.input_freeLookVert, Input.GetAxis("CamLookY"), cam.freeLookLerpRate);
            cam.input_freeLookHoriz = Input.GetAxis("CamLookX");
            cam.input_freeLookVert = Input.GetAxis("CamLookY");
            cam.input_mouseSpeedX = mouseX;
            cam.input_mouseSpeedY = mouseY;


            // CANNONS
            if(Input.GetAxis("Cannon") > 0.5f)// cannon button is definitely pressed
            {
                // we need to send value to cannon
                if (!cannons.cannonInput) 
                {
                    photonView.RPC("rpcActivateCannons", RpcTarget.All, true);
                }
            }
            else // cannon button is definitely released
            {
                // we need to send value to cannon
                if (cannons.cannonInput)
                {
                    photonView.RPC("rpcActivateCannons", RpcTarget.All, false);
                }

            }


            launchManagement();

        }
    }

    [PunRPC]
    private void rpcActivateCannons(bool doActivate)
    {
        cannons.cannonInput = doActivate;
    }

    void getWeaponSelectNumber()
    {
        short selectIndex = -1; // -1 for no change by default if none pressed
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetAxis("D-Pad Vert") > 0.5f)
        {
            selectIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxis("D-Pad Horiz") > 0.5f)
        {
            selectIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxis("D-Pad Vert") < -0.5f)
        {
            selectIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetAxis("D-Pad Horiz") < -0.5f)
        {
            selectIndex = 3;
        }
        // there will only ever be 4 options


        if (selectIndex != -1) 
        {
            hardpointController.setWeaponType(selectIndex);
        }
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
