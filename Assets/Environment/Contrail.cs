using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contrail : MonoBehaviour
{

    private AirEnvironmentStats air;
    private TrailRenderer trail;

    public Transform afterburnerCenter;


    private Vector3 originLocalPos;


    public bool engineOn;

    private void Awake()
    {
        air = GameObject.Find("AirEnvironmentProperties").GetComponent<AirEnvironmentStats>();
        trail = GetComponent<TrailRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originLocalPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        trail.emitting = transform.position.y > air.contrailAltitude && engineOn;

        float zOffset = 0.0f;

        if (afterburnerCenter != null)
        {
            zOffset = afterburnerCenter.transform.localScale.z;
        }
        

        transform.localPosition = originLocalPos - new Vector3(0.0f, 0.0f, zOffset);
    }
}
