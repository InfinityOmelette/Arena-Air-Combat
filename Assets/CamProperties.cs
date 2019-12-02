using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamProperties : MonoBehaviour
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



}
