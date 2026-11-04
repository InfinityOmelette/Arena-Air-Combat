using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFadeInitializer : MonoBehaviour
{
    private float treeFadeEnd;
    public float treeFadeStartPercent;

    private float fadeEndBuffer = .98f;


    private static readonly int GlobalFadeEndDistID = Shader.PropertyToID("_fadeEndDist");
    private static readonly int GlobalFadeStartPercentID = Shader.PropertyToID("_fadeStartPercent");

    private Terrain terrain;


    private void Awake()
    {
        terrain = GetComponent<Terrain>();

        

        updateShaderVariables();
    }

    public Terrain getTerrain()
    {
        if(terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }
        return terrain;
    }

    private void OnValidate()
    {
        updateShaderVariables();
    }

    void updateShaderVariables()
    {
        tryReadFromTerrain();

        Shader.SetGlobalFloat(GlobalFadeEndDistID, treeFadeEnd);
        Shader.SetGlobalFloat(GlobalFadeStartPercentID, treeFadeStartPercent);
    }
    
    void tryReadFromTerrain()
    {
        Terrain terr = getTerrain();
        if(terr != null)
        {
            treeFadeEnd = terr.treeDistance * fadeEndBuffer;
        }
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
