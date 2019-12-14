using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TgtComputer : MonoBehaviour
{

    public hudControl mainHud;


    // Start is called before the first frame update
    void Start()
    {
        mainHud = hudControl.mainHud.GetComponent<hudControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // once per frame, after physics update
    private void LateUpdate()
    {

        // Loop through all combatUnits
        for(int i = 0; i < CombatFlow.combatUnits.Count; i++)
        {

            // attempt to see combatUnits here

            // if visible, draw HUD item at position
            // otherwise, hide HUD item (move to useless position or set to 
            

        }
    }
}
