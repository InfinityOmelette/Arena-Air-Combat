using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // TERRIBLE FUNDAMENTAL INEFFICIENCY -- COMPLICATED OBJECT FOR EVERY EXPLOSION


    public static float MEDIUM_SIZE = 8f;
    public static float LARGE_SIZE = 30f;

    public float radius;
    public float coreDamage; // damage falls off linearly from max at core to zero at radius

    public float expandTime; // number of seconds
    public float fadeOutTime; // number of seconds -- graphic will continue to expand without dealing damage
    public float fadeRadiusScale;

    public bool doExplode = false;
    private bool radiusMaxed = false;

    public float explosiveForce;

    public float lightRangeScaleFactor;
    public float flashIntensity;
    public float lightDecayTime; // seconds to full decay
    public float smokeGlowDecayTime;

    // NEWLY ADDED PROPERTIES
    public CombatFlow.Team team;
    public bool friendlyFire = true;
    public bool damageProjectiles = true;

    public Material mat;
    private MeshRenderer rend;

    
    public Color emissionColor;
    public Color smokeColor;



    private static GameObject explosionPrefab;

    private List<GameObject> explosionVictimRootList;

    public bool localOwned = false;


    private AudioClip activeSound;
    private AudioSource audioSource;

    public AudioClip smallExplodeSound;
    public AudioClip mediumExplodeSound;
    public AudioClip largeExplodeSound;

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

    // expand time
    // team
    // damageProjectiles
    // friendlyFire
    public static void createExplosionAt(Vector3 position, float setRadius, float setCoreDamage,
        bool doCollide, float dissipationTime, Color glowColor, bool doEmitLight, Color newSmokeColor,
        float newExpandTime, CombatFlow.Team newTeam, bool newDamageProjectiles, bool newFriendlyFire, 
        float newExplosiveForce)
    {

        ExplodeManager expMan = ExplodeManager.getExplodeManager();

        expMan.createExplosionAt(position, setRadius, setCoreDamage, doCollide, dissipationTime, glowColor,
            doEmitLight, newSmokeColor, newExpandTime, newTeam, newDamageProjectiles, newFriendlyFire, newExplosiveForce);

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


        audioSource = GetComponent<AudioSource>();
        

        

    }

    private void setExplodeSound()
    {
        if(radius < MEDIUM_SIZE)
        {
            activeSound = smallExplodeSound;
        }
        else if(radius < LARGE_SIZE)
        {
            activeSound= mediumExplodeSound;
        }
        else
        {
            activeSound = largeExplodeSound;
        }

        audioSource.clip = activeSound;
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

        // temporary shitty way to disable explosions on rockets and cannon impact
        //if (radius > 15) // 15 is arbitrary number between rocket and amraam radius
        {
            setExplodeSound();
            audioSource.Play();

        }

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



    // theoretically, this should not be called unless collider is active
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Explosion collided with " + collision.other.gameObject);
        explosionContactProcess(collision.gameObject);
    }

    // theoretically, this should not be called unless collider is active
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Explosion triggered on: " + other.gameObject);
        explosionContactProcess(other.gameObject);
    }

    // theoretically, this should not be called unless collider is active
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


            // whether or not explosion should act upon victim
            bool doAct = true; //various conditions will try to make this false


            //  if target has a CombatFlow
            if(targetCF != null)
            {
                // if target is friendly, and friendlyFire is disabled
                if (targetCF.team == team && !friendlyFire)
                    doAct = false;

                // if target is a Projectile, and 
                if (targetCF.type == CombatFlow.Type.PROJECTILE && !damageProjectiles)
                    doAct = false;

            }


            //  if explosion will act upon target
            if (doAct)
            {
                // if target has a rigidBody, add explosive force
                if(targetRb != null)
                {
                    targetRb.AddExplosionForce(explosiveForce, transform.position, radius);
                }

                if(targetCF != null && coreDamage > 0)
                {
                    //Debug.Log("Explosion acting upon victim: " + targetRootObj + " for " + coreDamage + " damage, list count: " + explosionVictimRootList.Count);
                    //targetCF.currentHP -= coreDamage;
                    targetCF.dealDamage(coreDamage);
                }
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
