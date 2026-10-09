using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockIndicator : MonoBehaviour
{
    public GameObject lockImageCenter;
    public Image lockImage;

    public float minScale = .42f;
    public float maxScale = 1.5f;

    public float baseScale = 1.0f;

    public Color activeColor;

    public float beginRotation = -360f;
    public float endRotation = 45f;

    public float oversizeBeginPercent = .85f;
    public float oversizeScaleAdd = .5f;

    TgtIconManager manag;

    // Start is called before the first frame update
    void Start()
    {
        manag = TgtIconManager.tgtIconManager;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setLockProgress(float progressLerp)
    {
        progressLerp = Mathf.Clamp(progressLerp, 0.0f, 1.0f);

        // SCALE
        float effectiveMaxScale = effectiveMax();


        float scale = Mathf.Lerp(effectiveMaxScale, minScale * baseScale, progressLerp);
        lockImageCenter.transform.localScale = new Vector3(scale, scale, 1.0f);

        // ROTATION
        float rotation = Mathf.Lerp(beginRotation, endRotation, progressLerp);
        Vector3 rotEuler = lockImageCenter.transform.localEulerAngles;
        rotEuler.z = rotation;
        lockImageCenter.transform.localEulerAngles = rotEuler;


    }

    public float effectiveMax()
    {
        float scalePercent = manag.readScalePercent(baseScale);
        float effectiveOversize = (1.0f + oversizeScaleAdd) * maxScale;

        return Mathf.Lerp(maxScale, effectiveOversize, oversizePercent(scalePercent));
    }

    private float oversizePercent(float scalePercent)
    {
        float percent = (scalePercent - oversizeBeginPercent) / (1.0f - oversizeBeginPercent);
        return Mathf.Clamp(percent, 0.0f, 1.0f);
    }

    public void setColor(Color color)
    {
        if(color != activeColor)
        {
            activeColor = color;
            lockImage.color = activeColor;
        }
    }
}
