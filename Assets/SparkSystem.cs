using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkSystem : MonoBehaviour
{

    ParticleSystem particles;

    public static float minSize = 1f;
    public static float lerpBeginRadius_size = 8f;
    public static float maxSize = 15f;
    public static float lerpEndRadius_size = 150f;

    public static float minSpeed = 320f;
    public static float lerpBeginRadius_speed = 5f;
    public static float maxSpeed = 3000f;
    public static float lerpMaxRadius_speed = 80f;

    public static float minLifeTime = .275f;
    public static float lerpBeginRadius_lifetime = 5;
    public static float maxLifeTime = .45f;
    public static float lerpMaxRadius_lifetime = 80f;

    public static int minCount = 4;
    public static float lerpBeginRadius_count = 5f;
    public static int maxCount = 30;
    public static float lerpMaxRadius_count = 30f;


    public float destroyTimer = 1f;

    private void Awake()
    {
        getParticles();
    }

    public ParticleSystem getParticles()
    {
        if(particles == null)
        {
            particles = GetComponent<ParticleSystem>();
        }
        return particles;
    }

    public void initSparkByRadius(float radius)
    {
        int sparkCount =  (int)lerpProperty(minCount, maxCount, 
            lerpBeginRadius_count, lerpMaxRadius_count, radius);

        float sparkVelocity = lerpProperty(minSpeed, maxSpeed, lerpBeginRadius_speed, 
            lerpMaxRadius_speed, radius);

        float sparkLifeTime = lerpProperty(minLifeTime, maxLifeTime, 
            lerpBeginRadius_lifetime, lerpMaxRadius_lifetime, radius);

        float sparkSize = lerpProperty(minSize, maxSize, 
            lerpBeginRadius_size, lerpEndRadius_size, radius);


        initSpark(sparkCount, sparkVelocity, sparkLifeTime, sparkSize);
    }

    public float lerpProperty(float min, float max, float lerpMinRadius, float lerpMaxRadius, float radius)
    {
        float lerpRate = Mathf.Clamp((radius - lerpMinRadius) / (lerpMaxRadius - lerpMinRadius), 0.0f, 1.0f);

        return Mathf.Lerp(min, max, lerpRate);
    }

    public void initSpark(int sparkCount, float sparkVelocity, float sparkLifetime, float sparkSize)
    {
        particles.startLifetime = sparkLifetime;
        particles.startSpeed = sparkVelocity;
        particles.startSize = sparkSize;
        particles.emission.SetBurst(0, new ParticleSystem.Burst(0.0f, sparkCount));


        destroyTimer = sparkLifetime + .1f;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void begin()
    {
        particles.Play();
    }

    private void FixedUpdate()
    {
        destroyTimer -= Time.fixedDeltaTime;

        if(destroyTimer < 0f)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
