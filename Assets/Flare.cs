using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flare : MonoBehaviour
{

    public float burnTime;
    public float lifeTime;

    private bool burning;

    public GameObject sphere;

    private AudioSource audio;

    private ParticleSystem sparkParticles;
    public ParticleSystem smokeParticles;

    Light myLight;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        myLight = GetComponent<Light>();
        sparkParticles = GetComponent<ParticleSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {

        burning = true;
        smokeParticles.Play();
        sparkParticles.Play();
    }

    // Update is called once per frame
    void Update()
    {
        countDownBurn();

        countDownLife();
    }
    
    private void countDownBurn()
    {
        if(burning && burnTime > 0f)
        {
            burnTime -= Time.deltaTime;
        }
        else
        {
            burning = false;

            //GetComponent<TrailRenderer>().emitting = false;
            myLight.enabled = false;

            sphere.SetActive(false);
            smokeParticles.Stop();
            audio.Stop();

            sparkParticles.Stop();
        }
    }

    private void countDownLife()
    {
        if(lifeTime > 0f)
        {
            lifeTime -= Time.deltaTime;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
