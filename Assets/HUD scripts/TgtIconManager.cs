using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TgtIconManager : MonoBehaviour
{

    public static TgtIconManager tgtIconManager;

    public GameObject tgtIconPrefab;

    //public float estimatedFarDistance;
    public float estimatedCloseDistance;
    public float rationalCoeff; // a / (x + a) --> rational where x = 0 is always 1
    public float linearCoeff;

    //public float exponentDecay;
    public float maxIconScale;
    public float minIconScale;

    public Color friendlyColor;
    public Color enemyColor;
    public Color lockedColor;

    public float targetedBlinkTime;

    public Sprite aircraftHudImageLOS;
    public Sprite aircraftHudImageNoLOS;

    public Sprite missileHudImageLOS;
    public Sprite missileHudImageNoLOS;

    public Sprite groundHudImageLOS;
    public Sprite groundHudImageNoLOS;

    public Sprite antiAirHudImageLOS;
    public Sprite antiAirHudImageNoLOS;

    public Sprite SamHudImageLOS;
    public Sprite SamHudImageNoLOS;

    private void Awake()
    {

        aircraftHudImageLOS = Resources.Load<Sprite>("HUD Images/HudSquare");
        aircraftHudImageNoLOS = Resources.Load<Sprite>("HUD Images/HudSquareNoLOS");

        missileHudImageLOS = Resources.Load<Sprite>("HUD Images/MissileLOS");
        missileHudImageNoLOS = Resources.Load<Sprite>("HUD Images/MissileNoLOS");

        groundHudImageLOS = Resources.Load<Sprite>("HUD Images/GroundUnitLOS");
        groundHudImageNoLOS = Resources.Load<Sprite>("HUD Images/GroundUnitNoLOS");

        antiAirHudImageLOS = Resources.Load<Sprite>("HUD Images/AntiAirUnitLOS");
        antiAirHudImageNoLOS = Resources.Load<Sprite>("HUD Images/AntiAirUnitNoLOS");

        SamHudImageLOS = Resources.Load<Sprite>("HUD Images/SamUnitLOS");
        SamHudImageNoLOS = Resources.Load<Sprite>("HUD Images/SamUnitNoLOS");

        if (TgtIconManager.tgtIconManager == null)
            TgtIconManager.tgtIconManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    public GameObject spawnIcon(CombatFlow unitFlow)
    {
        GameObject iconObj = Instantiate(tgtIconPrefab, transform);

        // initialize icon's data
        TgtHudIcon iconScript = iconObj.GetComponent<TgtHudIcon>();
        iconScript.rootFlow = unitFlow;
        iconScript.tgtIconManager = this;
        iconScript.tgtTitleText.text = unitFlow.gameObject.name;

        // IMAGE SETTING
        if(unitFlow.type == CombatFlow.Type.AIRCRAFT)
        {
            iconScript.tgtImageLOS.sprite = aircraftHudImageLOS;
            iconScript.tgtImageNoLOS.sprite = aircraftHudImageNoLOS;
        }
        else if(unitFlow.type == CombatFlow.Type.PROJECTILE)
        {
            iconScript.tgtImageLOS.sprite = missileHudImageLOS;
            iconScript.tgtImageNoLOS.sprite = missileHudImageNoLOS;
        }
        else if(unitFlow.type == CombatFlow.Type.GROUND)
        {
            iconScript.tgtImageLOS.sprite = groundHudImageLOS;
            iconScript.tgtImageNoLOS.sprite = groundHudImageNoLOS;
        }
        else if(unitFlow.type == CombatFlow.Type.ANTI_AIR)
        {
            iconScript.tgtImageLOS.sprite = antiAirHudImageLOS;
            iconScript.tgtImageNoLOS.sprite = antiAirHudImageNoLOS;
        }
        else if(unitFlow.type == CombatFlow.Type.SAM)
        {
            iconScript.tgtImageLOS.sprite = SamHudImageLOS;
            iconScript.tgtImageNoLOS.sprite = SamHudImageNoLOS;
        }
       
        return iconObj;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
