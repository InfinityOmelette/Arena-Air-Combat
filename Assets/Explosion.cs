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

    public float lightRangeScaleFactor;
    public float flashIntensity;
    public float lightDecayTime; // seconds to full decay
    public float smokeColorFadeTime;


    public Material mat;
    private MeshRenderer rend;

    public Color emissionColor;
    public Color smokeColor;


    // Start is called before the first frame update
    void Start()
    {

        transform.localScale *= 0f; // start small

        // convert references to copies of original material
        mat = new Material(mat);

        mat.EnableKeyword("_EMISSION"); // lets us access material emission

        //mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

        mat.SetColor("_EmissionColor", emissionColor); // emission layer will have desired flash color
        mat.color = smokeColor;    // main color will have desired smoke color


        rend = GetComponent<MeshRenderer>();
        rend.material = mat;   
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
                float rapidExpandScale = stepValOverTime(transform.localScale.x, radius, 0.0f, expandTime);
                transform.localScale = new Vector3(rapidExpandScale, rapidExpandScale, rapidExpandScale);

                


                // change behavior when radius is maxed
                if (Mathf.Approximately(transform.localScale.x, radius))
                {
                    

                    radiusMaxed = true;
                    GetComponent<Collider>().enabled = false; // dissipation will not collide


                    
                }


            }
            else // radius is maxed --> dissipate and expand slowly
            {
                // STEP SIZE GROWTH
                float slowExpandScale = stepValOverTime(transform.localScale.x, radius * fadeRadiusScale, radius, fadeOutTime);
                transform.localScale = new Vector3(slowExpandScale, slowExpandScale, slowExpandScale);

                // STEP LIGHT INTENSITY DECAY
                Light light = GetComponent<Light>();
                light.intensity = stepValOverTime(light.intensity, 0.0f, flashIntensity, lightDecayTime);

                // STEP LIGHT RANGE DECAY
                light.range = stepValOverTime(light.range, 0.0f, radius * lightRangeScaleFactor, lightDecayTime);



                
                if (light.range < transform.localScale.x)
                {
                    Color color = mat.color;


                    // IMMEDIATELY TURN COLOR BLACK
                    mat.SetColor("_EmissionColor", stepColorOverTime(mat.GetColor("_EmissionColor"), smokeColor, emissionColor, lightDecayTime));
                    


                    color.a = stepValOverTime(color.a, 0.0f, 1.0f, fadeOutTime - lightDecayTime); // subtracting light decay time is overkill
                    mat.color = color;         // save modified values into reference material color
                }


                

                

                if (Mathf.Approximately(transform.localScale.x, radius * fadeRadiusScale))
                {
                   // Debug.Log("Current scale: " + transform.localScale.x + ", targetRadius: " + dissipateRadius);
                    Destroy(gameObject);
                }



            }
        }
        

    }

    // STATIC METHOD OTHER OBJECTS WILL CALL
    public static void createExplosionAt(Vector3 position, float radius, float coreDamage)
    {

    }


    private Color stepColorOverTime(Color currentColor, Color targetColor, Color beginColor, float timeToCompletion)
    {
        currentColor.r = stepValOverTime(currentColor.r, targetColor.r, beginColor.r, timeToCompletion);
        currentColor.g = stepValOverTime(currentColor.g, targetColor.g, beginColor.g, timeToCompletion);
        currentColor.b = stepValOverTime(currentColor.b, targetColor.b, beginColor.b, timeToCompletion);
        currentColor.a = stepValOverTime(currentColor.a, targetColor.a, beginColor.a, timeToCompletion);
        return currentColor;
    }

    private float stepValOverTime(float currentVal, float targetVal, float beginVal, float timeToCompletion)
    {
        // step blast size
        float difference = Mathf.Abs(targetVal - beginVal);
        float stepSize = difference * Time.deltaTime / timeToCompletion;
        return Mathf.MoveTowards(currentVal, targetVal, stepSize);
    }

    public void goExplode(float setRadius, float setCoreDamage)
    {
        radius = setRadius;
        coreDamage = setCoreDamage;
        doExplode = true;

        Light light = GetComponent<Light>();
        light.enabled = true;
        light.range = radius * lightRangeScaleFactor;
        light.intensity = flashIntensity;


    }
}
