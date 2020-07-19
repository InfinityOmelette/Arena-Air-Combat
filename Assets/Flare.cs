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


    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {

        burning = true;
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

            GetComponent<TrailRenderer>().emitting = false;
            GetComponent<Light>().enabled = false;

            sphere.SetActive(false);

            audio.Stop();

            GetComponent<ParticleSystem>().Stop();
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
