using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    public float fireRateDelay;
    private float fireRateTimer;

    public float reloadDelay;
    private float reloadTimer;

    public int roundsPerMag;
    private int roundsInCurrentMag;

    private bool roundChambered = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        tryReload(Time.fixedDeltaTime);
        tryChamber(Time.fixedDeltaTime);

    }

    private void tryChamber(float deltaTime)
    {
        if(!roundChambered && roundsInCurrentMag > 0)
        {
            if(fireRateTimer > 0)
            {
                fireRateTimer -= deltaTime;
            }
            else // wait done, chamber round
            {
                roundChambered = true;
                fireRateTimer = fireRateDelay;
            }
        }
    }

    public bool tryShoot()
    {
        bool doShoot = roundChambered;
        if (doShoot)
        {
            spendAmmo();
            roundChambered = false;
        }
        return doShoot;
    }

    private void spendAmmo(int ammoCost = 1)
    {
        roundsInCurrentMag -= ammoCost;
        
    }

    private void tryReload(float deltaTime)
    {
        if(roundsInCurrentMag <= 0)
        {
            if (reloadTimer > 0) // wait for reload
            {
                reloadTimer -= deltaTime;
            }
            else // wait complete, perform reload
            {
                reloadTimer = reloadDelay;
                roundsInCurrentMag = roundsPerMag;

            }

        }
    }
}
