using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeContactor : MonoBehaviour
{
    public Collider coll;

    private ExplosionSizer sizer;

    public bool friendlyFire = false;
    public bool damageProjectiles = false;
    public float coreDamage = 0.0f;
    public float explosiveForce;

    private List<GameObject> explosionVictimRootList;

    public CombatFlow.Team team;

    private void Awake()
    {
        explosionVictimRootList = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private ExplosionSizer getSizer()
    {
        if(sizer == null)
        {
            sizer = GetComponent<ExplosionSizer>();
        }
        return sizer;
    }

    private float getRadius()
    {
        return getSizer().radius;
    }

    // theoretically, this should not be called unless collider is active
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Explosion collided with " + collision.other.gameObject);
        explosionContactProcess(collision.gameObject);
    }

    // theoretically, this should not be called unless collider is active
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Explosion triggered on: " + other.gameObject);
        explosionContactProcess(other.gameObject);
    }

    // theoretically, this should not be called unless collider is active
    private void explosionContactProcess(GameObject victim)
    {
        // deal damage
        // apply force


        GameObject targetRootObj = victim.transform.root.gameObject;
        //Debug.Log("Explosion contacted: " + victim.name + " with root: " + targetRootObj.name);

        // only act upon victim root if he is a new victim
        if (isNewVictim(targetRootObj))
        {
            explosionVictimRootList.Add(targetRootObj);


            Rigidbody targetRb = targetRootObj.GetComponent<Rigidbody>();
            CombatFlow targetCF = targetRootObj.GetComponent<CombatFlow>();


            // whether or not explosion should act upon victim
            bool doAct = true; //various conditions will try to make this false


            //  if target has a CombatFlow
            if (targetCF != null)
            {
                // if target is friendly, and friendlyFire is disabled
                if (targetCF.team == team && !friendlyFire)
                    doAct = false;

                // if target is a Projectile, and 
                if (targetCF.type == CombatFlow.Type.PROJECTILE && !damageProjectiles)
                    doAct = false;

            }


            //  if explosion will act upon target
            if (doAct)
            {
                // if target has a rigidBody, add explosive force
                if (targetRb != null)
                {
                    targetRb.AddExplosionForce(explosiveForce, transform.position, getRadius());
                }

                if (targetCF != null && coreDamage > 0)
                {
                    //Debug.Log("Explosion acting upon victim: " + targetRootObj + " for " + coreDamage + " damage, list count: " + explosionVictimRootList.Count);
                    //targetCF.currentHP -= coreDamage;
                    targetCF.dealDamage(coreDamage);
                }
            }

        }
    }


    private bool isNewVictim(GameObject victimRoot)
    {
        bool isNewVictim = true;

        string victimListContents = "Victim list (size " + explosionVictimRootList.Count.ToString() + ") contents: ";
        for (short i = 0; i < explosionVictimRootList.Count; i++)
        {
            //victimListContents += "(" + explosionVictimRootList[i].name + "), ";
            if (victimRoot == explosionVictimRootList[i])
            {
                isNewVictim = false;
            }


        }
        // Debug.Log("Is new victim?: " + isNewVictim + " from " + victimListContents);
        return isNewVictim;
    }
    public void init(bool damageProjectiles, int teamNum, float coreDamage, bool friendlyFire)
    {
        init(damageProjectiles, (CombatFlow.Team)teamNum, coreDamage, friendlyFire);
    }

    public void init(bool damageProjectiles, CombatFlow.Team team, float coreDamage, bool friendlyFire)
    {
        this.friendlyFire = friendlyFire;
        this.damageProjectiles = damageProjectiles;
        this.team = team;
        this.coreDamage = coreDamage;
    }
}
