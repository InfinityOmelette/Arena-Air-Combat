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

    // load these values on weapon select
    public bool useDropComputer;
    public float dropInitSpeed;
    public float dropComputerMaxRange;

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

    public bool networkImpact = false;


    public bool lineCastViaDistance;
    public float linecastDistance;

    public EffectsBehavior effectsBehavior;

    public AudioClip launchSound;

    //protected PhotonView photonView;

    // call from fixedUpdate()
    // either countdown reposition timer
    public void checkLinecastCollision()
    {
        if (lineCastViaDistance)
        {
            // project line toward rear
            previousPos = transform.position - transform.forward * linecastDistance;
        }
        else
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
        }

        if (armed)
        {
            RaycastHit hitInfo = new RaycastHit();
            short terrainLayer = 1 << 10; // only check collisions with terrain
            if (Physics.Linecast(previousPos, transform.position,
                out hitInfo, terrainLayer))
            {
                Debug.DrawLine(transform.position, hitInfo.point, Color.green, 1.0f);

                // hitInfo.point is confirmed absolutely to be correct. transform isn't moving as it should.
                //  - explosion triggering before able to move?

                Debug.Log("*********************************************************************  linecast hit");
                transform.position = hitInfo.point;

                int id = -1;
                GameObject otherRootObj = hitInfo.collider.transform.root.gameObject;
                CombatFlow otherFlow = otherRootObj.GetComponent<CombatFlow>();
                if(otherFlow != null)
                {
                    id = otherRootObj.GetComponent<PhotonView>().ViewID;
                }

                //if (explodeOnOther(otherRootObj))
                {
                    if (networkImpact)
                    {
                        photonView.RPC("rpcContactProcess", RpcTarget.AllBuffered, transform.position,
                        id);
                    }
                    else // local impact
                    {
                        //Debug.LogError("MYERROR: linecast triggered");
                        //rpcContactProcess(transform.position, id);
                        impactLocal(transform.position, otherRootObj);
                    }
                    
                }

                
            }
        }

    }

    public int getVictimId(GameObject other)
    {
        int id = -1;
        if(other != null)
        {
            CombatFlow otherFlow = other.GetComponent<CombatFlow>();

            if (otherFlow != null)
            {
                id = other.GetComponent<PhotonView>().ViewID;
            }
        }
        return id;
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
                //myFlow.currentHP -= myFlow.currentHP;
                myFlow.dealLocalDamage(myFlow.getHP());
            }
        }
    }

    public void killIfBelowFloor()
    {
        CombatFlow combatFlow = GetComponent<CombatFlow>();
        if (transform.position.y < 0f && combatFlow.localOwned)
        {
            combatFlow.dealDamage(combatFlow.getHP());
            
            //combatFlow.currentHP -= combatFlow.currentHP;
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

    virtual public void destroyWeapon()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public float setArmTime(float newArmTime)
    {
        armingTime = newArmTime;
        armTimeRemaining = newArmTime;
        return newArmTime;
    }


    [PunRPC]
    public virtual void rpcContactProcess(Vector3 position, int otherId)
    {

    }

    
    public virtual bool explodeOnOther(GameObject other)
    {
        bool doAct = true; // various conditions will try to make this false

        CombatFlow otherFlow = null;
        if(other != null)
        {
            otherFlow = other.GetComponent<CombatFlow>();
        }

        // impact with effects
        if (other.CompareTag("Effects") && !impactOnEffects)
            doAct = false;


        if (otherFlow != null)
        {
            if (otherFlow.team == myTeam && !friendlyImpact)
                doAct = false;
        }
        return doAct;
    }

    public virtual void impactLocal(Vector3 position, GameObject other)
    {

    }



    

}
