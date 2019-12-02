using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float radius;
    public float coreDamage; // damage falls off linearly from max at core to zero at radius

    public float expandTime; // number of seconds
    public float fadeOutTime; // number of seconds -- graphic will continue to expand without dealing damage
    public float fadeRadiusScale;

    private bool doExplode = false;
    private bool radiusMaxed = false;

    public float lightRange;
    public float flashIntensity;
    public float lightDecayFactor;
    

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale *= 0f; // start small
        GetComponent<Light>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            goExplode(radius, coreDamage);
        }

        if (doExplode)
        {
            if (!radiusMaxed) // radius isn't maxed -- rapid expansion
            {
                // rapidly expand towards radius
                stepSize(radius, expandTime);

                // change behavior when radius is maxed
                if (Mathf.Approximately(transform.localScale.x, radius))
                {
                    radiusMaxed = true;
                    GetComponent<Collider>().enabled = false; // dissipation will not collide
                    
                }


            }
            else // radius is maxed --> dissipate and expand slowly
            {
                float dissipateRadius = radius * fadeRadiusScale;
                stepSize(dissipateRadius, fadeOutTime);
                Light light = GetComponent<Light>();
                light.intensity *= lightDecayFactor;

                if (Mathf.Approximately(transform.localScale.x, dissipateRadius))
                {
                    Destroy(gameObject);
                }



            }
        }
        

    }

    private void stepSize(float maxRadius, float myExpandTime)
    {
        // step blast size
        float stepSize = maxRadius * Time.deltaTime / myExpandTime;
        float scale = transform.localScale.x;
        scale = Mathf.MoveTowards(scale, maxRadius, stepSize);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void goExplode(float setRadius, float setCoreDamage)
    {
        radius = setRadius;
        coreDamage = setCoreDamage;
        doExplode = true;

        Light light = GetComponent<Light>();
        light.enabled = true;
        light.range = lightRange;
    }
}
