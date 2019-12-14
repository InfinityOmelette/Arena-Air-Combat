using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{

    public static IconManager tgtIconManager;

    public GameObject tgtIconPrefab;

    private void Awake()
    {
        if (IconManager.tgtIconManager == null)
            IconManager.tgtIconManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public GameObject spawnIcon(CombatFlow unitFlow)
    {
        GameObject iconObj = Instantiate(tgtIconPrefab, transform);
        iconObj.GetComponent<TgtHudIcon>().rootFlow = unitFlow;
        return iconObj;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
