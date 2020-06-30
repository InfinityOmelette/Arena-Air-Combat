using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RWR : MonoBehaviour
{
    private CombatFlow myFlow;


    //private WarningComputer warnComputer;
    void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
    }


    // Start is called before the first frame update
    void Start()
    {
        //warnComputer = hudControl.mainHud.GetComponent<hudControl>().warningComputer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void tryPing(Radar radarSource)
    {
        if (myFlow.team != radarSource.myFlow.team)
        {

            bool isPinging = radarSource.radarOn && radarSource.withinScope(transform.position);
            float distance = Vector3.Distance(transform.position, radarSource.transform.position);
            float bearing = calculateBearing(radarSource.transform.position);

            IconRWR rwrIcon = radarSource.rwrIcon;

            rwrIcon.showPingResult(isPinging, distance, bearing);
        }
    }

    private float calculateBearing(Vector3 position)
    {
     

        position = transform.GetChild(0).InverseTransformPoint(position);
        position = new Vector3(position.x, 0f, position.z); // put onto xz plane

        

        float bearing = Vector3.Angle(Vector3.forward, position);
        if (position.x > 0)
        {
            bearing *= -1;
        }

        //Debug.LogWarning("rwr bearing angle: " + bearing);

        return bearing;
    }
}
