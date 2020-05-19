using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCompass : MonoBehaviour
{

    public MapManager mapManager;

    public Transform n;
    public Transform w;
    public Transform s;
    public Transform e;

    public float margin;

    // all positive. Think as distance from center
    private float xMax;
    private float yMax;
    private float yMin;

    private float currentAngle;

    private float[] switches = new float[2];

    // Start is called before the first frame update
    void Start()
    {
        xMax = mapManager.xMax - margin;
        yMax = mapManager.yMax - margin;
        yMin = Mathf.Abs(mapManager.yMin) - margin;

        // FOR USING TOP PORTION OF SQUARE
        // Greater than this angle, xMax is limiting factor
        // Less than this angle, yMax is limiting factor
        switches[0] = Mathf.Atan(xMax / yMax) * Mathf.Rad2Deg;


        // FOR USING BOTTOM PORTION OF SQUARE
        // Greater than this angle, xMax is limiting factor
        // Less than this angle, yMax is limiting factor
        switches[0] = Mathf.Atan(xMax / yMax) * Mathf.Rad2Deg;


        switches[1] = Mathf.Atan(xMax / yMin) * Mathf.Rad2Deg;
    }

    // Update is called once per frame
    void Update()
    {
        currentAngle = convertAngle(mapManager.transform.localEulerAngles.z);
        levelLetters(currentAngle);
        //Debug.LogWarning("Current angle: " + currentAngle);

        n.transform.localPosition = new Vector3(0.0f, getLetterRadius(0.0f), 0.0f);
        w.transform.localPosition = new Vector3(-getLetterRadius(90f), 0.0f, 0.0f);
        s.transform.localPosition = new Vector3(0.0f, -getLetterRadius(180), 0.0f);
        e.transform.localPosition = new Vector3(getLetterRadius(-90f), 0.0f, 0.0f);

    }

    private float getLetterRadius(float angleOffset)
    {
        float angle = Mathf.Abs(convertAngle(currentAngle + angleOffset));
        float radius = 0.0f;

        // if using top portion of square
        if(angle < 90)
        {
            if(angle < switches[0]) // if yMax is limit
            {
                radius = yMax / Mathf.Cos(Mathf.Deg2Rad * angle);
            }
            else // if xMax is limit
            {
                radius = xMax / Mathf.Sin(Mathf.Deg2Rad * angle);
            }
        }
        else // using bottom portion of square (smaller y limit)
        {
            angle = Mathf.Abs(180 - angle);
            
            if (angle < switches[1]) // if yMin is limit
            {
                radius = yMin / Mathf.Cos(Mathf.Deg2Rad * angle);
            }
            else // if xMax is limit
            {
                radius = xMax / Mathf.Sin(Mathf.Deg2Rad * angle);
            }

        }

        
        return radius;
    }

    private void levelLetters(float angle)
    {
        Vector3 newEuler = new Vector3(0.0f, 0.0f, -angle);
        n.localEulerAngles = newEuler;
        w.localEulerAngles = newEuler;
        s.localEulerAngles = newEuler;
        e.localEulerAngles = newEuler;
    }


    private float convertAngle(float angleSource)
    {
        if (angleSource > 180)
        {
            angleSource -= 360;
        }
        return angleSource;
    }
}
