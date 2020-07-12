using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{

    public float mapScaleFactor;
    public float playerBearing;

    public GameObject playerObj;
    public GameObject displayCenter;

    public GameObject mapIconPrefab;
    public GameObject mapRadiusPrefab;

    public GameObject backgroundContainer;

    public Transform target;

    public Color friendlyColor;
    public Color missileColor;
    public Color enemyColor;

    public Text rangeReadout;

    public float xMax;
    public float xMin;
    public float yMax;
    public float yMin;

    public float rangeMax = 2850; // set scaleFactor such that unit x meters away is right at edge of view

    public float rangeLerpRate;

    public short rangeTargetIndex = 0;

    public short[] RANGE_SETTINGS;

    private short currentStep = 1;

    public Color friendlyKillZoneColor;
    public Color enemyKillZoneColor;

    // not including one for fill radius. That's already default prefab img
    public Sprite outlineImage;

    

    public static float getBearing(Transform transform)
    {
        float bearing = 0f;
        Vector3 dir = transform.forward;
        dir = new Vector3(dir.x, 0f, dir.z);

        bearing = Vector3.Angle(dir, new Vector3(1, 0, 0));

        if(dir.z > 0)
        {
            bearing *= -1;
        }

        //Debug.LogWarning("Bearing is " + bearing);

        return bearing;
    }

    //public static Vector3 getRelativePosition

    // Start is called before the first frame update
    void Awake()
    {
        stepIndex();
        setRangeMax(rangeMax);
    }

    public void setRangeMax(float rangeMax)
    {
        this.rangeMax = rangeMax;

        // set scaleFactor such that unit x meters away is right at edge of view

        // yMax * scaleFactor --> rangeMax;
        // thereby, scaleFactor = rangeMax / yMax;
        mapScaleFactor = yMax / rangeMax;

        //Debug.LogError("mapScaleFactor is " + mapScaleFactor);

    }


    public void spawnIcon(CombatFlow linkFlow)
    {
        MapUnit newMapUnit = GameObject.Instantiate(mapIconPrefab, transform).GetComponent<MapUnit>();
        newMapUnit.linkToFlow(linkFlow);
    }

    // Update is called once per frame
    void Update()
    {
        if(playerObj == null)
        {
            playerObj = GameManager.getGM().localPlayer;
        }
        else
        {
            transform.localEulerAngles = new Vector3(0f, 0f, getBearing(playerObj.transform));
        }

        if (Input.GetButtonDown("Map Zoom"))
        {
            stepIndex();
        }

        if(target != null && playerObj != null)
        {
            selectedTargetRange(Vector3.Distance(target.position, playerObj.transform.position));
        }

        lerpToTargetRange();

    }

    private void stepIndex()
    {
        target = null;

        if(rangeTargetIndex == 0)
        {
            currentStep = 1;
        }
        if(rangeTargetIndex == RANGE_SETTINGS.Length - 1)
        {
            currentStep = -1;
        }

        rangeTargetIndex += currentStep;

        rangeReadout.text = "RANGE " + RANGE_SETTINGS[rangeTargetIndex] + "m";
    }

    private void lerpToTargetRange()
    {
        setRangeMax(Mathf.Lerp(rangeMax, RANGE_SETTINGS[rangeTargetIndex],
            Time.deltaTime * rangeLerpRate));
    }

    public bool withinBounds(Vector3 localDisplayPosition)
    {
        localDisplayPosition = Quaternion.Euler(transform.localEulerAngles) * localDisplayPosition;

        

        bool inBounds = localDisplayPosition.x > xMin &&
            localDisplayPosition.x < xMax &&
            localDisplayPosition.y < yMax &&
            localDisplayPosition.y > yMin;

        return inBounds;
    }


    public void selectedTargetRange(float range)
    {
        short newRangeIndex = 0;
        bool tryNext = true;

        

        // -1 because checking greater than first will go to second. Goes past end without -1
        for (int i = 0; i < RANGE_SETTINGS.Length - 1 && tryNext; i++)
        {
            tryNext = false;
            float tempScaleFactor = yMax / RANGE_SETTINGS[i];

            Vector3 relPos = target.position - playerObj.transform.position;
            relPos = new Vector3(relPos.x, 0f, -relPos.z);
            relPos = new Vector3(relPos.z, relPos.x, 0f);

            relPos *= tempScaleFactor;


            //Debug.LogError("INDEX " + i + ", range " + RANGE_SETTINGS[i]);
            bool inBounds = withinBounds(relPos);
            

            if (!inBounds)
            {
                newRangeIndex++; // step to next range
                tryNext = true;  // iterate loop again
            }
        }

        // if making range larger
        if(newRangeIndex > rangeTargetIndex)
        {
            if(newRangeIndex != RANGE_SETTINGS.Length - 1)
            {
                currentStep = 1; // continue making larger
            }
            else
            {
                currentStep = -1; // reached end, go back
            }
        }
        else if(newRangeIndex < rangeTargetIndex) // if making range smaller
        {
            if(newRangeIndex != 0)
            {
                currentStep = -1; // continue making smaller
            }
            else
            {
                currentStep = 1; // reached start, go larger
            }
        }

        rangeTargetIndex = newRangeIndex;

        rangeReadout.text = "RANGE " + RANGE_SETTINGS[rangeTargetIndex] + "m";
    }
}
