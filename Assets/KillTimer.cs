using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTimer : MonoBehaviour
{

    public float KillTime = 7f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        KillTime -= Time.fixedDeltaTime;
        if(KillTime < 0.0f)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
