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
    public WheelsControl wheels;

    public bool isApplied;

    public float maxErrorAngle;
    public float angVelDerivativeGain;
    public float angVelErrorScalar;
    public float angVelCorrectionScalar;
    

    public float inputTransferMargin;

    public float controllerPitch;
    public float controllerYaw;
    public float controllerRoll;

    public float rudderRollOverrideFactor;

    public bool freeLookOn;

    private Rigidbody myRb;

    private Vector3 prevCorrectionTorque;

    void Awake()
    {
        flight = GetComponent<RealFlightControl>();
        myFlow = GetComponent<CombatFlow>();
        myRb = GetComponent<Rigidbody>();
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

        

        //Debug.Log("Current angular velocity: " + myRb.angularVelocity.magnitude * Mathf.Rad2Deg);

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
        //float currentAngleError = Vector3.Angle(transform.forward, commandDir); // degrees


        Vector3 targetAngularVelocity = Vector3.Cross(transform.forward, commandDir) * angVelErrorScalar; 

        // do angular velocity setting here
        Vector3 currentAngularVel_NoRoll = Vector3.ProjectOnPlane( myRb.angularVelocity, transform.forward); // remove roll component

        Vector3 correctiveTorqueVect = (targetAngularVelocity - currentAngularVel_NoRoll) * angVelCorrectionScalar;

        // DERIVATIVE GAIN
        //  the RATE OF CHANGE of the correction torque
        Vector3 errorRate = (correctiveTorqueVect - prevCorrectionTorque) * angVelDerivativeGain * Time.fixedDeltaTime;

        prevCorrectionTorque = correctiveTorqueVect; // discluding derivative gain

        correctiveTorqueVect += errorRate;
        

        if(correctiveTorqueVect.magnitude > 1.0f)
        {
            correctiveTorqueVect = correctiveTorqueVect.normalized;
        }


        // Convert to yaw/pitch inputs, -1.0 to 1.0
        correctiveTorqueVect = transform.InverseTransformDirection(correctiveTorqueVect);

        //Debug.Log("Corrective torque vect: " + correctiveTorqueVect + ", magnitude: " + correctiveTorqueVect.magnitude);

        float aiPitch = correctiveTorqueVect.x;
        float aiYaw = correctiveTorqueVect.y;
        float aiRoll = correctiveTorqueVect.y;

        
        // YAW - controller overrides yaw and roll
        if(Mathf.Abs(controllerYaw) > inputTransferMargin)
        {
            aiYaw = controllerYaw;
            aiRoll = aiRoll * rudderRollOverrideFactor;
        }

        // PITCH - controller overrides pitch and roll
        if (Mathf.Abs(controllerPitch) > inputTransferMargin)
        {
            aiPitch = controllerPitch;
            aiRoll = controllerRoll;
        }

        // ROLL - controller overrides roll only
        if (Mathf.Abs(controllerRoll) > inputTransferMargin)
        {
            aiRoll = controllerRoll; // even if yaw created roll input, this will happen later, thereby overriding
        }
        
        //flight.input_roll = Mathf.Clamp(Mathf.Sign(correctiveTorqueVect.y) * Vector3.Angle(transform.up, correctiveTorqueVect) / maxErrorAngle,
        //    0.0f, 1.0f);

        flight.input_pitch = aiPitch;
        flight.input_yaw = aiYaw;
        flight.input_roll = aiRoll;

        if (wheels != null)
        {
            wheels.input_rudderAxis = aiYaw;
        }
    }

    void OnDestroy()
    {
        hudRef.setWarThunderIndOn(false);
    }



}
