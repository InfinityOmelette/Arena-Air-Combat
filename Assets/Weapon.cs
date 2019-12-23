using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
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


    // hardpointController will call Fire and FireEnd on all of this type simultaneously
    // hardpointcontroller will NOT step to next type
    public bool groupHardpointsTogether;




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


    public float setArmTime(float newArmTime)
    {
        armingTime = newArmTime;
        armTimeRemaining = newArmTime;
        return newArmTime;
    }

}
