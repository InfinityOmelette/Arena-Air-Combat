﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : MonoBehaviour
{
    public GameObject projectileSpawn;

    // copy this for every shot
    public GameObject shellSettings;

    public GameObject target;

    private CombatFlow rootFlow;

    private float maxDist;

    public float shellSpeed;

    public bool highTraject = false;

    public float shellSpreadHoriz;
    public float shellSpreadVert;




    public float fireRateDelay;
    private float fireRateTimer;

    public float reloadDelay;
    private float reloadTimer;

    public int roundsPerMag;
    private int roundsInCurrentMag;


    private bool fireMission = false;


    public Transform explodePosition;
    public bool explodeOnLaunch;

    private ExplodeStats explode;

    private TankShell tankShell;

    //public float elev;

        void Awake()
    {
        tankShell = shellSettings.GetComponent<TankShell>();
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
        if (roundsInCurrentMag > 0) // rounds in mag, try to fire
        {

            if (fireRateTimer > 0) // keep waiting until shot is loaded
            {
                fireRateTimer -= Time.fixedDeltaTime;
            }
            else if (fireMission)   // wait complete, firemission active, do a shot
            {
                if (target != null)
                {
                    fireRateTimer = fireRateDelay;
                    fireSequence();
                }
            }
        }
        else // no rounds in mag, try to reload
        {
            if (reloadTimer > 0) // wait for reload
            {
                reloadTimer -= Time.fixedDeltaTime;
            }
            else // wait complete, perform reload
            {
                reloadTimer = reloadDelay;
                roundsInCurrentMag = roundsPerMag;

            }
        }
    }


    private void fireSequence()
    {
        roundsInCurrentMag--;
        setAim(target);
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

            if (highTraject)
            {
                float diff = 45 - elev;
                elev = 45 + diff;
            }


            //Debug.LogError(rootFlow.gameObject.name + "'s elevation: " + elev);

            transform.LookAt(target.transform, Vector3.up);

            Debug.DrawLine(transform.position, target.transform.position, Color.red, .5f);


            //Quaternion elevRot = Quaternion.AngleAxis(elev, -transform.right);


            transform.localEulerAngles = new Vector3(-elev, transform.localEulerAngles.y, 0.0f);
        }
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
        GameObject shell = GameObject.Instantiate(shellSettings);
        shell.transform.position = projectileSpawn.transform.position;
        shell.transform.rotation = projectileSpawn.transform.rotation;
        shell.transform.rotation *= getShellSpreadRotation(shellSpreadHoriz, shellSpreadVert);
        shell.SetActive(true);

        shell.GetComponent<Rigidbody>().velocity = shell.transform.forward * shellSpeed;

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
