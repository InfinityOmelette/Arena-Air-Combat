using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hudControl : MonoBehaviour
{


    public Text speedText;
    public Text altitudeText;
    public Text throttletext;
    public Text climbText;

    private Rigidbody rbRef;
    private RealFlightControl flightInfoObjRef;


    // Start is called before the first frame update
    void Start()
    {
        rbRef = GetComponent<Rigidbody>();
        flightInfoObjRef = GetComponent<RealFlightControl>();
    }

    // Update is called once per frame
    void Update()
    {
        speedText.text = "Speed: " + Mathf.RoundToInt(rbRef.velocity.magnitude).ToString() + "m/s";
        throttletext.text = "Thrust: " + Mathf.RoundToInt(flightInfoObjRef.currentThrustPercent).ToString() + "%";

        altitudeText.text = "Altitude: " + Mathf.RoundToInt(transform.position.y).ToString() + "m";
        climbText.text = "Climb: " + Mathf.RoundToInt(flightInfoObjRef.readVertVelocity).ToString() + "m/s";
        
    }
}
