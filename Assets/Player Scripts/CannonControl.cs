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

    public Rigidbody myRb;
    public bool autoTrackTarget = false;
    public TgtComputer targetingComputer;
    public float bulletSpeed = 0.0f;

    public Transform camTransform;

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

    private void FixedUpdate()
    {
        if (autoTrackTarget)
        {
            // Cannons look forward by default
            //  --> is it crusty to set this each fixedupdate?
            Quaternion fireSolutionRot = Quaternion.LookRotation(camTransform.forward, transform.up);

            if (targetingComputer.currentTarget != null)
            {
                fireSolutionRot = AI_TurretMG.calculateBulletLeadRot(myRb, targetingComputer.currentTarget.myRb, bulletSpeed);
            }
            else
            {
                fireSolutionRot = aimGroundPoint(fireSolutionRot);
            }

            for (int i = 0; i < cannons.Length; i++)
            {
                // aim turret at fire solution
                //  - parent of MG emitter is entire MG object
                cannons[i].transform.parent.transform.rotation = fireSolutionRot;
                //cannons[i].transform.rotation = fireSolutionRot;
            }

        }

    }

    // If camera is looking at a ground, calculate bullet lead to strike that point on ground
    private Quaternion aimGroundPoint(Quaternion defaultAimRot)
    {
        Quaternion aimRot = defaultAimRot;
        RaycastHit hit;
        int terrainLayer = 1 << 10; // line only collides with terrain layer

        if (Physics.Raycast(myRb.transform.position, camTransform.forward, out hit, terrainLayer))
        {
            aimRot = AI_TurretMG.calculateBulletLeadRot(myRb.transform.position, hit.point, -myRb.velocity, bulletSpeed);
        }


        return aimRot;
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
