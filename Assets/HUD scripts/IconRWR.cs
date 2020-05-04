using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconRWR : MonoBehaviour
{
    public static string DASH_STR = "v\n";
    public static float RANGE_EXTRA_LONG =  5250;   // no dashes
    public static float RANGE_LONG =        2800;   // one dash
    public static float RANGE_MEDIUM =      1200;   // two dashes
    // 0 - 1200 is close range

    public static float UI_POSITION_Y_MISSILE = 90f; //223 is prefab default
    
    public bool isPinging = false;

    private bool isPingingSet = true; // prevent repeated processing for unchanging isPinging state


    public float relativeBearing = 0f; // -180 to 180

    private byte distanceDashesNum = 0; // 0 extra long, 1 long, 2 medium, 3 close

    public Radar radarSource;



    public GameObject iconCenter;
    public Text textID;
    public Text textDashes;

    public GameObject linkedObj;

    bool hasSet = false;

    // inefficient to have each icon save this. This does let it be edited in Editor easily, though
    // not worth creating a manager script for
    public Color missileColor;


    // Start is called before the first frame update
    void Start()
    {
        showPingResult(false, 0, 0);
    }

    void Update()
    {
        if(linkedObj == null && hasSet)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void linkToRadarSource(Radar radar)
    {
        

        hasSet = true;
        textID.text = radar.radarID;

        CombatFlow radarFlow = radar.GetComponent<CombatFlow>();
        radarSource = radar;
        linkedObj = radar.gameObject;

        if(radarFlow.type == CombatFlow.Type.PROJECTILE)
        {
            Debug.LogWarning("Linking projectile radar " + radar.gameObject.name);
            iconCenter.transform.localPosition = new Vector3(0, UI_POSITION_Y_MISSILE, 0);
            makeRed();
        }

    }

    public void makeRed()
    {
        textID.color = missileColor;
        textDashes.color = missileColor;
    }

    public void showPingResult(bool isPinging, float distance, float relBearing)
    {
        this.isPinging = isPinging;
        showIcon(isPinging);
        setDistanceDashes(distance);

        //Debug.LogWarning("showPing result " + distance);

        transform.localRotation = Quaternion.Euler(0f, 0f, relBearing);
        textID.transform.localRotation = Quaternion.Euler(0f, 0f, -relBearing);
    }

    private void setDistanceDashes(float distance)
    {
       // Debug.LogWarning("set distance dashes: " + distance);
        if (isPinging) // don't bother processing if icon not visible
        {
            //// only change string value if it needs to
            byte currentDashCalc = calculateDistanceDashes(distance);
            if (currentDashCalc != distanceDashesNum)
            {
                showDashString(currentDashCalc);
            }
        }
    }

    private byte calculateDistanceDashes(float distance)
    {
        //Debug.LogWarning("distance dash sees " + distance + " distance");

        if(distance > RANGE_EXTRA_LONG)
        {
            // no dashes
            return 0;
        }
        else if(distance > RANGE_LONG)
        {
            // one dash
            return 1;
        }
        else if(distance > RANGE_MEDIUM)
        {
            // two dashes
            return 2;
        }
        else
        {
            // three dashes
            return 3;
        }
    }

    private void showDashString(byte num)
    {
       // Debug.LogWarning("Showing " + num + " dashes");

        distanceDashesNum = num;

        string newString = "";
        for(int i = 0; i < num; i++)
        {
            newString += DASH_STR;
        }

        textDashes.text = newString;
    }

    private void showIcon(bool doShow)
    {
        if(doShow != isPingingSet) // avoid repeated processing for unchanging value
        {
            isPingingSet = doShow;

            Vector3 newScale = new Vector3(0.0f, 0.0f, 0.0f); // default to hide

            if (doShow)
            {
                newScale = new Vector3(1.0f, 1.0f, 1.0f);
            }

            iconCenter.transform.localScale = newScale;
        }
    }
}
