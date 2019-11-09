using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelAlignment : MonoBehaviour
{
    public WheelCollider wheelCollider;

    private Vector3 wheelCCenter;
    private RaycastHit hit;

    void Start()
    {

    }

    void Update()
    {
        wheelCCenter = wheelCollider.transform.position + wheelCollider.center;

        // cast a ray from wheel center, in the downwards direction, the length of suspension distance plus radius
        // check if this ray collides with anything
        //  save the collision point into hit
        if (Physics.Raycast(wheelCCenter, -wheelCollider.transform.up, out hit, wheelCollider.suspensionDistance + wheelCollider.radius))
        {
            // if ray collided, move wheel to position where its edge contacts point
            transform.position = hit.point + (wheelCollider.transform.up * wheelCollider.radius);
        }
        else
        {
            // if ray didn't collide, move wheel to full suspension extension
            transform.position = wheelCCenter - (wheelCollider.transform.up * wheelCollider.suspensionDistance);
        }


        

    }


    
}
