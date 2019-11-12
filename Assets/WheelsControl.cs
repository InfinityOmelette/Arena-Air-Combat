using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsControl : MonoBehaviour
{

    public WheelBehavior[] wheels;
    public Rigidbody aircraftRootRB;


    public float steerReductionSpeedFactor;

    
    
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        processAllWheels();
    }

    private void processAllWheels()
    {
        for(int i = 0; i < wheels.Length; i++)
        {
            // ================== STEER PROCESS
            
            wheels[i].doSteer(steerInputProcess());

            // ====================== BRAKE PROCESS
            float brakeInput = -Input.GetAxis("Throttle"); // negative so that decreasing throttle will have positive brake input
            wheels[i].doBrake(brakeInput);

        }
    }

    private float steerInputProcess()
    {
        // get velocity from root parent
        float readVel = 0.0f;
        if (aircraftRootRB != null)
            readVel = aircraftRootRB.velocity.magnitude; // only access reference if not null

        // set steering
        return (steerReductionSpeedFactor * Input.GetAxis("Rudder")) /
          (steerReductionSpeedFactor + readVel); // (a / (a+x)) graph to approach 0 at increasing x, starting val 1 at x = 0
        //return Input.GetAxis("Rudder");

    }

    private float brakeInputProcess()
    {
        return -Input.GetAxis("Throttle");
    }


   
}
