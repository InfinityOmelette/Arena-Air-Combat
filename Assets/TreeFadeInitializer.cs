using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFadeInitializer : MonoBehaviour
{
    public float treeFadeEnd;
    public float treeFadeStartPercent;


    private static readonly int GlobalFadeEndDistID = Shader.PropertyToID("_fadeEndDist");
    private static readonly int GlobalFadeStartPercentID = Shader.PropertyToID("_fadeStartPercent");


    private void Awake()
    {
        updateShaderVariables();
    }

    private void OnValidate()
    {
        updateShaderVariables();
    }

    void updateShaderVariables()
    {
        Shader.SetGlobalFloat(GlobalFadeEndDistID, treeFadeEnd);
        Shader.SetGlobalFloat(GlobalFadeStartPercentID, treeFadeStartPercent);
    }
    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
