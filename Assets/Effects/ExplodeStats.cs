using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExplodeStats : MonoBehaviourPunCallbacks
{

    public float radius;
    public float damage;
    public bool collisionsEnabled;
    public float dissipationTime;
    public bool emitLightEnabled;
    public Color glowColor;
    public Color smokeColor;
    public float expandTime;
    public float explosiveForce;

    public bool damageProjectiles;
    public bool friendlyFire;
    public CombatFlow.Team team;


    private CombatFlow myFlow;

    public bool doExplode = true;

    [SerializeField]
    private float armingTime;
    

    // Start is called before the first frame update
    void Start()
    {
        myFlow = transform.root.gameObject.GetComponent<CombatFlow>();
        if(myFlow != null)
            team = myFlow.team;
        else
        {
            //Debug.Log("Unable to find combatFlow for: " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (armingTime > 0)
        {
            armingTime -= Time.deltaTime;
        }
    }

    //public void netExplodeRelative(CombatFlow victim, Vector3 position)
    //{

    //}

    public void cosmeticLocalExplode(Vector3 position)
    {
        if (doExplode && armingTime <= 0)
        {
            // no damage, collider disabled, no explosive force
            Explosion.createExplosionAt(position, radius, 0f, false, dissipationTime, glowColor, emitLightEnabled, smokeColor,
                expandTime, team, damageProjectiles, friendlyFire, 0f);
        }
    }

    public void explode(Vector3 position)
    {
        if (doExplode && armingTime <= 0)
        {
            Explosion.createExplosionAt(position, radius, damage, collisionsEnabled, dissipationTime, glowColor, emitLightEnabled, smokeColor,
                expandTime, team, damageProjectiles, friendlyFire, explosiveForce);
        }
    }

    public void netExplode(Vector3 position)
    {
        if (doExplode && armingTime <= 0)
        {
            ExplodeManager.getExplodeManager().createNetExplosionAt(position, radius, damage, collisionsEnabled, dissipationTime, glowColor, emitLightEnabled, smokeColor,
                expandTime, team, damageProjectiles, friendlyFire, explosiveForce);
        }
    }

    

}
