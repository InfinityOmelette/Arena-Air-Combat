using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CannonControl : MonoBehaviourPunCallbacks
{

    public GameObject[] cannons;


    public float convergenceRange;


    public bool cannonInput;

    

    private bool gunsOn;

    private CombatFlow rootFlow;

    // Start is called before the first frame update
    void Start()
    {
        rootFlow = transform.root.GetComponent<CombatFlow>();
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
        //cannonInput > 0.5f
        if (cannonInput) // if button is definitely pressed
        {

            if (!gunsOn) // turn or keep guns on
            {
                //photonView.RPC("rpcSetGunsOn", RpcTarget.All, true);
                gunsOn = true;
                for (int i = 0; i < cannons.Length; i++)
                {
                    cannons[i].GetComponent<ParticleSystem>().Play();
                }
            }
            
        }
        else // turn or keep guns off
        {
            if (gunsOn)
            {
                //photonView.RPC("rpcSetGunsOn", RpcTarget.All, false);
                gunsOn = false;
                for (int i = 0; i < cannons.Length; i++)
                {
                    cannons[i].GetComponent<ParticleSystem>().Stop();
                }
            }
            
        }

    }

}
