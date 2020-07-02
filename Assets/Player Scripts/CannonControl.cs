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

    public AudioSource mgSoundSource;
    public AudioSource cannonSoundSource;

    public AudioSource mgSoundEnd;
    public AudioSource cannonSoundEnd;

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


                gunsSound(true);

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
                gunsSound(false);
                //photonView.RPC("rpcSetGunsOn", RpcTarget.All, false);
                gunsOn = false;
                for (int i = 0; i < cannons.Length; i++)
                {
                    cannons[i].GetComponent<ParticleSystem>().Stop();
                }
            }
            
        }

    }

    private void gunsSound(bool active)
    {
        if (active)
        {
            mgSoundSource.loop = true;
            cannonSoundSource.loop = true;

            mgSoundSource.Play();
            cannonSoundSource.Play();
        }
        else
        {
            mgSoundSource.loop = false;
            cannonSoundSource.loop = false;

            mgSoundSource.Stop();
            cannonSoundSource.Stop();

            mgSoundEnd.Play();
            cannonSoundEnd.Play();
        }
    }
    public void setIgnoreLayer(int layerToIgnore)
    {
        for(int i = 0; i < cannons.Length; i++)
        {

            ParticleBehavior pbCannon = cannons[i].GetComponent<ParticleBehavior>();
            pbCannon.setIgnoreLayer(layerToIgnore);

        }
    }

}
