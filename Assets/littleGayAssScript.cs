using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class littleGayAssScript : MonoBehaviour
{
    public float disappearProximity;

    public GameObject[] other;
    public GameObject disableMe;

    public GameObject hudRef;

    private bool camInitiated = false;
    //private bool camReady = false;

    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {

        if (camInitiated)
        {
            bool itemEnabled = true;
            for (short i = 0; i < other.Length && itemEnabled; i++)
            {
                float distance = Vector3.Distance(other[i].transform.localPosition, disableMe.transform.localPosition);
                //Debug.Log("Distance from pipper to nose UI is: " + distance);
                if (distance < disappearProximity)
                {
                    itemEnabled = false;
                }
            }
            disableMe.SetActive(itemEnabled);

        }
        else if (hudRef.GetComponent<hudControl>().aircraftRootObj != null)
        {

            Vector3 initialLocalPosition = disableMe.transform.localPosition;

            GameObject mainCam = PerspectiveManager.getPManager().getActiveCam();

            hudControl.mainHud.GetComponent<hudControl>().drawItemOnScreen(disableMe, mainCam.transform.position +
                mainCam.gameObject.transform.root.transform.forward, 1f);

            disableMe.transform.localPosition += initialLocalPosition; // player forward is effectively 0,0.

        }
    }
}
