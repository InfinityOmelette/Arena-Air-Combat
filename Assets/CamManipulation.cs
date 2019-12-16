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

    public float freeLookLerpRate;

    public float rollRateMod;
    public float rollRateOffsetLerpRate;


    public float horizTravelMod = 120f;
    public float vertTravelMod = 80f;

    public GameObject lookAtObj;
    public bool lookAtEnabled = false;

    private Quaternion defaultCamRotation;


    public bool input_camLookAtButtonDown = false; // to be set by external input script
    public float input_freeLookHoriz;
    public float input_freeLookVert;

    // Start is called before the first frame update
    void Start()
    {
        

        defaultCamRotation = camRef.transform.localRotation;

        //camAxisXref.transform.rotation = new Quaternion(0.0f, 180f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(input_camLookAtButtonDown)
            toggleLookAt();


        
    }

    private void FixedUpdate()
    {


        

        //  ========================================     VELOCITY EFFECTS ON CAMERA POSITION
        camRef.transform.localPosition = new Vector3(0.0f, camDefaultHeight, -camDefaultHorizDist);
        camRef.transform.localPosition -= camDistanceByVelocity();
        camRef.transform.localPosition -= camRef.transform.InverseTransformDirection(velocityGlobalForwardMinimized(fwdGlobalVelocityScale) * camVelocityMod);



        if (lookAtEnabled)
        {
            if (lookAtObj != null)
            {  // slightly redundant null check
                camAxisRollRef.transform.LookAt(lookAtObj.transform.position, aircraftRootRB.transform.up);
            }
            else // if look at is enabled but reference is null, re-toggle look at
                toggleLookAt();
        }
        else
        {
            // Roll angular velocity on camera rotation
            camAxisRollRef.transform.localRotation = processAngularVelocityRotation();

            // right stick to look around aircraft
            processFreeLook();
        }

    }

    // toggle lookAt
    private void toggleLookAt()
    {
        

        if (!lookAtEnabled && lookAtObj != null)  // lookAt is not turned on, turn it on if possible
        {
            // Reset cam axes -- start from zero, then lookat will modify from there
            Quaternion zeroQuat = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);   // remove any free look input
            camAxisRollRef.transform.localRotation = zeroQuat;
            camAxisHorizRef.transform.localRotation = zeroQuat;
            camAxisVertRef.transform.localRotation = zeroQuat;

            // switch enabled state
            lookAtEnabled = true;


        }
        else // lookAt is on or failed to turn on, turn it off
        {
            // switch enabled state
            lookAtEnabled = false;

            //// Reset cam axes -- so free look can modify an undisturbed value
            Quaternion zeroQuat = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            camAxisRollRef.transform.localRotation = zeroQuat;

            // return cam rotation to default
            camRef.transform.localRotation = defaultCamRotation;


        }

        
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
        float horizLookTarget = input_freeLookHoriz * horizTravelMod;
        float vertLookTarget = input_freeLookVert * vertTravelMod;


        Vector3 currentLocalEuler = camAxisHorizRef.transform.localEulerAngles;


        camAxisHorizRef.transform.localEulerAngles = new Vector3(vertLookTarget, horizLookTarget, currentLocalEuler.z);


    }


    private Quaternion processAngularVelocityRotation()
    {
        Vector3 rollRateVect = Vector3.Project(aircraftRootRB.angularVelocity, transform.forward);    // Get roll component of total angular velocity vector
        float rollRateOffsetTarget = rollRateVect.magnitude * rollRateMod; // Use magnitude to determine camera z offset strength
        if (rollRateVect.normalized == transform.forward)
            rollRateOffsetTarget *= -1;
        float rollRateOffsetResult = Mathf.Lerp(camAxisRollRef.transform.localRotation.z, rollRateOffsetTarget, rollRateOffsetLerpRate);
        Quaternion returnQuat = new Quaternion(
            camAxisRollRef.transform.localRotation.x,
            camAxisRollRef.transform.localRotation.y,
            rollRateOffsetResult,
            camAxisRollRef.transform.localRotation.w);

        return returnQuat;
    }

    



}
