using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechObject : MonoBehaviour
{

    public float researchTime;


    private bool isResearchComplete = true;

    public TechInventory teamInventory;

    public AbilityParent ability;

    private void Awake()
    {
        ability = GetComponent<AbilityParent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(researchTime > 0)
        {
            researchTime -= Time.deltaTime;
        }
        else if (isResearchComplete)
        {
            onResearchComplete();
        }
    }

    public void onResearchComplete()
    {
        isResearchComplete = false;
        Debug.Log(gameObject.name + " has completed research!");

        // award ability/weapon/whatever to team
        if(ability != null)
        {
            // team inventory will continue to reference script instance from this object
            teamInventory.addAbility(ability);
        }

    }

   
}
