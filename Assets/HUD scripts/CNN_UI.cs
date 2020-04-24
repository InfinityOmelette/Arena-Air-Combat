using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CNN_UI : MonoBehaviour
{

    public Sprite noseSprite;
    public Sprite cnnIndicatorSprite;

    public Image noseImageObj;
    public Image cnnImageObj;


    public float cnnStayTime; //seconds
    private float currentTimeRemain;
    public bool cnnOn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // tick counter while cannon is on
        if (cnnOn)
        {
            currentTimeRemain -= Time.deltaTime;
        }


        // cannon timer runs out
        if(currentTimeRemain < 0.0f)
        {
            cnnOn = false;

            // change image to nose orient sprite
            noseImageObj.enabled = true;
            cnnImageObj.enabled = false;
        }

        // cannon activated
        if (Input.GetAxis("Cannon") > 0.5f)
        {
            cnnOn = true;
            currentTimeRemain = cnnStayTime;

            // change image to cannon sprite
            noseImageObj.enabled = false;
            cnnImageObj.enabled = true;
        }

    }
}
