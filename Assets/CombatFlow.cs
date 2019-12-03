using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatFlow : MonoBehaviour
{

    public enum Team 
    { 
        RED, BLUE, NEUTRAL 
    }

    public enum Type
    {
        AIRCRAFT, PROJECTILE
    }
    
    public float maxHP;
    public float currentHP;
    public bool isLocalPlayer;


    public float explosionRadius; // damage falls off linearly from maximum at center, to zero at radius
    public float explosionMaxDamage;
    
    // inefficient -- lots of non-player combat objects will have useless perspective references
    public PerspectiveManager camManager;
    public GameObject unitCam; // leave null if item won't have its own camera

    public Team team;
    public Type type;
    public bool isAlive = true;


    // Start is called before the first frame update
    void Start()
    {
        if(isLocalPlayer)
            camChange(PerspectiveProperties.CamType.PLAYER);
        
    }

    

    void FixedUpdate()
    {
        if (currentHP <= 0 && isAlive) // 0hp and is currently alive
            die(); // kill self
    }

    // Update is called once per frame
    private void Update()
    {
        // Debug key to test damage player
        if (Input.GetKeyDown(KeyCode.C) && isLocalPlayer)
            currentHP -= 3;

        // Debug key to test damage all NPC's with this script
        if (Input.GetKeyDown(KeyCode.V) && !isLocalPlayer)
        {
            currentHP -= 30;
        }
    }


    void die()
    {
        isAlive = false; // he ded now
        if(isLocalPlayer)
            camChange(PerspectiveProperties.CamType.SPECTATOR);
        explode();
        destroySelf();

    }

    void camChange(PerspectiveProperties.CamType camType)
    {
        if(camManager != null)
            camManager.switchToType(camType);
    }

    void explode()
    {
        // blast radius deals damage
        // explosion render
    }

    void destroySelf()
    {
        if(camManager != null)
        {
            // remove this camera from perspectiveManager's list
            //  Is this a crusty way of doing this? Might be perspectiveManager's responsibility instead?
            camManager.cameras.Remove(unitCam);
        }
        Destroy(gameObject);
    }



}
