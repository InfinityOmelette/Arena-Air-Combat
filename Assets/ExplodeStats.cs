using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeStats : MonoBehaviour
{

    public float radius;
    public float damage;
    public bool collisionsEnabled;
    public float dissipationTime;
    public bool emitLightEnabled;
    public Color glowColor;
    public Color smokeColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void explode(Vector3 position)
    {
        Explosion.createExplosionAt(position, radius, damage, collisionsEnabled, dissipationTime, glowColor, emitLightEnabled, smokeColor);
    }

}
