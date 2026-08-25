using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.UIElements;

public class AirSpawnController : MonoBehaviour
{
    // Start is called before the first frame update

    public List<TeamSpawner> spawners;

    private static GameObject localPlayerInstance;
    public static float timeSincePlayerDeath = 0.0f;

    public Dropdown spawnSelector;

    public Text timerText;

    private CombatFlow myFlow;

    public Text ticketDisplay;
    public Text ticketGenerateTimeDisplay;

    private static List<AirSpawnController> teamControllers;

    private void Awake()
    {
        //// arbitrarily high value, allow immediate first-time spawn
        //timeSincePlayerDeath = 100f;

        

        if(teamControllers == null)
        {
            initializeStaticRefs();
        }
        else if(teamControllers.Count < 2)
        {
            initializeStaticRefs();
        }
        else
        {
            linkToStaticRef();
        }
    }

    

    void Start()
    {
        buildSpawnDropdown();
    }

    // Update is called once per frame
    void Update()
    {
        tryIncrementPlayerRespawnTimer(Time.deltaTime);

        getSelectedSpawner().refreshTicketDisplay(ticketDisplay);
        getSelectedSpawner().updateTicketGenerateTimerDisplay(ticketGenerateTimeDisplay);
        
    }

    public static GameObject getLocalPlayerInstance()
    {
        return localPlayerInstance;
    }

    public static void setLocalPlayerInstance(GameObject localPlayerObj)
    {
        localPlayerInstance = localPlayerObj;
    }

    private void tryIncrementPlayerRespawnTimer(float deltaTime)
    {
        if(localPlayerInstance == null)
        {
            timeSincePlayerDeath += deltaTime / 2f;
            updateTimerText();
        }
    }

    private void updateTimerText()
    {
        TeamSpawner selectedSpawner = getSpawner(spawnSelector.value);

        float spawnTimer = selectedSpawner.respawnTimeEffective - timeSincePlayerDeath;
        timerText.text = (Mathf.Max(Mathf.RoundToInt(spawnTimer), 0)).ToString();
    }

    public bool playerCanRespawn()
    {
        return getSelectedSpawner().playerCanRespawn();
    }

    public TeamSpawner getSelectedSpawner() 
    {
        return spawners[spawnSelector.value];
    }

    public TeamSpawner getSpawner(int index = 0)
    {
        return spawners[index];
    }

    private void cleanSpawnerList()
    {
        for(int i = 0; i < spawners.Count; i++)
        {
            TeamSpawner spawner = spawners[i];
            if(spawner == null || getTeam() != spawner.team)
            {
                spawners.RemoveAt(i);
                i--;
            }
        }
    }

    public void buildSpawnDropdown()
    {
        cleanSpawnerList();

        spawnSelector.options.Clear();

        for(int i = 0; i < spawners.Count; i++)
        {
            TeamSpawner spawner = spawners[i];

            spawnSelector.options.Add(new Dropdown.OptionData(spawner.displayName));
        }

        spawnSelector.value = 0;
        spawnSelector.RefreshShownValue();
    }

    public void tryAddSpawner(TeamSpawner spawner)
    {
        if (!spawners.Contains(spawner))
        {
            spawners.Add(spawner);
        }
    }

    public void removeSpawner(TeamSpawner spawner)
    {
        spawners.Remove(spawner);
        buildSpawnDropdown();
    }

    public CombatFlow.Team getTeam()
    {
        return getFlow().team;
    }

    public CombatFlow getFlow()
    {
        if (myFlow == null)
        {
            myFlow = GetComponent<CombatFlow>();
        }
        return myFlow;
    }

    private void linkToStaticRef()
    {
        //Debug.LogError("Linking spawncontroller to static: " + gameObject.name +
        //    " while list size = " + teamControllers.Count);
        teamControllers[(int)getFlow().team] = this;
    }

    private void initializeStaticRefs()
    {
        teamControllers = new List<AirSpawnController>();
        teamControllers.Add(this);
        teamControllers.Add(this);
    }

    

    public static AirSpawnController getTeamController(CombatFlow.Team team)
    {
        return teamControllers[(int)team];
    }

}
