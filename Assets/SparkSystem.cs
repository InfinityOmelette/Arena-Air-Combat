using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkSystem : MonoBehaviour
{

    protected ParticleSystem particles;

    //public static float sizeRandPercent = .35f;

    //public static float minSize = 1f;
    //public static float lerpBeginRadius_size = 8f;
    //public static float maxSize = 15f;
    //public static float lerpEndRadius_size = 150f;

    //public static float minSpeed = 320f;
    //public static float lerpBeginRadius_speed = 5f;
    //public static float maxSpeed = 3000f;
    //public static float lerpMaxRadius_speed = 80f;

    //public static float speedRandPercent = .5f; // percent range above and below

    //public static float minLifeTime = .275f;
    //public static float lerpBeginRadius_lifetime = 5;
    //public static float maxLifeTime = .45f;
    //public static float lerpMaxRadius_lifetime = 80f;

    //public static int minCount = 2;
    //public static float lerpBeginRadius_count = 5f;
    //public static int maxCount = 30;
    //public static float lerpMaxRadius_count = 30f;


    public float destroyTimer = 1f;

    public ParticleBurstSettings settings; // prefab reference

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

    public virtual void initSparkByRadius(float radius, int countOverride = -1)
    {

        settings.initSparkByRadius(this, radius, countOverride);
    }

    public static float lerpProperty(float min, float max, float lerpMinRadius,
        float lerpMaxRadius, float radius)
    {
        float lerpRate = Mathf.Clamp((radius - lerpMinRadius) / (lerpMaxRadius - lerpMinRadius), 0.0f, 1.0f);

        return Mathf.Lerp(min, max, lerpRate);
    }

    public void initSpark(int sparkCount, float sparkVelocity, float sparkLifetime, 
        float sparkSize, float speedRandPercent, float sizeRandPercent, float emissionRadius)
    {


        particles.startLifetime = sparkLifetime;

        float randSpeedRange = sparkVelocity * speedRandPercent;

        var main = particles.main;

        main.startSpeed = new ParticleSystem.MinMaxCurve(sparkVelocity - randSpeedRange, 
            sparkVelocity + randSpeedRange);

        float randSizeRange = sparkSize * sizeRandPercent;
        
        //particles.startSize = sparkSize;
        main.startSize = new ParticleSystem.MinMaxCurve(sparkSize - randSizeRange, 
            sparkSize + randSizeRange);

        particles.emission.SetBurst(0, new ParticleSystem.Burst(0.0f, sparkCount));

        //particles.shape.radius = emissionRadius;

        var shape = particles.shape;
        shape.radius = emissionRadius;
        //particles.

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
        selfDestructTimer();
    }

    protected void selfDestructTimer()
    {
        destroyTimer -= Time.fixedDeltaTime;

        if (destroyTimer < 0f)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
