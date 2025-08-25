using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepSpawnPoint : MonoBehaviour
{

    public StrategicTarget myStrat;


    // Lane manager handles timing and spawn triggering as usual
    // but propogates spawn copies to each factory's position in lane

    public LaneManager parentLane;

    private void Awake()
    {
        myStrat = GetComponent<StrategicTarget>();
    }

    // Start is called before the first frame update
    void Start()
    {
        parentLane = myStrat.myLane;
    }

    void FixedUpdate()
    {
        updateLaneRef();
    }



    private void updateLaneRef()
    {
        if (parentLane != myStrat.myLane)
        {
            if(parentLane != null)
            {
                parentLane.spawnFactories.Remove(this);
            }
            

            parentLane = myStrat.myLane;
            parentLane.spawnFactories.Add(this);
        }
    }
}
