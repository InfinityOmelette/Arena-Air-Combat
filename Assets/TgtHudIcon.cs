using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TgtHudIcon : MonoBehaviour
{

    public CombatFlow rootFlow;

    public bool isVisible;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        hudControl hudObj = hudControl.mainHud.GetComponent<hudControl>();


        if (rootFlow != null)
        {
            //Debug.Log("Rootflow position: " + rootFlow.transform.position);

            if (isVisible)
                hudObj.drawItemOnScreen(gameObject, rootFlow.transform.position, 1.0f); // 1.0 lerp rate
            else
                transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2); // place offscreen
        }
    }


    
}
