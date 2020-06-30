using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningComputer : MonoBehaviour
{

    public List<IconRWR> incomingMissiles;

    public GameObject missileWarning;

    private bool warningActive = false;

    public float missileWarningBlinkDelayMax;
    private float missileWarningBlinkTimer;

    public float missileWarningBlinkOffset;

    public float missileRangeLong;

    public float missileRangeMultiplier = 1.0f;


    void Awake()
    {
        incomingMissiles = new List<IconRWR>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    void Update()
    {
        
        warningActive = incomingMissiles.Count > 0;

        missileRangeMultiplier = calculateMissileRangeMult();

        processMissileWarning(warningActive);
    }

    private float calculateMissileRangeMult()
    {
        float shortestRange = missileRangeLong;
        for(int i = 0; i < incomingMissiles.Count; i++)
        {
            float range = incomingMissiles[i].distance;
            if(range < shortestRange)
            {
                shortestRange = range;
            }
        }

        return shortestRange / missileRangeLong;
    }

    private void processMissileWarning(bool incoming)
    {
        if (incoming)
        {

            if (missileWarningBlinkTimer <= 0)
            {
                missileWarning.SetActive(!missileWarning.active);
                missileWarningBlinkTimer = missileWarningBlinkDelayMax * missileRangeMultiplier + missileWarningBlinkOffset;
            }
            else
            {
                missileWarningBlinkTimer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            missileWarning.SetActive(false);
        }
    }


    public void addMissileIncoming(IconRWR mslIcon)
    {
        //if (!mslIcon.hasPinged)
        //{

        //}
        if (!incomingMissiles.Contains(mslIcon))
        {
            incomingMissiles.Add(mslIcon);
        }
    }

    public void removeMissileIncoming(IconRWR mslIcon)
    {
        if (incomingMissiles.Contains(mslIcon))
        {
            incomingMissiles.Remove(mslIcon);
        }
    }
}
