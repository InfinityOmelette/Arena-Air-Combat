using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrestingWire : MonoBehaviour
{
    public Transform wireHalfL;
    public Transform wireHalfR;

    public Transform wireCenter;

    public ArrestingHook caughtHook;

    private bool centered = false;

    public CombatFlow rootFlow;
    public Transform root;

    private Vector3 localRestPos;
    public static float returnSpeed = 9f; // meters/second
    public static float centeredSnapDist = .25f; // meters

    private void Awake()
    {

        initWireReturnPos();

        fullyReturnWire();
        
    }

    private void fullyReturnWire()
    {
        stretchToPoint(transform.root.TransformPoint(localRestPos));
    }

    private void initWireReturnPos()
    {
        localRestPos = getRoot().InverseTransformPoint(wireCenter.position);

    }

    public Transform getRoot()
    {
        if(root == null)
        {
            root = transform.root;
        }
        return root;
    }


    // Start is called before the first frame update
    void Start()
    {
        //stretchToPoint(wireCenter.position);
        returnToRestPos();
    }

    public CombatFlow getRootFlow()
    {
        if(rootFlow == null)
        {
            rootFlow = transform.root.GetComponent<CombatFlow>();
        }

        return rootFlow;
    }

    public Vector3 getVelocity()
    {
        Vector3 vel = new Vector3();
        if(rootRB() != null)
        {
            vel = rootRB().velocity;
        }
        return vel;
    }

    public Rigidbody rootRB()
    {
        if (getRootFlow() != null)
        {
            return getRootFlow().myRb;
        }
        else
        {
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(caughtHook != null)
        {
            wireCenter.position = caughtHook.getHookPoint().position;
            stretchToPoint(wireCenter.position);
        }
        else
        {
            returnToRestPos(Time.deltaTime);
        }




        //if(caughtHook != null)
        //{
        //    stretchToPoint(caughtHook.getHookPoint().position);
        //}
        //else
        //{
        //    returnToCenter();
        //}
    }

    private void returnToRestPos(float deltaTime = 1.0f)
    {
        if (!centered)
        {
            //stretchToPoint(wireCenter.position);
            Vector3 restPosWorldSpace = getRoot().transform.TransformPoint(localRestPos);

            float maxDistDelta = Vector3.Distance(wireCenter.transform.position, restPosWorldSpace);
            maxDistDelta = Mathf.Min(maxDistDelta, deltaTime * returnSpeed);

            wireCenter.transform.position = Vector3.MoveTowards(wireCenter.transform.position,
                restPosWorldSpace, maxDistDelta);

            stretchToPoint(wireCenter.position);

            //centered = Vector3.Equals(wireCenter.transform.position, restPosWorldSpace);
            checkCentered(restPosWorldSpace);
        }
    }

    void checkCentered(Vector3 restPosWorldSpace)
    {
        float distToCenter = Vector3.Distance(wireCenter.transform.position, restPosWorldSpace);
        if(distToCenter < centeredSnapDist)
        {
            centered = true;
            fullyReturnWire();
        }
    }

    

    public void stretchToPoint(Transform wire, Vector3 point)
    {
        float distance = Vector3.Distance(wire.position, point);
        Vector3 dirToPoint = point - wire.position;

        Quaternion lookRot = Quaternion.LookRotation(dirToPoint, Vector3.up);
        wire.localScale = new Vector3(1.0f, 1.0f, distance);
        wire.rotation = lookRot;
    }

    public void stretchToPoint(Vector3 point)
    {
        stretchToPoint(wireHalfL, point);
        stretchToPoint(wireHalfR, point);


    }

    public void Catch(ArrestingHook hook)
    {
        caughtHook = hook;
        centered = false;

        catchWireEffects(hook);
    }

    public void catchWireEffects(ArrestingHook hook)
    {
        // Sound?
        // Particles?
    }

    public void release(ArrestingHook hook)
    {
        caughtHook = null;
        stretchToPoint(wireCenter.position);
    }

}
