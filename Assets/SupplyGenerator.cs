using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyGenerator : MonoBehaviour
{
    //static int DEFAULT_SUPPLY_AMOUNT = 1;

    private StrategicTarget myStrat;

    public int supplyAmt = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public StrategicTarget getStrat()
    {
        if(myStrat == null)
        {
            myStrat = GetComponent<StrategicTarget>();
        }
        return myStrat;
    }
    public bool checkSuppression()
    {
        return getStrat().isSuppressed;
    }

    public int getSupply()
    {
        int supply = 0;
        if (!checkSuppression())
        {
            supply = supplyAmt;
        }
        return supply;
    }
    
}
