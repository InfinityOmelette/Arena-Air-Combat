using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIndicator : MonoBehaviour
{


    // read from hardpoint, set icon imagery accordingly

    public Hardpoint myHardpoint;

    public Image weaponImage;
    public GameObject weaponBackgroundFillCenter; // set Y scale to indicate reload progress / rounds remain

    public Image readyToFireFill;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(myHardpoint != null)
        {
            float yScale;
            if (myHardpoint.readyToFire) // not reloading -- show remaining rounds
            {
                // show rounds remaininig
                yScale = myHardpoint.roundsRemain / ((float)myHardpoint.roundsMax); // assuming full for now
                readyToFireFill.enabled = true;
                
            }
            else // currently reloading -- show reload time
            {
                yScale = 1f - (myHardpoint.currentReloadTimer / myHardpoint.reloadTimeMax);
                
                readyToFireFill.enabled = false;
                //Debug.Log(">>>>>>>>>>>>>>>>>  INDICATOR RELOADING with rounds remaining: " + myHardpoint.);
            }
            
            weaponBackgroundFillCenter.transform.localScale = new Vector3(1.0f, yScale, 1.0f);



        }
    }


    
}
