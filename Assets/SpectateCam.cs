using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectateCam : MonoBehaviour
{

    public float yawRateMod;
    public float pitchRateMod;

    public float fwdSpeed;
    public float strafeSpd;
    public float vertSpd;


    public float pitchMax;
    public float pitchMin;

    private bool gunsOn = false;

    public GameObject cannon;

    


    // Start is called before the first frame update
    void Start()
    {
        cannon.GetComponent<ParticleSystem>().Stop();
    }

    // Update is called once per frame
    void Update()
    {

        rotateCam(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        flyCam(Input.GetAxis("Pitch"), Input.GetAxis("Roll"), Input.GetAxis("Throttle"));

        cannonProcess(Input.GetKey(KeyCode.Space));

    }


    void rotateCam(float inputX, float inputY)
    {
        float currentYawRate = yawRateMod * inputX;
        float currentPitchRate = -pitchRateMod * inputY;

        

        Vector3 newRotationEuler = transform.localEulerAngles + new Vector3(currentPitchRate, currentYawRate, 0.0f);

        // Convert pitch angle to  -180 to 180 scale because unity gay
        float angleTemp = newRotationEuler.x;
        if (angleTemp > 180)
            angleTemp -= 360f;

        angleTemp = Mathf.Clamp(angleTemp, pitchMin, pitchMax);

        
        // Convert pitch angle back to 0-360 scale
        if (angleTemp < 0)
            angleTemp += 360;

        newRotationEuler.x = angleTemp;

        transform.localEulerAngles = newRotationEuler;

    }

    void flyCam(float fwdinput, float sideInput, float vertInput)
    {
        transform.position += (transform.forward * fwdSpeed * Input.GetAxis("Pitch") +
            transform.right * strafeSpd * Input.GetAxis("Roll") +
            transform.up * vertSpd * Input.GetAxis("Throttle")) * Time.deltaTime;
    }

    void cannonProcess(bool input)
    {
        if (input) // turn or keep guns on
        {
            if (!gunsOn)
            {
                cannon.GetComponent<ParticleSystem>().Play();
            }

            gunsOn = true;
        }
        else // turn or keep guns off
        {
            if (gunsOn)
            {
                cannon.GetComponent<ParticleSystem>().Stop();
            }
            gunsOn = false;
        }
    }
}
