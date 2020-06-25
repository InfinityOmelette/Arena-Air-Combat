using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetPositionHub : MonoBehaviourPunCallbacks
{

    public List<NetPosition> allLocalOwned;

    public short updatesPerSecond;

    private float waitTime;
    private float currentTimer;


    void Awake()
    {
        allLocalOwned = new List<NetPosition>();
        waitTime = 1.0f / updatesPerSecond;
        currentTimer = waitTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (allLocalOwned != null && allLocalOwned.Count > 0)
        {

            if (currentTimer < 0f)
            {
                Debug.LogWarning("Pulsing positions");
                pulseAllPositions();
                currentTimer = waitTime;
            }
            else
            {
                currentTimer -= Time.fixedDeltaTime;
            }
        }
    }

    void pulseAllPositions()
    {
        if (allLocalOwned.Count > 0)
        {

            List<int> idList = new List<int>();
            List<Vector3> targetPosList = new List<Vector3>();
            List<Quaternion> targetRotList = new List<Quaternion>();
            List<Vector3> targetVelList = new List<Vector3>();
            List<float> originLifeTimeList = new List<float>();


            for (int i = 0; i < allLocalOwned.Count; i++)
            {
                NetPosition currentNetPos = allLocalOwned[i];

                if (currentNetPos != null && currentNetPos.active && (currentNetPos.myFlow.localOwned || currentNetPos.myFlow.isLocalPlayer))
                {
                    idList.Add(currentNetPos.photonView.ViewID);
                    targetPosList.Add(currentNetPos.transform.position);
                    targetRotList.Add(currentNetPos.transform.rotation);
                    targetVelList.Add(currentNetPos.myRB.velocity);
                    originLifeTimeList.Add(currentNetPos.lifeTime);
                }
                else
                {
                    allLocalOwned.RemoveAt(i);
                    i--;
                }

            }

            photonView.RPC("rpcPulsePositions", RpcTarget.Others,
                idList.ToArray(), targetPosList.ToArray(), targetRotList.ToArray(),
                targetVelList.ToArray(), originLifeTimeList.ToArray());
        }
    }

    [PunRPC]
    public void rpcPulsePositions(int[] IDs, Vector3[] targetPositions,
        Quaternion[] targetRots, Vector3[] targetVels, float[] originLifeTimes)
    {
        //Debug.LogError("NetPosition pulse received! with size " + IDs.Length);

        for(int i = 0; i < IDs.Length; i++)
        {
            PhotonView currentView = PhotonNetwork.GetPhotonView(IDs[i]);


            //Debug.LogWarning("============  Entering rpc pulse loop iteration");
            if(currentView != null)
            {
                NetPosition currentNetPosition = currentView.GetComponent<NetPosition>();

                //Debug.LogWarning("************ photonView found, updating current position");
                currentNetPosition.updatePositionAndVelocity(targetPositions[i], targetRots[i], targetVels[i], originLifeTimes[i]);

                
            }
        }
    }
}
