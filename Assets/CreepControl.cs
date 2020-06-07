﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CreepControl : MonoBehaviourPun
{

    public LaneManager parentLane;
    public List<Transform> waypoints;

    

    public float movementSpeed;
    public float waypointRadius;
    public float effectiveRange; // stop when opposing creep leader within range

    public bool doMove = true;

    private Rigidbody rb;

    // instantiating to prevent any null errors. Effectively zero length vect initially
    private Vector3 myOffset = new Vector3(); 

    public float lookWaypointDelay;
    private float lookWaypointCounter;

    public float leaderCheckDelay;
    private float leaderCheckCounter;

    private CombatFlow myFlow;

    private Vector3 movementDir;

    private TurretNetworking turret;

    private CombatFlow currentTarget;

    void Awake()
    {
        waypoints = new List<Transform>();
        rb = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
        turret = GetComponent<TurretNetworking>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void rpcInit(int parentLaneID, Vector3 offset, float range, int teamNum)
    {
        parentLane = PhotonNetwork.GetPhotonView(parentLaneID).GetComponent<LaneManager>();
        this.myOffset = offset;
        effectiveRange = range;
        myFlow.team = CombatFlow.convertNumToTeam((short)teamNum);
        //parentLane.waypoints.CopyTo(waypoints);
        parentLane.myLaneUnits.Add(myFlow);


        // copy list from parent
        waypoints = new List<Transform>(parentLane.waypoints);

        lookAtWaypoint();

        //transform.rotation = Quaternion.LookRotation(waypoints[0].position - transform.position, Vector3.up);
    }

    

    void FixedUpdate()
    {

        //checkLeaderCounterProcess();


        if (doMove && waypoints != null && waypoints.Count > 0)
        {
            //lookAtWaypoint();
            checkWaypoint();
            lookWaypointTimerProcess();
            //rb.velocity = transform.forward * movementSpeed;
            Vector3 newVel = transform.forward * movementSpeed;
            rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);

            //rb.position += movementDir * movementSpeed * Time.fixedDeltaTime;
            //Debug.DrawLine(transform.position, waypoints[0].position, Color.green);

        }
    }

    private bool canShootCurrentTarget()
    {
        return myFlow.localOwned && currentTarget != null &&
            Vector3.Distance(currentTarget.transform.position, transform.position) < effectiveRange;
    }

    private void checkLeaderCounterProcess()
    {
        if(leaderCheckCounter < 0)
        {
            leaderCheckCounter = leaderCheckDelay;
            doMove = enemyLeaderWithinRange();
            if (!canShootCurrentTarget())
            {
                findTarget();
            }

        }
        else
        {
            leaderCheckCounter -= Time.fixedDeltaTime;
        }
    }

    private bool enemyLeaderWithinRange()
    {
        CombatFlow enemyLeader = parentLane.opponentLM.getLeader();

        return enemyLeader != null && 
            Vector3.Distance(enemyLeader.transform.position, transform.position) < effectiveRange;
    }

    private void findTarget()
    {
        

        if(turret != null)
        {
            // loop through lead wave of opponent lane
            List<CombatFlow> enemyLeadWave = parentLane.opponentLM.frontWave;
            List<CombatFlow> possibleTargets = new List<CombatFlow>();

            for(int i = 0; i < enemyLeadWave.Count; i++)
            {
                CombatFlow currentFlow = enemyLeadWave[i];
                if(currentFlow != null)
                {
                    float dist = Vector3.Distance(currentFlow.transform.position, transform.position);
                    if(dist < effectiveRange)
                    {
                        possibleTargets.Add(currentFlow);
                    }
                }
            }


            int randIndex = Random.Range(0, possibleTargets.Count - 1);
            currentTarget = possibleTargets[randIndex];
            turret.setTarget(currentTarget); // networked

        }
    }

    private void checkWaypoint()
    {
        if(Vector3.Distance(waypoints[0].position + myOffset, transform.position) < waypointRadius)
        {
            waypoints.RemoveAt(0);
            lookAtWaypoint();
        }
    }
    

    private void lookWaypointTimerProcess()
    {
        if(lookWaypointCounter < 0)
        {
            lookWaypointCounter = lookWaypointDelay;
            lookAtWaypoint();
        }
        else
        {
            lookWaypointCounter -= Time.fixedDeltaTime;
        }
    }

    private void lookAtWaypoint()
    {
        Vector3 targetPos = waypoints[0].position + myOffset;

        // place target pos to be co-altitude with this creep
        targetPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);

        transform.rotation = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);

        movementDir = (targetPos - transform.position).normalized;

       // Debug.DrawRay(transform.position, targetPos - transform.position, Color.green, 1f);
        //Debug.DrawRay(transform.position, transform.forward * 50f, Color.red, 1f);

        
    }
}
