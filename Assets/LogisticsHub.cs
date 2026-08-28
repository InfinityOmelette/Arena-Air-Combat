using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogisticsHub : MonoBehaviour
{

    public List<LaneManager> lanes;
    public List<float> laneSupplyBias;

    public List<SupplyGenerator> factories;

    public CombatFlow myFlow;
    public StrategicTarget myStrat;


    public float creepWaveInterval;
    private float creepWaveTimer;

    public float supplyPullInterval;
    private float supplyPullTimer;

    private bool enableSpawners = false;

    private void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        myStrat = GetComponent<StrategicTarget>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            enableSpawners = true;
        }   
    }

    private void FixedUpdate()
    {
        if (enableSpawners)
        {
            creepWaveTimerProcess();

            supplyPullTimerProcess();
        }
        


    }

    private void supplyPullTimerProcess()
    {
        if(supplyPullTimer > 0)
        {
            supplyPullTimer -= Time.fixedDeltaTime;
        }
        else
        {
            
            propagateSupplies(supplyPullAllSources());
            supplyPullTimer = supplyPullInterval;
        }
    }

    private int supplyPullAllSources()
    {
        int supply = 0;

        for(int i = 0; i < factories.Count; i++)
        {
            supply += factories[i].getSupply();
        }

        return supply;
    }

    private void propagateSupplies(int supply)
    {
        for(int i = 0; i < lanes.Count; i++)
        {
            int supplyAmt = Mathf.RoundToInt(supply * laneSupplyBias[i]);
            lanes[i].relaySuppliesToLead(supplyAmt);
        }
    }

    private void creepWaveTimerProcess()
    {
        if (creepWaveTimer > 0)
        {
            creepWaveTimer -= Time.fixedDeltaTime;
        }
        else
        {
            commandWaveBegin();
            creepWaveTimer = creepWaveInterval;
        }
    }

    private void commandWaveBegin()
    {
        for(int i = 0; i < lanes.Count; i++)
        {
            lanes[i].relayWaveBeginCommand();
        }
    }
}
