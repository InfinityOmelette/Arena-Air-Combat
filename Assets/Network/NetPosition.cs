using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(CombatFlow))]
public class NetPosition : MonoBehaviour
{

    //public short updatesPerSecond;

    //private float waitTime = 0.0f;

    //private float currentTimer;

    public CombatFlow myFlow;
    public Rigidbody myRB;
    public PhotonView photonView;

    public float lifeTime;
    
    public float posLerp;

    public bool active;

    private bool didReport = false;

    public NetPositionHub netPosHub;

    void Awake()
    {

        netPosHub = GameManager.getGM().GetComponent<NetPositionHub>();
        myRB = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
        photonView = PhotonView.Get(this);
        //waitTime = 1.0f / updatesPerSecond;
        //currentTimer = waitTime;

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
            if (myFlow.localOwned || myFlow.isLocalPlayer)
            {
                lifeTime += Time.fixedDeltaTime;

                if (!didReport && active)
                {
                    netPosHub.allLocalOwned.Add(this);
                    didReport = true;
                }

            }
        }
    }

    
    
    
    public void updatePositionAndVelocity(Vector3 targetPos, Quaternion targetRotation, Vector3 targetVel, float originLifeTime) 
    {
        if (active)
        {
            // Ignore any out of order calls
            if (originLifeTime > lifeTime)
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
