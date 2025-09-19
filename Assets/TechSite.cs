using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechSite : MonoBehaviour
{
    public CombatFlow myFlow;

    public List<CombatFlow> aircraftInZone;

    public float captureTime;

    public CombatFlow.Team capturingTeam;

    public List<float> captureTimers;

    public float cleanListInterval = 1f;
    private float cleanListTimer;

    public TechObject techObj; // prefab reference. make sure to instantiate before inserting into tech inventories



    private void Awake()
    {
        myFlow = GetComponent<CombatFlow>();
        aircraftInZone = new List<CombatFlow>();
        cleanListTimer = cleanListInterval;


    }


    void FixedUpdate()
    {
        // increment timer for capturing team
        if(capturingTeam != CombatFlow.Team.NEUTRAL)
        {
            int teamIndex = (int)capturingTeam;
            captureTimers[teamIndex] += Time.fixedDeltaTime;

            // If capture time completed, award capture to winning team
            if(captureTimers[teamIndex] > captureTime)
            {
                doCapture();
            }
        }


        // increment cleanListTimer
        if(cleanListTimer < 0)
        {
            cleanList();
            cleanListTimer = cleanListInterval;
        }
        else
        {
            cleanListTimer -= Time.fixedDeltaTime;
        }
        
    }

    // probably make this an RPC for networking?
    public void doCapture()
    {
        Debug.Log("Tech object captured for: " + capturingTeam);

        // instantiate tech object from prefab ref
        GameObject newObj = GameObject.Instantiate(techObj.gameObject);
        TechObject newTech = newObj.GetComponent<TechObject>();

        // add tech to winning team's tech inventory
        TechInventory.teamTechInventories[(int)capturingTeam].addTech(newTech);

        // destroy this object
        myFlow.destroySelf();
    }

    private void OnTriggerEnter(Collider other)
    {
        CombatFlow otherFlow = other.transform.root.GetComponent<CombatFlow>();
        Debug.Log("Tech side collided with: " + other.transform.root.name);
        if(otherFlow != null && otherFlow.type == CombatFlow.Type.AIRCRAFT)
        {
            Debug.Log("Tech site collision valid");
            if (!aircraftInZone.Contains(otherFlow))
            {
                aircraftInZone.Add(otherFlow);

                setCapturingTeam(checkCapturingTeam());

            }
        }
    }

    private CombatFlow.Team checkCapturingTeam()
    {
        CombatFlow.Team dominantTeam = CombatFlow.Team.NEUTRAL;

        // check if only one team's aircraft are in zone
        for(int i = 0; i < aircraftInZone.Count; i++)
        {
            CombatFlow currAircraft = aircraftInZone[i];

            if(dominantTeam != currAircraft.team)
            {
                // if this is the first non-neutral team found
                if(dominantTeam == CombatFlow.Team.NEUTRAL)
                {
                    dominantTeam = currAircraft.team; // this aircraft's team is now capturing
                }
                else // if there are multiple teams within trigger
                {
                    // nobody is capturing. Exit loop and function entirely to prevent any re-capture bugs
                    return CombatFlow.Team.NEUTRAL;
                }
            }
        }

        // if all aircraft leave, airspace remains secured for last triggering team
        if(aircraftInZone.Count == 0)
        {
            dominantTeam = capturingTeam;
        }

        return dominantTeam;
    }

    private void OnTriggerExit(Collider other)
    {
        CombatFlow otherFlow = other.transform.root.GetComponent<CombatFlow>();

        if (otherFlow != null && otherFlow.type == CombatFlow.Type.AIRCRAFT)
        {
            if (aircraftInZone.Contains(otherFlow))
            {
                aircraftInZone.Remove(otherFlow);

                setCapturingTeam(checkCapturingTeam());
                
            }
        }
    }

    private void setCapturingTeam(CombatFlow.Team team)
    {
        capturingTeam = team;
        myFlow.setNetTeam(capturingTeam);
        myFlow.myHudIconRef.setTeamInfo();
    }

    private void cleanList()
    {
        for(int i = 0; i < aircraftInZone.Count; i++)
        {
            if(aircraftInZone[i] == null)
            {
                aircraftInZone.RemoveAt(i);
                i--; //re-check same index next iteration
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
    }
}
