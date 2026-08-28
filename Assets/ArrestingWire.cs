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

    

    // Start is called before the first frame update
    void Start()
    {
        //stretchToPoint(wireCenter.position);
        returnToCenter();
    }

    public CombatFlow getRootFlow()
    {
        if(rootFlow == null)
        {
            rootFlow = transform.root.GetComponent<CombatFlow>();
        }

        return rootFlow;
    }

    public Rigidbody rootRB()
    {
        return getRootFlow().myRb;
    }

    // Update is called once per frame
    void Update()
    {
        if(caughtHook != null)
        {
            stretchToPoint(caughtHook.getHookPoint().position);
        }
        else
        {
            returnToCenter();
        }
    }

    private void returnToCenter()
    {
        if (!centered)
        {
            stretchToPoint(wireCenter.position);
            centered = true;
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
    }

    public void release(ArrestingHook hook)
    {
        caughtHook = null;
        stretchToPoint(wireCenter.position);
    }

}
