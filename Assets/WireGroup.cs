using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireGroup : MonoBehaviour
{


    public List<ArrestingWire> wires;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ArrestingWire selectWire(Vector3 catchPosition)
    {
        // will select closest wire to catchPosition
        ArrestingWire selectedWire = wires[0];
        float minDist = 100f; // arbitrary large num
        for(int i = 0; i < wires.Count; i++)
        {
            ArrestingWire wire = wires[i];
            float dist = Vector3.Distance(catchPosition, wire.transform.position);

            if(dist < minDist)
            {
                selectedWire = wire;
                minDist = dist;
            }
        }


        return selectedWire;
    }
}
