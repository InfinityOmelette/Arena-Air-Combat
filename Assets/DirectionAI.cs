using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionAI : MonoBehaviour
{
    public CamManipulation camRef;

    public RealFlightControl flight;

    public CombatFlow myFlow;

    public Vector3 inputDir;

    private float rollCommand;
    private float effectiveRollCommand;

    public float currentBankAngle;
    public float zAxizAngle;

    hudControl hudRef;
    GameObject aimpointIconRef;

    public bool isApplied;

    public float maxErrorAngle;

    public float inputTransferMargin;

    public float controllerPitch;
    public float controllerYaw;
    public float controllerRoll;


    public bool freeLookOn;

    void Awake()
    {
        flight = GetComponent<RealFlightControl>();
        myFlow = GetComponent<CombatFlow>();
    }


    // Start is called before the first frame update
    void Start()
    {
        hudRef = hudControl.mainHud.GetComponent<hudControl>();
        aimpointIconRef = hudRef.wtAimpointObj;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            camRef.toggleWarThunderCam();
            hudRef.setWarThunderIndOn(camRef.warThunderCamEnabled);
            isApplied = camRef.warThunderCamEnabled;
        }

        if (isApplied)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                freeLookOn = true;
                hudRef.screenCenterObj.SetActive(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                freeLookOn = false;
                hudRef.screenCenterObj.SetActive(false);
                camRef.worldLockedLookDirection = inputDir;
            }
        }
        

        if (myFlow.isLocalPlayer && camRef != null && !freeLookOn)
        {
            inputDir = camRef.worldLockedLookDirection;
        }

        if (camRef.warThunderCamEnabled)
        {
            hudRef.drawItemOnScreen(aimpointIconRef, camRef.camRef.transform.position + inputDir, 1.0f);
        }

        
    }

    
    void FixedUpdate()
    {
        

        currentBankAngle = Quaternion.ToEulerAngles(transform.rotation).z * Mathf.Rad2Deg;

        if (isApplied)
        {
            applyCorrectionTorque(inputDir);
        }
        else
        {
            flight.input_pitch = controllerPitch;
            flight.input_yaw = controllerYaw;
            flight.input_roll = controllerRoll;
        }

    }

    private void applyCorrectionTorque(Vector3 commandDir)
    {
        float currentVelocityErrorAngle = Vector3.Angle(transform.forward, commandDir); // degrees

        Vector3 correctiveTorqueVect =
            Vector3.ProjectOnPlane(Vector3.Cross(transform.forward, commandDir), transform.forward).normalized * // torque direction
            (Mathf.Min(currentVelocityErrorAngle / maxErrorAngle, 1.0f));  // torque magnitude



        // Convert to yaw/pitch inputs, -1.0 to 1.0
        correctiveTorqueVect = transform.InverseTransformDirection(correctiveTorqueVect);


        // PITCH
        if(Mathf.Abs(controllerPitch) < inputTransferMargin)
        {
            flight.input_pitch = correctiveTorqueVect.x;
        }
        else
        {
            flight.input_pitch = controllerPitch;
        }

        // YAW
        if(Mathf.Abs(controllerYaw) < inputTransferMargin)
        {
            flight.input_yaw = correctiveTorqueVect.y;
        }
        else
        {
            flight.input_yaw = controllerYaw;
        }

        // ROLL
        if(Mathf.Abs(controllerRoll) < inputTransferMargin)
        {
            flight.input_roll = correctiveTorqueVect.y;
        }
        else
        {
            flight.input_roll = controllerRoll;
        }
        //flight.input_roll = Mathf.Clamp(Mathf.Sign(correctiveTorqueVect.y) * Vector3.Angle(transform.up, correctiveTorqueVect) / maxErrorAngle,
        //    0.0f, 1.0f);

    }

    void OnDestroy()
    {
        hudRef.setWarThunderIndOn(false);
    }



}
