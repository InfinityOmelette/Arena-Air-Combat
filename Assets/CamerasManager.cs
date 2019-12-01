using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerasManager : MonoBehaviour
{

    public GameObject[] cameras;
    public GameObject uiRef;
    public short activeCamIndex;

    public PlayerInput_Aircraft aircraftInputRef;

    public bool showUI;
    public bool aircraftInputActive;
    public bool lockMouse;

    

    // Start is called before the first frame update
    void Start()
    {
        enableCamIndex(activeCamIndex, showUI, aircraftInputActive); // enable first camera in list
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if(activeCamIndex == 0)
            {
                activeCamIndex = 1; // show airplane perspective
                showUI = true;
                aircraftInputActive = true;
                lockMouse = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                activeCamIndex = 0; // show spectator cam
                showUI = false;
                aircraftInputActive = false;
                lockMouse = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            enableCamIndex(activeCamIndex, showUI, aircraftInputActive);
        }
    }

    void enableCamIndex(short index, bool showUI, bool enableAircraftControl)
    {

        cameras[index].SetActive(true); // activate proper camera

        for (short i = 0; i < cameras.Length; i++)
        {
            if(i != index)
                cameras[i].SetActive(false); // deactivate all cameras that aren't specified cam
        }

        uiRef.SetActive(showUI);
        aircraftInputRef.enabled = enableAircraftControl;
  
    }




    
}
