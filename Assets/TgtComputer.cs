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

        List<GameObject> flowArray = CombatFlow.combatUnits;

        // Loop through all combatUnits
        for(int i = 0; i < CombatFlow.combatUnits.Count; i++)
        {
            // attempt to see target
            CombatFlow currentFlow = flowArray[i].GetComponent<CombatFlow>();

            if (currentFlow.isLocalPlayer)
            {
                currentFlow.myHudIconRef.GetComponent<TgtHudIcon>().isVisible = false;
            }
            else
            {
                currentFlow.myHudIconRef.GetComponent<TgtHudIcon>().isVisible = true;
            }
            
            
            

        }
    }
}
