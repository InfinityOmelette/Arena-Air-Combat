using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManipulation : MonoBehaviour
{

    public GameObject camAxisHorizRef;
    public GameObject camAxisVertRef;
    public GameObject camAxisRollRef;
    public Camera camRef;

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
    public float mouse_yawRateSlow;
    public float mouse_pitchRateSlow;

    public float highZoom;
    public float lowZoom;
    public float zoomLerpRate;

    private bool zoomKeyPressed = false;


    public float maxMouseTraverseSpeed;

    public bool mouseLookEnabled;
    public bool input_mouseLookToggleBtnDown;

    private Quaternion previousRotationTarget;

    private Vector3 camTargetLocalPos;
    public float camTargetLocalPosLerpRate;

    private Quaternion worldLockedLookRotation;
    public Vector3 worldLockedLookDirection;

    public bool warThunderCamEnabled = false;
    public float warThunderLerpRate;
    public float warThunderVertMod;

    public GameObject testObj;

    public float altRollRateMod;


    public bool levelCamera;

    // Start is called before the first frame update
    void Start()
    {

        camTargetLocalPos = camRef.transform.localPosition;

        defaultCamRotation = camRef.transform.localRotation;

        

        //camAxisXref.transform.rotation = new Quaternion(0.0f, 180f, 0f, 0f);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            levelCamera = !levelCamera;
        }

        zoomKeyPressed = Input.GetKey(KeyCode.LeftAlt);
        float zoomTarget = lowZoom;
        if (zoomKeyPressed)
        {
            zoomTarget = highZoom;
        }
        camRef.fieldOfView = Mathf.Lerp(camRef.fieldOfView, zoomTarget, zoomLerpRate * Time.deltaTime);



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
                    camAxisRollRef.transform.InverseTransformPoint(lookAtObj.transform.position),
                    Vector3.up) * (Quaternion.Inverse(defaultCamRotation));

                activeRotateLerpRate = lookAtLerpRate;

                // this will not move the camera. Only the aim point
                worldLockedLookDirection = warThunderMouseAim(input_mouseSpeedX, input_mouseSpeedY);


            }
            else // if look at is enabled but reference is null, re-toggle look at
                toggleLookAt();
        }
        else
        {
            // right stick to look around aircraft
            processFreeLook();
        }

        if (levelCamera)
        {
            float bank = unEulerize(aircraftRootRB.transform.eulerAngles.z);
            Vector3 locEul = camAxisRollRef.transform.localEulerAngles;
            camAxisRollRef.transform.localEulerAngles = new Vector3(locEul.x, locEul.y, -bank);
        }
        else
        {
            // Roll angular velocity on camera rotation
            camAxisRollRef.transform.localRotation = processAngularVelocityRotation();
        }

        


        camAxisHorizRef.transform.localRotation = Quaternion.Lerp(camAxisHorizRef.transform.localRotation, targetLocalRotation, activeRotateLerpRate * Time.deltaTime);


        

        IconRWR.cameraHorizOffset = camAxisHorizRef.transform.localEulerAngles.y;

        //Debug.Log("CamAxisHoriz Y euler: " + IconRWR.cameraHorizOffset);
    }

    public void toggleWarThunderCam()
    {
        warThunderCamEnabled = !warThunderCamEnabled;
        worldLockedLookRotation = aircraftRootRB.transform.rotation;
        worldLockedLookDirection = camRef.transform.forward;
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

    private Vector3 warThunderMouseAim(float mouseSpeedX, float mouseSpeedY)
    {
        Vector3 newAimWorldDir = worldLockedLookDirection;

        float currentYawRate = mouse_yawRate;
        float currentPitchRate = mouse_pitchRate;

        if (zoomKeyPressed)
        {
            currentYawRate = mouse_yawRateSlow;
            currentPitchRate = mouse_pitchRateSlow;
        }

        float angleOffsetHoriz = currentYawRate * mouseSpeedX * Time.deltaTime;
        float angleOffsetVert = -currentPitchRate * mouseSpeedY * Time.deltaTime;

        // DEBUG --- RULE OUT ANY HORIZONTAL MOUSE INPUT AS CAUSE OF ISSUE
        //angleOffsetHoriz = 0f;

        float maxMouseStep = maxMouseTraverseSpeed * Time.deltaTime;

        Vector3 tempOldDir = worldLockedLookDirection;
        Vector3 oldDirEuler = dir2Euler(tempOldDir);

        float maxVertStep = (warThunderVertMod - Mathf.Abs(unEulerize(oldDirEuler.x)))*Mathf.Sign(oldDirEuler.x);

        angleOffsetHoriz = Mathf.Clamp(angleOffsetHoriz, -maxMouseStep, maxMouseStep);
        angleOffsetVert = Mathf.Clamp(angleOffsetVert, -maxMouseStep, maxMouseStep);

        //Debug.Log(angleOffsetVert + ", " + Mathf.Sign(angleOffsetVert));

        //Debug.Log("OldDirEuler: " + oldDirEuler);

        // if maxVertStep and angleOffsetVert are in same direction
        if (Mathf.Sign(angleOffsetVert).Equals(Mathf.Sign(maxVertStep)) &&
            Mathf.Sign(angleOffsetVert).Equals(Mathf.Sign(oldDirEuler.x)))
        {
            // if moving mouse in positive direction, downwards
            if(maxVertStep > 0)
            {
                if(maxVertStep < angleOffsetVert)
                {
                    angleOffsetVert  = 0f;
                    //Debug.Log("======= Positive overshoot detected. maxVertStep: " + maxVertStep + ", angleOffsetVert: " + angleOffsetVert +
                    //    ", oldDirEuler.x: " + oldDirEuler.x);
                }

                //angleOffsetVert = Mathf.Min(angleOffsetVert, maxVertStep);
            }
            else // if moving mouse in negative direction, upwards
            {
                if(maxVertStep > angleOffsetVert)
                {
                    angleOffsetVert = 0f;
                    //Debug.Log("======= Negative overshoot detected. maxVertStep: " + maxVertStep + ", angleOffsetVert: " + angleOffsetVert +
                    //    ", oldDirEuler.x: " + oldDirEuler.x);
                }

               // angleOffsetVert = Mathf.Max(angleOffsetVert, maxVertStep);
            }
            
        }

        //Debug.Log("maxVertStep: " + maxVertStep + ", oldDirEuler: " + oldDirEuler + ", angleOffsetVert: " + angleOffsetVert + ", maxMouseStep: " + maxMouseStep);
        // convert world look dir to local space
        //newAimWorldDir = aircraftRootRB.transform.InverseTransformDirection(newAimWorldDir);

        // calculate and perform euler rotation of look direction, in local space

        Quaternion vertRot = Quaternion.AngleAxis(angleOffsetVert, camAxisHorizRef.transform.right);
        Quaternion horizRot = Quaternion.AngleAxis(angleOffsetHoriz, camAxisRollRef.transform.up);

        Quaternion rotateBy = vertRot * horizRot;

        //Debug.Log("maxVertStep: " + maxVertStep + ", oldDirEuler: " + oldDirEuler);

        //// clamp resulting vertical angle within bounds
        ////  there has....gotta be a better way to do this....
        ////  so much effort just to convert the direction to a local euler vector
        Vector3 tempNewDir = rotateBy * newAimWorldDir;
        Vector3 camEuler = dir2Euler(tempNewDir);

        ////Debug.Log("camEuler: " + camEuler);

        float vertOvershoot;

        // if camera outside vert limit
        if (!levelCamera && ( (camEuler.x > warThunderVertMod) || (camEuler.x < -warThunderVertMod) ))
        {
            //Debug.Log("Checking vert limit, levelCamera is " + levelCamera);

            // correct it by moving camera in opposite direction
            vertOvershoot = (Mathf.Abs(camEuler.x) - warThunderVertMod) * Mathf.Sign(camEuler.x);
            vertRot = Quaternion.AngleAxis(-vertOvershoot, camAxisHorizRef.transform.right);

            vertOvershoot = Mathf.Clamp(vertOvershoot, -(90f - warThunderVertMod), 90f - warThunderVertMod);

            float horizCorrectCoeff = 10f;

            float horizMaxCorrection = unEulerize(camAxisHorizRef.transform.localEulerAngles.y);

            float horizCorrectionDegrees = -Mathf.Min(Mathf.Abs(horizCorrectCoeff * vertOvershoot), Mathf.Abs(horizMaxCorrection));

            //Debug.Log("Mouse speedY: " + mouseSpeedY + ", vertOvershoot: " + vertOvershoot);

            //Debug.Log("read camY rot: " + unEulerize(camAxisHorizRef.transform.localEulerAngles.y) + 
            //    ", horizMaxCorrection: " + horizMaxCorrection + 
            //    ", horizCorrectionDegrees: " + horizCorrectionDegrees);

            //Debug.Log("read camY rot: " + Mathf.Abs(unEulerize(camAxisHorizRef.transform.localEulerAngles.y)) + " overShoot: " + vertOvershoot);

            // only center if there's no horizontal mouse input
            if (angleOffsetHoriz.Equals(0.0f) && angleOffsetVert.Equals(0.0f))
            {
                // rotate horizontally towards center
                horizRot *= Quaternion.AngleAxis(horizCorrectionDegrees * Mathf.Sign(camEuler.y), camAxisRollRef.transform.up);
            }

            rotateBy = horizRot * vertRot; // recombine rotations with new values

        }
        
        //Debug.Log("camEuler: " + camEuler + ", vertOvershoot: " + vertOvershoot);

        newAimWorldDir = rotateBy * newAimWorldDir;

        // reconvert back to world space
        //newAimWorldDir = aircraftRootRB.transform.TransformDirection(newAimWorldDir);

        return newAimWorldDir;
    }

    private Vector3 dir2Euler(Vector3 dir)
    {
        dir = aircraftRootRB.transform.InverseTransformDirection(dir);
        Quaternion localOldDirRotation = Quaternion.LookRotation(dir);
        return Quaternion.ToEulerAngles(localOldDirRotation) * Mathf.Rad2Deg;
    }

    private void processFreeLook()
    {
        if (warThunderCamEnabled)
        {
            activeRotateLerpRate = warThunderLerpRate;

            worldLockedLookDirection = warThunderMouseAim(input_mouseSpeedX, input_mouseSpeedY);

            targetLocalRotation = Quaternion.LookRotation(
                    camAxisRollRef.transform.InverseTransformPoint(aircraftRootRB.transform.position + worldLockedLookDirection),
                    Vector3.up) * (Quaternion.Inverse(defaultCamRotation));
        }
        else
        {

            // prioritize stick -- if no stick input, change by mouse
            Vector3 targetLocalEuler;
            //

            // If there is no stick input..
            if (mouseLookEnabled)
            {

                activeRotateLerpRate = mouseRotateLerpRate;

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

        }

        previousRotationTarget = targetLocalRotation;
    }

    

    // read mouse input, return a euler angle
    // call from fixedUpdate
    private Vector3 mouseFreeLookEuler(float mouseSpeedX, float mouseSpeedY)
    {
        // horribly inefficient. Surely this can be done in 5 lines or fewer

        float angleOffsetHoriz = mouse_yawRate * mouseSpeedX * Time.deltaTime;
        float angleOffsetVert = -mouse_pitchRate * mouseSpeedY * Time.deltaTime;

        Vector3 newRotationEuler = Mathf.Rad2Deg * Quaternion.ToEulerAngles(targetLocalRotation) +
                new Vector3(angleOffsetVert, angleOffsetHoriz, 0.0f);

        // convert from (0 - 360) to (-180 - +180)
        newRotationEuler.x = unEulerize(newRotationEuler.x);
        newRotationEuler.y = unEulerize(newRotationEuler.y);

        // CLAMP PITCH
        newRotationEuler.x = Mathf.Clamp(newRotationEuler.x, -vertTravelMod, vertTravelMod);

        // CLAMP YAW
        //newRotationEuler.y = Mathf.Clamp(newRotationEuler.y, -horizTravelMod, horizTravelMod);

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

        float activeRollRateMod = rollRateMod;

        // only perform roll rate offset if NEITHER lookAt or warThunderCam are enabled
        //if (warThunderCamEnabled)
        //{
        //    activeRollRateMod = rollRateMod * altRollRateMod;
        //}

        //if (lookAtEnabled)
        //{
        //    activeRollRateMod = 0f;
        //}
        
        Vector3 rollRateVect = Vector3.Project(aircraftRootRB.angularVelocity, transform.forward);    // Get roll component of total angular velocity vector
        rollRateOffsetTarget = rollRateVect.magnitude * activeRollRateMod; // Use magnitude to determine camera z offset strength
        if (rollRateVect.normalized == transform.forward)
            rollRateOffsetTarget *= -1;
        
        

        rollRateOffsetResult = Mathf.Lerp(camAxisRollRef.transform.localRotation.z, rollRateOffsetTarget, rollRateOffsetLerpRate * Time.deltaTime);

        //if (Input.GetKey(KeyCode.B))
        //{
        //    rollRateOffsetResult = .1f;
        //}

        returnQuat = new Quaternion(
            camAxisRollRef.transform.localRotation.x,
            camAxisRollRef.transform.localRotation.y,
            rollRateOffsetResult,
            camAxisRollRef.transform.localRotation.w);



        //returnEuler = new Vector3(0.0f, 0.0f, rollRateOffsetResult);

        return returnQuat;
    }

    



}
