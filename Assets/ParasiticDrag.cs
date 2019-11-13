using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiticDrag : MonoBehaviour
{

    public RealFlightControl root_flightData;

    public float dragValue;
    public bool dragActive;

    
    // Start is called before the first frame update
    void Start()
    {
        if (dragActive)
        {
            addDrag(dragValue);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // only add drag when value is changed
    public void setDragActive(bool becomeActive)
    {
        if(dragActive != becomeActive) // if value changed during this call
        {
            if (becomeActive)
                addDrag(dragValue);  // value changed to ACTIVE, add drag
            else
                addDrag(-dragValue); // value changed to OFF, subtract drag

        }

        dragActive = becomeActive;
    }

    public void changeDragVal(float newDrag)
    {
        if (dragActive)
        {
            addDrag(-dragValue);    // remove old drag
            addDrag(newDrag);     // add new drag
        }
        dragValue = newDrag;    // change value
    }

    // add parasitic drag to flight computer obj
    private void addDrag(float dragVal)
    {
        if (root_flightData != null)
            root_flightData.totalParasiticDrag += dragVal;
        else
            Debug.Log("Error: ParasiticDrag object " + gameObject + " unable to add drag. root_flightData reference is null");
    }

    

}
