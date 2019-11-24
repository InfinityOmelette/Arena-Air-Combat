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

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Input.GetKeyDown(KeyCode.C))
            currentHP -= 3;

        if (currentHP <= 0)
            die();
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
