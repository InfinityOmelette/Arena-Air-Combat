using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
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
       
        return iconObj;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
