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
    public Team team;
    public Type type;
    public string unitName;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    void FixedUpdate()
    {
        if (currentHP <= 0)
            die();
    }

    // Update is called once per frame
    private void Update()
    {
        // putting this in Update so that frame freeze doesn't repeat damage for each physics step during freeze
        if (Input.GetKeyDown(KeyCode.C))
            currentHP -= 3;
    }


    void die()
    {
        if(isLocalPlayer)
            camChange();
        explode();
        destroySelf();

    }

    void camChange()
    {

    }

    void explode()
    {

    }

    void destroySelf()
    {
        
    }



}
