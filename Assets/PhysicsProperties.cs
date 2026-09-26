using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsProperties : MonoBehaviour
{
    private static PhysicsProperties phys;

    private const int UNITY_MAX_LAYERS = 32;
    public int numLayers = 0;

    private void Awake()
    {
        phys = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void initNumLayers()
    {
        //int numLayers = 0;

        int highestIndex = 0;

        for(int i = 0; i < UNITY_MAX_LAYERS; i++)
        {
            if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
            {
                highestIndex = i;
            }
        }

        this.numLayers = highestIndex;
    }

    public LayerMask getIgnoreLayerMask(int layerToIgnore)
    {
        int layerCount = getNumLayers();
        LayerMask mask = 0;

        for (int i = 0; i <= layerCount; i++)
        {
            if (i != layerToIgnore)
            {
                int tempMask = 1 << i;
                mask = mask | tempMask;
            }

        }

        return mask;
    }

    public int getNumLayers()
    {
        if(numLayers == 0)
        {
            initNumLayers();
        }
        return numLayers;
    }


    public static PhysicsProperties getPhys()
    {
        return phys;
    }
}
