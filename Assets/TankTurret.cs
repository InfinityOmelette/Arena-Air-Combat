using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : MonoBehaviour
{
    public enum TrajectMode
    {
        LOW,
        HIGH,
        LOSLOW
    }

    public GameObject projectileSpawn;



    // copy this for every shot
    public GameObject shellSettings;

    public GameObject target;

    private CombatFlow rootFlow;

    private float maxDist;

    public float shellSpeed;

    public bool highTraject = false;
    public TrajectMode trajectyMode = TrajectMode.LOW;


    public float shellSpreadHoriz;
    public float shellSpreadVert;




    public float fireRateDelay;
    private float fireRateTimer;

    public float reloadDelay;
    private float reloadTimer;

    public int roundsPerMag;
    private int roundsInCurrentMag;


    public bool fireMission = false;


    public Transform explodePosition;
    public bool explodeOnLaunch;

    private ExplodeStats explode;

    private TankShell tankShell;

    public bool useExternAim = false;


    public bool setFuze = false;

    public AI_TurretMG parentTurret;

    public Magazine mag;

    //public float elev;

    void Awake()
    {
        tankShell = shellSettings.GetComponent<TankShell>();
        mag = GetComponent<Magazine>();
    }

    // Start is called before the first frame update
    void Start()
    {
        explode = GetComponent<ExplodeStats>();

        fireRateTimer = fireRateDelay;
        reloadTimer = reloadDelay;
        roundsInCurrentMag = roundsPerMag;


        rootFlow = transform.root.GetComponent<CombatFlow>();


        //maxDist = shellSpeed * shellSpeed
        //  * Mathf.Asin(2 * Mathf.Deg2Rad * 45f) / Physics.gravity.magnitude;

        maxDist = shellSpeed * shellSpeed * Mathf.Sin(Mathf.PI / 2) / Physics.gravity.magnitude;
        //maxDist = 3500f;

        //Debug.LogError(rootFlow.gameObject.name + "'s max dist: " + maxDist);
    }


    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    fireMission = !fireMission;

        //}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (target != null)
        //{
        //    fireMissionProcess();
        //}

        fireMission = target != null;

        if(target != null)
        {
            Debug.DrawLine(target.transform.position, transform.position, Color.green);
        }

        fireMissionProcess();


    }

    public void setShellTeam(CombatFlow.Team team)
    {
        //shellSettings
        shellSettings.GetComponent<ExplodeStats>().team = team;
    }

    private void fireMissionProcess()
    {

        if (target != null && fireMission && mag.tryShoot())
        {
            //fireRateTimer = fireRateDelay;
            fireSequence();
        }

        //if (roundsInCurrentMag > 0) // rounds in mag, try to fire
        //{

        //    if (fireRateTimer > 0) // keep waiting until shot is loaded
        //    {
        //        fireRateTimer -= Time.fixedDeltaTime;
        //    }
        //    else if (fireMission)   // wait complete, firemission active, do a shot
        //    {

        //    }
        //}
        //else // no rounds in mag, try to reload
        //{
        //    if (reloadTimer > 0) // wait for reload
        //    {
        //        reloadTimer -= Time.fixedDeltaTime;
        //    }
        //    else // wait complete, perform reload
        //    {
        //        reloadTimer = reloadDelay;
        //        roundsInCurrentMag = roundsPerMag;

        //    }
        //}
    }


    private void fireSequence()
    {
        //roundsInCurrentMag--;
        if (!useExternAim)
        {
            setAim(target);
        }
        
        fire();
    }

    private void setAim(GameObject target)
    {


        Vector3 myPos = new Vector3(rootFlow.transform.position.x, 0.0f, rootFlow.transform.position.z);
        Vector3 targetPos = new Vector3(target.transform.position.x, 0.0f, target.transform.position.z);

        targetPos = leadTargetPos(targetPos, target.GetComponent<Rigidbody>().velocity);


        float distance = Vector3.Distance(myPos, targetPos);

        //Debug.LogError("Shooting artillery at " + distance + " meters");

        if (distance < maxDist)
        {

            // d = V₀² * sin(2 * α) / g  rearrange this, solve for angle (α)



            float elev = calculateElev(distance);


            if(trajectyMode == TrajectMode.HIGH || losCheck(target))
            {
                elev = convertToHigh(elev);
            }


            //Debug.LogError(rootFlow.gameObject.name + "'s elevation: " + elev);

            transform.LookAt(target.transform, Vector3.up);

            Debug.DrawLine(transform.position, target.transform.position, Color.red, .5f);


            //Quaternion elevRot = Quaternion.AngleAxis(elev, -transform.right);


            transform.localEulerAngles = new Vector3(-elev, transform.localEulerAngles.y, 0.0f);
        }
    }

    public bool losCheck(GameObject target)
    {
        int terrainLayer = 1 << 10;
        bool losObstructed = Physics.Linecast(transform.position, target.transform.position, terrainLayer);
        return trajectyMode == TrajectMode.LOSLOW && losObstructed;
    }

    public float convertToHigh(float elev)
    {
        float diff = 45 - elev;
        elev = 45 + diff;
        return elev;
    }

    private Vector3 leadTargetPos(Vector3 targetPos, Vector3 targetVel)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        Vector3 targetBearingLine = targetPos - transform.position;
        targetBearingLine = Vector3.Project(targetVel, targetBearingLine);

        float closingVel = targetBearingLine.magnitude;
        if (Vector3.Distance(transform.position, targetPos + targetBearingLine) < distance)
        {
            closingVel *= -1f;
        }

        float timeToTarget = distance / (shellSpeed - closingVel);

        return targetPos + targetVel * timeToTarget;
    }

    private float calculateElev(float distance)
    {
        return Mathf.Rad2Deg * Mathf.Asin(distance * Physics.gravity.magnitude 
            / (shellSpeed * shellSpeed)) / 2;
    }


    private void fire()
    {
        // copy variable data over to determine what kind of shell
        GameObject shellObj = GameObject.Instantiate(shellSettings);
        shellObj.transform.position = projectileSpawn.transform.position;
        shellObj.transform.rotation = projectileSpawn.transform.rotation;
        shellObj.transform.rotation *= getShellSpreadRotation(shellSpreadHoriz, shellSpreadVert);
        shellObj.SetActive(true);

        shellObj.GetComponent<Rigidbody>().velocity = shellObj.transform.forward * shellSpeed;

        //TankShell shell = shellObj.GetComponent<TankShell>();
        

        


        if(parentTurret != null && parentTurret.targetRb != null && setFuze)
        {
            //Vector3 fuzePos = parentTurret.getFuzePos();
            float fuzeTime = parentTurret.getFuzeTime();
            shellObj.GetComponent<TankShell>().programFuze(parentTurret.targetRb.gameObject, fuzeTime);
            ExplodeStats shellExplode = shellObj.GetComponent<ExplodeStats>();
            shellExplode.team = parentTurret.myRb.GetComponent<CombatFlow>().team;
        }


        //shell.GetComponent<TankShell>().readyEmit();

        if (explodeOnLaunch)
        {
            explode.explode(explodePosition.position);
        }

    }


    private Quaternion getShellSpreadRotation(float horizSpread, float vertSpread)
    {

        horizSpread = Random.Range(-horizSpread, horizSpread);
        vertSpread = Random.Range(-vertSpread, vertSpread);

        Vector3 newEuler = new Vector3(vertSpread, horizSpread, 0.0f); // rocket rotation will change by this
        return Quaternion.Euler(newEuler);
    }
}
