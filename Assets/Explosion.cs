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
    private bool killCollNextUpdate = false;

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

    // *********************************************************************************
    // **************************   STATIC METHODS   ***********************************
    // *********************************************************************************

    public static GameObject getExplodePrefab()
    {
        if (explosionPrefab == null)
            linkExplosionPrefabRef();
        return explosionPrefab;
    }



    // STATIC METHOD OTHER OBJECTS WILL CALL
    // caller can set:
    //  position
    //  radius
    //  damage
    //  collisions enabled
    //  dissipation time
    //  glow color
    //  emit light enabled
    public static void createExplosionAt(Vector3 position, float setRadius, float setCoreDamage,
        bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight)
    {

        GameObject newExplosion = GameObject.Instantiate(getExplodePrefab());
        newExplosion.transform.position = position;
        newExplosion.GetComponent<Explosion>().goExplode(setRadius, setCoreDamage, doCollide, dissipationTime, glowColor, doEmitLight);
        //Debug.Log("static create explosion called");
    }

    public static void linkExplosionPrefabRef()
    {
        explosionPrefab = (GameObject)Resources.Load("PrefabExplosion", typeof(GameObject));
        
    }


    // *********************************************************************************
    // **********************   NON-STATIC METHODS   ***********************************
    // *********************************************************************************


    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Light>().enabled = false;
        transform.localScale *= 0f; // start small

        // convert references to copies of original material
        mat = new Material(mat);

        mat.EnableKeyword("_EMISSION"); // lets us access material emission

        mat.SetColor("_EmissionColor", emissionColor); // emission layer will have desired flash color
        mat.color = smokeColor;    // main color will have desired smoke color

        
        rend = GetComponent<MeshRenderer>();
        rend.material = mat;
        
        
        

        
    }


    private void FixedUpdate()
    {
        if (doExplode && GetComponent<Collider>().enabled)
        {
            killCollNextUpdate = true;
        }

        if (killCollNextUpdate)
        {
            GetComponent<Collider>().enabled = false;
        }
    }


    // Update is called once per frame
    void Update()
    {



        

        if (doExplode)
        {

            

            //if (!secondCallMade)
            //{
                
            //    goExplode(radius, coreDamage, GetComponent<Collider>().enabled, fadeOutTime, emissionColor, GetComponent<Light>().enabled);
            //    secondCallMade = true;
            //}

            
            
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

    // radius
    // damage
    // dissipation time
    // glow color
    // WHEN THIS FUNCTION IS CALLED, THIS RUNS BEFORE START
    // CONSIDER MAKING THESE PROCEDURAL TO SIMPLIFY CALL
    public void goExplode(float setRadius, float setCoreDamage, bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight)
    {
        // smoke settings
        emissionColor = glowColor;
        //mat.SetColor("_EmissionColor", Color.yellow);
        radius = setRadius;
        coreDamage = setCoreDamage;
        doExplode = true;
        fadeOutTime = dissipationTime;
        GetComponent<Collider>().enabled = false;

        // sphere collider radius set to 1 in prefab. This won't change. .5 matches object radius, 1 is double


        // light settings
        Light light = GetComponent<Light>();
        light.enabled = doEmitLight;
        //light.enabled = false;
        //Debug.Log("emitLight set to: " + doEmitLight + ", setting is: " + light.enabled);
        if (doEmitLight)
        {
            //light.intensity = flashIntensity;
            light.color = glowColor;
            light.range = radius * lightRangeScaleFactor;
            //light.intensity = flashIntensity;
            Debug.Log("Light intensity set to: " + flashIntensity + ", current setting: " + light.intensity) ;
        }
        


    }


    private void OnTriggerEnter(Collider other)
    {
        // deal damage
        // apply force


        GameObject targetRootObj = other.transform.root.gameObject;

        Rigidbody targetRb = targetRootObj.GetComponent<Rigidbody>();
        CombatFlow targetCF = targetRootObj.GetComponent<CombatFlow>();




        Debug.Log("Explosion collided with: " + targetRootObj.name);

        if (targetRb != null)
        {
            targetRb.AddExplosionForce(coreDamage, transform.position, radius * 2);
            


        }

        if (targetCF != null)
        {
            targetCF.currentHP -= coreDamage;
        }
    }

    
}
