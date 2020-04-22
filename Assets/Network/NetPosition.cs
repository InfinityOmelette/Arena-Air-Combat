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
    
    public float posLerp;

    public bool active;

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
        if (active)
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
        }
    }

    
    [PunRPC]
    private void updatePositionAndVelocity(Vector3 targetPos, Quaternion targetRotation, Vector3 targetVel, float originLifeTime) 
    {
        if (active)
        {
            // Ignore any out of order calls
            if (originLifeTime > lifeTime || myFlow.isLocalPlayer)
            {
                // project target position forward based on time to send
                targetPos = targetPos + targetVel * (originLifeTime - lifeTime);
                transform.position = Vector3.Lerp(transform.position, targetPos, posLerp);
                lifeTime = originLifeTime;
                transform.rotation = targetRotation;
                myRB.velocity = targetVel;
            }
        }
    }




}
