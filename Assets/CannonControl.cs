using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonControl : MonoBehaviour
{

    public GameObject[] cannons;


    public float convergenceRange;


    public float cannonInput;

    

    private bool gunsOn;

    // Start is called before the first frame update
    void Start()
    {
        gunsOn = false;
        for (int i = 0; i < cannons.Length; i++)
        {
            cannons[i].transform.LookAt(transform.position + transform.forward * convergenceRange);
            cannons[i].GetComponent<ParticleSystem>().Stop();

        }
    }

    // Update is called once per frame
    void Update()
    {

        if (cannonInput > 0.5f) // if button is definitely pressed
        {

            

            if (!gunsOn) // turn or keep guns on
            {
                for (int i = 0; i < cannons.Length; i++)
                {
                    cannons[i].GetComponent<ParticleSystem>().Play();
                }
            }

            gunsOn = true;
        }
        else // turn or keep guns off
        {
            if (gunsOn)
            {
                for (int i = 0; i < cannons.Length; i++)
                {
                    cannons[i].GetComponent<ParticleSystem>().Stop();
                }
            }
            gunsOn = false;
        }





    }
}
