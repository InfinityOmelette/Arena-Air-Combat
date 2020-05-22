using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunLeadReticle : MonoBehaviour
{

    public hudControl hud;

    public GunLeadComputer comp;

    private bool showSight = false;

    public GameObject image;

    public float aimPointDist;

    void Awake()
    {
        image.SetActive(showSight);
    }

    // Start is called before the first frame update
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
