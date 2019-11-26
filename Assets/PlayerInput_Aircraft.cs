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


    // Start is called before the first frame update
    void Start()
    {

        
        
    }





    //  BUTTON DOWN PRESSES GO HERE
    void Update()
    {
        cam.input_camLookAtButtonDown = Input.GetButtonDown("CamLookAt");

    }


    //  CONTINUOUS AXIS INPUTS GO HERE
    private void FixedUpdate()
    {
        float yaw = Input.GetAxis("Rudder");
        float throttle = Input.GetAxis("Throttle");

        // FLIGHT
        flight.input_pitch = Input.GetAxis("Pitch");
        flight.input_yaw = yaw;
        flight.input_roll = Input.GetAxis("Roll");

        // ENGINE
        engine.input_throttleAxis = throttle;


        // WHEELS
        wheels.input_brakeAxis = throttle;
        wheels.input_dPadHoriz = Input.GetAxis("D-Pad Horiz");
        wheels.input_rudderAxis = yaw;

        // CAMERA
        cam.input_freeLookHoriz = Input.GetAxis("CamLookX");
        cam.input_freeLookVert = Input.GetAxis("CamLookY");

    }
}
