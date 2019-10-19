using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManipulation : MonoBehaviour
{

    public GameObject camAxisHorizRef;
    public GameObject camAxisVertRef;
    public GameObject camAxisRollRef;
    public GameObject camRef;

    private Rigidbody myRB_ref;

    public float camDefaultHorizDist;
    public float camDefaultHeight;
    public float camVelocityMod;
    public float thrustModMaxDistOffset;
    public float fwdGlobalVelocityScale;

    public float freeLookLerpRate;

    public float rollRateMod;
    public float rollRateOffsetStepSize;


    public float horizTravelMod = 120f;
    public float vertTravelMod = 80f;

    

    // Start is called before the first frame update
    void Start()
    {
        myRB_ref = GetComponent<Rigidbody>();

        camRef.transform.localPosition = new Vector3(0.0f, camDefaultHeight, -camDefaultHorizDist);

        //camAxisXref.transform.rotation = new Quaternion(0.0f, 180f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {




        //  ==========================================  ROLL ANGULAR VELOCITY ON CAMERA ROTATION
        camAxisRollRef.transform.localRotation = processAngularVelocityRotation();


        //  ========================================     VELOCITY EFFECTS ON CAMERA POSITION
        camRef.transform.localPosition = new Vector3(0.0f, camDefaultHeight, -camDefaultHorizDist);
        camRef.transform.localPosition -= camDistanceByThrust();
        camRef.transform.position -= velocityGlobalForwardMinimized(fwdGlobalVelocityScale) * camVelocityMod;


        //  ==================================  FREE LOOK
        processFreeLook();

    }


    private Vector3 camDistanceByThrust()
    {
        FlightControl fc_scriptRef = GetComponent<FlightControl>();
        float currentThrust = fc_scriptRef.thrust;
        float THRUST_MIN = fc_scriptRef.THRUST_MIN;
        float THRUST_MAX = fc_scriptRef.THRUST_MAX;
        return new Vector3(0.0f, 0.0f, 
            (currentThrust - THRUST_MIN) * (thrustModMaxDistOffset / THRUST_MAX));
    }

    private Vector3 localizeFwdVelocity()
    {

        
        Vector3 velocityVect = myRB_ref.velocity;
        Vector3 zDriftVectGlobal = Vector3.Project(velocityVect, transform.forward);
        float zDriftValLocal = zDriftVectGlobal.magnitude;

        if (zDriftVectGlobal.normalized != transform.forward)
            zDriftValLocal *= -1;

        Vector3 totalDrift = new Vector3(0.0f, 0.0f, zDriftValLocal);


        return totalDrift;
    }

    private Vector3 velocityGlobalForwardMinimized(float fwdScaling)
    {
        Vector3 returnVect = myRB_ref.velocity;
        Vector3 fwdVelocityMod = Vector3.Project(returnVect, transform.forward);
        returnVect -= fwdVelocityMod * (1.0f - fwdScaling);
        return returnVect;
    }

    private void processFreeLook()
    {
        float horizLookTarget = Input.GetAxis("CamLookX") * horizTravelMod;
        float vertLookTarget = Input.GetAxis("CamLookY") * vertTravelMod;

        // set horizontal rotation
        camAxisHorizRef.transform.localRotation = new Quaternion(
            camAxisHorizRef.transform.localRotation.x,      // x
            Mathf.Lerp(camAxisHorizRef.transform.localRotation.y, horizLookTarget, freeLookLerpRate),                                // y
            camAxisHorizRef.transform.localRotation.z,      // z
            camAxisHorizRef.transform.localRotation.w);     // w

        // set vertical rotation
        camAxisVertRef.transform.localRotation = new Quaternion(
            Mathf.Lerp(camAxisVertRef.transform.localRotation.x, vertLookTarget, freeLookLerpRate),  // x
            camAxisVertRef.transform.localRotation.y,       // y
            camAxisVertRef.transform.localRotation.z,       // z
            camAxisVertRef.transform.localRotation.w);      // w
    }


    private Quaternion processAngularVelocityRotation()
    {
        Vector3 rollRateVect = Vector3.Project(myRB_ref.angularVelocity, transform.forward);    // Get roll component of total angular velocity vector
        float rollRateOffsetTarget = rollRateVect.magnitude * rollRateMod; // Use magnitude to determine camera z offset strength
        if (rollRateVect.normalized == transform.forward)
            rollRateOffsetTarget *= -1;
        float rollRateOffsetResult = Mathf.Lerp(camAxisRollRef.transform.localRotation.z, rollRateOffsetTarget, rollRateOffsetStepSize);
        Quaternion returnQuat = new Quaternion(
            camAxisRollRef.transform.localRotation.x,
            camAxisRollRef.transform.localRotation.y,
            rollRateOffsetResult,
            camAxisRollRef.transform.localRotation.w);

        return returnQuat;
    }

    



}
