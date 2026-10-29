using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTextureActivation : MonoBehaviour
{

    private void Awake()
    {
        Camera cam = GetComponent<Camera>();

        cam.depthTextureMode |= DepthTextureMode.Depth;
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
