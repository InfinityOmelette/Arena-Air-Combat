using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class ExplodeManager : MonoBehaviourPunCallbacks
{

    public static string ExplodeManagerName = "ExplosionManager";
    private static ExplodeManager explodeManager;

    public GameObject explodePrefab;

    public static ExplodeManager getExplodeManager()
    {
        if (explodeManager == null)
        {
            explodeManager = GameObject.Find(ExplodeManagerName).GetComponent<ExplodeManager>();
        }
        return explodeManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createExplosionAt(Vector3 position, float setRadius, float setCoreDamage,
        bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight, Color newSmokeColor,
        float newExpandTime, CombatFlow.Team newTeam, bool newDamageProjectiles, bool newFriendlyFire,
        float newExplosiveForce)
    {
        // If the weapon that initiated the explosion is not local-owned, it will have
        //  to set the damage, doCollide, and explosive force to zero

        // create local-only explosion
        rpcCreateExplosionAt(position, setRadius, setCoreDamage,
        doCollide, dissipationTime, glowColor, doEmitLight, newSmokeColor,
        newExpandTime, CombatFlow.convertTeamToNum(newTeam), newDamageProjectiles, newFriendlyFire,
        newExplosiveForce);

    }

    public void createNetExplosionAt(Vector3 position, float setRadius, float setCoreDamage,
        bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight, Color newSmokeColor,
        float newExpandTime, CombatFlow.Team newTeam, bool newDamageProjectiles, bool newFriendlyFire,
        float newExplosiveForce)
    {
        // create local explosion, will deal damage
        rpcCreateExplosionAt(position, setRadius, setCoreDamage,
        doCollide, dissipationTime, glowColor, doEmitLight, newSmokeColor,
        newExpandTime, CombatFlow.convertTeamToNum(newTeam), newDamageProjectiles, newFriendlyFire,
        newExplosiveForce);

        // create networked explosion, cosmetic only
        //  zero damage, will not collide, zero explosive force
        photonView.RPC("rpcCreateExplosionAt", RpcTarget.Others, position, setRadius, 0,
        false, dissipationTime, glowColor, doEmitLight, newSmokeColor,
        newExpandTime, CombatFlow.convertTeamToNum(newTeam), newDamageProjectiles, newFriendlyFire,
        0);

    }

    public void rpcCreateExplosionAt(Vector3 position, float setRadius, float setCoreDamage,
        bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight, Color newSmokeColor,
        float newExpandTime, short newTeamNum, bool newDamageProjectiles, bool newFriendlyFire,
        float newExplosiveForce)
    {
        GameObject newExplosion = GameObject.Instantiate(explodePrefab);
        newExplosion.transform.position = position;
        
        Explosion newExplosionScript = newExplosion.GetComponent<Explosion>();
        //newExplosionScript.localOwned = localOwned;
        CombatFlow.Team newTeam = CombatFlow.convertNumToTeam(newTeamNum);


        // smoke settings
        newExplosionScript.emissionColor = glowColor;
        newExplosionScript.smokeColor = newSmokeColor;

        //mat.SetColor("_EmissionColor", Color.yellow);
        newExplosionScript.radius = setRadius;
        newExplosionScript.coreDamage = setCoreDamage;
        newExplosionScript.doExplode = true;
        newExplosionScript.fadeOutTime = dissipationTime;
        newExplosionScript.GetComponent<Collider>().enabled = doCollide;

        newExplosionScript.expandTime = newExpandTime;
        newExplosionScript.team = newTeam;
        newExplosionScript.damageProjectiles = newDamageProjectiles;
        newExplosionScript.friendlyFire = newFriendlyFire;
        newExplosionScript.explosiveForce = newExplosiveForce;

        // light settings
        Light light = newExplosionScript.GetComponent<Light>();
        light.enabled = doEmitLight;

        if (doEmitLight)
        {

            light.color = glowColor;
            light.range = newExplosionScript.radius * newExplosionScript.lightRangeScaleFactor;
        }

    }



}
