using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionAI : MonoBehaviour
{
    public CamManipulation camRef;

    public RealFlightControl flight;

    public CombatFlow myFlow;

    public Vector3 targetDir;
    private Vector3 currentDir;
    public float targetDirLerpRate;

    private float rollCommand;
    private float effectiveRollCommand;

    public float currentBankAngle;
    public float zAxizAngle;

    hudControl hudRef;
    GameObject aimpointIconRef;
    public WheelsControl wheels;

    public bool isApplied;

    //public float maxErrorAngle;
    public float angVelDerivativeGain;
    public float angVelErrorScalar;
    public float angVelCorrectionScalar;
    //public float aiRollDerivativeGain;

    public float inputTransferMargin;

    public float controllerPitch;
    public float controllerYaw;
    public float controllerRoll;

    public float rudderRollOverrideFactor;

    public bool freeLookOn;

    private Rigidbody myRb;

    private Vector3 prevCorrectionTorque;

    //public float aiYawBiasCoeff;
    //public float maxYawErrorForRoll;
    public float maxRollAngularVel;
    public float maxRollRateError;
    public float rollRateGain;
    //private float prevRollError;
    private float prevRollRateError;

    //public float maxAutoLevelErrorAngle;
    //public float maxAutoLevelTorque;

    public float pitchLerpOverride;
    private float prevPitch;

    public float rudderLerpOverride;
    private float prevRudder;

    public float rollLerpOverride;
    private float prevRoll;


    private AI_Aircraft ai;

    public bool useAi;

    private Vector3 mouseDir;

    void Awake()
    {
        flight = GetComponent<RealFlightControl>();
        myFlow = GetComponent<CombatFlow>();
        myRb = GetComponent<Rigidbody>();
        ai = GetComponent<AI_Aircraft>();
    }


    // Start is called before the first frame update
    void Start()
    {
        hudRef = hudControl.mainHud.GetComponent<hudControl>();
        aimpointIconRef = hudRef.wtAimpointObj;

        
    }

    void Update()
    {
        if (myFlow.isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                toggleAi();
            }


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
                    camRef.worldLockedLookDirection = targetDir;
                }
            }

            if (myFlow.isLocalPlayer && camRef != null && !freeLookOn)
            {
                //inputDir = Vector3.Lerp( inputDir.normalized, camRef.worldLockedLookDirection.normalized, targetDirLerpRate * Time.deltaTime).normalized;
                mouseDir = camRef.worldLockedLookDirection;
            }

            if (camRef.warThunderCamEnabled)
            {
                hudRef.drawItemOnScreen(aimpointIconRef, camRef.camRef.transform.position + mouseDir, 1.0f);
            }

        }



        if (myFlow.isLocalPlayer)
        {
            if (useAi)
            {
                ai.targetDir = mouseDir;
            }
            else
            {
                targetDir = mouseDir;
            }
        }

        

        currentDir = Vector3.Lerp(currentDir.normalized, targetDir.normalized, targetDirLerpRate * Time.deltaTime).normalized;

    }

    
    private void toggleAi()
    {
        useAi = !useAi;

        ai.enabled = useAi;

        Debug.Log("Ai Enabled: " + useAi);

        //if (useAi)
        //{
        //    ai.enabled = true;
        //}
    }

    void FixedUpdate()
    {

        currentBankAngle = Quaternion.ToEulerAngles(transform.rotation).z * Mathf.Rad2Deg;

        //Debug.Log("Current angular velocity: " + myRb.angularVelocity.magnitude * Mathf.Rad2Deg);
        
        if (isApplied)
        {
            applyCorrectionTorque(currentDir);
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
        //commandDir = ai.groundAvoid(commandDir);

        
        //float currentAngleError = Vector3.Angle(transform.forward, commandDir); // degrees

        // already discludes any roll component -- cross is perpendicular to forward
        Vector3 targetAngularVelocity = Vector3.Cross(transform.forward, commandDir) * angVelErrorScalar; 

        // do angular velocity setting here
        Vector3 currentAngularVel_NoRoll = Vector3.ProjectOnPlane( myRb.angularVelocity, transform.forward); // remove roll component

        Vector3 correctiveTorqueVect = (targetAngularVelocity - currentAngularVel_NoRoll) * angVelCorrectionScalar;

        // DERIVATIVE GAIN
        //  the RATE OF CHANGE of the correction torque
        Vector3 errorRate = (correctiveTorqueVect - prevCorrectionTorque) * angVelDerivativeGain * Time.fixedDeltaTime;

        prevCorrectionTorque = correctiveTorqueVect; // discluding derivative gain

        correctiveTorqueVect += errorRate;

        // Convert to yaw/pitch inputs, -1.0 to 1.0
        correctiveTorqueVect = transform.InverseTransformDirection(correctiveTorqueVect);


        float rawPitch = Mathf.Clamp(correctiveTorqueVect.x, -1.0f, 1.0f);

        //correctiveTorqueVect.x = Mathf.Clamp(correctiveTorqueVect.x, -1.0f, 1.0f);
        //correctiveTorqueVect.y = Mathf.Clamp(correctiveTorqueVect.y, -1.0f, 1.0f);
        //correctiveTorqueVect.z = Mathf.Clamp(correctiveTorqueVect.z, -1.0f, 1.0f);

        if (correctiveTorqueVect.magnitude > 1.0f)
        {
            correctiveTorqueVect = correctiveTorqueVect.normalized;
        }

        correctiveTorqueVect.x = rawPitch;

        //Debug.Log("Corrective torque vect: " + correctiveTorqueVect + ", magnitude: " + correctiveTorqueVect.magnitude);

        float aiPitch = correctiveTorqueVect.x; //  Mathf.Lerp(prevAiPitch, correctiveTorqueVect.x, aiPitchLerp * Time.fixedDeltaTime);
        float aiYaw = correctiveTorqueVect.y;


        //float autoLevelMod = Mathf.Clamp(currentBankAngle / maxAutoLevelErrorAngle, -1.0f, 1.0f) * maxAutoLevelTorque;

        //Vector3 localDirLateral = Vector3.ProjectOnPlane(commandDir, transform.up);
        //float yawAngle = Vector3.Angle(localDirLateral, transform.forward ) * Mathf.Sign(correctiveTorqueVect.y);
        //float yawError = Mathf.Clamp(yawAngle / maxYawErrorForRoll, -1.0f, 1.0f);

        Vector3 rollRateVect = Vector3.Project(myRb.angularVelocity, transform.forward);
        float currentRollRate = rollRateVect.magnitude * Mathf.Sign(rollRateVect.z);
        float targetRollRate = correctiveTorqueVect.y * maxRollAngularVel;
        float rollRateError = Mathf.Clamp( (targetRollRate - currentRollRate) / maxRollRateError, -1.0f, 1.0f);
        float rollRateDeriv = (rollRateError - prevRollRateError) * rollRateGain * Time.fixedDeltaTime;
        prevRollRateError = rollRateError;

        //float aiYawBias = aiYaw * aiYawBiasCoeff;

        
        //Debug.Log("autoLevelTorque: " + autoLevelMod);

        float aiRoll = Mathf.Clamp(rollRateError + rollRateDeriv, -1.0f, 1.0f);

        //Debug.Log("CurrentRollRate: " + currentRollRate + ", TargetRollRate: " + targetRollRate +
        //    ", RollRateError: " + rollRateError + ", RollRateDeriv: " + rollRateDeriv +
        //    "aiRoll: " + aiRoll);

        //float rollGain = (aiRoll - prevRollError) * aiRollDerivativeGain * Time.fixedDeltaTime;
        //aiRoll = Mathf.Clamp(aiRoll + rollGain, -1.0f, 1.0f);

        

        // YAW - controller overrides yaw and roll
        if (Mathf.Abs(controllerYaw) > inputTransferMargin)
        {
            aiYaw = controllerYaw;
            aiRoll = aiRoll * rudderRollOverrideFactor;
        }
        else
        {
            aiYaw = Mathf.Lerp(prevRudder, aiYaw, rudderLerpOverride);
            prevRudder = aiYaw;
        }

        // PITCH - controller overrides pitch and roll
        if (Mathf.Abs(controllerPitch) > inputTransferMargin)
        {
            aiPitch = controllerPitch;
            aiRoll = controllerRoll;
        }
        else // if using WarThunder aim
        {
            aiPitch = Mathf.Lerp(prevPitch, aiPitch, pitchLerpOverride);
            prevPitch = aiPitch;

            flight.effective_pitch = aiPitch;
        }

        // ROLL - controller overrides roll only
        if (Mathf.Abs(controllerRoll) > inputTransferMargin)
        {
            aiRoll = controllerRoll; // even if yaw created roll input, this will happen later, thereby overriding
        }
        else
        {
            aiRoll = Mathf.Lerp(prevRoll, aiRoll, rollLerpOverride);
            prevRoll = aiRoll;
            flight.effective_roll = aiRoll;
        }

        //flight.input_roll = Mathf.Clamp(Mathf.Sign(correctiveTorqueVect.y) * Vector3.Angle(transform.up, correctiveTorqueVect) / maxErrorAngle,
        //    0.0f, 1.0f);

        


        flight.input_pitch = aiPitch;
        flight.input_yaw = aiYaw;
        flight.input_roll = aiRoll;
        

        
        //flight.effective_yaw = aiYaw;
        //flight.effective_roll = aiRoll;

        if (wheels != null)
        {
            wheels.input_rudderAxis = aiYaw;
        }
    }

    void OnDestroy()
    {
        if (myFlow.isLocalPlayer)
        {
            hudRef.setWarThunderIndOn(false);
        }
    }
}
