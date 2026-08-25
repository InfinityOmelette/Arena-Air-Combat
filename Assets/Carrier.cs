using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carrier : MonoBehaviour
{

    public TeamSpawner linkedSpawner;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if(linkedSpawner != null)
        {
            GameObject.Destroy(linkedSpawner.gameObject);
        }
        
    }
}
