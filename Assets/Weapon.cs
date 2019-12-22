using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{

    public GameObject myTarget;
    public bool launched;
    public float armingTime;
    public float armTimeRemaining;
    public bool armed = false;
    public bool friendlyImpact;
    public bool impactOnEffects;
    public CombatFlow.Team myTeam;


    public void tryArm()
    {
        if (launched && !armed)
        {

            if (armTimeRemaining < 0)
            {
                armed = true;
                setColliders(true);
            }

            armTimeRemaining -= Time.deltaTime;

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

    }

    virtual public void linkToOwner( GameObject owner)
    {
        Debug.Log("Parent LinkToOwner called");
    }

}
