using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FlareIcon : MonoBehaviour
{

    public Color notReadyColor;
    public Color readyColor;


    public Image flareImg;
    public GameObject fillCenter;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    public void setReloadStatus(bool isReady, float timeRemain, float timeMax)
    {
        if (isReady)
        {
            flareImg.color = readyColor;
        }
        else
        {
            flareImg.color = notReadyColor;
        }


        float ratio = Mathf.Clamp(timeRemain / timeMax, 0.0f, 1.0f);
        ratio = 1.0f - ratio;

        Vector3 fillScale = fillCenter.transform.localScale;

        fillCenter.transform.localScale = new Vector3(ratio, fillScale.y, fillScale.z);

    }
}
