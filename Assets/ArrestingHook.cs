using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrestingHook : MonoBehaviour
{

    public bool isCatching = false;


    private CombatFlow rootFlow;

    //public float releaseThreshold = 3f;

    public float linearDecayRate = 10000000000f; // lose this many m/s per second when catching

    public const int CARRIER_OPS_LAYER = 14;

    public float decayFactor = .99f;

    public float releaseTime = 3f;
    private float releaseTimer = 0.0f;

    private Vector3 catchPoint;

    public float maxDisplacement = 30f;
    //public float fullStretchDecayFactor = 1.5f;
    public float minimumDecayFactor = 0.5f;

    private ArrestingWire caughtWire;
    private WireGroup caughtGroup;

    public Transform hookPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.layer = 14;
    }

    private void FixedUpdate()
    {
        if (isCatching)
        {
            decayVelocity();
            tryRelease();
        }


    }

    public void tryRelease()
    {
        releaseTimer -= Time.fixedDeltaTime;
        if( releaseTimer < 0)
        {
            isCatching = false;
            caughtWire.release(this);
            caughtGroup = null;
            caughtWire = null;
        }
    }

    public void decayVelocity()
    {
        

        Rigidbody rootRB = getRootFlow().myRb;

        float displacement = Vector3.Distance(rootRB.transform.position, caughtWire.transform.position);
        float displacementFactor =
            Mathf.Clamp(displacement / maxDisplacement, minimumDecayFactor, 1.0f);


        float effectiveDecay = linearDecayRate * Time.fixedDeltaTime * displacementFactor;

        if (rootRB.velocity.magnitude < effectiveDecay)
        {
            effectiveDecay = rootRB.velocity.magnitude;
        }

        Vector3 decayVect = (rootRB.velocity.normalized) * effectiveDecay;
        decayVect = new Vector3(decayVect.x, 0f, decayVect.z);
        rootRB.velocity -= decayVect;

        //rootRB.velocity *= decayFactor*Time.fixedDeltaTime;


    }

    public CombatFlow getRootFlow()
    {
        if(rootFlow == null)
        {
            rootFlow = transform.root.GetComponent<CombatFlow>();
        }
        return rootFlow;
    }

    // One and only way this can trigger is if hook touches carrier wires
    //  - if touches another carrierops object
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == CARRIER_OPS_LAYER && !isCatching)
        {
            isCatching = true;
            releaseTimer = releaseTime;
            catchPoint = other.transform.position;
            caughtGroup = other.GetComponent<WireGroup>();
            caughtWire = caughtGroup.selectWire(getHookPoint().position);
            caughtWire.Catch(this);

        }
        //isCatching = other.gameObject.layer == 14;
    }

    public Transform getHookPoint()
    {
        if(hookPoint != null)
        {
            return hookPoint;
        }
        else
        {
            return transform;
        }
    }

}
