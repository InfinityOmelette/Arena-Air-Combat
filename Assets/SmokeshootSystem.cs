using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeshootSystem : SparkSystem
{

    new public static float minSize = 5f;
    new public static float lerpBeginRadius_size = 10f;
    new public static float maxSize = 12f;
    new public static float lerpEndRadius_size = 40f;

    new public static float minSpeed = 120f;
    new public static float lerpBeginRadius_speed = 10f;
    new public static float maxSpeed = 220f;
    new public static float lerpMaxRadius_speed = 65f;

    //new public static float speedRandPercent = .5f; // percent range above and below

    new public static float minLifeTime = 1.75f;
    new public static float lerpBeginRadius_lifetime = 10f;
    new public static float maxLifeTime = 4.25f;
    new public static float lerpMaxRadius_lifetime = 100f;

    new public static int minCount = 3;
    new public static float lerpBeginRadius_count = 10f;
    new public static int maxCount = 8;
    new public static float lerpMaxRadius_count = 100f;

    public static float minRadius = 9f;


    private void Awake()
    {
        getParticles();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void initSparkByRadius(float radius)
    {
        //base.initSparkByRadius(radius);

        int sparkCount = (int)lerpProperty(minCount, maxCount,
            lerpBeginRadius_count, lerpMaxRadius_count, radius);

        float sparkVelocity = lerpProperty(minSpeed, maxSpeed, lerpBeginRadius_speed,
            lerpMaxRadius_speed, radius);

        float sparkLifeTime = lerpProperty(minLifeTime, maxLifeTime,
            lerpBeginRadius_lifetime, lerpMaxRadius_lifetime, radius);

        float sparkSize = lerpProperty(minSize, maxSize,
            lerpBeginRadius_size, lerpEndRadius_size, radius);


        initSpark(sparkCount, sparkVelocity, sparkLifeTime, sparkSize);
    }

    private void FixedUpdate()
    {
        selfDestructTimer();
    }
}
