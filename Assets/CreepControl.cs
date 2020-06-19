using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CreepControl : MonoBehaviourPun
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
            doMove = !enemyLeaderWithinRange();
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
            differenceFromSpawn(enemyLeader) < effectiveRange;
    }

    private float differenceFromSpawn(CombatFlow other)
    {
        return Mathf.Abs(
            Vector3.Distance(other.transform.position, parentLane.transform.position) -
            Vector3.Distance(transform.position, parentLane.transform.position));
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

            if (possibleTargets.Count > 0)
            {

                int randIndex = Random.Range(0, possibleTargets.Count - 1);
                currentTarget = possibleTargets[randIndex];
                //turret.setTarget(currentTarget); // networked
                photonView.RPC("rpcSetTankTurretTarget", RpcTarget.All, currentTarget.photonView.ViewID);
            }

        }
    }

    [PunRPC]
    private void rpcSetTankTurretTarget(int targetID)
    {
        PhotonView view = null;
        if (targetID != -1)
        {
            view = PhotonNetwork.GetPhotonView(targetID);
        }

        if(turret != null)
        {
            if (view != null)
            {
                turret.target = view.gameObject;
            }
            else
            {
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


        if (Vector3.Distance(waypoint, transform.position) < waypointRadius)
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
}
