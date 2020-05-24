using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsBehavior : MonoBehaviour
{

    public float deathCountDown;
    public bool doCount;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (doCount)
        {
            if(deathCountDown > 0)
            {
                deathCountDown -= Time.deltaTime;
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }
    }

    
}
