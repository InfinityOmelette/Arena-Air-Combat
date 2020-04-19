using UnityEngine;
using System.Collections;

using Photon.Pun;
using Photon.Realtime;

public class Weapon : MonoBehaviourPunCallbacks
{

    public GameObject myTarget;
    public bool launched = false;
    public float armingTime;
    public float armTimeRemaining;
    public bool armed = false;
    public bool friendlyImpact;
    public bool impactOnEffects;

    public Hardpoint myHardpoint;

    public CombatFlow.Team myTeam;

    public float projectileLifetime;

    public GameObject effectsOriginalObj;
    public GameObject effectsObj;
    public Transform effectsCenter;

    // hardpointController will call Fire and FireEnd on all of this type simultaneously
    // hardpointcontroller will NOT step to next type
    public bool groupHardpointsTogether;
    public bool sequentialLaunch; // ultimately ignored if groupHardpointsTogether is false

    public float fireRateTimer;

    public short roundsMax;
    public short roundsRemain;


    private Vector3 previousPos;
    public float lineCastBackTime;
    private float linecastCurrentTimer;

    public Sprite iconImageSpriteFile; // inefficient -- not necessary for every weapon instance to contain this reference.

    // call from fixedUpdate()
    // either countdown reposition timer
    public void checkLinecastCollision()
    {

        if (linecastCurrentTimer > 0)
        {
            linecastCurrentTimer -= Time.deltaTime;
        }
        else
        {
            previousPos = transform.position;
            linecastCurrentTimer = lineCastBackTime;
        }

        if (armed)
        {
            RaycastHit hitInfo = new RaycastHit();
            short terrainLayer = 1 << 10; // only check collisions with terrain
            if (Physics.Linecast(previousPos, transform.position,
                out hitInfo, terrainLayer))
            {
                Debug.Log("*********************************************************************  linecast hit");
                transform.position = hitInfo.point;
                contactProcess(hitInfo.collider.gameObject);
            }
        }

    }

    public void tryArm()
    {
        if (launched && !armed) // only attempt to arm if already launched
        {

            if (armTimeRemaining < 0)
            {
                armed = true;
                Debug.Log("arming weapon");
                setColliders(true);
            }

            armTimeRemaining -= Time.deltaTime;

        }
    }


    public void killIfLifetimeOver()
    {
        if (launched)
        {
            if (projectileLifetime > 0f)
            {
                projectileLifetime -= Time.deltaTime;
            }
            else // timer ran out
            {
                // Not bothering to check if null because all Weapons should contain a CombatFlow object
                CombatFlow myFlow = GetComponent<CombatFlow>();
                myFlow.currentHP -= myFlow.currentHP;
            }
        }
    }

    public void killIfBelowFloor()
    {
        if(transform.position.y < 0f)
        {
            
            CombatFlow combatFlow = GetComponent<CombatFlow>();
            combatFlow.currentHP -= combatFlow.currentHP;
        }
            
    }


    // consider making this recursive to go into all of the children's children as well?
    public bool setColliders(bool enableColl)
    {
        //Debug.Log("SET COLLIDERS BEGINS");
        return setChildColliders(gameObject, enableColl);
    }


    public bool setChildColliders(GameObject obj, bool enableColl)
    {
        //Debug.Log("SetChildColliders called for: " + obj + ", which has " + obj.transform.childCount + " children");
        if (obj.transform.childCount > 0)
        {

            for (short i = 0; i < obj.transform.childCount; i++)
            {
                GameObject childObj = obj.transform.GetChild(i).gameObject;
                Collider childColl = childObj.GetComponent<Collider>();
                if (childColl != null)
                    childColl.enabled = enableColl;
                setChildColliders(childObj, enableColl);
            }
        }
        return enableColl;
    }

    virtual public void launch()
    {
        Debug.Log("Parent Launch called");
    }


    virtual public void launchEnd()
    {
        Debug.Log("launchEnd doing nothing");
    }

    virtual public void linkToOwner( GameObject owner)
    {
        Debug.Log("Parent LinkToOwner called");
    }


    // called repeatedly from update until reload is complete
    virtual public void reloadProcess()
    {
        Debug.Log("weapon reloadProcess doing nothing"); // should only be called if weapon does not drop on launch
    }

    public float setArmTime(float newArmTime)
    {
        armingTime = newArmTime;
        armTimeRemaining = newArmTime;
        return newArmTime;
    }

    
    public virtual void contactProcess(GameObject other)
    {

    }


    

}
