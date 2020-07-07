using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeLadder : MonoBehaviour
{


    public float LADDER_TOP_COORD;
    public float LADDER_BOTTOM_COORD;

    public float maxLockRange = 1000f; // top range on ladder


    public GameObject goodFill;

    public Text longText;
    public Text goodText;
    public Text killText;
    public Text targetText;

    public Text maxRangeText;


    public Radar linkedRadar;

    public TgtComputer linkedTgtComputer;


    public float tgtRange = 1.0f;

    public float defaultRangeFactor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    void Update()
    {
        if(linkedRadar != null && linkedTgtComputer != null)
        {
            maxLockRange = linkedRadar.maxLockRange;

            setMaxRangeText(maxLockRange);

            if (linkedTgtComputer.radarLocked && linkedTgtComputer.currentTarget != null)
            {
                targetText.enabled = true;
                setTgtRange(tgtRange);

                setObjectAtRange(longText.gameObject, linkedRadar.effectiveLongRange);
                setObjectAtRange(goodText.gameObject, linkedRadar.effectiveGoodRange);
                setObjectAtRange(killText.gameObject, linkedRadar.effectiveKillRange);


                setGoodFillToRange(linkedRadar.effectiveGoodRange);
            }
            else
            {
                targetText.enabled = false;
                setObjectAtRange(longText.gameObject, linkedRadar.baseLongRange * defaultRangeFactor);
                setObjectAtRange(goodText.gameObject, linkedRadar.baseGoodRange * defaultRangeFactor);
                setObjectAtRange(killText.gameObject, linkedRadar.baseKillRange * defaultRangeFactor);

                setGoodFillToRange(linkedRadar.baseGoodRange * defaultRangeFactor);
            }
            
        }
    }

    private void setMaxRangeText(float range)
    {
        maxRangeText.text = (range / 1000f).ToString("F1") + "km";
    }

    public void setGoodFillToRange(float range)
    {
        float yScale = Mathf.Clamp(range / maxLockRange, 0.0f, 1.0f);
        Vector3 scale = goodFill.transform.localScale;
        goodFill.transform.localScale = new Vector3(scale.x, yScale, scale.y);
    }

    public void setTgtRange(float range)
    {
        setObjectAtRange(targetText.gameObject, range);

        // Convert meters to kilometers, show 2 decimal places
        targetText.text = "TGT >>>\n" +
            (range / 1000f).ToString("F1") + "km   ";
    }

    private void setObjectAtRange(GameObject obj, float range)
    {
        float yPos = getRangeCoord(range);
        Vector3 pos = obj.transform.localPosition;
        obj.transform.localPosition = new Vector3(pos.x, yPos, pos.z);
    }

    private float getRangeCoord( float range)
    {
        return getScaleCoord(range / maxLockRange);
    }

    private float getScaleCoord( float scale)
    {
        scale = Mathf.Clamp(scale, 0.0f, 1.0f);
        return (LADDER_TOP_COORD - LADDER_BOTTOM_COORD) * scale + LADDER_BOTTOM_COORD;

    }
}
