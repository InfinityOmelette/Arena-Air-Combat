using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveProperties : MonoBehaviour
{
    public enum CamType
    {
        SPECTATOR,
        DEATH,
        PLAYER
    }

    public CamType camType;
    public bool showUI;
    public bool aircraftInputActive;
    public bool mouseIsLocked;
    public Camera camRef;

    // Start is called before the first frame update
    // Perspective is automatically enabled. This is a bit crusty. 
    //  - limits this script to ONLY be used when spawning a player
    void Start()
    {
        Debug.Log("PerspectiveProperties started");
        PerspectiveManager pManager = PerspectiveManager.getPManager();
        pManager.addCam(gameObject);
        pManager.enableCam(gameObject);
    }



}
