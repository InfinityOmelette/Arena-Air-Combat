﻿using System.Collections;
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

    private bool doBlink;
    private float currentBlinkUpTime;

    public bool isDetected;
    public bool hasLineOfSight;
    public bool showInfo;

    public float currentDistance;

    public enum TargetedState
    {
        NONE,
        TARGETED,
        LOCKED
    }

    public TargetedState targetedState;

    

    public Color teamColor;
    public Color activeColor;
    public bool isFriendly;


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

                
                // SET COLOR BASED ON LOCK STATE
                if (targetedState == TargetedState.LOCKED) // LOCKED
                {
                    changeChildColors(tgtIconManager.lockedColor);
                    doBlink = false;
                }
                else // NONE OR TARGETED
                {
                    setTeamColor(); // a bit inefficient. Checks team every frame
                    if(targetedState == TargetedState.TARGETED)
                    {
                        doBlink = true;
                    }
                    else
                    {
                        doBlink = false;
                    }
                }



                blinkProcess(); // either show steady or blink depending on targeted state
                updateTexts();
                resizeForDist(currentDistance);
                setImageLOS(hasLineOfSight);
                hudObj.drawItemOnScreen(gameObject, rootFlow.transform.position, 1.0f); // 1.0 lerp rate
            }
            else
                transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2); // place offscreen if not detected
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
        // between min and max dist, currentScale follows curved graph (rational)

        float currentScale;

        // at or below close distance will be seen as zero
        dist = Mathf.Max(dist - tgtIconManager.estimatedCloseDistance, 0.0f);

        // =========================  EXPONENTIAL

        //float vertStretch = tgtIconManager.maxIconScale - tgtIconManager.minIconScale;
        //float exponent = Mathf.Pow(tgtIconManager.exponentDecay, dist);

        //currentScale = vertStretch * exponent + tgtIconManager.minIconScale;


        // ==========================  RATIONAL

        // vert stretch graph so x = 0 is always result in maxIconScale no matter vertical offset
        float vertStretch = tgtIconManager.maxIconScale - tgtIconManager.minIconScale;

        // core rational graph horizontally offset so x = 0 results in 1
        float rational = tgtIconManager.rationalCoeff / (dist + tgtIconManager.rationalCoeff);

        // minimum scale for icon
        float vertOffset = tgtIconManager.minIconScale;

        // linear component
        float linear = tgtIconManager.linearCoeff * dist;

        // Combine the above into rational graph
        currentScale = Mathf.Max(vertStretch * rational + vertOffset + linear , tgtIconManager.minIconScale);


        // ======================  OUTPUT

        // Output: change scale of image
        tgtImageCenter.transform.localScale = new Vector3(currentScale, currentScale, 1.0f);


        // ====================== LINEAR

        //// First, get ratio for currentDistance along the range from estimated close to maximum (ex: 1.0 max distance, 0.0 min distance, 0.5 halfway)
        //float currentDistOnRange = Mathf.Clamp(currentDistance - tgtIconManager.estimatedCloseDistance, 0.0f, tgtIconManager.estimatedFarDistance);
        //currentScale = currentDistOnRange / (tgtIconManager.estimatedFarDistance - tgtIconManager.estimatedCloseDistance);

        
        //currentScale = -currentScale * (tgtIconManager.maxIconScale - tgtIconManager.minIconScale) + tgtIconManager.maxIconScale;

        //tgtImageCenter.transform.localScale = new Vector3(currentScale, currentScale, 1.0f);
    }


    public Color setTeamColor()
    {
        Color returnColor;
        if (isFriendly)
            returnColor = tgtIconManager.friendlyColor;
        else
            returnColor = tgtIconManager.enemyColor;
        return changeChildColors(teamColor = returnColor);
    }


    // go to all child references, change their colors
    public Color changeChildColors(Color color)
    {
        if (activeColor != color)  // don't set anything if no change required
        {
            activeColor = color;

            tgtImageLOS.color = activeColor;
            tgtImageNoLOS.color = activeColor;
            tgtTitleText.color = activeColor;
            tgtVisConditionsText.color = activeColor;
            tgtDistText.color = activeColor;

            
        }

        return color;
    }


    void blinkProcess()
    {
        // count down until switch time, then change image center enabled state

        if (doBlink)
        {
            currentBlinkUpTime -= Time.deltaTime;

            if (currentBlinkUpTime < 0.0f)
            {
                tgtImageCenter.SetActive(!tgtImageCenter.active); // switch image enabled state
                currentBlinkUpTime = tgtIconManager.targetedBlinkTime; // reset timer
            }
        }
        else
        {
            tgtImageCenter.SetActive(true); // keep image enabled if doBlink is not enabled
        }

    }
}
