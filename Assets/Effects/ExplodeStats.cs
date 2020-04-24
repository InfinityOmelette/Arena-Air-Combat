﻿using System.Collections;
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
        
    }

    public void explode(Vector3 position)
    {
        if (doExplode)
        {
            Explosion.createExplosionAt(position, radius, damage, collisionsEnabled, dissipationTime, glowColor, emitLightEnabled, smokeColor,
                expandTime, team, damageProjectiles, friendlyFire, explosiveForce);
        }
    }

    public void netExplode(Vector3 position)
    {
        if (doExplode)
        {
            ExplodeManager.getExplodeManager().createNetExplosionAt(position, radius, damage, collisionsEnabled, dissipationTime, glowColor, emitLightEnabled, smokeColor,
                expandTime, team, damageProjectiles, friendlyFire, explosiveForce);
        }
    }

    

}
