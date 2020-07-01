using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManipulation : MonoBehaviour
{

    public GameObject camAxisHorizRef;
    public GameObject camAxisVertRef;
    public GameObject camAxisRollRef;
    public GameObject camRef;

    public Rigidbody aircraftRootRB;

    public float camDefaultHorizDist;
    public float camDefaultHeight;
    public float camVelocityMod;

    public float lookAheadDist;


    //public float camHeightToDistRatio;

    public float camDistOffsetMax;    // camera height will be constant
    public float estHighSpeed;

    public float estLowSpeed;


   // public float thrustModMaxDistOffset;
    public float fwdGlobalVelocityScale;

   

    public float camRotateLerpRate;
    public float lookAtLerpRate;
    public float mouseRotateLerpRate;
    private float activeRotateLerpRate;

    public float rollRateMod;
    public float rollRateOffsetLerpRate;

    private Quaternion targetLocalRotation;


    public float horizTravelMod = 120f;
    public float vertTravelMod = 80f;

    public GameObject lookAtObj;
    public bool lookAtEnabled = false;

    private Quaternion defaultCamRotation;

    // Euler, camera will point offset by these
    public float camAxisTargetOffset_Horiz;
    public float camAxisTargetOffset_Vert;

    public bool input_camLookAtButtonDown = false; // to be set by external input script
    public float input_freeLookHoriz;
    public float input_freeLookVert;


    public float input_mouseSpeedX;
    public float input_mouseSpeedY;
    public float mouse_yawRate;
    public float mouse_pitchRate;

    public float maxMouseTraverseSpeed;

    public bool mouseLookEnabled;
    public bool input_mouseLookToggleBtnDown;

    private Quaternion previousRotationTarget;

    private Vector3 camTargetLocalPos;
    public float camTargetLocalPosLerpRate;

    // Start is called before the first frame update
    void Start()
    {

        camTargetLocalPos = camRef.transform.localPosition;

        defaultCamRotation = camRef.transform.localRotation;

        //camAxisXref.transform.rotation = new Quaternion(0.0f, 180f, 0f, 0f);
    }


    void Update()
    {

        camRef.transform.localPosition = Vector3.Lerp(camRef.transform.localPosition, camTargetLocalPos, camTargetLocalPosLerpRate * Time.deltaTime);
        


        if (input_camLookAtButtonDown)
            toggleLookAt();

        if (input_mouseLookToggleBtnDown)
            mouseLookEnabled = !mouseLookEnabled;

        if (lookAtEnabled)
        {
            if (lookAtObj != null)
            {  // slightly redundant null check
                //camAxisRollRef.transform.LookAt(lookAtObj.transform.position, aircraftRootRB.transform.up);
                targetLocalRotation = Quaternion.LookRotation(
                    aircraftRootRB.transform.InverseTransformPoint(lookAtObj.transform.position),
                    Vector3.up) * (Quaternion.Inverse(defaultCamRotation));

                activeRotateLerpRate = lookAtLerpRate;


            }
            else // if look at is enabled but reference is null, re-toggle look at
                toggleLookAt();
        }
        else
        {


            // right stick to look around aircraft
            processFreeLook();
        }

        // Roll angular velocity on camera rotation
        camAxisRollRef.transform.localRotation = processAngularVelocityRotation();

        camAxisHorizRef.transform.localRotation = Quaternion.Lerp(camAxisHorizRef.transform.localRotation, targetLocalRotation, activeRotateLerpRate * Time.deltaTime);


        

        IconRWR.cameraHorizOffset = camAxisHorizRef.transform.localEulerAngles.y;

        //Debug.Log("CamAxisHoriz Y euler: " + IconRWR.cameraHorizOffset);
    }

    void FixedUpdate()
    {
        //  ========================================     VELOCITY EFFECTS ON CAMERA POSITION



        camTargetLocalPos = new Vector3(0.0f, camDefaultHeight, -camDefaultHorizDist);
        camTargetLocalPos -= camDistanceByVelocity();
        camTargetLocalPos -= camRef.transform.InverseTransformDirection(velocityGlobalForwardMinimized(fwdGlobalVelocityScale) * camVelocityMod);


    }

    
    private void velocityOffsets()
    {
        //  ========================================     VELOCITY EFFECTS ON CAMERA POSITION
        camRef.transform.localPosition = new Vector3(0.0f, camDefaultHeight, -camDefaultHorizDist);
        camRef.transform.localPosition -= camDistanceByVelocity();
        camRef.transform.localPosition -= camRef.transform.InverseTransformDirection(velocityGlobalForwardMinimized(fwdGlobalVelocityScale) * camVelocityMod);
    }

    public void setLookAt(bool setLook)
    {
        if (setLook != lookAtEnabled)
            toggleLookAt();
    }

    // toggle lookAt
    private void toggleLookAt()
    {

        lookAtEnabled = !lookAtEnabled;


        
    }

    //private Vector3 camDistanceByThrust()
    //{
    //    FlightControl fc_scriptRef = GetComponent<FlightControl>();
    //    float currentThrust = fc_scriptRef.thrust;
    //    float THRUST_MIN = fc_scriptRef.THRUST_MIN;
    //    float THRUST_MAX = fc_scriptRef.THRUST_MAX;
    //    return new Vector3(0.0f, 0.0f, 
    //        (currentThrust - THRUST_MIN) * (thrustModMaxDistOffset / THRUST_MAX));
    //}

    private Vector3 camDistanceByVelocity()
    {
        // at or below minimum speed, camera distance will be set to minimum
        // at or above maximum speed, camera distance will be set to maximum
        // in between, camera distance will scale linearly from minimum to maximum
        // only horizontal distance will be changed

        //  percentage along range
        float percentageToMaxSpeed = (aircraftRootRB.velocity.magnitude - estLowSpeed) / (estHighSpeed - estLowSpeed);

        //  clamp within range 0% to 100%
        percentageToMaxSpeed = Mathf.Clamp(percentageToMaxSpeed, 0.0f, 1.0f);

        //  set z distance to a % of max offset
        //  set y distance (height) to maintain height/distance ratio
        float horizOffset = percentageToMaxSpeed * camDistOffsetMax;
        return new Vector3(0.0f, -horizOffset * (camDefaultHeight / camDefaultHorizDist), horizOffset);
    }

    private Vector3 localizeFwdVelocity()
    {

        
        Vector3 velocityVect = aircraftRootRB.velocity;
        Vector3 zDriftVectGlobal = Vector3.Project(velocityVect, transform.forward);
        float zDriftValLocal = zDriftVectGlobal.magnitude;

        if (zDriftVectGlobal.normalized != transform.forward)
            zDriftValLocal *= -1;

        Vector3 totalDrift = new Vector3(0.0f, 0.0f, zDriftValLocal);


        return totalDrift;
    }

    private Vector3 velocityGlobalForwardMinimized(float fwdScaling)
    {
        Vector3 returnVect = aircraftRootRB.velocity;
        Vector3 fwdVelocityMod = Vector3.Project(returnVect, transform.forward);
        returnVect -= fwdVelocityMod * (1.0f - fwdScaling);
        return returnVect;
    }

    private void processFreeLook()
    {
        // prioritize stick -- if no stick input, change by mouse
        Vector3 targetLocalEuler;
        //

        // If there is no stick input..
        if (mouseLookEnabled)
        {

            activeRotateLerpRate = mouseRotateLerpRate * Time.deltaTime;

            // use mouse input to rotate camera
            targetLocalEuler = mouseFreeLookEuler(
                input_mouseSpeedX, 
                input_mouseSpeedY);

            
        }
        else // THERE IS STICK INPUT, PRIORITIZE STICK
        {
            activeRotateLerpRate = camRotateLerpRate * Time.deltaTime;

            float horizLookTarget = Mathf.Clamp(input_freeLookHoriz * horizTravelMod + camAxisTargetOffset_Horiz, -horizTravelMod, horizTravelMod);
            float vertLookTarget = Mathf.Clamp(input_freeLookVert * vertTravelMod + camAxisTargetOffset_Vert, -vertTravelMod, vertTravelMod);

            targetLocalEuler = new Vector3(vertLookTarget, horizLookTarget, 0f);
        }

        // Convert targetLocal to local quaternion
        targetLocalRotation = Quaternion.Euler(targetLocalEuler);

        previousRotationTarget = targetLocalRotation;

    }

    // read mouse input, return a euler angle
    // call from fixedUpdate
    private Vector3 mouseFreeLookEuler(float mouseSpeedX, float mouseSpeedY)
    {
        // horribly inefficient. Surely this can be done in 5 lines or fewer

        float angleOffsetHoriz = mouse_yawRate * mouseSpeedX * Time.fixedDeltaTime;
        float angleOffsetVert = -mouse_pitchRate * mouseSpeedY * Time.fixedDeltaTime;



        // NEW TARGET
        Vector3 newRotationEuler = Mathf.Rad2Deg * Quaternion.ToEulerAngles(targetLocalRotation) +
            new Vector3(angleOffsetVert, angleOffsetHoriz, 0.0f);


        newRotationEuler.x = unEulerize(newRotationEuler.x);
        newRotationEuler.y = unEulerize(newRotationEuler.y);

        // CLAMP PITCH
        newRotationEuler.x = Mathf.Clamp(newRotationEuler.x, -vertTravelMod, vertTravelMod);

        // CLAMP YAW
        newRotationEuler.y = Mathf.Clamp(newRotationEuler.y, -horizTravelMod, horizTravelMod);

        newRotationEuler.z = 0f;

        //Debug.Log("Entering mouseFreeLookEuler with mouse speed: (" + mouseSpeedX + ", " + mouseSpeedY + "), target offset of: (" +
        //    angleOffsetHoriz + ", " + angleOffsetVert + "), newEuler is (" + newRotationEuler.x + ", " + newRotationEuler.y + ", " +
        //    newRotationEuler.z + ")");

        return newRotationEuler;
    }

    private float unEulerize(float val)
    {
        if (val > 180)
            val -= 360f;
        return val;
    }

    


    private Quaternion processAngularVelocityRotation()
    {
        Quaternion returnQuat;
        float rollRateOffsetTarget = 0f; // will target 0 z rotation if lookAt is enabled
        float rollRateOffsetResult;

        
        if (!lookAtEnabled)
        {
            Vector3 rollRateVect = Vector3.Project(aircraftRootRB.angularVelocity, transform.forward);    // Get roll component of total angular velocity vector
            rollRateOffsetTarget = rollRateVect.magnitude * rollRateMod; // Use magnitude to determine camera z offset strength
            if (rollRateVect.normalized == transform.forward)
                rollRateOffsetTarget *= -1;
        }

        rollRateOffsetResult = Mathf.Lerp(camAxisRollRef.transform.localRotation.z, rollRateOffsetTarget, rollRateOffsetLerpRate * Time.deltaTime);
        
        returnQuat = new Quaternion(
            camAxisRollRef.transform.localRotation.x,
            camAxisRollRef.transform.localRotation.y,
            rollRateOffsetResult,
            camAxisRollRef.transform.localRotation.w);

        return returnQuat;
    }

    



}
