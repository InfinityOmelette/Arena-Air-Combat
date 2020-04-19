using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GayAssColliderScript : MonoBehaviour
{

    public bool isTriggered = false;

    //private Collider collider;


    // Start is called before the first frame update
    void Start()
    {
        //collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
     
        
    }

    private void OnTriggerStay(Collider other)
    {
        isTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isTriggered = false;
    }
    


}
