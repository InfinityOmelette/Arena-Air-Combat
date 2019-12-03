using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // TERRIBLE FUNDAMENTAL INEFFICIENCY -- COMPLICATED OBJECT FOR EVERY EXPLOSION
    

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
    public float smokeGlowDecayTime;


    public Material mat;
    private MeshRenderer rend;

    public Color emissionColor;
    public Color smokeColor;


    bool secondCallMade = false;

    private static GameObject explosionPrefab;

    public static GameObject getExplodePrefab()
    {
        if (explosionPrefab == null)
            linkExplosionPrefabRef();
        return explosionPrefab;
    }

    // STATIC METHOD OTHER OBJECTS WILL CALL
    public static void createExplosionAt(Vector3 position, float radius, float coreDamage)
    {

        GameObject newExplosion = GameObject.Instantiate(getExplodePrefab());
        newExplosion.transform.position = position;
        newExplosion.GetComponent<Explosion>().goExplode(radius, coreDamage);

    }

    public static void linkExplosionPrefabRef()
    {
        explosionPrefab = (GameObject)Resources.Load("PrefabExplosion", typeof(GameObject));
    }

    // Start is called before the first frame update
    void Start()
    {

        transform.localScale *= 0f; // start small

        // convert references to copies of original material
        mat = new Material(mat);

        mat.EnableKeyword("_EMISSION"); // lets us access material emission

        mat.SetColor("_EmissionColor", emissionColor); // emission layer will have desired flash color
        mat.color = smokeColor;    // main color will have desired smoke color

        
        rend = GetComponent<MeshRenderer>();
        rend.material = mat;
        GetComponent<Light>().intensity = flashIntensity;
        GetComponent<Light>().enabled = false;
        

        
    }

    


    // Update is called once per frame
    void Update()
    {



        

        if (doExplode)
        {

            if (!secondCallMade)
            {
                goExplode(radius, coreDamage);
                secondCallMade = true;
            }

            
            
            //Debug.Log("Light intensity: " + GetComponent<Light>().intensity.ToString());
            Light light = GetComponent<Light>();
            if (!radiusMaxed) // radius isn't maxed -- rapid expansion
            {
                

                // rapidly expand towards radius
                float rapidExpandScale = stepValOverTime(transform.localScale.x, radius, 0.0f, expandTime);
                transform.localScale = new Vector3(rapidExpandScale, rapidExpandScale, rapidExpandScale);

                // expand flash range relative to radius
                light.range = transform.localScale.x * lightRangeScaleFactor;


                // change behavior when radius is maxed
                if (Mathf.Approximately(transform.localScale.x, radius))
                {
                    

                    radiusMaxed = true;
                    GetComponent<Collider>().enabled = false; // dissipation will not collide


                    
                }


            }
            else // radius is maxed --> dissipate and expand slowly
            {
                Color color = mat.color;


                // SMOKE GLOW DECAY
                mat.SetColor("_EmissionColor", stepColorOverTime(mat.GetColor("_EmissionColor"), smokeColor, emissionColor, smokeGlowDecayTime));

                // STEP SIZE GROWTH
                float slowExpandScale = stepValOverTime(transform.localScale.x, radius * fadeRadiusScale, radius, fadeOutTime);
                transform.localScale = new Vector3(slowExpandScale, slowExpandScale, slowExpandScale);

                // STEP LIGHT INTENSITY DECAY
                light.intensity = stepValOverTime(light.intensity, 0.0f, flashIntensity, lightDecayTime);

                // STEP LIGHT RANGE INCREASE
                light.range = stepValOverTime(light.range, lightRangeScaleFactor * slowExpandScale, radius * lightRangeScaleFactor, lightDecayTime);


                //Debug.Log("Current light intensity: " + light.intensity.ToString());
                
                if (Mathf.Approximately(light.intensity, 0.0f))
                {


                    // SMOKE GLOW DECAY
                    mat.SetColor("_EmissionColor", stepColorOverTime(mat.GetColor("_EmissionColor"), smokeColor, emissionColor, smokeGlowDecayTime));
                    

                    // SMOKE ALPHA DECAY
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
