using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CreepControl : MonoBehaviourPunCallbacks
{
    //public struct CreepData
    //{
    //    Vector3 position;
    //    Vector3 rotation;
        
    //}


    public LaneManager parentLane;
    public List<Vector3> waypoints;

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

    public CombatFlow currentTarget;

    public TankTurret turret;

    public float bumperRerouteDistance;

    public Collider bumper;

    public GameObject raycastStart;
    public GameObject linecastEnd;
    public float raycastLength;
    public float raycastDelay;
    private float raycastTimer;

    private bool bumperCorrecting = false;



    void Awake()
    {
        waypoints = new List<Vector3>();
        rb = GetComponent<Rigidbody>();
        myFlow = GetComponent<CombatFlow>();
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

        //Debug.LogError(" CREEP INIT RPC CALLED");

        parentLane = PhotonNetwork.GetPhotonView(parentLaneID).GetComponent<LaneManager>();
        this.myOffset = offset;
        effectiveRange = range;
        myFlow.team = CombatFlow.convertNumToTeam((short)teamNum);
        //parentLane.waypoints.CopyTo(waypoints);
        parentLane.myLaneUnits.Add(myFlow);

        if(turret != null)
        {
            turret.setShellTeam(myFlow.team);
        }


        // copy list from parent
        waypoints = new List<Vector3>(parentLane.waypoints);

        lookAtWaypoint();

        //transform.rotation = Quaternion.LookRotation(waypoints[0].position - transform.position, Vector3.up);
    }

    

    void FixedUpdate()
    {

        //Debug.DrawRay(transform.position, -myOffset.normalized * bumperRerouteDistance, Color.white);

        if (parentLane != null)
        {

            checkLeaderCounterProcess();
            raycastCountdown();

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
            else
            {
                rb.velocity = Vector3.zero;
            }
            

        }
    }

    private bool canShootTarget(CombatFlow target)
    {
        return myFlow.localOwned && target != null &&
            Vector3.Distance(target.transform.position, transform.position) < effectiveRange;
    }

    // Creep checks distance only to the leader instead of checking distance to all creeps
    private void checkLeaderCounterProcess()
    {
        

        if (leaderCheckCounter < 0)
        {
            leaderCheckCounter = leaderCheckDelay;

            // If enemy leader is within range, shoot creeps
            // Otherwise, if enemy strategic target is within range, shoot that.

            CombatFlow enemyLeader = parentLane.opponentLM.getLeader();


            CombatFlow tryTarget = null;

            // Detect proximity of oncoming enemy creep wave by examining just their leader
            if (myFlow.localOwned)
            {
                tryCapture();

                if (targetWithinRange(enemyLeader))
                {
                    tryTarget = findCreepTarget(); // targets a random creep among enemy lead wave
                }
                else // enemy leader NOT within range, try to find a valid strategic target
                {
                    tryTarget = findStrategicTarget();
                }
                
            }

            // Enemy leader may be in range, but target might not be. Keep moving till target is in range
            
            currentTarget = tryTarget;

            bool stop = targetWithinRange(enemyLeader) || targetWithinRange(currentTarget);
            doMove = !stop; // this is a really silly way to do boolean logic. Surely

            

            //bool stop = currentTarget != null && targetWithinRange(currentTarget);
            //doMove = !stop;

            //doMove = currentTarget == null;


            if (currentTarget != null)
            {
                photonView.RPC("rpcSetTankTurretTarget", RpcTarget.All, currentTarget.photonView.ViewID);
            }
            else
            {
                photonView.RPC("rpcSetTankTurretTarget", RpcTarget.All, -1);
            }
        }
        else
        {
            leaderCheckCounter -= Time.fixedDeltaTime;
        }
        
    }

    // If creep has proceeded past an enemy structure that is suppressed, the structure will be captured
    //  - Compare this creep's distance from its spawner vs the strategic structures' distance from THIS creep's spawner
    //  - If this creep's distance from spawner is greater than the structure's, it has proceeded past the structure.
    private void tryCapture()
    {
        List<StrategicTarget> stratTargets = StrategicTarget.AllStrategicTargets;

        float myDistFromHome = Vector3.Distance(transform.position, parentLane.transform.position);

        // Loop through all strategic targets in battle
        for (int i = 0; i < stratTargets.Count; i++)
        {
            StrategicTarget currentStrat = stratTargets[i];

            // If strategic target is on enemy team and is on the same lane as this creep
            if(currentStrat.myFlow.team != myFlow.team && currentStrat.lane == parentLane.lane)
            {
                float stratDistFromMyHome = Vector3.Distance(currentStrat.transform.position, parentLane.transform.position);


                if(myDistFromHome > stratDistFromMyHome)
                {
                    currentStrat.tryCapture(myFlow.team);
                }

            }

            
            
        }
    }


    private CombatFlow findStrategicTarget()
    {
        List<StrategicTarget> stratTargets = StrategicTarget.AllStrategicTargets;

        CombatFlow goodTarget = null;

        for(int i = 0; i < stratTargets.Count && goodTarget == null; i++)
        {
            StrategicTarget currentTarget = stratTargets[i];

            // only shoot if strategic target is within range AND is NOT suppressed
            if(currentTarget != null && currentTarget.myFlow.team != myFlow.team &&
                targetWithinRange(currentTarget.myFlow) && !currentTarget.isSuppressed)
            {
                goodTarget = currentTarget.myFlow;
            }
        }

        return goodTarget;
    }

    private bool targetWithinRange(CombatFlow target)
    {

        return target != null &&
            //differenceFromSpawn(enemyLeader) < effectiveRange;
            Vector3.Distance(target.transform.position, transform.position) < effectiveRange;
    }

    // ...somehow this is better than just checking range to target???
    private float differenceFromSpawn(CombatFlow other)
    {
        // Distance of other from its base
        // subtracted from distance of this one to its base

        return Mathf.Abs(
            Vector3.Distance(other.transform.position, parentLane.transform.position) -
            Vector3.Distance(transform.position, parentLane.transform.position));
    }


    // Builds a list of possible creep targets and then selects one at random
    private CombatFlow findCreepTarget()
    {

        CombatFlow newTarget = null;

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

            if (possibleTargets.Count > 0)
            {
                Debug.Log("Possible targets found: " + possibleTargets.Count);

                int randIndex = Random.Range(0, possibleTargets.Count - 1);
                newTarget = possibleTargets[randIndex];

                // Only execute this locally.
                //  Lane manager will pick this up, and call rpc to propogate to other clients
                //rpcSetTankTurretTarget(currentTarget.photonView.ViewID);
            }

        }

        return newTarget;
    }

    [PunRPC]
    public void rpcSetTankTurretTarget(int targetID)
    {
        PhotonView view = null;

        parentLane.ifCount++;
        if (targetID != -1)
        {
            view = PhotonNetwork.GetPhotonView(targetID);
            
        }

        parentLane.ifCount++;
        if (turret != null)
        {
            parentLane.ifCount++;
            if (view != null)
            {
                currentTarget = view.GetComponent<CombatFlow>();
                turret.target = view.gameObject;
            }
            else
            {
                currentTarget = null;
                turret.target = null;
            }
        }

    }

    private void checkWaypoint()
    {
        Vector3 activeOffset = myOffset;
        if (bumperCorrecting)
        {
            activeOffset *= 0.0f;
        }

        Vector3 waypoint = waypoints[0] + activeOffset;
        waypoint = new Vector3(waypoint.x, transform.position.y, waypoint.z);

        // If we are close to OR past the waypoint, remove the waypoint from our container
        if (Vector3.Distance(waypoint, transform.position) < waypointRadius || 
            Vector3.Distance(transform.position, parentLane.transform.position) > Vector3.Distance(waypoint, parentLane.transform.position))
        {
            bumperCorrecting = false;
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
        Vector3 activeOffset = myOffset;
        if (bumperCorrecting)
        {
            activeOffset *= 0.0f;
        }

        Vector3 targetPos = waypoints[0] + activeOffset;

        // place target pos to be co-altitude with this creep
        targetPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);

        transform.rotation = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);

        movementDir = (targetPos - transform.position).normalized;

       // Debug.DrawRay(transform.position, targetPos - transform.position, Color.green, 1f);
        //Debug.DrawRay(transform.position, transform.forward * 50f, Color.red, 1f);

        
    }


    private void raycastCountdown()
    {
        Debug.DrawLine(raycastStart.transform.position, linecastEnd.transform.position, Color.blue);

        if(raycastTimer < 0)
        {
            // do thing
            RaycastHit hitInfo = new RaycastHit();
            short unitLayer = 1 << 9; // only check collisions with units


            if (Physics.Linecast(raycastStart.transform.position, linecastEnd.transform.position,
                out hitInfo, unitLayer))
            {
                //Debug.LogError("Linecast triggered");
                //Debug.DrawLine(raycastStart.transform.position, hitInfo.point, Color.cyan, 100);
                bumperReroute(hitInfo);
            }

                raycastTimer = raycastDelay;
        }
        else
        {
            raycastTimer -= Time.fixedDeltaTime;
        }
    }

    private void bumperReroute(RaycastHit hitInfo)
    {
        

        Vector3 pos = hitInfo.point;
        pos -= myOffset.normalized * (bumperRerouteDistance + waypointRadius);

        //GameObject newWpt = new GameObject();
        //newWpt.transform.position = pos;

        

        // if bumper triggered while already correcting a previous trigger
        if (bumperCorrecting)
        {
            // don't create a new waypoint
            waypoints[0] = pos;
        }
        else // starting new correction
        {
            waypoints.Insert(0, pos);
        }

        bumperCorrecting = true;

        lookAtWaypoint();
        //Debug.DrawLine(transform.position, waypoints[0].transform.position, Color.yellow, 100);
        //Debug.DrawLine(waypoints[0].transform.position, waypoints[1].transform.position, Color.red, 100);


        
    }

    // remove all waypoints that this creep is ahead of
    // must be called after rpcInit()
    private void checkInitWaypoints()
    {
        if(parentLane != null && waypoints != null && waypoints.Count > 0)
        {
            float myDist = Vector3.Distance(parentLane.transform.position, transform.position);

            for(int i = 0; i < waypoints.Count; i++)
            {
                float ptDist = Vector3.Distance(parentLane.transform.position, waypoints[i]);

                if(myDist > ptDist)
                {
                    waypoints.RemoveAt(i);
                    i--; // check this same index again next iteration
                }
            }
        }

        if(waypoints.Count == 0)
        {
            Debug.LogError("CreepControl: No waypoints set!! unable to checkInitWaypoints()!!");
        }
    }

    public void setFromLaneUpdate(float hp, int targetID,
        Vector3 position, Quaternion rotation)
    {
        myFlow.setHP(hp);

        setTargetFromId(targetID);

        transform.position = position;
        transform.rotation = rotation;

        checkInitWaypoints();
    }

    public int getTargetId()
    {
        int id = -1;
        if(currentTarget != null)
        {
            id = currentTarget.photonView.ViewID;
        }
        return id;
    }

    public void setTargetFromId(int targetId)
    {
        if(targetId != -1)
        {
            PhotonView targetView = PhotonNetwork.GetPhotonView(targetId);
            if(targetView != null)
            {
                currentTarget = targetView.GetComponent<CombatFlow>();
            }
        }
        else
        {
            currentTarget = null;
        }

        rpcSetTankTurretTarget(targetId); // execute locally, don't network
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    Debug.LogWarning("This instance is new master client. Taking ownership of creep");

        //    myFlow.isHostInstance = true;
        //    myFlow.localOwned = true;
        //}

        bool isMaster = PhotonNetwork.IsMasterClient;

        myFlow.isHostInstance = isMaster;
        myFlow.localOwned = isMaster;
    }

    void OnDestroy()
    {
        if(parentLane != null)
        {
            parentLane.myLaneUnits.Remove(GetComponent<CombatFlow>());
        }
    }

}
