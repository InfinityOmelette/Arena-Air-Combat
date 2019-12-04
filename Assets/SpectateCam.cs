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
        float currentYawRate = yawRateMod * Input.GetAxis("Mouse X");
        float currentPitchRate = -pitchRateMod * Input.GetAxis("Mouse Y");

        transform.localEulerAngles += new Vector3(currentPitchRate, currentYawRate, 0.0f);



        transform.position += (transform.forward * fwdSpeed * Input.GetAxis("Pitch") + 
            transform.right * strafeSpd * Input.GetAxis("Roll") + 
            transform.up * vertSpd * Input.GetAxis("Throttle")) * Time.deltaTime;


        if (Input.GetKey(KeyCode.Space)) // turn or keep guns on
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
