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
    
    // inefficient -- lots of non-player combat objects will have useless perspective references
    public PerspectiveManager camManager;
    public GameObject unitCam;

    public Team team;
    public Type type;
    public bool isAlive = true;


    // Start is called before the first frame update
    void Start()
    {
        if(isLocalPlayer)
            camChange(CamProperties.CamType.PLAYER);
        
    }

    

    void FixedUpdate()
    {
        if (currentHP <= 0 && isAlive) // 0hp and is currently alive
            die(); // kill self
    }

    // Update is called once per frame
    private void Update()
    {
        // putting this in Update so that frame freeze doesn't repeat damage for each physics step during freeze
        if (Input.GetKeyDown(KeyCode.C) && isLocalPlayer)
            currentHP -= 3;
    }


    void die()
    {
        isAlive = false; // he ded now
        if(isLocalPlayer)
            camChange(CamProperties.CamType.SPECTATOR);
        explode();
        destroySelf();

    }

    void camChange(CamProperties.CamType camType)
    {
        if(camManager != null)
            camManager.switchToType(camType);
    }

    void explode()
    {

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
