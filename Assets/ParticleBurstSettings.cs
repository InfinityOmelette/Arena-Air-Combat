using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBurstSettings : MonoBehaviour
{

    public float sizeRandPercent = .35f;

    public float minSize = 1f;
    public float lerpBeginRadius_size = 8f;
    public float maxSize = 15f;
    public float lerpEndRadius_size = 150f;

    public float minSpeed = 320f;
    public float lerpBeginRadius_speed = 5f;
    public float maxSpeed = 3000f;
    public float lerpMaxRadius_speed = 80f;

    public float speedRandPercent = .5f; // percent range above and below

    public float minLifeTime = .275f;
    public float lerpBeginRadius_lifetime = 5;
    public float maxLifeTime = .45f;
    public float lerpMaxRadius_lifetime = 80f;

    public int minCount = 2;
    public float lerpBeginRadius_count = 5f;
    public int maxCount = 30;
    public float lerpMaxRadius_count = 30f;

    public float minRadius;

    public float emissionRadiusPercent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void initSparkByRadius(SparkSystem spark, float radius, int countOverride = -1)
    {
        int sparkCount;

        if (countOverride < 0)
        {
            sparkCount = (int)lerpProperty(minCount, maxCount,
            lerpBeginRadius_count, lerpMaxRadius_count, radius);
        }
        else
        {
            sparkCount = countOverride;
        }


        float sparkVelocity = lerpProperty(minSpeed, maxSpeed, lerpBeginRadius_speed,
            lerpMaxRadius_speed, radius);

        float sparkLifeTime = lerpProperty(minLifeTime, maxLifeTime,
            lerpBeginRadius_lifetime, lerpMaxRadius_lifetime, radius);

        float sparkSize = lerpProperty(minSize, maxSize,
            lerpBeginRadius_size, lerpEndRadius_size, radius);


        float emissionRadius = emissionRadiusPercent * radius;

        spark.initSpark(sparkCount, sparkVelocity, sparkLifeTime, sparkSize, 
            speedRandPercent, sizeRandPercent, emissionRadius);
    }

    public float lerpProperty(float min, float max, float lerpMinRadius, 
        float lerpMaxRadius, float radius)
    {
        float lerpRate = Mathf.Clamp((radius - lerpMinRadius) / (lerpMaxRadius - lerpMinRadius), 0.0f, 1.0f);

        return Mathf.Lerp(min, max, lerpRate);
    }
}
