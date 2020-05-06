using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{

    public float mapScaleFactor;
    public float playerBearing;

    public GameObject playerObj;
    public GameObject displayCenter;

    public GameObject mapIconPrefab;

    public Color friendlyColor;
    public Color missileColor;
    public Color enemyColor;


    public float xMax;
    public float xMin;
    public float yMax;
    public float yMin;

    public float rangeMax = 2850; // set scaleFactor such that unit x meters away is right at edge of view

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
    }

    public bool withinBounds(Vector3 localDisplayPosition)
    {
        localDisplayPosition = Quaternion.Euler(transform.localEulerAngles) * localDisplayPosition;

        return localDisplayPosition.x > xMin &&
            localDisplayPosition.x < xMax &&
            localDisplayPosition.y < yMax &&
            localDisplayPosition.y > yMin;
    }
}
