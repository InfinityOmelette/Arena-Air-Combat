using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSightReticle : MonoBehaviour
{

    public hudControl hud;

    public DropSightComputer comp;

    private bool showSight = false;

    public GameObject image;


    void Awake()
    {
        image.SetActive(showSight);
    }


    void Start()
    {
        hud = hudControl.mainHud.GetComponent<hudControl>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showReticle(bool show)
    {
        if (show != showSight)
        {
            showSight = show;
            image.SetActive(show);
        }
    }


    public void placeReticle(Vector3 aimPoint)
    {
        hud.drawItemOnScreen(gameObject, aimPoint, 1.0f);
    }
}
