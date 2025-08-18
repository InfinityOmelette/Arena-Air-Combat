using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speen : MonoBehaviour
{

    public float speed;

    private int currentAngle = 0;

    public GameObject axisObj;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float rotAngle = speed * Time.deltaTime;
        currentAngle += Mathf.RoundToInt( rotAngle);

        Vector3 lookAt = Quaternion.AngleAxis(currentAngle, axisObj.transform.up) * axisObj.transform.forward;

        //Debug.Log("lookAt: " + lookAt);

        transform.rotation = Quaternion.LookRotation(lookAt, axisObj.transform.up);

    }
}
