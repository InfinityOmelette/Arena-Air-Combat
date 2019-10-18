using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public float camRearDist = 7.0f;
    public float camElevation = 4.0f;
    public float camLookAheadMultiplier = 30.0f;
    public float followBias = 0.9f;

    public float showVelocity;


    public GameObject followTarget;

    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        if (followTarget != null)
        {
            Vector3 targetVelocity = followTarget.GetComponent<Rigidbody>().velocity;
            showVelocity = targetVelocity.magnitude;
            Vector3 moveCamTo = followTarget.transform.position - followTarget.transform.forward * camRearDist + Vector3.up * camElevation;           // Target position behind and above
            Camera.main.transform.position = Camera.main.transform.position * followBias + moveCamTo * (1.0f - followBias);
            Camera.main.transform.LookAt(followTarget.transform.position + followTarget.transform.forward * camLookAheadMultiplier * targetVelocity.magnitude);                        // look ahead of player based on velocity
        }
    }
}
