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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setLockProgress(float progressLerp)
    {
        progressLerp = Mathf.Clamp(progressLerp, 0.0f, 1.0f);

        // SCALE
        float effectiveMax = Mathf.Max(maxScale, maxScale * baseScale);
        float scale = Mathf.Lerp(effectiveMax, minScale * baseScale, progressLerp);
        lockImageCenter.transform.localScale = new Vector3(scale, scale, 1.0f);

        // ROTATION
        float rotation = Mathf.Lerp(beginRotation, endRotation, progressLerp);
        Vector3 rotEuler = lockImageCenter.transform.localEulerAngles;
        rotEuler.z = rotation;
        lockImageCenter.transform.localEulerAngles = rotEuler;


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
