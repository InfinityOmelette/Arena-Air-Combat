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

    private List<GameObject> explosionVictimRootList;


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
        bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight, Color newSmokeColor)
    {
        //Debug.Log("Explosion settings: position: " + position + ", radius: " + setRadius + ", damage: " + setCoreDamage + " doCollide: " + doCollide +
         //   ", dissipationTime: " + dissipationTime + "glowColor: " + glowColor + ", doEmitLight: " + doEmitLight + ", newSmokeColor: " + newSmokeColor);
        GameObject newExplosion = GameObject.Instantiate(getExplodePrefab());
        newExplosion.transform.position = position;
        newExplosion.GetComponent<Explosion>().goExplode(setRadius, setCoreDamage, doCollide, dissipationTime, glowColor, doEmitLight, newSmokeColor);
        //Debug.Log("***********************************************  static create explosion called");
    }

    public static void linkExplosionPrefabRef()
    {
        explosionPrefab = (GameObject)Resources.Load("PrefabExplosion", typeof(GameObject));
        
    }


    // *********************************************************************************
    // **********************   NON-STATIC METHODS   ***********************************
    // *********************************************************************************

    private void Awake()
    {
        // material reference points to copy of original -- each explosion has its own material
        mat = new Material(mat);
        explosionVictimRootList = new List<GameObject>();

    }

    // Start is called before the first frame update
    void Start()
    {

        //Debug.Log("Start called");

        //GetComponent<Light>().enabled = false;
        transform.localScale *= 0f; // start small
        
        

        setEmissionEnabled(true);
        

        mat.SetColor("_EmissionColor", emissionColor); // emission layer will have desired flash color
        mat.color = smokeColor;    // main color will have desired smoke color

        
        rend = GetComponent<MeshRenderer>();
        rend.material = mat;
        
        
        

        
    }

    bool setEmissionEnabled(bool doEmit)
    {

        if(doEmit)
            mat.EnableKeyword("_EMISSION"); // lets us access material emission
        else
            mat.DisableKeyword("_EMISSION"); // lets us access material emission

        return doEmit;
    }


    private void FixedUpdate()
    {

        // Start explosion

        //Collider coll = GetComponent<Collider>();

        //if (doExplode && coll.enabled)
        //{
        //    killCollNextUpdate = true;
        //    SphereCollider sphereColl = (SphereCollider)coll;
        //    sphereColl.radius = radius;
        //}

        //if (killCollNextUpdate)
        //{
        //    coll.enabled = false;
        //}
    }


    // Update is called once per frame
    void Update()
    {

        if (doExplode)
        {

            Light light = GetComponent<Light>();

            //SphereCollider coll = GetComponent<SphereCollider>();
            //Debug.Log("SphereCollider info: enabled: " + coll.enabled + ", radius: " + coll.radius);

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
                GetComponent<Collider>().enabled = false; // collider won't intersect during dissipation phase

                Color color = mat.color;

                // SMOKE GLOW DECAY
                 mat.SetColor("_EmissionColor", stepColorOverTime(mat.GetColor("_EmissionColor"), smokeColor, emissionColor, smokeGlowDecayTime));
                

                // STEP SIZE GROWTH
                float slowExpandScale = stepValOverTime(transform.localScale.x, radius * fadeRadiusScale, radius, fadeOutTime);
                transform.localScale = new Vector3(slowExpandScale, slowExpandScale, slowExpandScale);

                // STEP LIGHT INTENSITY DECAY
                light.intensity = stepValOverTime(light.intensity, 0.0f, flashIntensity, lightDecayTime);

                // STEP LIGHT RANGE INCREASE -- light range will step to 1.5 times the expansion phase's max radius
                light.range = stepValOverTime(light.range, lightRangeScaleFactor * radius * 1.2f, radius * lightRangeScaleFactor, lightDecayTime);
                
                if (Mathf.Approximately(light.intensity, 0.0f)) // light component has fully faded
                {
                    // SMOKE ALPHA DECAY
                    color.a = stepValOverTime(color.a, 0.0f, 1.0f, fadeOutTime - lightDecayTime); // subtracting light decay time is overkill
                    mat.color = color;         // save modified values into reference material color
                }

                //  max radius has been reched -- kill self
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
    public void goExplode(float setRadius, float setCoreDamage, bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight, Color newSmokeColor)
    {
       // Debug.Log("=================================================== goExplode called");

        // smoke settings
        emissionColor = glowColor;
        smokeColor = newSmokeColor;

        //mat.SetColor("_EmissionColor", Color.yellow);
        radius = setRadius;
        coreDamage = setCoreDamage;
        doExplode = true;
        fadeOutTime = dissipationTime;
        GetComponent<Collider>().enabled = doCollide;

        //Debug.Log("Setting collider to " + GetComponent<Collider>().enabled);


        // sphere collider radius set to 1 in prefab. This won't change. .5 matches object radius, 1 is double


        // light settings
        Light light = GetComponent<Light>();
        light.enabled = doEmitLight;
        //light.enabled = false;
        //Debug.Log("emitLight set to: " + doEmitLight + ", setting is: " + light.enabled);
        if (doEmitLight)
        {
            
            light.color = glowColor;
            light.range = radius * lightRangeScaleFactor;
            // light.intensity dynamically changes at runtime.
            //Debug.Log("Light intensity set to: " + flashIntensity + ", current setting: " + light.intensity) ;
        }
        


    }


    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Explosion collided with " + collision.other.gameObject);
        explosionContactProcess(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Explosion triggered on: " + other.gameObject);
        explosionContactProcess(other.gameObject);
    }

    private void explosionContactProcess(GameObject victim)
    {
        // deal damage
        // apply force


        GameObject targetRootObj = victim.transform.root.gameObject;
        //Debug.Log("Explosion contacted: " + victim.name + " with root: " + targetRootObj.name);

        // only act upon victim root if he is a new victim
        if (isNewVictim(targetRootObj))
        {

            
            
            explosionVictimRootList.Add(targetRootObj);
            

            Rigidbody targetRb = targetRootObj.GetComponent<Rigidbody>();
            CombatFlow targetCF = targetRootObj.GetComponent<CombatFlow>();

            bool noForce = false; // I know this is super crusty way to do this


            if(targetCF != null)
            {


                // explosions will never intersect with projectiles
                if (targetCF.type != CombatFlow.Type.PROJECTILE)
                {
                    Debug.Log("Explosion acting upon victim: " + targetRootObj + " for " + coreDamage + " damage, list count: " + explosionVictimRootList.Count);
                    targetCF.currentHP -= coreDamage;
                }
                else
                    noForce = true;
            }


            if (targetRb != null && !noForce)
            {
                targetRb.AddExplosionForce(coreDamage, transform.position, radius * 2);

            }





        }
    }


    private bool isNewVictim(GameObject victimRoot)
    {
        bool isNewVictim = true;

        string victimListContents = "Victim list (size " + explosionVictimRootList.Count.ToString() + ") contents: ";
        for(short i = 0; i < explosionVictimRootList.Count; i++)
        {
            //victimListContents += "(" + explosionVictimRootList[i].name + "), ";
            if(victimRoot == explosionVictimRootList[i])
            {
                isNewVictim = false;
            }


        }
       // Debug.Log("Is new victim?: " + isNewVictim + " from " + victimListContents);
        return isNewVictim;
    }




    
}
