using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TgtComputer : MonoBehaviour
{

    public hudControl mainHud;


    public GameObject lockedTarget;

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
            // =====================  VISIBILITY

            // Current CombatFlow to attempt to see
            CombatFlow currentFlow = flowArray[i].GetComponent<CombatFlow>();
            TgtHudIcon currentFlowHudIcon = currentFlow.myHudIconRef.GetComponent<TgtHudIcon>();

            // Various conditions will attempt to make this true
            bool isVisible = false; 

            // Show unit if this is NOT the local player
            if (!currentFlow.isLocalPlayer)
            {
                isVisible = true;
            }

            //  Send visibility result
            currentFlowHudIcon.isDetected = isVisible;



            //  =====================  DISTANCE

            // Distance between this gameobject and target
            currentFlowHudIcon.currentDistance = Vector3.Distance(currentFlow.transform.position, transform.position);



            // ======================== LINE OF SIGHT
            int terrainLayer = 1 << 10; // line only collides with terrain layer
            currentFlowHudIcon.hasLineOfSight = !Physics.Linecast(transform.position, currentFlow.transform.position, terrainLayer);


        }
    }
}
