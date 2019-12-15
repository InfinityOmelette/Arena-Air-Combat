﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TgtIconManager : MonoBehaviour
{

    public static TgtIconManager tgtIconManager;

    public GameObject tgtIconPrefab;

    public float estimatedFarDistance;
    public float estimatedCloseDistance;

    public float maxIconScale;
    public float minIconScale;

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

        //Debug.Log(iconScript.tgtIconManager);
        return iconObj;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
