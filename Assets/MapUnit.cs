using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUnit : MonoBehaviour
{
    //public Vector3 relativePosition;
    //public float bearing;

    private MapManager mapManager;

    private Text text;

    private GameObject localPlayer;

    private CombatFlow linkedFlow;
    private TgtHudIcon linkedTgtIcon;
    private RectTransform rectTransform;

    private bool isLinked = false;

    private bool isShowing = true;


    private float blinkTimer;
    public float blinkTimeMax;

    public Image coverageRangeImg;
    public Image maxRangeImg;
    

    void Awake()
    {
        //Debug.LogError("MapUnit instantiated. Finding mapManager");
        mapManager = transform.parent.GetComponent<MapManager>();
        text = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //show(false);
    }

    public void linkToFlow(CombatFlow flow)
    {
        text = GetComponent<Text>();

        linkedFlow = flow;
        //Debug.LogWarning(flow.name + " radar symbol " + flow.radarSymbol);
        text.text = flow.radarSymbol;
        gameObject.name = flow.gameObject.name + " map icon";
        linkedTgtIcon = flow.myHudIconRef;
        isLinked = true;

        showIcon(flow.isActive);

        if (linkedFlow.killCoverageRadius > 100f)
        {
            //Debug.LogError("Creating kill circle for " + linkedFlow.name);
            createRadius();
        }
    }


    private void createRadius()
    {
        //Debug.LogWarning("Mapmanager: " + mapManager.name);
       // Debug.LogWarning("MapradiusPrefab: " + mapManager.mapRadiusPrefab.name);
        //Debug.LogWarning("BackgroundContainer: " + mapManager.backgroundContainer.name);

        coverageRangeImg = GameObject.Instantiate( mapManager.mapRadiusPrefab, mapManager.backgroundContainer.transform)
            .GetComponent<Image>();

        

        coverageRangeImg.transform.position = transform.position;

        maxRangeImg = GameObject.Instantiate(mapManager.mapRadiusPrefab, mapManager.backgroundContainer.transform)
            .GetComponent<Image>();

        maxRangeImg.transform.position = transform.position;

        maxRangeImg.sprite = mapManager.outlineImage;


    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (localPlayer == null)
        {
            localPlayer = GameManager.getGM().localPlayer;
            // set color. This object should only be created after to-be-linked CombatFlow already exists
        }
        else
        {
            if (linkedFlow == null)
            {
                if (isLinked)
                {
                    if(coverageRangeImg != null)
                    {
                        GameObject.Destroy(coverageRangeImg.gameObject);
                        GameObject.Destroy(maxRangeImg.gameObject);
                    }

                    GameObject.Destroy(this.gameObject);
                }
            }
            else
            {
                // update position and rotation
                updatePosition();
                updateRotation();
                updateColor();

                //bool withinBounds = 

                bool visible = linkedTgtIcon.isDetected || linkedTgtIcon.dataLink;

                if ((linkedTgtIcon.targetedState == TgtHudIcon.TargetedState.TARGETED
                    || linkedTgtIcon.targetedState == TgtHudIcon.TargetedState.LOCKED) && visible)
                {
                    doBlink();

                    //if (mapManager.withinBounds(transform.localPosition) && (linkedTgtIcon.isDetected || linkedTgtIcon.dataLink))
                    //{
                        
                    //}
                    //else
                    //{
                    //    showIcon(false);
                        
                    //}
                }
                else
                {
                    //showIcon(mapManager.withinBounds(transform.localPosition) && (linkedTgtIcon.isDetected || linkedTgtIcon.dataLink));
                    showIcon(visible);
                    showRadius(visible);
                }

                
                // Debug.LogError("Showing: " + showing);

            }

        }

    }

    private void doBlink()
    {
        blinkTimer -= Time.fixedDeltaTime;

        if(blinkTimer < 0f)
        {
            blinkTimer = blinkTimeMax;
            text.enabled = !text.enabled;
            showRadius(text.enabled);
        }

    }

    private void updateColor()
    {
        text.color = linkedTgtIcon.activeColor;

        if (coverageRangeImg != null)
        {

            if (linkedTgtIcon.isFriendly)
            {
                coverageRangeImg.color = mapManager.friendlyKillZoneColor;
                maxRangeImg.color = mapManager.friendlyKillZoneColor;
            }
            else
            {
                coverageRangeImg.color = mapManager.enemyKillZoneColor;
                maxRangeImg.color = mapManager.enemyKillZoneColor;
            }
        }
    }

    private void updatePosition()
    {
        Vector3 relPos = linkedFlow.transform.position - localPlayer.transform.position;
        relPos = new Vector3(relPos.x, 0f, -relPos.z); // remove y component
        relPos *= mapManager.mapScaleFactor;
        
        transform.localPosition = new Vector3(relPos.z, relPos.x, 0f);


        if(coverageRangeImg != null)
        {
            coverageRangeImg.transform.position = transform.position;
            maxRangeImg.transform.position = transform.position;

            coverageRangeImg.transform.rotation = transform.rotation;
            maxRangeImg.transform.rotation = transform.rotation;

            scaleRadius();
        }
    }

    private void scaleRadius()
    {

        float radiusRatio = linkedFlow.killCoverageRadius / mapManager.rangeMax;

        coverageRangeImg.transform.localScale = new Vector3(radiusRatio, radiusRatio, 1.0f);


        float outlineRatio = linkedFlow.maxCoverageRadius / mapManager.rangeMax;

        maxRangeImg.transform.localScale = new Vector3(outlineRatio, outlineRatio, 1.0f);
    }

    private void updateRotation()
    {
        transform.localEulerAngles = new Vector3(0f, 0f, -MapManager.getBearing(linkedFlow.transform));
    }

    private void showIcon(bool doShow)
    {
        text.enabled = doShow;
    }

    private void showRadius(bool doShow)
    {
        if(maxRangeImg != null && coverageRangeImg != null)
        {
            maxRangeImg.enabled = doShow;
            coverageRangeImg.enabled = doShow;
        }
    }


}
