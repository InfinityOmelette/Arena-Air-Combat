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
    public bool mouseIsLocked;

    

    // Start is called before the first frame update
    void Start()
    {
        enableCamIndex(activeCamIndex); // enable first camera in list
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {

            // toggle 
            if (activeCamIndex == 0) // if cam currently on spectator
                activeCamIndex = 1; // show airplane
            else
                activeCamIndex = 0; // show spectator

            enableCamIndex(activeCamIndex);
        }
    }

    //  SWITCH TO CAM PERSPECTIVE
    void enableCamIndex(short index)
    {

        // activate proper camera
        cameras[index].SetActive(true);

        // deactivate all cameras that aren't specified cam
        for (short i = 0; i < cameras.Length; i++)
        {
            if (i != index)
                cameras[i].SetActive(false);
        }

        // Set object data according to camera properties reference
        CamProperties camPropertiesRef = cameras[index].GetComponent<CamProperties>();
        showUI = camPropertiesRef.showUI;
        mouseIsLocked = camPropertiesRef.mouseIsLocked;
        aircraftInputActive = camPropertiesRef.aircraftInputActive;


        // Use that data to complete the perspective change
        uiRef.SetActive(showUI);
        aircraftInputRef.enabled = aircraftInputActive;
        setMouseLock(mouseIsLocked);
    }


    //  LOCK OR UNLOCK MOUSE CURSOR
    public void setMouseLock(bool lockMouse)
    {
        Cursor.visible = !lockMouse;
        if (!lockMouse)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    




    
}
