using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hangar : MonoBehaviour
{

    public TeamSpawner teamSpawner;
    public StrategicTarget myStrat;

    public float respawnPenaltyWhenSuppressed;

    public bool applyingSpawnPenalty = false;

    private void Awake()
    {
        myStrat = GetComponent<StrategicTarget>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if(applyingSpawnPenalty != myStrat.isSuppressed)
        {
            setSpawnPenalty(myStrat.isSuppressed);
        }



    }

    public void setSpawnPenalty(bool doPenalize)
    {
        Debug.Log("Setting spawn penalty: " + doPenalize);

        if (doPenalize)
        {
            teamSpawner.respawnTimeEffective += respawnPenaltyWhenSuppressed;
        }
        else
        {
            teamSpawner.respawnTimeEffective -= respawnPenaltyWhenSuppressed;
        }
        applyingSpawnPenalty = doPenalize;
    }

}
