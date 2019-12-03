using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonControl : MonoBehaviour
{

    public GameObject[] cannons;

    public float debugFireRate;

    public float convergence;


    private bool gunsOn;

    // Start is called before the first frame update
    void Start()
    {
        gunsOn = false;
        for (int i = 0; i < cannons.Length; i++)
        {
            cannons[i].transform.LookAt(transform.position + transform.forward * convergence);
            cannons[i].GetComponent<ParticleSystem>().Stop();

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Input.GetKey(KeyCode.P))
        {


            if (!gunsOn)
            {
                for (int i = 0; i < cannons.Length; i++)
                {
                    cannons[i].GetComponent<ParticleSystem>().Play();
                }
            }

            gunsOn = true;
        }
        else
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
