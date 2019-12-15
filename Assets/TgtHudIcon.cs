using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TgtHudIcon : MonoBehaviour
{

    public CombatFlow rootFlow;
    public TgtIconManager tgtIconManager;


    public GameObject tgtImageCenter;
    public Image tgtImageLOS;
    public Image tgtImageNoLOS;


    public Text tgtTitleText;
    public Text tgtVisConditionsText;
    public Text tgtDistText;

    public bool isDetected;
    public bool hasLineOfSight;
    public bool showInfo;

    public float currentDistance;


    private Vector3 distTextOriginPos;
    private Vector3 titleTextOriginPos;

    // Start is called before the first frame update
    void Start()
    {
        distTextOriginPos = tgtDistText.transform.localPosition;
        titleTextOriginPos = tgtTitleText.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        hudControl hudObj = hudControl.mainHud.GetComponent<hudControl>();


        if (rootFlow != null)
        {

            if (isDetected)
            {
                
                updateTexts();
                resizeForDist(currentDistance);
                setImageLOS(hasLineOfSight);
                hudObj.drawItemOnScreen(gameObject, rootFlow.transform.position, 1.0f); // 1.0 lerp rate
            }
            else
                transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2); // place offscreen
        }
    }

    void setImageLOS(bool hasLOS)
    {
        if (hasLOS)
        {
            // Show only LOS image
            tgtImageLOS.enabled = true;
            tgtImageNoLOS.enabled = false;
        }
        else // no line of sight
        {
            // show only no LOS image
            tgtImageLOS.enabled = false;
            tgtImageNoLOS.enabled = true;
        }
    }

    void updateTexts()
    {
        // Convert meters to kilometers, show 2 decimal places
        tgtDistText.text = (currentDistance / 1000f).ToString("F2") + "km";


        // Move text to stay aligned with box
        tgtDistText.transform.localPosition = tgtImageCenter.transform.localScale.x * distTextOriginPos;
        tgtTitleText.transform.localPosition = tgtImageCenter.transform.localScale.x * titleTextOriginPos;
    }


    //  Visually intuitive way to indicate object's distance
    void resizeForDist(float dist)
    {
        // at or below minimum distance, currentScale is set to maxIconScale
        // at or above maximum distance, currentScale is set to minIconScale
        // when distance is between minimum and maximum, current scale is linearly scaled between min and max iconScale


        float currentScale;

        // First, get ratio for currentDistance along the range from estimated close to maximum (ex: 1.0 max distance, 0.0 min distance, 0.5 halfway)
        float currentDistOnRange = Mathf.Clamp(currentDistance - tgtIconManager.estimatedCloseDistance, 0.0f, tgtIconManager.estimatedFarDistance);
        currentScale = currentDistOnRange / (tgtIconManager.estimatedFarDistance - tgtIconManager.estimatedCloseDistance);

        
        currentScale = -currentScale * (tgtIconManager.maxIconScale - tgtIconManager.minIconScale) + tgtIconManager.maxIconScale;

        tgtImageCenter.transform.localScale = new Vector3(currentScale, currentScale, 1.0f);
    }


    
}
