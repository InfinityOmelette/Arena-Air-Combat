using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(CombatFlow))]
public class NetPosition : MonoBehaviour
{

    public short updatesPerSecond;

    private float waitTime = 0.0f;

    private float currentTimer;

    private CombatFlow myFlow;
    private Rigidbody myRB;
    private PhotonView photonView;

    private float lifeTime;

    
    private Vector3 targetPos;
    private bool doLerp = false;

    public float posLerp;
    public float proceedRadius;


    void Awake()
    {
        myRB = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
        photonView = PhotonView.Get(this);
        waitTime = 1.0f / updatesPerSecond;
        currentTimer = waitTime;

        lifeTime = 0.0f;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (myFlow.isLocalPlayer)
        {
            lifeTime += Time.fixedDeltaTime;
            currentTimer -= Time.fixedDeltaTime;
            if (currentTimer < 0)
            {
                currentTimer = waitTime; // reset counter

                Vector3 myPos = transform.position;
                Quaternion myRotation = transform.rotation;
                Vector3 myVel = myRB.velocity;

                photonView.RPC("updatePositionAndVelocity", RpcTarget.All, myPos, myRotation, myVel, lifeTime);

            }
        }
        else if (targetPos != null && doLerp) 
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, posLerp);

            // Once enters radius around target position, continue flying off of velocity
            if ((targetPos - transform.position).magnitude < proceedRadius)
            {
                doLerp = false;
            }
        }
    }

    
    [PunRPC]
    private void updatePositionAndVelocity(Vector3 targetPos, Quaternion targetRotation, Vector3 targetVel, float originLifeTime) 
    {
        // Ignore any out of order calls
        if(originLifeTime > lifeTime || myFlow.isLocalPlayer)
        {
            doLerp = true;
            lifeTime = originLifeTime;
            this.targetPos = targetPos; // lerp towards this in fixedUpdate
            transform.rotation = targetRotation;
            myRB.velocity = targetVel;
        }
    }



}
