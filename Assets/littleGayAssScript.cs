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
        Vector3 initialLocalPosition = disableMe.transform.localPosition;

        // this line is super stupid, I know. CBA to find better solution for this stupid ass problem
        // (project line from cam position in the direction of player forward, put thing there
        Vector3 camPosition = Camera.main.transform.position;
        Vector3 playerForward = Camera.main.transform.root.transform.forward;

        hudControl.mainHud.GetComponent<hudControl>().drawItemOnScreen(disableMe, Camera.main.transform.position + 
            Camera.main.gameObject.transform.root.transform.forward, 1f);

        disableMe.transform.localPosition += initialLocalPosition; // player forward is effectively 0,0.

    }

    // Update is called once per frame
    void Update()
    {
        bool itemEnabled = true;
        for(short i = 0; i < other.Length && itemEnabled; i++)
        {
            float distance = Vector3.Distance(other[i].transform.localPosition, disableMe.transform.localPosition);
            //Debug.Log("Distance from pipper to nose UI is: " + distance);
            if(distance < disappearProximity)
            {
                itemEnabled = false;
            }
        }
        disableMe.SetActive(itemEnabled);
    }
}
