using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class littleGayAssScript : MonoBehaviour
{
    public float disappearProximity;

    public GameObject[] other;
    public GameObject disableMe;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool itemEnabled = true;
        for(short i = 0; i < other.Length && itemEnabled; i++)
        {
            float distance = Vector3.Distance(other[i].transform.localPosition, disableMe.transform.localPosition);
            Debug.Log("Distance from pipper to nose UI is: " + distance);
            if(distance < disappearProximity)
            {
                itemEnabled = false;
            }
        }
        disableMe.SetActive(itemEnabled);
    }
}
