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

    public static PerspectiveManager globalPerspectiveManager;


    public Camera mainCam;

    public bool mouseLookEnabled;
    public bool warThunderCamEnabled = false;
    public bool levelCamera;

    // Start is called before the first frame update
    void Start()
    {
        //enableCamIndex(activeCamIndex); // enable first camera in list
        cameras = new List<GameObject>();
    }

    public static PerspectiveManager getPManager()
    {
        if(globalPerspectiveManager == null)
        {
            globalPerspectiveManager = GameObject.Find("PerspectiveManager").GetComponent<PerspectiveManager>();
        }
        return globalPerspectiveManager;
    }

    // Update is called once per frame
    void Update()
    {

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
                if (i != index && cameras[i] != null) 
                    cameras[i].SetActive(false);

            }

            // Set object data according to camera properties reference
            PerspectiveProperties camPropertiesRef = cameras[index].GetComponent<PerspectiveProperties>();
            showUI = camPropertiesRef.showUI;
            mouseIsLocked = camPropertiesRef.mouseIsLocked;
            aircraftInputActive = camPropertiesRef.aircraftInputActive;


            
        }
    }

    public GameObject getActiveCam()
    {
        return cameras[activeCamIndex];
    }

    // Must be called on object that has a Camera and PerspectiveProperties object
    public void enableCam(GameObject cam)
    {
        Debug.Log("EnableCam called");

        for(short i = 0; i < cameras.Count; i++)
        {
            bool camActive = cameras[i] == cam;
            if (cameras[i] != null)
            {
                cameras[i].SetActive(camActive);
            }

            if (camActive)
            {
                Debug.Log("Enabling cam " + i);
                activeCamIndex = i;
            }

        }


        // Set object data according to camera properties reference
        PerspectiveProperties camPropertiesRef = cam.GetComponent<PerspectiveProperties>();
        showUI = camPropertiesRef.showUI;
        mouseIsLocked = camPropertiesRef.mouseIsLocked;
        aircraftInputActive = camPropertiesRef.aircraftInputActive;

      
        // Use that data to complete the perspective change
        uiRef.GetComponent<hudControl>().setHudVisible(showUI);

        //aircraftPlayerInputRef.enabled = aircraftInputActive;
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

    
    // SOMEWHAT INEFFICIENT. SCRIPT LOOPS THROUGH NON-DESIRED CAMERAS TWICE
    //  Doesn't need to be called rapidly, so might not be a problem
    public void switchToType(PerspectiveProperties.CamType type)
    {
        bool camFound = false;
        // search for camera of desired type
        for(short i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
            {
                PerspectiveProperties camProperties = cameras[i].GetComponent<PerspectiveProperties>();
                if (camProperties.camType == type) // if this camera is desired type
                {
                    enableCamIndex(i); // switch to camera
                    camFound = true;
                }
            }
        }

        if (!camFound)
        {
            Debug.Log("Unable to find camera of type: " + type.ToString());
        }
    }

    public void addCam(GameObject newCam)
    {
        cameras.Add(newCam);
    }


    
}
