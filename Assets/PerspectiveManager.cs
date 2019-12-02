using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveManager : MonoBehaviour
{

    
    public GameObject uiRef;
    public short activeCamIndex;

    public List<GameObject> cameras;

    public PlayerInput_Aircraft aircraftPlayerInputRef;

    public bool showUI;
    public bool aircraftInputActive;
    public bool mouseIsLocked;

    

    // Start is called before the first frame update
    void Start()
    {
        //enableCamIndex(activeCamIndex); // enable first camera in list
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            short tempIndex;
            // toggle 
            if (activeCamIndex == 0) // if cam currently on spectator
                tempIndex = 1; // show airplane
            else
                tempIndex = 0; // show spectator

            if(cameras[tempIndex] != null)
            {
                activeCamIndex = tempIndex;
                enableCamIndex(activeCamIndex);
            }
            
        }
    }

    //  SWITCH TO CAM PERSPECTIVE
    void enableCamIndex(short index)
    {

        if (cameras[index] != null) // only proceed if index is valid
        {

            // activate proper camera
            cameras[index].SetActive(true);

            // deactivate all cameras that aren't specified cam
            for (short i = 0; i < cameras.Count; i++)
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
            aircraftPlayerInputRef.enabled = aircraftInputActive;
            setMouseLock(mouseIsLocked);
        }
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

    
    // SOMEWHAT INEFFICIENT. SCRIPT LOOPS THROUGH NON-DESIRED CAMERAS TWICE
    public void switchToType(CamProperties.CamType type)
    {
        bool camFound = false;
        // search for camera of desired type
        for(short i = 0; i < cameras.Count; i++)
        {
            CamProperties camProperties = cameras[i].GetComponent<CamProperties>();
            if(camProperties.camType == type) // if this camera is desired type
            {
                enableCamIndex(i); // switch to camera
                camFound = true;
            }
        }

        if (!camFound)
        {
            Debug.Log("Unable to find camera of type: " + type.ToString());
        }
    }


    
}
