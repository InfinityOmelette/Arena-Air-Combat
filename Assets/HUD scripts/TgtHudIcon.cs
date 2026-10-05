using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TgtHudIcon : MonoBehaviour
{
    private static float HIDE_DISTANCE = 5500f;
    public static string dotLOS = "✦";
    public static string dotNoLOS = "✧";

    public CombatFlow rootFlow;
    private Rigidbody rootRB;
    public TgtIconManager tgtIconManager;


    public GameObject tgtImageCenter;
    public Image tgtImageLOS;
    public Image tgtImageNoLOS;


    public Text tgtTitleText;
    public Text tgtVisConditionsText;
    public Text tgtDistText;
    public GameObject statusTextCenter;

    private bool doBlink;
    private float currentBlinkUpTime;

    public bool isDetected;
    public bool hasLineOfSight;
    public bool showInfo;

    public bool dataLink;
    public Text dataLinkText;
    public Text txtKPH;

    public float currentDistance;

    public bool isFar;

    public GameObject nearImages;
    public Text farDotText;

    public bool incomingMissile = false;

    private bool losSet;
    private bool dlSet;
    private bool isFarSet;

    private bool losInit = false;
    private bool dlInit = false;
    private bool isFarInit = false;

    public bool neverFar = false;
    public float maxStrategicDist = 800f;

    public GameObject hpBarCenter;
    public Image hpBarImage;
    public float hpDisplayDecimal;
    public bool isSuppressed = false;
    public Text suppressedText;

    public Text retrievingText;

    private hudControl hudObj;

    public Text suppliesText;
    public bool showSupplies;

    //public bool halfLocked = false;
    

    public enum TargetedState
    {
        NONE,
        TARGETED,
        HALFLOCKED,
        LOCKED,
        NULL
    }

    public TargetedState targetedState;

    private TargetedState activeState = TargetedState.NULL;


    public Color teamColor;
    public Color activeColor;
    public bool isFriendly;
    public bool isNeutral = false;


    private Vector3 statusTextOriginPos;
    private Vector3 titleTextOriginPos;
    private Vector3 dataLinkTextOriginPos;

    private bool init = false;

    public Vector3 targetScreenPos;
    public float initLerpRate;
    public float activeLerpRate;

    private Vector3 suppliesTextOrigPos;

    public List<GameObject> showOnLock;
    public List<GameObject> hideOnLock;

    public List<GameObject> showOnHalflock;
    public List<GameObject> hideOnHalflock;

    public List<GameObject> showOnTargeted;
    public List<GameObject> hideOnTargeted;

    public List<GameObject> showUntargeted;
    public List<GameObject> hideUntargeted;

    public List<GameObject> showUntargetedIfFriendly;

    public bool showHPBar = false;

    public List<Text> specialTextsToColor;

    public ReloadIndicator reloadIndicator;

    public List<Text> specialMoveTexts;
    public List<Vector3> specialMoveTextsOrigPos;
    public FlareIndicator flareIndic;

    public AlertnessIndicator alertIndic;

    void Awake()
    {
        transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        getReloadIndic();
        flareIndic = GetComponent<FlareIndicator>();
    }

    private void initMovingTextPositions()
    {
        if(specialMoveTexts != null)
        {
            specialMoveTextsOrigPos = new List<Vector3>();

            for(int i = 0; i < specialMoveTexts.Count; i++)
            {
                specialMoveTextsOrigPos.Add(specialMoveTexts[i].transform.localPosition);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        statusTextOriginPos = statusTextCenter.transform.localPosition;
        titleTextOriginPos = tgtTitleText.transform.localPosition;
        dataLinkTextOriginPos = dataLinkText.transform.localPosition;
        suppliesTextOrigPos = suppliesText.transform.localPosition;
        initMovingTextPositions();

        GameManager.getGM().playerSpawnEvent.AddListener(spawnCallback);
        hudObj = hudControl.mainHud.GetComponent<hudControl>();
        //resizeForDist(currentDistance);

        // only show health bar if is strategic
        hpBarCenter.SetActive(showHPBar);

        //FixedUpdate();
    }

    public void setFlares(int numFlares)
    {
        if(flareIndic != null)
        {
            flareIndic.setFlareText(numFlares);
        }
    }

    public ReloadIndicator getReloadIndic()
    {
        if(reloadIndicator == null)
        {
            reloadIndicator = GetComponent<ReloadIndicator>();
        }
        return reloadIndicator;
    }

    public AlertnessIndicator getAlertnessIndic()
    {
        if(alertIndic == null)
        {
            alertIndic = GetComponent<AlertnessIndicator>();
        }
        return alertIndic;
    }

    public void setReload(bool reloadSet)
    {
        // raw nullcheck instead of get method to avoid excessive GetComponenet calls
        // on sams that do NOT use reload ui
        if(reloadIndicator != null)
        {
            reloadIndicator.setReloadStatus(reloadSet);
        }
    }

    void spawnCallback()
    {
        Debug.LogWarning(rootFlow.name + " icon callback called");

        isFriendly = rootFlow.team == GameManager.getGM().localTeam;
        isNeutral = rootFlow.team == CombatFlow.Team.NEUTRAL;
        setTeamInfo();
        setTargetedState();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (rootFlow != null && (isDetected || dataLink))
        {

            if (!init)
            {
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                init = true;
            }

            isFar = currentDistance > HIDE_DISTANCE && targetedState == TargetedState.NONE;

            //
            //setIsFar(false);
            setIsFar(isFar && !neverFar);

            setTargetedState();

            


            setImageLOS(hasLineOfSight);
            if (!(isFar && !neverFar))
            {
                setDataLinkText();
                updateTexts();
            }
            blinkProcess(); // either show steady or blink depending on targeted state
            resizeForDist(currentDistance);
            setImageLOS(hasLineOfSight);

            setHP_Display(hpDisplayDecimal);
            suppressedText.gameObject.SetActive(isSuppressed);
            suppliesText.gameObject.SetActive(showSupplies);


            bool retrieving = rootFlow.type == CombatFlow.Type.TECH &&
                rootFlow.myTechSite.capturingTeam != CombatFlow.Team.NEUTRAL;
            if (retrieving)
            {
                retrievingText.text = rootFlow.myTechSite.reportStatusString();
            }
            retrievingText.gameObject.SetActive(retrieving);
            



            hudObj.drawItemOnScreen(gameObject, rootFlow.transform.position, 1.0f); // 1.0 lerp rate
        }
        else
        {
            transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2); // place offscreen if not detected
        }
    }

    private void setHP_Display(float hpDecimal)
    {
        hpBarCenter.transform.localScale = new Vector3(1.0f, hpDecimal, 1.0f);
    }

    public void FixedUpdate()
    {
        
            
    }
    public void setShowSupplies(bool doShow) 
    {
        showSupplies = doShow;
    }

    public void updateSupplyText(int supplies)
    {
        suppliesText.text = supplies.ToString();
    }

    protected virtual void lockStateProcess()
    {
        //tgtTitleText.enabled = true;
        //tgtDistText.enabled = true;
        //txtKPH.enabled = true;
        setElementsActive(showOnLock, true);
        setElementsActive(hideOnLock, false);
    }

    protected virtual void targetedStateProcess()
    {
        //if (!(isFar && !neverFar))
        //if(!isFar || neverFar)
        {
            //tgtTitleText.enabled = true;
            //tgtDistText.enabled = true;
            //txtKPH.enabled = true;

            setElementsActive(showOnTargeted, true);
            setElementsActive(hideOnTargeted, false);




        }
        
    }

    protected virtual void halflockStateProcess()
    {
        setElementsActive(showOnHalflock, true);
        setElementsActive(hideOnHalflock, false);
    }


    protected virtual void untargetedStateProcess()
    {

        //tgtTitleText.enabled = false;
        //tgtDistText.enabled = false;
        setElementsActive(hideUntargeted, false);
        setElementsActive(showUntargetedIfFriendly, isFriendly);
        setElementsActive(showUntargeted, true);
        
    }

    private void setTargetedState()
    {
        if (targetedState != activeState)
        {


            switch (targetedState)
            {
                case TargetedState.LOCKED:
                    changeChildColors(tgtIconManager.lockedColor);
                    doBlink = false;
                    lockStateProcess();
                    break;

                case TargetedState.HALFLOCKED:
                    doBlink = true;
                    changeChildColors(tgtIconManager.neutralColor);
                    halflockStateProcess();
                    break;

                case TargetedState.TARGETED:
                    setTeamInfo();
                    doBlink = true;
                    targetedStateProcess();
                    break;

                default:    // not targeted at all
                    setTeamInfo();
                    doBlink = false;
                    untargetedStateProcess();
                    break;

            }

            //// SET COLOR BASED ON LOCK STATE
            //if (targetedState == TargetedState.LOCKED) // LOCKED
            //{

            //    changeChildColors(tgtIconManager.lockedColor);
            //    doBlink = false;

            //    lockStateProcess();




            //}
            //else if (targetedState == TargetedState.HALFLOCKED)
            //{
            //    doBlink = true;
            //    changeChildColors(tgtIconManager.halfLockedColor);
            //    halflockStateProcess();

            //}
            //else // NONE OR TARGETED
            //{

            //    setTeamInfo(); // a bit inefficient. Checks team every frame

            //    //switch


            //    if (targetedState == TargetedState.TARGETED)
            //    {
            //        doBlink = true;
            //        targetedStateProcess();

            //    }
            //    else // NONE -- NOT TARGETED AT ALL
            //    {
            //        //txtKPH.enabled = false;
            //        doBlink = false;

            //        untargetedStateProcess();


            //    }
            //}

        }

        activeState = targetedState;
    }

    
    protected virtual void setElementsActive(List<GameObject> elements, bool doShow)
    {
        for(int i = 0; i < elements.Count; i++)
        {
            elements[i].SetActive(doShow);
        }
    }


    private void setIsFar(bool isFar)
    {
        if (isFar != isFarSet || !isFarInit)
        {
            isFarInit = true;
            isFarSet = isFar;
            

            if (isFar)
            {
                farDotText.enabled = true;


                nearImages.SetActive(false);
                dataLinkText.text = "";
                txtKPH.enabled = false;
                tgtDistText.enabled = false;
                tgtTitleText.enabled = false;
            }
            else
            {
                // show normal hudIcon data next time setState is run
                activeState = TargetedState.NULL; // re-run targeting check next update

                farDotText.enabled = false;

                nearImages.SetActive(true);
                txtKPH.enabled = true;
                tgtDistText.enabled = true;
                tgtTitleText.enabled = true;
                dataLinkText.enabled = true;
            }
        }
        
    }


    private Rigidbody getRB()
    {
        if(rootRB == null)
        {
            rootRB = rootFlow.GetComponent<Rigidbody>();
        }
        return rootRB;
    }

    void setDataLinkText()
    {
        if (dataLink != dlSet || !dlInit)
        {
            dlInit = true;
            dlSet = dataLink;

            if (dataLink)
            {
                dataLinkText.text = "DL";
            }
            else
            {
                dataLinkText.text = "";
            }
        }
    }

    void setImageLOS(bool hasLOS)
    {
        // if value is changing
        if (hasLOS != losSet || !losInit)
        {
            losInit = true;
            losSet = hasLOS;

            if (hasLOS)
            {
                // Show only LOS image
                tgtImageLOS.enabled = true;
                tgtImageNoLOS.enabled = false;
                farDotText.text = dotLOS;
            }
            else // no line of sight
            {
                // show only no LOS image
                tgtImageLOS.enabled = false;
                tgtImageNoLOS.enabled = true;
                farDotText.text = dotNoLOS;
            }
        }
    }

    void updateTexts()
    {
        // Convert meters to kilometers, show 2 decimal places
        tgtDistText.text = (currentDistance / 1000f).ToString("F2") + "km";


        float spd = getRB().velocity.magnitude * 3.6f; // 3.6 to convert m/s to kph
        txtKPH.text = spd.ToString("F0") + "kph";

        float scale = tgtImageCenter.transform.localScale.x;

        // Move text to stay aligned with box
        statusTextCenter.transform.localPosition = scale * statusTextOriginPos;
        tgtTitleText.transform.localPosition = scale * titleTextOriginPos;
        dataLinkText.transform.localPosition = scale * dataLinkTextOriginPos;
        suppliesText.transform.localPosition = scale * suppliesTextOrigPos;
        updateSpecialTextMovers(scale);
    }

    private void updateSpecialTextMovers(float scale)
    {
        for(int i = 0; i < specialMoveTexts.Count; i++)
        {
            Text text = specialMoveTexts[i];
            Vector3 origPos = specialMoveTextsOrigPos[i];

            //Vector3 pos = text.transform.localPosition;

            text.transform.localPosition = origPos * scale;
        }
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


        if (neverFar)
        {
            dist = Mathf.Min(dist, maxStrategicDist);
        }

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


    public Color setTeamInfo()
    {
        Color returnColor;

        if (isNeutral)
        {
            returnColor = tgtIconManager.neutralColor;
        }
        else
        {
            if (isFriendly)
            {
                returnColor = tgtIconManager.friendlyColor;
            }
            else
            {
                returnColor = tgtIconManager.enemyColor;

            }
        }

        if (!isFar && rootFlow.type == CombatFlow.Type.AIRCRAFT)
        {
            tgtTitleText.enabled = true;
        }

        return changeChildColors(teamColor = returnColor);
    }


    // go to all child references, change their colors
    public Color changeChildColors(Color color)
    {
        //Debug.LogWarning("Entering changeChildColors()");
        if (activeColor != color)  // don't set anything if no change required
        {
            //Debug.LogWarning("Changing color");
            activeColor = color;

            tgtImageLOS.color = activeColor;
            tgtImageNoLOS.color = activeColor;
            tgtTitleText.color = activeColor;
            tgtVisConditionsText.color = activeColor;
            tgtDistText.color = activeColor;
            dataLinkText.color = activeColor;
            txtKPH.color = activeColor;
            farDotText.color = activeColor;
            hpBarImage.color = activeColor;
            suppressedText.color = activeColor;
            retrievingText.color = activeColor;
            suppliesText.color = activeColor;
            specialChildColors(activeColor);
            
        }

        return color;
    }

    // used for child classes to set color
    protected virtual void specialChildColors(Color color)
    {
        if(specialTextsToColor != null)
        {
            for (int i = 0; i < specialTextsToColor.Count; i++)
            {
                specialTextsToColor[i].color = color;
            }
        }
        
    }

    public void updateHPValue(float hpDecimal)
    {
        this.hpDisplayDecimal = Mathf.Max(hpDecimal, 0.0f);
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

    void OnDestroy()
    {
        //Debug.LogWarning("Icon destroy callback called for " + rootFlow.name);
        GameManager.getGM().playerSpawnEvent.RemoveListener(spawnCallback);
    }
}
