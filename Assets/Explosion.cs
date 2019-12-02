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
    public float lightDecayTime; // seconds to full decay


    private Material mat;


    // Start is called before the first frame update
    void Start()
    {
        transform.localScale *= 0f; // start small
        mat = GetComponent<MeshRenderer>().material;
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

                // set color to yellow
                mat.color = new Color(1.0f, 1.0f, 0.0f, 1.0f); // YELLOW


                // change behavior when radius is maxed
                if (Mathf.Approximately(transform.localScale.x, radius))
                {
                    mat.color = new Color(1.0f, 1.0f, 1.0f, 1.0f); // WHITE

                    radiusMaxed = true;
                    GetComponent<Collider>().enabled = false; // dissipation will not collide


                    // Set smoke alpha to 2/3. 
                    //  - for unknown reason, alpha decay would only reach .333 in specified time when alpha started at 1. This is compensating for that
                    Color color = mat.color;
                    color.a = 0.67f;
                    mat.color = color;
                }


            }
            else // radius is maxed --> dissipate and expand slowly
            {
                // STEP SIZE GROWTH
                float dissipateRadius = radius * fadeRadiusScale;
                stepSize(dissipateRadius, fadeOutTime);


                // INEFFICIENT -- LIGHT AND ALPHA DECAY AND STEP SIZE LIKELY COULD BE SIMPLIFIED TO ONE COMMON FUNCTION FOR EACH

                // STEP LIGHT DECAY
                Light light = GetComponent<Light>();
                float lightStepSize = flashIntensity * Time.deltaTime / lightDecayTime;
                light.intensity = Mathf.MoveTowards(light.intensity, 0.0f, lightStepSize);


                // STEP ALPHA DECAY
                Color color = mat.color; // copy of color data
                float colorStepSize = Time.deltaTime / (fadeOutTime); // will fully fade 1 second before deletion
                color.a = Mathf.MoveTowards(color.a, 0.0f, colorStepSize);
                mat.color = color;         // save modified values into reference material color

                Debug.Log("Current Alpha: " + mat.color.a);

                if (Mathf.Approximately(transform.localScale.x, dissipateRadius))
                {
                   // Debug.Log("Current scale: " + transform.localScale.x + ", targetRadius: " + dissipateRadius);
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
        light.intensity = flashIntensity;
    }
}
