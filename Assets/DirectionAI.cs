using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionAI : MonoBehaviour
{
    public CamManipulation camRef;

    public RealFlightControl flight;

    public CombatFlow myFlow;

    public Vector3 inputDir;

    private float rollCommand;
    private float effectiveRollCommand;

    public float currentBankAngle;
    public float zAxizAngle;


    void Awake()
    {
        flight = GetComponent<RealFlightControl>();
        myFlow = GetComponent<CombatFlow>();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    void FixedUpdate()
    {
        if(myFlow.isLocalPlayer && camRef != null)
        {
            inputDir = camRef.worldLockedLookDirection;
        }

        currentBankAngle = Quaternion.ToEulerAngles(transform.rotation).z * Mathf.Rad2Deg;

    }


    
}
