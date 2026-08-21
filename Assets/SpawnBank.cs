using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBank : MonoBehaviour
{

    public int supplies;

    public bool showSupplies;

    TgtHudIcon hudIconRef;


    private bool firstUpdate = true;

    private bool ableToReceive = true;

    // Start is called before the first frame update
    void Start()
    {
        //myFlow.myHudIconRef.setShowSupplies(true);

        //hudIconRef = GetComponent<StrategicTarget>().myFlow.myHudIconRef;

        //hudIconRef.setShowSupplies(showSupplies);

        //updateSupplyText();


    }

    private void linkToIcon()
    {
        if (showSupplies)
        {
            hudIconRef = GetComponent<CombatFlow>().myHudIconRef;
            hudIconRef.setShowSupplies(showSupplies);
            updateSupplyText();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (firstUpdate)
        {
            linkToIcon();
            firstUpdate = false;
        }


    }

    public void setAbleToReceive(bool able)
    {
        ableToReceive = able;
    }

    public bool getAbleToReceive()
    {
        return ableToReceive;
    }



    public void resetSupplies()
    {
        supplies = 0;

        if(showSupplies)
        {
            hudIconRef.updateSupplyText(supplies);
        }
    }

    public void tryAddSupplies(int add)
    {
        if (ableToReceive)
        {
            supplies += add;
            if (showSupplies)
            {
                updateSupplyText();
            }
        }
        
    }


    public void updateSupplyText()
    {
        hudIconRef.updateSupplyText(supplies);
    }
}
